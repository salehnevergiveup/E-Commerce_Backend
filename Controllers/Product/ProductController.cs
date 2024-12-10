using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PototoTrade.Controllers.CustomController;
using PototoTrade.DTO.Common;
using PototoTrade.DTO.Product;
using PototoTrade.Service.Product;

namespace PototoTrade.Controllers.Product
{
    [Route("api/product")]
    [ApiController]
    [Authorize(Roles = "User,Admin, SuperAdmin")]
    public class ProductController : CustomBaseController
    {
        private readonly ProductSrv _productService;

        private readonly ProductSrvBsn _productServiceBsn;
        public ProductController(ProductSrv productService, ProductSrvBsn productSrvBsn)
        {
            _productService = productService;
            _productServiceBsn = productSrvBsn;
        }

        [HttpPost("create-category")]
        public async Task<IActionResult> CreateCategory(CreateProductCategoryDTO categoryDto)
        {
            return MakeResponse(await _productService.CreateCategory<GeneralMessageDTO>(categoryDto));
        }

        [HttpPut("edit-category")]
        public async Task<IActionResult> EditCategory(EditProductCategoryDTO categoryDto)
        {
            return MakeResponse(await _productService.EditCategory<GeneralMessageDTO>(categoryDto));
        }

        [HttpPost("create-product")]
        public async Task<IActionResult> CreateProduct(CreateProductDTO createProductDto)
        {
            return MakeResponse(await _productServiceBsn.CreateProduct<GeneralMessageDTO>(User, createProductDto));
        }


        [HttpPut("edit-product")]
        public async Task<IActionResult> EditProduct(EditProductDetailDto editProductDto)
        {
            return MakeResponse(await _productService.EditProduct<GeneralMessageDTO>(User, editProductDto));
        }

        [HttpPost("place-order")]
        public async Task<IActionResult> PlaceOrder()
        {
            return MakeResponse(await _productServiceBsn.PlaceOrder<PlaceOrderResponseDTO>(User));
        }

        [HttpDelete("cancel-item")]
        public async Task<IActionResult> CancelItemInOrder(CancelItemRequestDTO cancelItemRequest)
        {
            return MakeResponse(await _productServiceBsn.CancelItemInOrder<GeneralMessageDTO>(cancelItemRequest));
        }

        [HttpGet("fetch-rebate-amount-list/{orderId}")]
        public async Task<IActionResult> FetchRebateAmountList(int orderId)
        {
            return MakeResponse(await _productServiceBsn.FetchRebateAmountList<RebateAmountListDTO>(orderId));
        }

        [HttpPost("accept-refund")]
        public async Task<IActionResult> AcceptRefund([FromBody] RefundDTO refundDto)
        {
            return MakeResponse(await _productServiceBsn.AcceptRefund<GeneralMessageDTO>(refundDto, User));
        }

        [HttpPost("rejectcancel-refund")]
        public async Task<IActionResult> RejectRefund([FromBody] RefundDTO refundDto)
        {
            return MakeResponse(await _productServiceBsn.RejectOrCancelRefund<GeneralMessageDTO>(refundDto, User));
        }


        [HttpGet("get-available-product")]
        public async Task<IActionResult> GetAvailableProducts()
        {
            return Ok(await _productService.GetUserProductsByStatus<ProductResponseDTO>("available", User));
        }

        [HttpGet("get-sold-out-product")]
        public async Task<IActionResult> GetSoldOutProducts()
        {
            return Ok(await _productService.GetUserProductsByStatus<ProductResponseDTO>("sold out", User));
        }

        [HttpGet("get-request-refund-product")]
        public async Task<IActionResult> GetRefundRequestProducts()
        {
            return Ok(await _productService.GetUserProductsByStatus<ProductResponseDTO>("request refund", User));
        }

        [HttpGet("get-singleproduct-details")]
        public async Task<IActionResult> ViewProductDetails([FromQuery] int productId)
        {
            return MakeResponse(await _productService.ViewProductDetails<GeneralMessageDTO>(productId, User));
        }

        [HttpPut("delete-product")]
        public async Task<IActionResult> DeleteProduct([FromQuery] int productId)
        {
            return MakeResponse(await _productService.DeleteProduct<GeneralMessageDTO>(productId, User));
        }
    }

}
