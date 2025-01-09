using System.Linq.Expressions;
using System.Security.Claims;
using Microsoft.AspNetCore.SignalR;
using PotatoTrade.DTO.Notification;
using PotatoTrade.Repository.Notification;
using PototoTrade.Data;
using PototoTrade.Models.Notification;
using PototoTrade.Repository.Users;
using PototoTrade.Service.User;
using PototoTrade.Service.Utilities.Response;

namespace PotatoTrade.Service.Notification{
    public class NotificationService{
        private readonly NotificationRepository _notificationRepository;
        private readonly IHubContext<NotificationHub> _notificationHubContext;

        private readonly UserAccountRepository _userAccountRepository;

        private readonly UserAccountService _userAccountService;
        private readonly DBC _context;



        public NotificationService(IHubContext<NotificationHub> notificationHubContext, NotificationRepository notificationRepository, UserAccountRepository userAccountRepository, UserAccountService userAccountService, DBC context)
        {
            _notificationHubContext = notificationHubContext;
            _notificationRepository = notificationRepository;
            _userAccountRepository = userAccountRepository;
            _userAccountService = userAccountService;
            _context = context;
        }

        public async Task<ResponseModel<List<OrderPurchasedNotificationDTO>>> createOrderPurchasedNotificationandSaveToDB(ClaimsPrincipal userClaims, List<SystemInnerNotificationDto> orderlist){
            var response  = new ResponseModel<List<OrderPurchasedNotificationDTO>>{
                Success = false,
                Data = null,
                Message = "Failed to create and save noti to db"
            };
            try{
                List<OrderPurchasedNotificationDTO> orderNotifactionList = new List<OrderPurchasedNotificationDTO>();
                
                if (orderlist.Any()){
                foreach (var order in orderlist){

                    var orders = new Notifications{
                    SenderId = 1,
                    SenderUsername = "System",
                    ReceiverId = order.ReceiverId, //
                    ReceiverUsername =order.ReceiverUsername ,//
                    Type = "item purchased notification",
                    Title = order.Title,
                    MessageText = order.MessageText,
                    CreatedAt = DateTime.UtcNow,
                    Status = "notRead",
                    
                };
                Console.WriteLine($"Creating Notification by system : {orders}");

                await _notificationRepository.CreateNotification(orders);

                var orderPurchasedDTO = new OrderPurchasedNotificationDTO
                {
                    NotificationId = orders.Id,
                    SenderUsername = "System",
                    Title = order.Title,
                    ReceiverUsername = orders.ReceiverUsername,
                    Type = orders.Type,
                    MessageText = orders.MessageText,
                    CreatedAt = orders.CreatedAt,
                    UpdatedAt = orders.CreatedAt,
                    Status = orders.Status,
                    IsRead = false,

                    
                };

                orderNotifactionList.Add(orderPurchasedDTO);
                Console.WriteLine($"{orderPurchasedDTO}");
                await _notificationHubContext.Clients.Group($"User-{orders.ReceiverId}")
                    .SendAsync("ReceivePurchasedNotification", orderPurchasedDTO);
                }
                
                }
                
    
                response.Success = true;
                response.Data = orderNotifactionList;
                response.Message = "broadcast message created and saved successfully";
               

            }catch(Exception e){
                response.Message = $"An error occurred: {e.Message}";
            }
        return response;
        }

        public async Task<ResponseModel<BroadcastReturnDTO>> CreateBroadcastNotificationWithUserNotifications(ClaimsPrincipal userClaims, string senderUsername, string title, string message)
        {
            var response = new ResponseModel<BroadcastReturnDTO>
            {
                Success = false,
                Data = null,
                Message = "Failed to create and save notification entries to the database."
            };

            using (var transaction = await _context.Database.BeginTransactionAsync())
            {
                try
                {
                    // Create the Notification
                    var userId = int.Parse(userClaims.FindFirst(ClaimTypes.Name)?.Value);
                    if (userId == 0)
                    {
                        response.Message = "Invalid Request";
                        return response;
                    }

                    //var senderUsername = await _userAccountRepository.GetUsernameByUserIdAsync(userId);

                    var broadcastMsg = new Notifications
                    {
                        SenderId = userId,
                        SenderUsername = senderUsername,
                        Title = title,
                        ReceiverId = 0,
                        ReceiverUsername = "all",
                        Type = "broadcast",
                        MessageText = message,
                        CreatedAt = DateTime.UtcNow,
                        Status = "delivered"
                    };

                    await _notificationRepository.CreateNotification(broadcastMsg);
                    await _notificationRepository.CreateBroadcastUserNotification(3, broadcastMsg.Id);

                    // Commit the transaction if both operations succeed
                    await transaction.CommitAsync();

                    var broadcastReturnDTO = new BroadcastReturnDTO
                    {
                        NotificationId = broadcastMsg.Id,
                        SenderUsername = senderUsername,
                        Title = title,
                        MessageText = message,
                        CreatedAt = broadcastMsg.CreatedAt,
                        Type = broadcastMsg.Type,
                        IsRead = false,
                        UpdatedAt = broadcastMsg.CreatedAt,

                    };
                    await _notificationHubContext.Clients.Group("Users").SendAsync("ReceiveNotification", broadcastReturnDTO);
                   
                    response.Success = true;
                    response.Data = broadcastReturnDTO;
                    response.Message = "Broadcast message and user notification entries created and saved successfully";
                    return response;
                }
                catch (Exception e)
                {
                   Console.WriteLine($"{e.Message}, Error creating broadcast notification.");
                    await transaction.RollbackAsync();
                    response.Message = $"An error occurred: {e.Message}";
                    return response;
                }
            }
        }

