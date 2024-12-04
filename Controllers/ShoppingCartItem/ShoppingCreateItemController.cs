using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PototoTrade.Controllers.CustomController;
using PototoTrade.DTO.CartItem;
using PototoTrade.Service.CartItem;
using PototoTrade.Service.Utilities.Response;

namespace PototoTrade.Controllers.ShoppingCartItem
{
    [Route("api/cartitems")]
    [ApiController]
    public class ShoppingCartItemController : CustomBaseController
    {
        ShoppingCartItemsService _shoppingCartItemsService;

        public ShoppingCartItemController(ShoppingCartItemsService shoppingCartItemsService)
        {
            _shoppingCartItemsService = shoppingCartItemsService;
        }

        [HttpPost]
        [Authorize(Roles = "User")]
        public async Task<IActionResult> CreateShoppingCartItem(CreateCartItemDTO cartItem)
        {
            return MakeResponse(await _shoppingCartItemsService.CreateCartItem(cartItem, User));
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteShoppingCartItem(int id)
        {
            return MakeResponse(await _shoppingCartItemsService.DeleteCartItemStatus(id));
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateShoppingCartItemStatus(int id, UpdateCartItemStatusDTO cartItemStatus)
        {
            return MakeResponse(await _shoppingCartItemsService.UpdateCartItemStatus(id, cartItemStatus));
        }

    }
}
