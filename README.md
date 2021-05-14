# Moriyama Config Builder

A Configuration builders for ASP.NET that uses a web deploy formatted parameters.xml file to define parameters and a secrets file to store values.

## About Configuration Builders

See https://docs.microsoft.com/en-us/aspnet/config-builder

ASP.NET (4.7) allows you to write code to construct application configuration dynamically at runtime rather than storing values in web.config or other configuration files.

This can be particularly useful when managing secret configuration that you do not want to commit to version control.

The default configuration builders only allow Key/Value parameters by default. This project allows you to define your parameters using XML/XPath - and then manage and distribute a single shared secrets file amongst your development team.

## Installing

You can add the following Nuget package to your .NET Framework project

- [Moriyama.ConfigBuilder](https://www.nuget.org/packages/Moriyama.ConfigBuilder/)

## Defining your parameters

The project uses the same Parameters.xml format as WebDeploy - see https://docs.microsoft.com/en-us/aspnet/web-forms/overview/deployment/web-deployment-in-the-enterprise/configuring-parameters-for-web-package-deployment

Create a file named Parameters.xml at the root of your project:

```
<parameters>

  <parameter
    description="Web:umbracoDbDSN"
    name="Web:umbracoDbDSN"
    defaultvalue="#{Web:umbracoDbDSN}#">
    <parameterEntry
      match="//configuration/connectionStrings/add[@name='umbracoDbDSN']/@connectionString"
      kind="XmlFile"
      scope="\\Web.config$" />
  </parameter>
  
  <parameter
    description="Web:umbracoLocalTempStorage"
    name="Web:umbracoLocalTempStorage"
    defaultvalue="#{Web:umbracoLocalTempStorage}#">
    <parameterEntry
      match="//configuration/appSettings/add[@key='umbracoLocalTempStorage']/@value"
      kind="XmlFile"
      scope="\\Web.config$" />
  </parameter>
  
</parameters>
```

The example above is defining a connection string and an appsetting in Web.Config but you can target any configuration file in your application.

Next create a file containing your secret values in the webroot of your project called SetParameters.environment.secret.config (note environment could be swapped with dev/staging/prod etc).

The SetParametrs file is key value:

```
<parameters>
  <setParameter
    name="Web:umbracoDbDSN"
    value="Insert connection string" />
  <setParameter
    name="Web:umbracoLocalTempStorage"
    value="True" />
</parameters>
```

## Setting up config builder in your project

Add the following to the configSections element of your Web.Config

```
<section name="configBuilders" 
    type="System.Configuration.ConfigurationBuildersSection, System.Configuration, Version=4.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a" 
    restartOnExternalChanges="false" requirePermission="false" />
```

Next add the following below the root element of your Web.Config file:

```
<configBuilders>
    <builders>
      <add name="Moriyama"
     type="Moriyama.ConfigBuilder.MoriyamaConfigBuilder, Moriyama.ConfigBuilder" 
           enabled="true" environment="preview" mode="paramsFile"/>
    </builders>
  </configBuilders>
```


The environment attribute parameter will allow you to switch between parameter files for environments e.g.

- preview - SetParameters.preview.secret.config
- myenv - SetParameters.myenv.secret.config

Finally for any configuration element that you want to use the config builder add the configBuidlers attribute e.g:

```
<appSettings configBuilders="Moriyama">
    ....

<smtp from="" configBuilders="Moriyama">
    ....
```

## Securing your secrets

Obviously the single file that contains all of your secrets contains very sensitive information about your project!

You should start by adding *.secret.config to your .gitignore file to make sure it isn't accidentally commited.

## Sharing secrets

Your secrets file can be shared between members of your team. At Moriyama we have a method of pushing and pulling the secrets files to secure storage - which is secured by the oragnisational account of the developer.

Doing this means, that you can programatically access project configuration with some sort of devops code - and programatically rotate secrets (such as connection strings) to increase secuirty

## I need help!

At Moriyama we help companies to setup their build/test/deploy processes for .NET and Umbraco applications - please [get in touch](https://moriyama.co.uk/contact-us/) if you need our help.


