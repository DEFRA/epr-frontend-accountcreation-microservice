# Frontend Scheme Registration

## Overview

Account Creation handles the front end creation of Organisations and users

## How To Run

### Prerequisites
In order to run the service you will need the following dependencies:
- .NET 8

#### epr-packaging-common
##### Developers working for a DEFRA supplier
In order to restore and build the source code for this project, access to the `epr-packaging-common` package store will need to have been setup.
 - Login to Azure DevOps
 - Navigate to [Personal Access Tokens](https://dev.azure.com/defragovuk/_usersSettings/tokens)
 - Create a new token
   - Enable the `Packaging (Read)` scope

Add the following to your `src/Nuget.Config`

```xml
<packageSourceCredentials>
  <epr-packaging-common>
    <add key="Username" value="<email address>" />
    <add key="ClearTextPassword" value="<personal access token>" />
  </epr-packaging-common>
</packageSourceCredentials>
```

##### Members of the public
Clone the [epr_common](https://dev.azure.com/defragovuk/RWD-CPR-EPR4P-ADO/_git/epr_common) repository and add it as a project to the solution you wish to use it in. By default the repository will reference the files as if they are coming from the NuGet package. You simply need to update the references to make them point to the newly added project.

### Run
Go to `src/FrontendAccountCreation.Web` directory and execute:

```
dotnet run
```

## Docker
Run in terminal at the solution source root:

```
docker build -t accountcreationfrontend -f FrontendAccountCreation.Web/Dockerfile .
```

To run the image, execute the following command:

```
docker run -p 5167:3000 --name accountcreationfrontendcontainer accountcreationfrontend
```

You can configure each appsetting by adding ```-e``` flag to the above command.

Do a GET Request to ```http://localhost:5167/create-account/health``` to confirm that the service is running.

## Redis

### App settings
Add the following variables to appsettings.Development.json/appsettings.json/launchSettings.json:
```
"Redis__ConnectionString": "localhost:6379"
```

### To install Redis and Redis Stack
Recommended way of running Redis is to run it via Docker. In terminal run:
```
docker run -d --name epr-producers- -p 6379:6379 -p 8001:8001 redis/redis-stack:latest
```

### Inspect Redis keys in the session
To view the keys in Redis cache, open browser and point at: http://localhost:8001/redis-stack/browser

# How To Test

### Unit tests

On `src`, execute:

```
dotnet test
```

### Running within your local development environment

The configuration below needs entering for your local development environment

```
{
  "FacadeAPI": {
    "BaseEndpoint": "localhost or dev environment"
  },
  "Redis": {
    "ConnectionString": "localhost:6379"
  },
  "AzureADB2C": {
    "ClientId": "[to be completed]",
    "ClientSecret": "[to be completed]",
    "Instance": "https://AZDCUSPOC2.b2clogin.com",
    "Domain": "AZDCUSPOC2.onmicrosoft.com",
  },
}
```


## How To Debug
Use debugging tools in your chosen IDE.

## Additional Information

### Monitoring and Health Check
A health check can be found at ```{BASE_URL}/admin/health```

## Contributing to this project

Please read the [contribution guidelines](CONTRIBUTING.md) before submitting a pull request.

## Licence

[Licence information](LICENCE.md).
