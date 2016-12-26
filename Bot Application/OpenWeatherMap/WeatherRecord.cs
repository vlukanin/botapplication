namespace Bot_Application.OpenWeatherMap
{
    using System;

    public class WeatherRecord
    {
        public double Temp { get; set; }
        public double Pressure { get; set; }
        public int Humidity { get; set; }
        public DateTime When { get; set; }
        public string Date
        {
            get { return $"{this.When.Day:D2}.{this.When.Month:D2}"; }
        }

        public string FullDate
        {
            get { return this.When.ToString("D"); }
        }

    }
}