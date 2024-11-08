using System.Security.Claims;
using PototoTrade.DTO.CartItem;
using PototoTrade.DTO.Product;
using PototoTrade.DTO.Role;
using PototoTrade.DTO.ShoppingCart;
using PototoTrade.Enums;
using PototoTrade.Models.ShoppingCart;
using PototoTrade.Repository.Cart;
using PototoTrade.Repository.MediaRepo;
using PototoTrade.Service.Utilities.Response;

namespace PototoTrade.Service.ShoppingCart;

public class ShoppingCartService
{
    private readonly ShoppingCartRepository _cartRepository;
    private readonly MediaRepository _mediaRepository;


    public ShoppingCartService(ShoppingCartRepository cartRepository, MediaRepository mediaRepository)
    {
        _cartRepository = cartRepository;
        _mediaRepository = mediaRepository;
    }
    public async Task<ResponseModel<ShoppingCartInfoDTO>> GetCartInfo(ClaimsPrincipal userClaims)
    {
        var response = new ResponseModel<ShoppingCartInfoDTO>
        {
            Success = false,
            Data = null,
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

            var cart = await _cartRepository.GetShoppingCartByUserId(userId);

            if (cart == null || cart.ShoppingCartItems == null)
            {
                response.Message = "Cart is empty.";
                return response;
            }

            var cartItems = cart.ShoppingCartItems;

            var cartInfo = new ShoppingCartInfoDTO
            {
                NumberOfItems = cartItems.Count,
                TotalPrice = cartItems.Sum(item => item.Product.Price)
            };

            response.Data = cartInfo;
            response.Success = true;
            response.Message = "Cart information retrieved successfully.";
        }
        catch (Exception e)
        {
            response.Message = $"An error occurred: {e.Message}";
        }

        return response;


    }

    public async Task<ResponseModel<GetShoppingCartDTO>> GetShoppingCart(ClaimsPrincipal userClaims, int? cartId = 0)
    {
        var response = new ResponseModel<GetShoppingCartDTO>
        {
            Success = false,
            Data = null,
            Message = "Failed to retrieve the shopping cart."
        };

        try
        {
            var userId = int.Parse(userClaims.FindFirst(ClaimTypes.Name)?.Value);
            var userRole = userClaims.FindFirst(ClaimTypes.Role)?.Value;
            ShoppingCarts cart;

            if (cartId != 0 && (userRole == UserRolesEnum.Admin.ToString() || userRole == UserRolesEnum.SuperAdmin.ToString()))
            {
                cart = await _cartRepository.GetShoppingCartsById((int)cartId);
            }
            else
            {
                cart = await _cartRepository.GetShoppingCartByUserId(userId);
            }

            if (cart == null || cart.ShoppingCartItems == null || !cart.ShoppingCartItems.Any())
            {
                response.Message = "No shopping cart available.";
                return response;
            }

            var shoppingCartDTO = new GetShoppingCartDTO
            {
                Id = cart.Id,
                shoppingCartItems = new List<GetShoppingCartItemDTO>()
            };

            foreach (var item in cart.ShoppingCartItems)
            {
                var product = item.Product;
                if (product != null)
                {

                    var media = await _mediaRepository.GetMediaBySourceIdAndType(product.Id, "Product");
                    var productDTO = new GetProductDTO
                    {
                        Id = product.Id,
                        Price = product.Price,
                        Image = media?.MediaUrl ?? string.Empty,
                        CreatedAt = product.CreatedAt,
                        RefundGuaranteedDuration = product.RefundGuaranteedDuration,
                        Title = product.Title,
                        Status = product.Status
                    };

                    shoppingCartDTO.shoppingCartItems.Add(new GetShoppingCartItemDTO
                    {
                        Id = item.Id,
                        Status = item.Status,
                        Product = productDTO
                    });
                }
            }

            if (cartId != 0 && (userRole == UserRolesEnum.Admin.ToString() || userRole == UserRolesEnum.SuperAdmin.ToString()))
            {
                var user = cart.User;
                if (user != null)
                {
                    shoppingCartDTO.user = new UserRoleDTO
                    {
                        Id = user.Id,
                        RoleId = user.RoleId,
                        Name = user.Name,
                        Status = user.Status
                    };
                }
            }

            response.Data = shoppingCartDTO;
            response.Success = true;
            response.Message = "Cart information retrieved successfully.";
        }
        catch (Exception e)
        {
            response.Message = $"An error occurred: {e.Message}";
        }

        return response;
    }

    public async Task<ResponseModel<List<GetShoppingCartDTO>>> GetShoppingCarts()
    {
        var response = new ResponseModel<List<GetShoppingCartDTO>>
        {
            Success = false,
            Data = null,
            Message = "Failed to retrieve shopping carts."
        };

        try
        {
            var carts = await _cartRepository.GetShoppingCarts();

            if (carts == null || !carts.Any())
            {
                response.Message = "No shopping carts available.";
                return response;
            }

            var shoppingCartDTOs = new List<GetShoppingCartDTO>();

            foreach (var cart in carts)
            {
                var shoppingCartDTO = new GetShoppingCartDTO
                {
                    Id = cart.Id,
                    shoppingCartItems = new List<GetShoppingCartItemDTO>()
                };

                foreach (var item in cart.ShoppingCartItems)
                {
                    shoppingCartDTO.shoppingCartItems.Add(new GetShoppingCartItemDTO
                    {
                        Id = item.Id,
                        Status = item.Status,
                    });
                }

                var user = cart.User;
                if (user != null)
                {
                    shoppingCartDTO.user = new UserRoleDTO
                    {
                        Id = user.Id,
                        RoleId = user.RoleId,
                        Name = user.Name,
                        Status = user.Status
                    };
                }

                shoppingCartDTOs.Add(shoppingCartDTO);
            }

            response.Data = shoppingCartDTOs;
            response.Success = true;
            response.Message = "Shopping carts retrieved successfully.";
        }
        catch (Exception e)
        {
            response.Message = $"An error occurred: {e.Message}";
        }

        return response;
    }


    public async Task<ResponseModel<bool>> DeleteShoppingCart(int id)
    {
        var response = new ResponseModel<bool>
        {
            Data = false,
            Message = "unable to  delete shopping cart",
            Success = false
        };
        try
        {
            var shoppingCart = await _cartRepository.GetShoppingCartsById(id);

            if (shoppingCart == null)
            {
                response.Message = "shopping cart not exist";
                return response;
            }

            await _cartRepository.DeleteShoppingCart(shoppingCart);

            response.Message = "cart deleted";
            response.Data = true;
            response.Success = true;

        }
        catch (Exception e)
        {
            response.Message = "Something went wrong undable to delete the shopping cart";

        }

        return response;
    }

}
