using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PototoTrade.Controllers.CustomerController;
using PototoTrade.DTO.Wallet;
using PototoTrade.Service.Wallet;
using System;
using System.Threading.Tasks;

namespace PototoTrade.Controllers.User
{
    [Route("api/wallet")]
    [ApiController]
    public class UserWalletController : CustomerBaseController
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



        [HttpPost("request-refund")] //executed by users
        [Authorize(Roles = "User")] 
        public async Task <IActionResult> RequestRefund ([FromBody] RefundRequestDTO refundRequest){
            return MakeResponse(await _userWalletService.RequestRefund(User, refundRequest.BuyerId, refundRequest.Amount));
        }



        [HttpPost("allow-refund")] //executed by users //flow: pass refundrequestDTO, from id, get buyer and seller wallets, perform refund process, save
        [Authorize(Roles = "User")]
        public async Task <IActionResult> AllowRefund ([FromBody] RefundRequestDTO refundRequest){

             return MakeResponse(await _userWalletService.AllowRefund(refundRequest.RefundRequestId));
        }

        [HttpPost("reject-refund")] //executed by admin
        public async Task <IActionResult> RejectRefund ([FromBody] RefundRequestDTO refundRequest){

            return MakeResponse(await _userWalletService.RejectRefund(refundRequest.RefundRequestId));
        }
    }
}

//TODO: do frontend for the purchase confirmation (add 4. to the flow of checkout) and then fetch wallet data to the frontend.