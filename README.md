## Subroute.io - Online C# IDE for Webhooks and Microservices

**Comming Soon:** Developer machine setup and deployment instructions. For now, source repository has been purged of all sensitive information related to hosting and deployment.

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
