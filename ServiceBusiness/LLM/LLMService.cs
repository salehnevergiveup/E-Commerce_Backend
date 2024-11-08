// MLLService.cs
using Newtonsoft.Json;
using PototoTrade.DTO.LLM;
using PototoTrade.Integrations;
using PototoTrade.Service.Utilities.Response;

namespace PototoTrade.ServiceBusiness.LLM
{
    public class LLMService
    {
        private readonly LlamaIntegration _llamaIntegration;
        private readonly HttpClient _httpClient;

        public LLMService(LlamaIntegration llamaIntegration)
        {
            _llamaIntegration = llamaIntegration;
            _httpClient = new HttpClient();
        }

        public async Task<ResponseModel<string>> SendLLamaRequestAsync(LLMDTO lLMDTO)
        {

            var response = new ResponseModel<string>
            {
                Message = "Service done",
                Data = "",
                Success = false
            };

            try
            {
                var endpoint = _llamaIntegration.GetBaseUrl() + "/chat/completions";

                var request = new HttpRequestMessage(HttpMethod.Post, endpoint);

                _llamaIntegration.SetUpHeaders(request);

                request.Content = _llamaIntegration.SetUpRequestBody(lLMDTO.message);

                var result = await _httpClient.SendAsync(request);

                var responseContent = await result.Content.ReadAsStringAsync();

                if (!result.IsSuccessStatusCode)
                {
                    response.Message = $"Request failed with status code {result.StatusCode}";
                    response.Data = responseContent;
                    response.Success = false;
                    return response;
                }

                var llamaResponse = JsonConvert.DeserializeObject<LlamaResponse>(responseContent);

                if (llamaResponse?.Choices == null || llamaResponse.Choices.Length == 0)
                {
                    response.Message = "No choices returned in the response.";
                    response.Data = responseContent;
                    response.Success = false;
                    return response;
                }

                response.Message = "Success";
                response.Data = llamaResponse.Choices[0].Message.Content;
                response.Success = true;
                return response;
            }
            catch (Exception ex)
            {
                response.Message = $"Error sending Llama request: {ex.Message}";
                response.Data = "";
                response.Success = false;
                return response;
            }
        }

        public class LlamaResponse
        {
            [JsonProperty("choices")]
            public Choice[] Choices { get; set; }
        }

        public class Choice
        {
            [JsonProperty("message")]
            public Message Message { get; set; }
        }

        public class Message
        {
            [JsonProperty("content")]
            public string Content { get; set; }
        }
    }
}
