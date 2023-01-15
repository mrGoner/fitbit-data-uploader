using System;
using System.IO;
using System.Text.Json;
using FitbitApi.DataModels.Activity;
using FitbitApi.DataModels.Sleep;
using NUnit.Framework;

namespace FitbitApiTests;

public class FitbitApiResponseDeserializationTests
{
    [Test]
    public void CreateSleepLogResponseDeserializationTest_Success()
    {
        var createSleepLogResponse =
            JsonSerializer.Deserialize<CreateSleepLogResponse>(
                File.ReadAllText("TestData/CreateSleepLogResponse.json"));

        Assert.NotNull(createSleepLogResponse);
        Assert.NotNull(createSleepLogResponse!.Sleep);
        
        Assert.AreEqual(CreateSleepLogResponse.SleepLogType.Classic, createSleepLogResponse.Sleep.LogType);
        Assert.AreEqual(12, createSleepLogResponse.Sleep.MinutesAsleep);
        Assert.AreEqual(0, createSleepLogResponse.Sleep.MinutesAwake);
        Assert.AreEqual(0, createSleepLogResponse.Sleep.MinutesAfterWakeup);
        Assert.AreEqual(25793948582, createSleepLogResponse.Sleep.LogId);
        Assert.AreEqual(false, createSleepLogResponse.Sleep.IsMainSleep);
        Assert.AreEqual(720000, createSleepLogResponse.Sleep.Duration);
        Assert.AreEqual(DateTime.Parse("2020-02-09T22:00:00.000"), createSleepLogResponse.Sleep.StartTime);
        Assert.AreEqual(DateTime.Parse("2020-02-09T22:12:00.000"), createSleepLogResponse.Sleep.EndTime);
    }

    [Test]
    public void CreateActivityLogDeserializationTest_Success()
    {
        var createActivityLogResponse =
            JsonSerializer.Deserialize<CreateActivityLogResponse>(
                File.ReadAllText("TestData/CreateActivityLogResponse.json"));
        
        Assert.NotNull(createActivityLogResponse);
        Assert.NotNull(createActivityLogResponse!.Activity);
        
        Assert.AreEqual("Bike", createActivityLogResponse.Activity.Name);
        Assert.AreEqual(1800000, createActivityLogResponse.Activity.Duration);
        Assert.AreEqual(1010, createActivityLogResponse.Activity.ActivityId);
        Assert.AreEqual(2, createActivityLogResponse.Activity.Distance);
        Assert.AreEqual(20510012273, createActivityLogResponse.Activity.LogId);
    }

    [Test]
    public void GetAllActivityTypesDeserializationTest_Success()
    {
        var getAllActivitiesResponse =
            JsonSerializer.Deserialize<GetAllActivitiesResponse>(
                File.ReadAllText("TestData/GetAllActivitiesResponse.json"));
        
        Assert.NotNull(getAllActivitiesResponse);
        Assert.NotNull(getAllActivitiesResponse!.Categories);
        Assert.AreEqual(1, getAllActivitiesResponse.Categories.Length);

        var firstCategory = getAllActivitiesResponse.Categories[0];
        
        Assert.AreEqual("Dancing", firstCategory.Name);
        Assert.AreEqual(0, firstCategory.Id);
        Assert.AreEqual(2, firstCategory.Activities.Length);

        var secondActivityInCategory = firstCategory.Activities[1];
        
        Assert.AreEqual("Aerobic, general", secondActivityInCategory.Name);
        Assert.AreEqual(3015, secondActivityInCategory.Id);
    }
}