        public async Task<ResponseModel<List<GetAllLatestNotificationsDTO>>> GetUserNotificationsAsync(ClaimsPrincipal userClaims)
        {
            var response = new ResponseModel<List<GetAllLatestNotificationsDTO>>
            {
                Success = false,
                Data = null,
                Message = "Failed to retrieve notifications."
            };

            try
            {
                var userId = int.Parse(userClaims.FindFirst(ClaimTypes.Name)?.Value);
                    if (userId == 0)
                    {
                        response.Message = "Invalid or missing user ID in claims";
                        return response;
                    }
                // Call the repository to fetch notifications
                var notifications = await _notificationRepository.GetNotificationsForUserAsync(userId);

                if (notifications != null && notifications.Any())
                {
                    response.Success = true;
                    response.Data = notifications;
                    response.Message = "Notifications retrieved successfully.";
                    Console.WriteLine(response.Data);
                }
                else
                {
                    response.Message = "No notifications found for the user.";
                }
            }
            catch (Exception ex)
            {
                response.Message = $"An error occurred while retrieving notifications: {ex.Message}";
            }

            return response;
        }

        public async Task<ResponseModel<bool>> MarkAllNotificationsAsRead(ClaimsPrincipal userClaims)
        {
            var response = new ResponseModel<bool>
            {
                Success = false,
                Data = false,
                Message = "Failed to mark all notifications as read."
            };

            try
            {
                var userId = int.Parse(userClaims.FindFirst(ClaimTypes.Name)?.Value);
                 if (userId == 0)
                    {
                        response.Message = "Invalid or missing user ID in claims";
                        return response;
                    }

                var updatedCount = await _notificationRepository.MarkNotificationsAsReadAsync(userId);

                Console.WriteLine($"Number of notifications marked as read: {updatedCount}+ {userId}");

                var latestNotifications = await _notificationRepository.GetLatestNotificationsByUserId(userId);
                foreach (var notification in latestNotifications)
                {
                    Console.WriteLine($"NEW Notification ID: {notification.NotificationId}, Title: {notification.Title}, Message: {notification.MessageText}, Created At: {notification.CreatedAt}, IsRead: {notification.IsRead}, Status: {notification.Status}");
                }

                try
                {
                    await _notificationHubContext.Clients.Group($"User-{userId}").SendAsync("ReceiveListofLatestNotification", latestNotifications);
                    Console.WriteLine($"SignalR event 'ReceiveListofLatestNotification' sent successfully to user {userId}.");
                }
                catch (Exception signalREx)
                {
                    Console.WriteLine($"Failed to send SignalR event to user {userId}. Exception: {signalREx.Message}");
                    if (signalREx.InnerException != null)
                    {
                        Console.WriteLine($"Inner Exception: {signalREx.InnerException.Message}");
                    }
                    Console.WriteLine($"Stack Trace: {signalREx.StackTrace}");
                }
                Console.WriteLine($"User {userId} notified with updated notifications.");

                response.Success = true;
                response.Message = $"{updatedCount} notifications marked as read.";
                Console.WriteLine("MarkAllNotificationsAsRead completed successfully.");
            }
            catch (Exception ex)
            {
                response.Message = $"An error occurred: {ex.Message}";
                Console.WriteLine($"Exception: {ex.Message}");
                if (ex.InnerException != null)
                {
                    Console.WriteLine($"Inner Exception: {ex.InnerException.Message}");
                }
                Console.WriteLine($"Stack Trace: {ex.StackTrace}");
            }

            return response;
        }

        public async Task<ResponseModel<List<NotificationDTO>>> GetNotificationsForAdminAsync()
        {
            var response = new ResponseModel<List<NotificationDTO>>
            {
                Success = false,
                Data = null,
                Message = "Failed to fetch notifications."
            };

            try
            {
                var notifications = await _notificationRepository.GetAllNotificationsForAdmin();
                response.Success = true;
                response.Data = notifications;
                response.Message = "Notifications fetched successfully.";
            }
            catch (Exception ex)
            {
                response.Message = $"An error occurred: {ex.Message}";
            }

            return response;
        }


        // public async Task<ResponseModel<Notifications>> RemindAdminToChangePassword()
        // {
        //     var response = new ResponseModel<List<Notifications>>
        //     {
        //         Success = false,
        //         Data = null,
        //         Message = "Failed to remind admin."
        //     };
        //     try{


        //     }catch (Exception ex){
        //         response.Message = $"An error occurred: {ex.Message}";
        //     }

        // }


    }


        
}
