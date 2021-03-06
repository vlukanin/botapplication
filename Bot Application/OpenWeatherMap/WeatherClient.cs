﻿namespace Bot_Application.OpenWeatherMap
{
    using System;
    using System.Collections.Generic;
    using System.Net.Http;
    using System.Threading.Tasks;

    public class WeatherClient
    {
        public string AppID { get; set; } = string.Empty;

        private HttpClient cli = new HttpClient();
        public WeatherClient(string AppID)
        {
            this.AppID = AppID;
        }

        public async Task<WeatherRecord[]> Forecast(string city)
        {
            var res = await this.cli.GetStringAsync($"http://api.openweathermap.org/data/2.5/forecast/daily?q={city}&mode=json&units=metric&cnt=7&APPID={this.AppID}");
            var f = new List<WeatherRecord>();
            dynamic x = Newtonsoft.Json.JsonConvert.DeserializeObject(res);
            foreach (var z in x.list)
            {
                f.Add(new WeatherRecord()
                {
                    When = this.Convert((long)z.dt),
                    Temp = z.temp.day,
                    Pressure = z.pressure,
                    Humidity = z.humidity,
                });
            }
            return f.ToArray();
        }
        private DateTime Convert(long x)
        {
            System.DateTime dtDateTime = new DateTime(1970, 1, 1, 0, 0, 0, 0, System.DateTimeKind.Utc);
            dtDateTime = dtDateTime.AddSeconds(x).ToLocalTime();
            return dtDateTime;
        }

    }
}