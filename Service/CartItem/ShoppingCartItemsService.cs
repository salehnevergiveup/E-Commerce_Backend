using System.Security.Claims;
using Microsoft.AspNetCore.SignalR;
using PototoTrade.DTO.CartItem;
using PototoTrade.DTO.ShoppingCart;
using PototoTrade.Models.ShoppingCart;
using PototoTrade.Repository.BuyerItem;
using PototoTrade.Repository.Cart;
using PototoTrade.Repository.CartItems;
using PototoTrade.Repository.Product;
using PototoTrade.Service.Utilities.Response;

namespace PototoTrade.Service.CartItem;

public class ShoppingCartItemsService
{
    private readonly ShoppingCartItemRepository _shoppingCartItem;
    private readonly ProductRepository _product;
    private readonly ShoppingCartRepository _shoppingCart;

    private readonly IHubContext<CartHub> _cartHubContext;

    private readonly BuyerItemRepository _buyerItemRepository;

    public ShoppingCartItemsService(ShoppingCartItemRepository shoppingCartItem, ProductRepository product, ShoppingCartRepository shoppingCart, IHubContext<CartHub> cartHubContext, IHttpContextAccessor httpContextAccessor,
    BuyerItemRepository buyerItemRepository)
    {
        _shoppingCartItem = shoppingCartItem;
        _product = product;
        _shoppingCart = shoppingCart;
        _cartHubContext = cartHubContext;
        _buyerItemRepository = buyerItemRepository;
    }

    public async Task<ResponseModel<int>> CreateCartItem(CreateCartItemDTO cartItem, ClaimsPrincipal userClaims)
    {

        var response = new ResponseModel<int>
        {
            Success = false,
            Data = 0,
            Message = "Failed to add item to the cart."
        };

        try
        {
            var userId = int.Parse(userClaims.FindFirst(ClaimTypes.Name)?.Value);

            if (userId == 0)
            {
                response.Message = "Invalide Request";
                return response;
            }

            var buyerItemExist = await _buyerItemRepository.GetSingerBuyerItemsByStatusAndUserId("pending", userId,cartItem.ProductId);
            if (buyerItemExist != null && buyerItemExist.Status == "pending")
            {
                response.Message = $"Product has been added to your purchase order {buyerItemExist.OrderId}";
                return response;
            }

            var product = await _product.GetProduct(cartItem.ProductId);
            if (product == null || product.Status != "available")
            {
                response.Message = "Product is not available.";
                return response;
            }

            var existingCart = await _shoppingCart.GetShoppingCartByUserId(userId);
            if (existingCart == null)
            {
                existingCart = new ShoppingCarts
                {
                    UserId = userId,
                    CreatedAt = DateTime.UtcNow,
                };
                await _shoppingCart.CreateShoppingCart(existingCart);
            }

            var existingCartItem = existingCart.ShoppingCartItems
                .FirstOrDefault(item => item.ProductId == cartItem.ProductId);

            if (existingCartItem != null)
            {
                response.Message = "product have been already added check your shopping cart please";
                return response;
            }

            var newCartItem = new ShoppingCartItem
            {
                CartId = existingCart.Id,
                ProductId = cartItem.ProductId,
                Status = "Selected",
                AddedAt = DateTime.UtcNow
            };

            await _shoppingCartItem.CreateItem(newCartItem);


            var cartInfo = new ShoppingCartInfoDTO
            {
                NumberOfItems = existingCart.ShoppingCartItems.Count,
                TotalPrice = existingCart.ShoppingCartItems.Sum(item => item.Product.Price)
            };

            //update the shoping cart after the item is being added 
            existingCart = await _shoppingCart.GetShoppingCartByUserId(userId);

            await _cartHubContext.Clients.Group($"User-{userId}").SendAsync("ReceiveCartUpdate", cartInfo);

            response.Success = true;
            response.Data = existingCart.Id;
            response.Message = "Item added to the cart successfully.";
            return response;
        }
        catch (Exception e)
        {
            response.Message = $"An error occurred: {e.Message}";
            return response;

        }

    }

    public async Task<ResponseModel<bool>> UpdateCartItemStatus(int id, UpdateCartItemStatusDTO cartStatus)
    {
        var response = new ResponseModel<bool>
        {
            Message = "Unable to update the status",
            Success = false,
            Data = false
        };
        try
        {
            var cartItem = await _shoppingCartItem.GetShoppingCartItemById(id);

            if (cartItem == null)
            {
                response.Message = "cart item is not avaliable";
                return response;
            }

            cartItem.Status = cartStatus.Status;

            await _shoppingCartItem.UpdateShoppingCartItem(cartItem);

            response.Message = "Cart item status updated";
            response.Data = true;
            response.Success = true;
        }
        catch (Exception e)
        {
            response.Data = false;
            response.Success = false;
        }

        return response;
    }
    public async Task<ResponseModel<bool>> DeleteCartItemStatus(int id)
    {
        var response = new ResponseModel<bool>
        {
            Message = "Unable to remove the status",
            Success = false,
            Data = false
        };

        try
        {
            var cartItem = await _shoppingCartItem.GetShoppingCartItemById(id);

            if (cartItem == null)
            {
                response.Message = "Cart item is not available";
                return response;
            }

            int CartId = cartItem.CartId;

            await _shoppingCartItem.DeleteShoppingCartItem(cartItem);

            var existingCart = await _shoppingCart.GetShoppingCartsById(CartId);

            if (existingCart == null)
            {
                response.Message = "Error updating the shopping cart status";
                return response;
            }

            // Prepare the updated shopping cart information
            var cartInfo = new ShoppingCartInfoDTO
            {
                NumberOfItems = existingCart.ShoppingCartItems.Count,
                TotalPrice = existingCart.ShoppingCartItems != null ? existingCart.ShoppingCartItems.Sum(item => item.Product.Price) : 0
            };

            await _cartHubContext.Clients.Group($"User-{existingCart.UserId}").SendAsync("ReceiveCartUpdate", cartInfo);

            response.Message = "Cart item removed successfully";
            response.Data = true;
            response.Success = true;
        }
        catch (Exception e)
        {
            Console.WriteLine($"Error: {e.Message}");

            response.Message = "An error occurred while removing the cart item";
            response.Data = false;
            response.Success = false;
        }

        return response;
    }

}
