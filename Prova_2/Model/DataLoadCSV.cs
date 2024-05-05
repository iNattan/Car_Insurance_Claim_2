using CsvHelper;
using Prova_2.Interfaces;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;

namespace Prova_2.Model;

public class DataLoadCSV : IDataLoad 
{
    public List<DriverData> Search()
    {
        return Load<DriverData>(".\\Prova_2\\Files\\Car_Insurance_Claim");
    }
    public List<T> Load<T>(string local)
    {
        local = local + ".csv";
        if (!File.Exists(local))
            throw new ArgumentException(local);
        
        using (var reader = new StreamReader(local))
        using (var csv = new CsvReader(reader, CultureInfo.InvariantCulture))
        {
            return csv.GetRecords<T>().ToList();
        }
    }
}