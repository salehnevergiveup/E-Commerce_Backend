using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using PototoTrade.DTO.ShoppingCart;
using PototoTrade.Repository.Cart;
using System.Security.Claims;

public class CartHub : Hub
{
    private readonly ShoppingCartRepository _cartRepository;

    public CartHub(ShoppingCartRepository cartRepository)
    {
        _cartRepository = cartRepository;
    }

    // Ensure only authenticated users can connect
    [Authorize(Roles = "User")] // only users have this feature 
    public override async Task OnConnectedAsync()
    {
        var userId = Context.User.FindFirst(ClaimTypes.Name)?.Value;
        if (!string.IsNullOrEmpty(userId))
        {
            await Groups.AddToGroupAsync(Context.ConnectionId, $"User-{userId}");

            var cart = await _cartRepository.GetShoppingCartByUserId(int.Parse(userId));
            //once the user connected he will recive the latest changes 
            if (cart != null && cart.ShoppingCartItems != null)
            {
                var cartInfo = new ShoppingCartInfoDTO
                {
                    NumberOfItems = cart.ShoppingCartItems.Count,
                    TotalPrice = cart.ShoppingCartItems.Sum(item => item.Product.Price)
                };
                await Clients.Caller.SendAsync("ReceiveCartUpdate", cartInfo);
            }
        }

        await base.OnConnectedAsync();
    }

    public override async Task OnDisconnectedAsync(Exception exception)
    {
        var userId = Context.User.FindFirst(ClaimTypes.Name)?.Value;
        if (!string.IsNullOrEmpty(userId))
        {
            await Groups.RemoveFromGroupAsync(Context.ConnectionId, $"User-{userId}");
        }

        await base.OnDisconnectedAsync(exception);
    }

    // send the update to connected user upoun the interaction with the cart 
    public async Task SendCartUpdate(int userId, ShoppingCartInfoDTO cartInfo)
    {
        await Clients.Group($"User-{userId}").SendAsync("ReceiveCartUpdate", cartInfo);
    }
}
