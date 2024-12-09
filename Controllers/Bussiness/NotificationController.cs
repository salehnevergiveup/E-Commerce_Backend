using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using PotatoTrade.DTO.Notification;
using PotatoTrade.Service.Notification;
using PototoTrade.Controllers.CustomerController;
namespace PotatoTrade.Controllers.Bussiness{
    [Route("api/notifications")]
    [ApiController]
    public class NotificationsController : CustomerBaseController
    {
        private readonly IHubContext<NotificationHub> _notificationHubContext;
        private readonly NotificationService _notificationService;

        public NotificationsController(IHubContext<NotificationHub> notificationHubContext, NotificationService notificationService)
        {
            _notificationHubContext = notificationHubContext;
            _notificationService = notificationService;
        }

        [HttpPost("users/broadcast")]
        [Authorize(Roles = "Admin,SuperAdmin")]
        public async Task<IActionResult> BroadcastNotificationToUsers([FromBody] NotificationDTO notificationDTO)
        {
            var responseObject = await _notificationService.CreateBroadcastNotificationWithUserNotifications(User, notificationDTO.Title, notificationDTO.MessageText);
            if (!responseObject.Success)
            {
                return BadRequest(responseObject.Message);
            }
            await _notificationHubContext.Clients.Group("Users").SendAsync("ReceiveNotification", responseObject.Data);

            return Ok(new { message = "Notification broadcasted successfully" });        
        }

        [HttpGet("users/all")]
        [Authorize(Roles = "User")]
        public async Task<IActionResult> FetchAllNotifications()
        {
           return MakeResponse(await _notificationService.GetUserNotificationsAsync(User));
            // if (!responseObject.Success)
            // {
            //     return BadRequest("bad request" + responseObject.Message + responseObject.Data);
            // }
            // return Ok(new { 
            //     message = responseObject.Message, 
            //     data = responseObject.Data 
            // });    
        }
        // [HttpGet("users/update_notifications")]
        // [Authorize(Roles = "User")]
        // public async Task<IActionResult> MarkAllNotificationsAsRead()
        // {
            
        // }

    }

}
