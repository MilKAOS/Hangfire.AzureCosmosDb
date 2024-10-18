using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics.CodeAnalysis;
using Hangfire.Azure.Tests.Fixtures;
using Hangfire.Common;
using Hangfire.Storage;
using Xunit;
using Xunit.Abstractions;

namespace Hangfire.Azure.Tests;

public class CosmosDbStorageFeatureFacts : IClassFixture<ContainerFixture>
{
    private ContainerFixture ContainerFixture { get; }

    private CosmosDbStorage Storage { get; }

    public CosmosDbStorageFeatureFacts(ContainerFixture containerFixture, ITestOutputHelper testOutputHelper)
    {
        ContainerFixture = containerFixture;
        Storage = containerFixture.Storage;

        ContainerFixture.SetupLogger(testOutputHelper);
    }    

    [Fact]
    public void AddOrUpdate_ThrowsAnException_WhenJobQueueIsSet_WhenStorageFeaturesDoesNotSupportIt()
    {
        // Arrange
        Storage.Features = new ReadOnlyDictionary<string, bool>(new Dictionary<string, bool>
        {
            { JobStorageFeatures.JobQueueProperty, false },
        });

        Job job = Job.FromExpression(() => Method(), "some-queue");
        RecurringJobManager manager = new (Storage);

        // Act & Assert
        Assert.Throws<NotSupportedException>(() => manager.AddOrUpdate("recurring-job-id", job, Cron.Minutely()));
    }

    [Fact]
    public void AddOrUpdate_NoException_WhenJobQueueIsSet_WhenStorageFeaturesDoesSupportIt()
    {
        // Arrange
        Storage.Features = new ReadOnlyDictionary<string, bool>(new Dictionary<string, bool>
        {
            { JobStorageFeatures.JobQueueProperty, true },
        });

        Job job = Job.FromExpression(() => Method(), "some-queue");
        RecurringJobManager manager = new (Storage);

        // Act & Assert
        try
        {
            manager.AddOrUpdate("recurring-job-id", job, Cron.Minutely());
        }
        catch
        {
            Assert.Fail("Expected no exception for feature JobStorageFeatures.JobQueueProperty set to true.");
        }
    }

    [SuppressMessage("Usage", "xUnit1013:Public method should be marked as test")]
    [SuppressMessage("ReSharper", "MemberCanBePrivate.Global")]
    public static void Method() { }
}
