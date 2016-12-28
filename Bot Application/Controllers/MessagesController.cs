namespace Bot_Application.Controllers
{
    using System;
    using System.Net;
    using System.Net.Http;
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

                var state = activity.GetStateClient();
                var userData = await state.BotState.GetUserDataAsync(activity.ChannelId, activity.From.Id);
                var x = userData.GetProperty<WeatherParam>("weather");
                //if (x != null) WP = x;

                var rep = await this.Reply(activity, x);
                Activity reply = activity.CreateReply(rep);

                await connector.Conversations.ReplyToActivityAsync(reply);

                if (this.WP != null && this.WP.MeasurementType != Measurement.None)
                {
                    x = this.WP;
                }

                userData.SetProperty<WeatherParam>("weather", x);
                await state.BotState.SetUserDataAsync(activity.ChannelId, activity.From.Id, userData);
            }
            else
            {
                this.HandleSystemMessage(activity);
            }

            var response = this.Request.CreateResponse(HttpStatusCode.OK);
            return response;
        }

        WeatherParam WP = new WeatherParam();

        async Task<string> Reply([FromBody]Activity activity, WeatherParam previousParam)
        {
            string userName = string.Empty;
            if (activity.From != null && !string.IsNullOrEmpty(activity.From.Name))
            {
                userName = $" {activity.From.Name}";
            }

            string msg = activity.Text;

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

            if (a.IsPresent("temperature"))
            {
                WP.MeasurementType = Measurement.Temperature;
            }

            if (a.IsPresent("humidity"))
            {
                WP.MeasurementType = Measurement.Humidity;
            }

            if (a.IsPresent("pressure"))
            {
                WP.MeasurementType = Measurement.Pressure;
            }

            if (a.IsPresent("weather"))
            {
                WP.MeasurementType = Measurement.Weather;
            }

            if (a.IsPresent("today"))
            {
                WP.Today();
            }

            if (a.IsPresent("tomorrow"))
            {
                WP.Tomorrow();
            }

            if (!string.IsNullOrEmpty(a.NextTo("in")))
            {
                WP.Location = a.NextTo("in");
            }

            if (a.IsPresent("emotions"))
            {
                WP.MeasurementType = Measurement.Emotions;
            }

            if (!string.IsNullOrEmpty(a.NextTo("of")))
            {
                WP.Person = a.NextToAllTheRest("of");
            }

            return await WP.BuildResult(userName, previousParam);
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