# ASP.NET Core Cloudant Sample

This application demonstrates how to use the Bluemix Cloudant NoSQL DB Service in an ASP.NET Core application.

[![Deploy to Bluemix](https://bluemix.net/deploy/button.png)](https://bluemix.net/deploy?repository=https://github.com/IBM-Bluemix/aspnet-core-cloudant)

## Run the app locally

1. Install ASP.NET Core and the Dotnet CLI by following the [Getting Started][] instructions
+ cd into this project's root directory, then `src/dotnetCloudantWebstarter`
+ Copy the value for the VCAP_SERVICES envirionment variable from the application running in Bluemix and paste it in the config.json file
+ Run `dotnet restore`
+ Run `dotnet run`
+ Access the running app in a browser at <http://localhost:5000>

[Getting Started]: http://docs.asp.net/en/latest/getting-started/index.html
