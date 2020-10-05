using System.Collections.Specialized;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Web;
using System.Xml;

namespace Moriyama.ConfigBuilder
{
    public class MoriyamaConfigBuilder : ConfigurationBuilder
    {
        private bool _enabled;
        private string _environment;
        private string _mode;

        public override void Initialize(string name, NameValueCollection config)
        {
            base.Initialize(name, config);
            this._enabled = true;

            this._environment = "preview";
            this._mode = "paramsFile";

            if (config["enabled"] != null)
            {
                bool.TryParse(config["enabled"], out this._enabled);
            }
            if (config["environment"] != null)
            {
                this._environment = config["environment"];
            }
            if (config["mode"] != null)
            {
                this._mode = config["mode"];
            }
        }

        public override XmlNode ProcessRawXml(XmlNode rawXml)
        {
            
            if (!this._enabled)
            {
                return rawXml;
            }

            string path = HttpRuntime.AppDomainAppPath;

            if (!Directory.Exists(path))
            {
                return rawXml;
            }

            string parametersFile = Path.Combine(path, "Parameters.xml");
            string setParametersFile = Path.Combine(path, string.Format("SetParameters.{0}.secret.config", this._environment));

            if (!File.Exists(parametersFile))
            {
                return rawXml;
            }

            if (!File.Exists(setParametersFile))
            {
                return rawXml;
            }

            XmlDocument parameters = new XmlDocument();
            parameters.Load(parametersFile);

            XmlDocument parameterValues = new XmlDocument();
            parameterValues.Load(setParametersFile);

            return ApplyValues(rawXml, parameters, parameterValues);
        }

        private XmlNode ApplyValues(XmlNode node, XmlDocument parameters, XmlDocument parameterValues)
        {
            string configName = node.Name;
            foreach (XmlNode parameter in parameters.SelectNodes("//parameters/parameter/parameterEntry"))
            {
                if (parameter.Attributes["match"] != null)
                {
                    string match = parameter.Attributes["match"].Value;
                    if (AppliesTo(configName, match))
                    {
                        string value = ValueFor(parameter, parameterValues);
                        if (!string.IsNullOrEmpty(value))
                        {
                            string xpath = RelativeXpath(configName, match);
                            XmlNode set = node.SelectSingleNode(xpath);
                            if (set != null)
                            {
                                if (set is XmlAttribute && string.IsNullOrEmpty(set.Value))
                                {
                                    set.Value = value;
                                }
                                else if(string.IsNullOrEmpty(set.InnerText))
                                {
                                    set.InnerText = value;
                                }
                            }
                        }
                    }
                }
            }

            return node;
        }

        private string ValueFor(XmlNode parameter, XmlDocument values)
        {
            if (parameter.ParentNode.Attributes["name"] == null)
            {
                return null;
            }

            string name = parameter.ParentNode.Attributes["name"].Value;
            XmlNode valueNode = values.SelectSingleNode(string.Format("//parameters/setParameter[@name ='{0}']", name));

            if (valueNode != null && valueNode.Attributes["value"] != null)
            {
                return valueNode.Attributes["value"].Value;
            }

            return null;
        }

        private string RelativeXpath(string name, string xpath)
        {
            if (xpath.StartsWith("//" + name))
            {
                return xpath;
            }

            xpath = xpath.Replace("//", string.Empty);
            string[] segments = xpath.Split('/');

            while (segments[0] != name)
            {
                segments = segments.Skip(1).ToArray();
            }
            segments = segments.Skip(1).ToArray();

            return string.Join("/", segments);
        }

        private bool AppliesTo(string name, string xpath)
        {
            return xpath.StartsWith("//" + name) || xpath.Contains("/" + name + "/");
            return false;
        }

    }
}