using Microsoft.AspNetCore.Mvc;
using PototoTrade.DTO.Wallet;
using PototoTrade.Service.Wallet;
using System;
using System.Threading.Tasks;

namespace PototoTrade.Controllers.User
{
    [Route("api/[controller]")]
    [ApiController]
    public class UserWalletController : ControllerBase
    {
        private readonly UserWalletService _userWalletService;

        public UserWalletController(UserWalletService userWalletService)
        {
            _userWalletService = userWalletService;
        }

        // 1. Get User Wallet Balance
        [HttpGet("{userId}/balance")]
        public async Task<IActionResult> GetWalletBalance([FromRoute] int userId)
        {
            var walletBalance = await _userWalletService.GetWalletBalanceAsync(userId);
            if (walletBalance != null)
            {
                return Ok(walletBalance);
            }
            return NotFound("Wallet not found.");
        }

        // 2. Create Stripe Checkout Session for Wallet Top-Up using DTO
        [HttpPost("top-up-session")]
        public async Task<IActionResult> CreateTopUpSession([FromBody] TopUpRequestDTO topUpRequest)
        {
            try
            {
                var sessionUrl = await _userWalletService.CreateTopUpSessionAsync(
                    topUpRequest.UserId, topUpRequest.Amount, topUpRequest.Currency);
                return Ok(new { Url = sessionUrl });
            }
            catch (Exception ex)
            {
                return BadRequest(new { Error = ex.Message });
            }
        }

        // 3. Update Wallet Balance After Payment Success using DTO
        [HttpPost("top-up")]
        public async Task<IActionResult> TopUpWallet([FromBody] TopUpRequestDTO topUpRequest)
        {
            var success = await _userWalletService.TopUpWalletAsync(topUpRequest.UserId, topUpRequest.Amount);
            if (success)
            {
                return Ok("Wallet topped up successfully.");
            }
            return BadRequest("Failed to top up wallet.");
        }

        [HttpPost("{buyerId}/request-refund")] //executed by users
        public async Task <IActionResult> RequestRefund ([FromRoute] int buyerId, [FromBody] RefundDTO refundRequest){
            var success = await _userWalletService.RequestRefund(buyerId, refundRequest.SellerId, refundRequest.Amount);

            if (success){
                return Ok("Refund successfully requested.");
            }
            return BadRequest("Failed to request for refund.");
        }

        [HttpPost("allow-refund")] //executed by admin
        public async Task <IActionResult> AllowRefund ([FromBody] RefundDTO refundRequest){
            var success = await _userWalletService.AllowRefund(refundRequest.BuyerId, refundRequest.SellerId, refundRequest.Amount);

            if (success){
                return Ok("Refund successfully allowed.");
            }
            return BadRequest("Failed to request for refund.");
        }

        [HttpPost("reject-refund")] //executed by admin
        public async Task <IActionResult> RejectRefund ([FromBody] RefundDTO refundRequest){
            var success = await _userWalletService.RejectRefund(refundRequest.BuyerId, refundRequest.SellerId, refundRequest.Amount);

            if (success){
                return Ok("Refund successfully rejected.");
            }
            return BadRequest("Failed to request for refund.");
        }
    }
}
