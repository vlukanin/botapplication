namespace Bot_Application
{
    using System.Collections.Generic;
    using System.Net.Http;
    using System.Text;
    using System.Threading.Tasks;

    public static class Utils
    {
        public static string NextTo(this string[] str, string pat)
        {
            for (int i = 0; i < str.Length - 1; i++)
            {
                if (str[i].ToLower() == pat.ToLower()) return str[i + 1];
            }
            return string.Empty;
        }

        public static string NextToAllTheRest(this string[] str, string pat)
        {
            for (int i = 0; i < str.Length - 1; i++)
            {
                if (str[i].ToLower() == pat.ToLower())
                {
                    var sb = new StringBuilder();
                    for (int j = i + 1; j < str.Length; j++)
                    {
                        sb.Append(str[j] + "+");
                    }

                    return sb.ToString();
                }
                    
            }
            return string.Empty;
        }

        public static bool IsPresent(this string[] str, string pat)
        {
            for (int i = 0; i < str.Length; i++)
            {
                if (str[i].ToLower() == pat.ToLower()) return true;
            }
            return false;
        }

        public static async Task<List<string>> Search(string query, int count)
        {
            var client = new HttpClient();
            //client.DefaultRequestHeaders.Add("Ocp-Apim-Subscription-Key", "291131ee5c274fb78a00797c16af69bc");
            // Across all Bing Search APIs (Web, Image, Video, News): 1,000 transactions per month, 5 per second. Trial keys expire after a 90 day period, after which a subscription may be purchased on the Azure portal.
            client.DefaultRequestHeaders.Add("Ocp-Apim-Subscription-Key", "dd3ca2ab17de4174b6e558e845da901a");
            var uri = $"https://api.cognitive.microsoft.com/bing/v5.0/images/search?count={count}&q={query}";
            var response = await client.GetStringAsync(uri);
            dynamic x = Newtonsoft.Json.JsonConvert.DeserializeObject(response);

            var list = new List<string>();
            foreach (var z in x.value)
            {
                var url = z.contentUrl.ToString();
                list.Add(url);
            }

            return list;
        }
    }
}