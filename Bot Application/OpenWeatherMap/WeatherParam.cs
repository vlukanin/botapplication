namespace Bot_Application.OpenWeatherMap
{
    using System;
    using System.Text;
    using System.Threading.Tasks;

    public enum Measurement { Temperature = 1, Humidity = 2, Pressure = 4, Weather = 8, None = 0 }

    public class WeatherParam
    {
        public DateTime When { get; set; }

        public string Date
        {
            get { return $"{this.When.Day:D2}.{this.When.Month:D2}.{this.When.Year:D4}"; }
        }

        public string Location { get; set; }
        public Measurement MeasurementType { get; set; }
        public WeatherParam()
        {
            Location = "Minsk";
            When = DateTime.Now;
            //MeasurementType = Measurement.Weather;
        }
        public void Today()
        {
            When = DateTime.Now;
        }
        public void Tomorrow()
        {
            When = DateTime.Now.AddDays(1);
        }

        public void AlsoMeasure(Measurement M)
        {
            MeasurementType |= M;
        }

        public bool Measure(Measurement M)
        {
            return (M & MeasurementType) > 0;
        }

        public int Offset
        {
            get
            {
                return (int)(((float)(When - DateTime.Now).Hours) / 24.0 + 0.5);
            }
        }

        public async Task<string> BuildResult(string userName, WeatherParam previousParam)
        {
            WeatherClient OWM = new WeatherClient(Config.OpenWeatherMapAPIKey);
            var res = await OWM.Forecast(Location);
            var r = res[Offset];

            StringBuilder sb = new StringBuilder();

            sb.Append($"Hello{userName}!<br/>");

            bool understand = false;

            if (Measure(Measurement.Temperature))
            {
                sb.Append($"The temperature on {r.Date} in {Location.ToUpper()} is {r.Temp} °C");
                understand = true;
            }

            if (Measure(Measurement.Pressure))
            {
                sb.Append($"The pressure on {r.Date} in {Location.ToUpper()} is {r.Pressure} hpa");
                understand = true;
            }

            if (Measure(Measurement.Humidity))
            {
                sb.Append($"Humidity on {r.Date} in {Location.ToUpper()} is {r.Humidity} %");
                understand = true;
            }

            if (Measure(Measurement.Weather))
            {
                sb.Append($"The temperature on {r.Date} in {Location.ToUpper()} is {r.Temp} °C.<br/>");
                sb.Append($"The pressure on {r.Date} in {Location.ToUpper()} is {r.Pressure} hpa.<br/>");
                sb.Append($"Humidity on {r.Date} in {Location.ToUpper()} is {r.Humidity} %.");
                understand = true;
            }

            if (!understand)
            {
                sb.Append("I do not understand you.<br/>Please write 'help' for details");
                if (previousParam != null && previousParam.MeasurementType != Measurement.None)
                {
                    sb.Append($"<br/>(Last time you asked about {previousParam.MeasurementType.ToString().ToLower()} in {previousParam.Location.ToUpper()} for {previousParam.Date})");
                }
            }

            return sb.ToString();
        }
    }
}