namespace Bot_Application.OpenWeatherMap
{
    using System;
    using System.Collections.Generic;
    using System.Net.Http;
    using System.Text;
    using System.Threading.Tasks;

    using Microsoft.ProjectOxford.Emotion;

    public enum Measurement { Temperature = 1, Humidity = 2, Pressure = 4, Weather = 8, Emotions = 16, None = 0 }

    public class WeatherParam
    {
        public DateTime When { get; set; }

        public string Date
        {
            get { return $"{this.When.Day:D2}.{this.When.Month:D2}.{this.When.Year:D4}"; }
        }

        public string Location { get; set; }
        public string Person { get; set; }
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

            if (Measure(Measurement.Emotions))
            {
                var searchRes = await Utils.Search(Person, 5);
                var cli = new EmotionServiceClient("e48ef61ea8514ecbb45a874c188c0917");
                float happyCount = 0, angerCount = 0, surprisedCount = 0;
                string happyImage = string.Empty, angerImage = string.Empty, surprisedImage = string.Empty;
                foreach (var imageUrl in searchRes)
                {
                    var recognized = await cli.RecognizeAsync(imageUrl);
                    if (recognized != null && recognized.Length > 0)
                    {
                        var f = recognized[0];
                        if (f.Scores.Happiness > happyCount)
                        {
                            happyCount = f.Scores.Happiness;
                            happyImage = imageUrl;
                        }
                        if (f.Scores.Anger > angerCount)
                        {
                            angerCount = f.Scores.Anger;
                            angerImage = imageUrl;
                        }
                        if (f.Scores.Surprise > surprisedCount)
                        {
                            surprisedCount = f.Scores.Surprise;
                            surprisedImage = imageUrl;
                        }
                        await Task.Delay(1000);
                    }
                }

                var person = Person.Replace("+", " ");

                sb.Append($"Happy image of {person} (happiness {happyCount * 100}%): {happyImage}<br/>");
                sb.Append($"Angry image of {person} (anger {angerCount * 100}%): {angerImage}<br/>");
                sb.Append($"Surprised image of {person} (surprise {surprisedCount * 100}%): {surprisedImage}<br/>");

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
            else if (!string.IsNullOrEmpty(this.Location) && !Measure(Measurement.Emotions))
            {
                var imageSearchResults = await Utils.Search(this.Location, 1);
                if (imageSearchResults.Count > 0)
                {
                    sb.Append("<br/>");
                    sb.Append(imageSearchResults[0]);
                }
            }

            return sb.ToString();
        }
    }
}