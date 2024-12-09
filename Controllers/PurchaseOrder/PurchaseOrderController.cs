using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PototoTrade.Controllers.CustomController;
using PototoTrade.DTO.Common;
using PototoTrade.DTO.Product;
using PototoTrade.Service.Product;

namespace PototoTrade.Controllers.PurchaseOrder
{
    [Route("api/order")]
    [ApiController]
    [Authorize(Roles = "User,Admin, SuperAdmin")]
    public class PurchaseOrderController : CustomBaseController
    {


        private readonly ProductSrvBsn _productServiceBsn;
        public PurchaseOrderController(ProductSrv productService, ProductSrvBsn productSrvBsn)
        {
            _productServiceBsn = productSrvBsn;
        }

        [HttpPost("make-payment")]
        public async Task<IActionResult> MakePayment(MakePaymentRequestDTO paymentRequest)
        {
            return MakeResponse(await _productServiceBsn.MakePayment<GeneralMessageDTO>(paymentRequest, User));
        }

        [HttpGet("view-pending-order")]
        public async Task<IActionResult> ViewPendingOrder()
        {
            return MakeResponse(await _productServiceBsn.ViewPendingOrder<PurchaseOrderDTO>(User));
        }

        [HttpGet("view-order")]
        public async Task<IActionResult> ViewPurchaseOrder([FromQuery] int purchaseOrderId)
        {
            return MakeResponse(await _productServiceBsn.ViewPurchaseOrderItems<PurchaseOrderDTO>(purchaseOrderId));
        }

    }

}
