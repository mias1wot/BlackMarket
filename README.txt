This is API for BlackMarket project.

LinkUp practice Team 5 - 2022.



For Dependency Injection I installed Unity.WebAPI 5.4.0
For Automapper I installer AutoMapper.Unity 6.0.0
For logging I installed NLog and for writing it to Azure Blob Storage - NLog.Extensions.AzureBlobStorage

For getting rif of cyclic dependendy problem when converting Model to JSON, I added to the constructor of the database model:
Configuration.ProxyCreationEnabled = false;

For setting up CORS I downloaded Microsoft.AspNet.WebApi.Cors ... and this didn't work!!!
