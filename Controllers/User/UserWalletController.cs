using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PototoTrade.Controllers.CustomController;
using PototoTrade.DTO.Wallet;
using PototoTrade.Service.Wallet;
using System;
using System.Threading.Tasks;

namespace PototoTrade.Controllers.User
{
    [Route("api/wallet")]
    [ApiController]
    public class UserWalletController : CustomBaseController
    {
        private readonly UserWalletService _userWalletService;

        public UserWalletController(UserWalletService userWalletService)
        {
            _userWalletService = userWalletService;
        }

        // 1. Get User Wallet Balance
        [HttpGet("balance")]
        [Authorize(Roles = "User")]
        public async Task<IActionResult> GetWalletBalance()
        {
            return MakeResponse(await _userWalletService.GetWalletBalanceAsync(User));
        }

        // 2. Create Stripe Checkout Session for Wallet Top-Up using DTO
        [HttpPost("top-up-session")]
        public async Task<IActionResult> CreateTopUpSession([FromBody] TopUpRequestDTO topUpRequest)
        {
            return MakeResponse(await _userWalletService.CreateTopUpSessionAsync(
                    User, topUpRequest.Amount, topUpRequest.Currency));
        }

        // 3. Update Wallet Balance After Payment Success using DTO
        // [HttpPost("top-up")]
        // [Authorize(Roles = "User")]

        // public async Task<IActionResult> TopUpWallet([FromBody] TopUpRequestDTO topUpRequest)
        // {
        //     return MakeResponse(await _userWalletService.TopUpWalletAsync(User, topUpRequest.Amount));
        // }

    }
}

//TODO: do frontend for the purchase confirmation (add 4. to the flow of checkout) and then fetch wallet data to the frontend.