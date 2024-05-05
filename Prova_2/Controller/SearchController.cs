using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;
using CsvHelper;
using System.Globalization;
using Prova_2.Interfaces;
using Prova_2.Model;

namespace Prova_2.Controller;

[ApiController]
[Route("[controller]/[action]")]
public class SearchController : ControllerBase
{
    private readonly IDataLoad _dataLoad;

    private readonly ILogger<SearchController> _logger;
    private readonly IMemoryCache _cache;
    private const string CsvCacheKey = "CsvData";

    public SearchController(ILogger<SearchController> logger, IDataLoad DataLoad, IMemoryCache cache)
    {
        _logger = logger;
        _dataLoad = DataLoad;
        _cache = cache;
    }

    [HttpGet(Name = "GetCsvData")]
    public async Task<IEnumerable<string>> GetCsvData()
    {
        string csvContent;
        if (!_cache.TryGetValue(CsvCacheKey, out csvContent))
        {
            using (var httpClient = new HttpClient())
            {
                try
                {
                    var response = await httpClient.GetAsync("https://insuranceapistorage.blob.core.windows.net/csv/Car_Insurance_Claim.csv?sv=2022-11-02&ss=b&srt=sco&sp=rlitf&se=2024-06-06T04:48:06Z&st=2024-05-05T20:48:06Z&spr=https&sig=W1%2FAoUXGf6XuVAp3J2q7gRNix0hGDEo0y1%2F%2FHdZLV%2BE%3D");
                    if (response.IsSuccessStatusCode)
                    {
                        csvContent = await response.Content.ReadAsStringAsync();
        
                        _cache.Set(CsvCacheKey, csvContent, new MemoryCacheEntryOptions
                        {
                            AbsoluteExpiration = DateTimeOffset.UtcNow.AddMinutes(30)
                        });
                    }
                    else
                    {                    
                        throw new Exception("Falha ao obter o conteúdo do CSV.");
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Erro ao acessar a URL do CSV.");        
                    throw;
                }
            }
        }
        var csvLines = csvContent.Split(Environment.NewLine, StringSplitOptions.RemoveEmptyEntries);
        return csvLines;
    }

    [HttpGet(Name = "Search Credit_Score")]
    public async Task<IEnumerable<string>> Credit_Score([FromHeader] int age, [FromHeader] string gender, [FromHeader] int drivingExperience, 
                                                        [FromHeader] string education, [FromHeader] string income, [FromHeader] int vehicleYear,
                                                        [FromHeader] string vehicleType, [FromHeader] string annualMileage)
    {
        try 
        {    
            IEnumerable<string> csvData = await GetCsvData();    

            List<DriverData> driverData = new List<DriverData>();

            using (var reader = new StringReader(string.Join(Environment.NewLine, csvData)))
            using (var csv = new CsvReader(reader, CultureInfo.InvariantCulture))
            {
                driverData = csv.GetRecords<DriverData>().ToList();
            }

            PeopleData peopleData = new PeopleData(age, gender, drivingExperience, education, income, vehicleYear, vehicleType, annualMileage);
            string ageGroup = peopleData.GetAgeGroup(age);
            string drivingExperienceGroup = peopleData.GetDrivingExperienceGroup(drivingExperience);
            string vehicleYearGroup = peopleData.GetVehicleYearGroup(vehicleYear);
            string annualMileageGroup = peopleData.GetAnnualMileageGroup(annualMileage);

            var dataFiltered = driverData.Where(d => d.AGE == ageGroup && d.GENDER == gender && d.DRIVING_EXPERIENCE == drivingExperienceGroup 
                                                && d.EDUCATION == education && d.INCOME == income && d.VEHICLE_YEAR == vehicleYearGroup 
                                                && d.VEHICLE_TYPE == vehicleType && d.ANNUAL_MILEAGE == annualMileageGroup).ToList();

            if (!dataFiltered.Any())
            {
                return new List<string> { "No person with this data was found." };
            }

            var creditScore = dataFiltered.Select(d => d.CREDIT_SCORE);

            if (!creditScore.Any())
            {
                return new List<string> { "This person does not have a credit score." };
            }

            return creditScore; 

        }
        catch (System.Exception ex) 
        {
            return new List<string> { "Chamou o endpoint mas deu erro: " + ex.Message + ex.StackTrace };
        }
    }

}