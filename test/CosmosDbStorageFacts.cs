﻿using System;
using System.Collections.Generic;
using System.Linq;
using Hangfire.Server;
using Hangfire.Storage;
using Microsoft.Azure.Cosmos;
using Microsoft.Extensions.Configuration;
using Xunit;

namespace Hangfire.Azure.Tests;

public class CosmosDbStorageFacts
{
	private readonly string url;
	private readonly string secret;
	private readonly string database;
	private readonly string container;

	public CosmosDbStorageFacts()
	{
		IConfiguration configuration = new ConfigurationBuilder()
			.AddJsonFile("appsettings.json", false, false)
			.AddEnvironmentVariables()
			.Build();

		IConfigurationSection section = configuration.GetSection("CosmosDB");
		url = section.GetValue<string>("Url") ?? string.Empty;
		secret = section.GetValue<string>("Secret") ?? string.Empty;
		database = section.GetValue<string>("Database") ?? string.Empty;
		container = section.GetValue<string>("Container") ?? string.Empty;
	}

	[Fact]
	public void Ctor_ThrowsAnException_WhenUrlIsNullOrEmpty()
	{
		ArgumentNullException exception = Assert.Throws<ArgumentNullException>(() => new CosmosDbStorage(null!, string.Empty, "databaseName", "container"));
		Assert.Equal("url", exception.ParamName);
	}

	[Fact]
	public void Ctor_ThrowsAnException_WhenCosmosClientIsNullOrEmpty()
	{
		ArgumentNullException exception = Assert.Throws<ArgumentNullException>(() => new CosmosDbStorage( null!,"databaseName", "container", null));
		Assert.Equal("cosmosClient", exception.ParamName);
	}

	[Fact]
	public void Ctor_ThrowsAnException_WhenSecretIsNullOrEmpty()
	{
		ArgumentNullException exception = Assert.Throws<ArgumentNullException>(() => new CosmosDbStorage("http://", null!, "databaseName", "container"));
		Assert.Equal("authSecret", exception.ParamName);
	}

	[Fact]
	public void Ctor_ThrowsAnException_WhenDatabaseNameIsNullOrEmpty()
	{
		ArgumentNullException exception = Assert.Throws<ArgumentNullException>(() => new CosmosDbStorage("http://", Guid.NewGuid().ToString(), null!, string.Empty));
		Assert.Equal("databaseName", exception.ParamName);
	}

	[Fact]
	public void Ctor_ThrowsAnException_WhenContainerNameIsNullOrEmpty()
	{
		ArgumentNullException exception = Assert.Throws<ArgumentNullException>(() => new CosmosDbStorage("http://", Guid.NewGuid().ToString(), "hangfire", string.Empty));
		Assert.Equal("containerName", exception.ParamName);
	}

	[Fact]
	public void Ctor_CanCreateCosmosDbStorage_WithExistingConnection()
	{
		CosmosDbStorage storage = new(url, secret, database, container);
		Assert.NotNull(storage);
	}

	[Fact]
	public void GetMonitoringApi_ReturnsNonNullInstance()
	{
		CosmosDbStorage storage = new(url, secret, database, container);
		IMonitoringApi api = storage.GetMonitoringApi();
		Assert.NotNull(api);
	}

	[Fact]
	public void GetConnection_ReturnsNonNullInstance()
	{
		CosmosDbStorage storage = new(url, secret, database, container);
		using CosmosDbConnection connection = (CosmosDbConnection)storage.GetConnection();
		Assert.NotNull(connection);
	}

	[Fact]
	public void WithExistingCosmosClient_GetMonitoringApi_ReturnsNonNullInstance()
	{
		CosmosDbStorage storage = CreateSutWithCosmosClient();
		IMonitoringApi api = storage.GetMonitoringApi();
		Assert.NotNull(api);
	}

	[Fact]
	public void WithExistingCosmosClient_GetConnection_ReturnsNonNullInstance()
	{
		CosmosDbStorage storage = CreateSutWithCosmosClient();
		using CosmosDbConnection connection = (CosmosDbConnection)storage.GetConnection();
		Assert.NotNull(connection);
	}

	[Fact]
	public void GetComponents_ReturnsAllNeededComponents()
	{
		CosmosDbStorage storage = new(url, secret, database, container);
#pragma warning disable CS0618
		IEnumerable<IServerComponent> components = storage.GetComponents();
#pragma warning restore CS0618
		Type[] componentTypes = components.Select(x => x.GetType()).ToArray();
		Assert.Contains(typeof(ExpirationManager), componentTypes);
	}

	private CosmosDbStorage CreateSutWithCosmosClient()
	{
        var cosmosClient = new CosmosClient(url, secret);
		return  new(cosmosClient, database, container);
	}
}