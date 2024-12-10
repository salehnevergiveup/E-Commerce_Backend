using Azure;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PototoTrade.Controllers.CustomController;
using PototoTrade.Service.ShoppingCart;
using PototoTrade.Service.Utilities.Response;

namespace PototoTrade.Controllers.ShoppingCart
{
    [Route("api/shoppingcart")]
    [ApiController]
    public class ShoppingCartController : CustomBaseController
    {
        private readonly ShoppingCartService _shoppingCartService;
        
        public ShoppingCartController(ShoppingCartService shoppingCartService)
        {
            _shoppingCartService = shoppingCartService;
        }

        [HttpGet("info")]
        [Authorize(Roles = "User")]
        public async Task<IActionResult> GetShoppingCartInfo()
        {
            return MakeResponse(await _shoppingCartService.GetCartInfo(User));
        }

        [HttpGet]
        [Authorize(Roles = "User,Admin,SuperAdmin")]
        public async Task<IActionResult> GetShoppingCartInfo([FromQuery] int cartId = 0)
        {
            return MakeResponse(await _shoppingCartService.GetShoppingCart(User, cartId));
        }

        [HttpGet("list")]
        [Authorize(Roles = "Admin,SuperAdmin")]
        public async Task<IActionResult> GetShoppingCartList()
        {
            return MakeResponse(await _shoppingCartService.GetShoppingCarts());
        }

        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin,SuperAdmin")]
        public async Task<IActionResult> DeleteShoppingCart(int id)
        {
            return MakeResponse(await _shoppingCartService.DeleteShoppingCart(id));
        }

    }
}
