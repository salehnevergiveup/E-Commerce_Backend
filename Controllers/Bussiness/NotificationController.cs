using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using PotatoTrade.DTO.Notification;
using PotatoTrade.Service.Notification;
using PototoTrade.Controllers.CustomController;
namespace PotatoTrade.Controllers.Bussiness{
    [Route("api/notifications")]
    [ApiController]
    public class NotificationsController : CustomBaseController
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
        public async Task<IActionResult> BroadcastNotificationToUsers([FromBody] SendNotificationDTO sendNotificationDTO)
        {

            return MakeResponse(await _notificationService.CreateBroadcastNotificationWithUserNotifications(User, sendNotificationDTO.SenderUsername, sendNotificationDTO.Title, sendNotificationDTO.MessageText));
            // var responseObject = await _notificationService.CreateBroadcastNotificationWithUserNotifications(User, sendNotificationDTO.SenderUsername, sendNotificationDTO.Title, sendNotificationDTO.MessageText);
            // if (!responseObject.Success)
            // {
            //     return BadRequest(responseObject.Message);
            // }
            

            // return Ok(responseObject.Success);        
        }

        [HttpGet("users/all")]
        [Authorize(Roles = "User")]
        public async Task<IActionResult> FetchAllNotificationsForUsers()
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

        
        [HttpGet("all")]
        [Authorize(Roles = "Admin,SuperAdmin")]
        public async Task<IActionResult> FetchAllNotificationsForAdmin(){
            return MakeResponse(await _notificationService.GetNotificationsForAdminAsync());
        }

        [HttpPost("users/update_notifications")]
        [Authorize(Roles = "User")]
        public async Task<IActionResult> MarkAllNotificationsAsRead()
        {
            return MakeResponse(await _notificationService.MarkAllNotificationsAsRead(User));            
        }

    }

}
