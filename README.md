
Orchestrate.Io
====

A .NET SDK for Orchestrate.io

To install Orchestrate.Io .NET SDK, run the following command in the [Package Manager Console](http://docs.nuget.org/consume/package-manager-console) 
```
PM> Install-Package orchestrate.io -Pre
```
Requires NuGet 2.8.5 or higher

Usage examples

```c#
    // Import the client
    using Orchestrate.Io

    // Create an Application
	var application = new Application("Your API Key");

	// Create a Client
	var client = new Client(application);

	// Get a Collection
	var collection = client.GetCollection("collection");

    // Get a value
	var kvObject = await collection.GetAsync<dynamic>("key");
	var value = kvObject.Value;
```