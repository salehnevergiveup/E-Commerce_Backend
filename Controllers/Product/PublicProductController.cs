using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PototoTrade.Controllers.CustomController;
using PototoTrade.DTO.Common;
using PototoTrade.DTO.Product;
using PototoTrade.Service.Product;

namespace PototoTrade.Controllers.Product
{
    [Route("api/product/public")]
    [ApiController]
    public class PublicProductController : CustomBaseController
    {
        private readonly ProductSrv _productService;

        private readonly ProductSrvBsn _productServiceBsn;
        public PublicProductController(ProductSrv productService, ProductSrvBsn productSrvBsn)
        {
            _productService = productService;
            _productServiceBsn = productSrvBsn;
        }

       
        [HttpGet("get-product-category-list")]
        public async Task<IActionResult> GetProductCategoryList()
        {
            return MakeResponse(await _productService.GetProductCategoryList<List<ProductCategoryListDTO>>());
        }

        [HttpPost("view-products-by-category")]
        public async Task<IActionResult> ViewProductsViaProductCategory(ViewProductsRequestDTO request)
        {
            return MakeResponse(await _productService.ViewProductsByCategory<List<ProductDetailsDTO>>(request));
        }

        [HttpGet("view-all-products")]
        public async Task<IActionResult> ViewAllProducts()
        {
            return MakeResponse(await _productService.ViewAllProducts<List<ProductDetailsDTO>>());
        }

        [HttpGet("view-all-available-products")]
        public async Task<IActionResult> ViewAllAvailableProducts()
        {
            return MakeResponse(await _productService.ViewAllAvailableProducts<List<ProductDetailsDTO>>());
        }

        [HttpPost("view-available-products-by-category")]
        public async Task<IActionResult> ViewAvailableProductsViaProductCategory(ViewProductsRequestDTO request)
        {
            return MakeResponse(await _productService.ViewAvailableProductsByCategory<List<ProductDetailsDTO>>(request));
        }

    }

}
