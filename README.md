## Subroute.io - Online C# IDE for Webhooks and Microservices

**Comming Soon:** Developer machine setup and deployment instructions. For now, source repository has been purged of all sensitive information related to hosting and deployment.

### Developer Machine Setup ###

You need to add a *.secret.config file to each of the following projects and they must contain the following settings. You'll need to use your own Azure subscription to satisfy some of the configuration requirements (Subroute.Storage.ConnectionString for example).

  * Subroute.Api

  When you provide your SQL connection string, the Entity Framework Code-First migration library will create all the required tables for you.
  You should be able to get by without providing the Subroute.Storage.ConnectionString (Azure Storage Connection String).
  For now you should also be able to get by without providing the MailChimp configuration settings.

        <?xml version="1.0"?>
        <appSettings>
            <add key="Subroute.ConnectionString" value="" />
            <add key="Subroute.Storage.ConnectionString" value="" />
            <add key="Subroute.ApiBaseUri" value="" />
            <add key="Subroute.ServiceBus.ConnectionString" value="" />
            <add key="Subroute.ServiceBus.RequestTopicName" value="" />
            <add key="Subroute.ServiceBus.ResponseTopicName" value="" />
            <add key="Subroute.ServiceBus.RequestSubscriptionName" value="" />
            <add key="Subroute.ServiceBus.ResponseSubscriptionNameFormat" value="" />
            <add key="Subroute.AppInsights.InstrumentationKey" value="" />
            <add key="Subroute.Auth0.ManagementApiUri" value="" />
            <add key="Subroute.Auth0.ManagementApiToken" value="" />
            <add key="Subroute.MailChimp.API.Key" value="" />
            <add key="Subroute.MailChimp.All.Subscriber.ListId" value="" />
            <add key="auth0:ClientId" value="" />
            <add key="auth0:ClientSecret" value="" />
            <add key="auth0:Domain" value="" />
        </appSettings>

  * Subroute.App

  Most of the UI run after providing the SQL connection string and the Auth0 configuration settings for the Subroute.Api project.
  It's important to provide the Url configuration settings because they are used to map the UI to the API.
  We typically host using IIS locally during development, and use a convention such as subroute-app.local for our bindings, and we also update our local host file.

        <?xml version="1.0"?>
        <appSettings>
          <add key="Client.Debug" value="true" />
          <add key="Client.EnableGallery" value="true" />
          <add key="Client.ApiUrl" value="" />
          <add key="Client.AppBaseUrl" value="" />
        </appSettings>

  * Subroute.Container

        <?xml version="1.0" encoding="utf-8" ?>
        <appSettings>
          <add key="Subroute.ConnectionString" value="" />
          <add key="Subroute.Storage.ConnectionString" value="" />
          <add key="Subroute.ServiceBus.ConnectionString" value="" />
          <add key="Subroute.ServiceBus.RequestTopicName" value="" />
          <add key="Subroute.ServiceBus.ResponseTopicName" value="" />
          <add key="Subroute.ServiceBus.RequestSubscriptionName" value="" />
          <add key="Subroute.ServiceBus.ResponseSubscriptionNameFormat" value="" />
        </appSettings>

This repository contains the following projects:

  * Subroute.Api
  
  Contains a C#.NET Web API project that provides all the functionality necessary to provide the single page app (IDE) with data. This API is secured with an Auth0.com authentication token.
  
  * Subroute.App
  
  Contains a C#.NET ASP.NET web project that hosts a Durandal site as well as a very small Web API component to proxy the auth calls to protect the auth0.com client secret as well as a set of class that generate a javascript file containing an AMD compatible JSON object that contains web.config settings. These classes allow the configuration of client-side settings using the server-side web.config.
  
  * Subroute.Common
  
  Contains a C#.NET class library project that contains the functionality used in the users route code as well as the core library.
  
  * Subroute.Container
  
  Contains a C#.NET console app that is the safe sandbox environment where the user's route code executes. It's triggered via the Azure service bus using the Azure Web Jobs framework.
  
  * Subroute.Core
  
  Contains a C#.NET class library that contains all the functionality used in the API project as well as the full-trust layer of the execution container. The user's route code executes in its own AppDomain inside the container console app.
