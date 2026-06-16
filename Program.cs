using System;
using System.Text;
using System.Collections.Generic;
using System.Net.Http;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using static Program;


public static class LocaleUnitsExtensions
{
    public static string GetSymbol(this LocaleUnits unit) => unit switch
    {
        LocaleUnits.Date => "iso8601",
        LocaleUnits.Fahrenheit => "°F",
        _ => string.Empty
    };
}

public class Program
{
    static async Task Main()
    {
        using HttpClient thermometer = new();

        /*
         * Open-Meteo.com API endpoint: 
         * assemble in Web browser address bar (without quotes) to test the request and view the JSON response before implementing in C# code
         */
        string url = "https://api.open-meteo.com/v1/forecast" +
            "?latitude=38.256186" +
            "&longitude=-85.744653" +
            "&daily=sunrise,sunset,temperature_2m_mean" +
            "&timezone=America%2FNew_York" + 
            "&forecast_days=1" +
            "&temperature_unit=fahrenheit";

        Console.OutputEncoding = Encoding.UTF8;
        Console.WriteLine("Requesting locale data\u2026");

        await using var responseStream = await thermometer.GetStreamAsync(url);

        var options = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        };
        options.Converters.Add(new JsonStringEnumConverter());

        // Parse the JSON string into c# object

        LocaleFile? somePlace = await JsonSerializer.DeserializeAsync<LocaleFile>(responseStream, options);

        if (somePlace is null)
        {
            Console.WriteLine("No data was returned.");
            return;
        }

        Console.WriteLine("\nThese are today's conditions at Louisville Slugger Field:");
        PrintMetadata(somePlace);
        PrintDailyValues(somePlace);
    }

    // Method to print the metadata properties of the LocaleFile object, as returned in the JSON response
    static void PrintMetadata(LocaleFile data)
    {
        Console.WriteLine($"The Latitude is {data.Latitude}");
        Console.WriteLine($"The Longitude is {data.Longitude}");
        Console.WriteLine($"The Generation time is {data.GenerationTimeMs} ms");
        Console.WriteLine($"The Timezone is {data.Timezone}");
        Console.WriteLine($"The Timezone abbreviation is {data.TimezoneAbbreviation}");
        Console.WriteLine($"The Elevation is {data.Elevation} meters");
    }

    // Method to print the daily values for the requested variables, as returned in the JSON response
    static void PrintDailyValues(LocaleFile data)
    {
        string dateUnit = data.DailyUnits.GetValueOrDefault("time", LocaleUnits.Date).GetSymbol();
        string sunriseUnit = data.DailyUnits.GetValueOrDefault("sunrise", LocaleUnits.DateTime).GetSymbol();
        string sunsetUnit = data.DailyUnits.GetValueOrDefault("sunset", LocaleUnits.DateTime).GetSymbol();
        string temperatureUnit = data.DailyUnits.GetValueOrDefault("temperature_2m_mean", LocaleUnits.Fahrenheit).GetSymbol();

        // TimeOnly struct to extract from ISO 8601 round-trip format for the sunrise/sunset times, in the local timezone
        TimeOnly sunriseTime = TimeOnly.Parse(data.Daily.Sunrise[0]);
        TimeOnly sunsetTime = TimeOnly.Parse(data.Daily.Sunset[0]);

        for (int i = 0; i < data.Daily.Time.Count; i++)
        {
            Console.WriteLine($"Date: {data.Daily.Time[i]} {dateUnit}");
            Console.WriteLine($"Sunrise: {sunriseTime} {sunriseUnit}");
            Console.WriteLine($"Sunset: {sunsetTime} {sunsetUnit}");
            Console.WriteLine($"Temperature (2m mean): {data.Daily.Temperature2mMean[i]} {temperatureUnit}");
        }
    }

    /* 
     * Taken from the Open-Meteo.com API documentation 
     * (https://open-meteo.com/en/docs?current=temperature_2m&timezone=America%2FNew_York&latitude=38.256186&longitude=-85.744653&hourly=), 
     * the JSON response is expected to have the following structure:
     */
    public class LocaleFile
    {
        [JsonPropertyName("latitude")]
        public float Latitude { get; set; }

        [JsonPropertyName("longitude")]
        public float Longitude { get; set; }

        [JsonPropertyName("generationtime_ms")]
        public float GenerationTimeMs { get; set; }

        [JsonPropertyName("utc_offset_seconds")]
        public int UtcOffsetSeconds { get; set; }

        [JsonPropertyName("timezone")]
        public required string Timezone { get; set; }

        [JsonPropertyName("timezone_abbreviation")]
        public required string TimezoneAbbreviation { get; set; }

        [JsonPropertyName("elevation")]
        public float Elevation { get; set; }

        [JsonPropertyName("daily_units")]
        public Dictionary<string, LocaleUnits> DailyUnits { get; set; } = new();

        [JsonPropertyName("daily")]
        public LocaleDaily Daily { get; set; } = new();
    }

    // Units of measurement for the daily data variables, as returned in the "daily_units" property of the JSON response
    public enum LocaleUnits
    {
        [JsonStringEnumMemberName("iso8601")]
        Date,

        [JsonStringEnumMemberName("iso8601")]
        DateTime,

        [JsonStringEnumMemberName("°F")]
        Fahrenheit
    }

    // Lists of daily values for the requested variables, as returned in the "daily" property of the JSON response
    public class LocaleDaily
    {
        // YYYY-MM-DD format for the time (date)
        [JsonPropertyName("time")]
        public List<string> Time { get; set; } = new();

        // YYYY-MM-DDThh:mm format for sunrise and sunset times, in the local timezone
        [JsonPropertyName("sunrise")]
        public List<string> Sunrise { get; set; } = new();

        [JsonPropertyName("sunset")]
        public List<string> Sunset { get; set; } = new();

        [JsonPropertyName("temperature_2m_mean")]
        public List<float> Temperature2mMean { get; set; } = new();
    }
}