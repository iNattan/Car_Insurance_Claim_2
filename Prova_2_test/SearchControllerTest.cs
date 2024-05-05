using Prova_2.Controller;
using Microsoft.Extensions.Logging;
using Moq;
using Prova_2.Model;
using Prova_2.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;

namespace Prova_2_test;

public class SearchControllerTest
{
    [Fact]
    public void Test1()
    {
        var dataLoadMock = new Mock<IDataLoad>();
        var loggerMock = new Mock<ILogger<SearchController>>();
        var cacheMock = new Mock<IMemoryCache>();

        // List<DriverData> driversDataMoq = [new DriverData{CREDIT_SCORE = "0.54155442", AGE = "16-25", GENDER = "male", DRIVING_EXPERIENCE = "0-9y", EDUCATION = "high school", 
        //                                                   INCOME = "upper class", VEHICLE_YEAR = "after 2015", VEHICLE_TYPE = "sedan", ANNUAL_MILEAGE = "10000.0"}];
        // dataLoadMock.Setup(method => method.Search()).Returns(driversDataMoq);
        // var controllerMock = new SearchController(loggerMock.Object, dataLoadMock.Object, cacheMock);

        // var result = controllerMock.Credit_Score(18, "male", 9, "high school", "upper class", 2020, "sedan", "10000.0");

        // Assert.Single(result);
        // Assert.Contains("0.541554421", result);

        string csvData = "AGE,GENDER,DRIVING_EXPERIENCE,EDUCATION,INCOME,VEHICLE_YEAR,VEHICLE_TYPE,ANNUAL_MILEAGE,CREDIT_SCORE\n" +
                         "16-25,male,0-9y,high school,upper class,after 2015,sedan,10000.0,0.54155442\n";
        IEnumerable<string> csvLines = csvData.Split(Environment.NewLine, StringSplitOptions.RemoveEmptyEntries);
        cacheMock.Setup(x => x.TryGetValue("CsvData", out csvData)).Returns(true);

        var controller = new SearchController(loggerMock.Object, null, cacheMock.Object);

        var result = await controller.Credit_Score(18, "male", 8, "high school", "upper class", 2020, "sedan", "10000.0");

        Assert.Single(result);
        Assert.Equal("0.54155442", result.First());
    }
}