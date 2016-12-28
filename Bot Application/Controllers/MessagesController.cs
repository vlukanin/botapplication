namespace Bot_Application.Controllers
{
    using System;
    using System.Net;
    using System.Net.Http;
    using System.Text;
    using System.Threading.Tasks;
    using System.Web.Http;
    using Microsoft.Bot.Connector;
    using OpenWeatherMap;

    [BotAuthentication]
    public class MessagesController : ApiController
    {
        /// <summary>
        /// POST: api/Messages
        /// Receive a message from a user and reply to it
        /// </summary>
        public async Task<HttpResponseMessage> Post([FromBody]Activity activity)
        {
            if (activity.Type == ActivityTypes.Message)
            {
                ConnectorClient connector = new ConnectorClient(new Uri(activity.ServiceUrl));
                var rep = await this.Reply(activity);
                Activity reply = activity.CreateReply(rep);
                await connector.Conversations.ReplyToActivityAsync(reply);
            }
            else
            {
                this.HandleSystemMessage(activity);
            }

            var response = this.Request.CreateResponse(HttpStatusCode.OK);
            return response;
        }

        enum Measurement { Temp = 1, Humidity = 2, Pressure = 4, Weather = 8, None = 0 };

        WeatherClient OWM = new WeatherClient(Config.OpenWeatherMapAPIKey);

        async Task<string> Reply([FromBody]Activity activity)
        {
            string userName = string.Empty;
            if (activity.From != null && !string.IsNullOrEmpty(activity.From.Name))
            {
                userName = $" {activity.From.Name}";
            }

            string msg = activity.Text;

            string city = "Minsk";
            int when = 0;
            string whens = "today";
            Measurement mes = Measurement.None;
            string[] a = msg.ToLower().Split(' ');

            if (a.IsPresent("help"))
            {
                return "Hey" + userName + @"! This is a simple weather bot.<br/>
Examples of commands:<br/>
  temperature today<br/>
  temperature in Minsk<br/>
  humidity tomorrow<br/>
  pressure today<br/>
  weather tomorrow in London";
            }
            if (a.IsPresent("temperature")) mes |= Measurement.Temp;
            if (a.IsPresent("humidity")) mes |= Measurement.Humidity;
            if (a.IsPresent("pressure")) mes |= Measurement.Pressure;
            if (a.IsPresent("weather")) mes |= Measurement.Weather;
            if (a.IsPresent("today")) { when = 0; whens = "today"; }
            if (a.IsPresent("tomorrow")) { when = 1; whens = "tomorrow"; }
            if (a.NextTo("in") != "") city = a.NextTo("in");

            var res = await OWM.Forecast(city);
            var r = res[when];

            StringBuilder sb = new StringBuilder();

            sb.Append($"Hello{userName}!<br/>");

            bool understand = false;
            if ((mes & Measurement.Temp) > 0)
            {
                sb.Append($"The temperature on {r.Date} in {city} is {r.Temp} °C");
                understand = true;
            }

            if ((mes & Measurement.Pressure) > 0)
            {
                sb.Append($"The pressure on {r.Date} in {city} is {r.Pressure} hpa");
                understand = true;
            }

            if ((mes & Measurement.Humidity) > 0)
            {
                sb.Append($"Humidity on {r.Date} in {city} is {r.Humidity} %");
                understand = true;
            }

            if ((mes & Measurement.Weather) > 0)
            {
                sb.Append($"The temperature on {r.Date} in {city} is {r.Temp} °C.<br/>");
                sb.Append($"The pressure on {r.Date} in {city} is {r.Pressure} hpa.<br/>");
                sb.Append($"Humidity on {r.Date} in {city} is {r.Humidity} %.");
                understand = true;
            }

            if (!understand)
            {
                sb.Append("I do not understand you.<br/>Please write 'help' for details");
            }

            return sb.ToString();
        }

        private Activity HandleSystemMessage(Activity message)
        {
            if (message.Type == ActivityTypes.DeleteUserData)
            {
                // Implement user deletion here
                // If we handle user deletion, return a real message
            }
            else if (message.Type == ActivityTypes.ConversationUpdate)
            {
                // Handle conversation state changes, like members being added and removed
                // Use Activity.MembersAdded and Activity.MembersRemoved and Activity.Action for info
                // Not available in all channels
            }
            else if (message.Type == ActivityTypes.ContactRelationUpdate)
            {
                // Handle add/remove from contact lists
                // Activity.From + Activity.Action represent what happened
            }
            else if (message.Type == ActivityTypes.Typing)
            {
                // Handle knowing tha the user is typing
            }
            else if (message.Type == ActivityTypes.Ping)
            {
            }

            return null;
        }
    }
}