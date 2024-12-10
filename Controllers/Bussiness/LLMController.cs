using Microsoft.AspNetCore.Mvc;
using PototoTrade.Controllers.CustomController;
using PototoTrade.DTO.LLM;
using PototoTrade.ServiceBusiness.LLM;

namespace PototoTrade.Controllers.Bussiness
{
    [Route("api/llm")]
    [ApiController]
    public class LLMController : CustomBaseController
    {
        public LLMService _LLMService;

        public LLMController(LLMService lLMService)
        {
            _LLMService = lLMService;
        }
        
        [HttpPost("chat")]
        public async Task<IActionResult> GetText(LLMDTO lLMDTO)
        {
            return MakeResponse(await _LLMService.SendLLamaRequestAsync(lLMDTO));
        }
    }
}
