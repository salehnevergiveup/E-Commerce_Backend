using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PototoTrade.Controllers.CustomerController;
using PototoTrade.DTO.Common;
using PototoTrade.DTO.Product;
using PototoTrade.Service.BuyerItem;
using PototoTrade.Service.Product;

namespace PototoTrade.Controllers.BuyerItem
{
    [Route("api/buyer-item")]
    [ApiController]
    [Authorize(Roles = "User,Admin, SuperAdmin")]
    public class BuyerItemController : CustomerBaseController
    {

        private readonly BuyerItemService _buyerItemService;

        public BuyerItemController(BuyerItemService buyerItemService)
        {
            _buyerItemService = buyerItemService;
        }

        [HttpGet("to-receive")]
        public async Task<IActionResult> ViewToReceiveItems()
        {
            return MakeResponse(await _buyerItemService.ViewItemsByStatus<BuyerItemDetailsDTO>("done payment", User));
        }

        [HttpGet("received")]
        public async Task<IActionResult> ViewReceivedItems()
        {
            return MakeResponse(await _buyerItemService.ViewItemsByStatus<BuyerItemDetailsDTO>("received", User));
        }

        [HttpGet("refund")]
        public async Task<IActionResult> ViewRefundItems()
        {
            return MakeResponse(await _buyerItemService.ViewRefundItems<BuyerItemDetailsDTO>(User));
        }

        [HttpPost("create-stage/arrived-sorting-facility")]
        public async Task<IActionResult> CreateSortingFacilityStage([FromQuery] int buyerItemId)
        {
            return MakeResponse(await _buyerItemService.CreateDeliveryStage<GeneralMessageDTO>(buyerItemId, "arrived in sorting facility"));
        }

        [HttpPost("create-stage/arrived-delivery-hub")]
        public async Task<IActionResult> CreateDeliveryHubStage([FromQuery] int buyerItemId)
        {
            return MakeResponse(await _buyerItemService.CreateDeliveryStage<GeneralMessageDTO>(buyerItemId, "arrived in sorting delivery hub"));
        }

        [HttpPost("create-stage/out-for-delivery")]
        public async Task<IActionResult> CreateOutForDeliveryStage([FromQuery] int buyerItemId)
        {
            return MakeResponse(await _buyerItemService.CreateDeliveryStage<GeneralMessageDTO>(buyerItemId, "out of delivery"));
        }

        [HttpPost("create-stage/item-delivered")]
        public async Task<IActionResult> CreateDeliveredStage([FromQuery] int buyerItemId)
        {
            return MakeResponse(await _buyerItemService.CreateDeliveredStage<GeneralMessageDTO>(buyerItemId));
        }

        [HttpGet("get-all")]
        public async Task<IActionResult> ViewAllBuyerItems()
        {
            return MakeResponse(await _buyerItemService.ViewAllBuyerItems<BuyerItemDetailsDTO>());
        }

        [HttpPost("request-refund")]
        public async Task<IActionResult> RequestRefund([FromQuery] int BuyerItemId)
        {
            return MakeResponse(await _buyerItemService.RequestRefund<GeneralMessageDTO>(BuyerItemId));
        }

        [HttpPost("make-pay-to-user")]
        public async Task<IActionResult> MakePayToUser()
        {
            return MakeResponse(await _buyerItemService.MakePayToUser());
        }
    }

}
