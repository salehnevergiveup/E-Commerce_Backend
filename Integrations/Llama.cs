// LlamaIntegration.cs

using System.Net.Http;
using System.Text;
using Newtonsoft.Json;

namespace PototoTrade.Integrations
{
    public class LlamaIntegration
    {
        private readonly string _apiKey;
        private readonly string _baseUrl;

        public LlamaIntegration(IConfiguration configuration)
        {
            _apiKey = configuration["LLamaApiSettings:ApiKey"];
            _baseUrl = configuration["LLamaApiSettings:BaseUrl"];

            if (string.IsNullOrEmpty(_apiKey) || string.IsNullOrEmpty(_baseUrl))
            {
                throw new ArgumentNullException("API Key and Base URL must be provided in the configuration.");
            }
        }

        // Method to set up request headers
        public void SetUpHeaders(HttpRequestMessage request)
        {
            request.Headers.Add("Authorization", $"Bearer {_apiKey}");
            request.Headers.Add("Accept", "application/json");
        }

        // Method to set up request body
        public StringContent SetUpRequestBody(string query)
        {
            var badWords = "\n If the message contains one of the following words: [damn, hell, crap, bastard, bitch, shit, fuck, ass, asshole, slut, whore, piss, dick, cock, cunt, prick, motherfucker, bastard, jackass, balls, wanker, douche, bloody, tosser, bugger, wimp, twat, skank, tramp, hag, fag, queer, cuntbag, bullshitter, bitchy, dumbass, nincompoop, idiot, moron, retard, scumbag, pisshead, turd, shithead, fucker, cocksucker, dipshit, scumbucket, dumbfuck, piss-off, buttfucker], please respond with: { \"message\": \"The details are not appropriate.\" }. If no inappropriate content is found, proceed with generating the response as usual.";
            var instractions = "please dont add any content that was not explicitly requested and avoid disclaimers repsponed to an explicitly based on the instractions pervided.\n";
            var requestBody = new
            {
                model = "llama3-8b-8192",
                messages = new[]
                {
                    new
                    {
                        role = "user",
                        content = instractions+query+badWords,  
                    }
                },
                temperature = 1,
                max_tokens = 1024,
                top_p = 1,
                stream = false,
                stop = (object)null
            };

            var jsonContent = JsonConvert.SerializeObject(requestBody);
            return new StringContent(jsonContent, Encoding.UTF8, "application/json");
        }

        // Method to get the base URL
        public string GetBaseUrl()
        {
            return _baseUrl;
        }
    }
}
