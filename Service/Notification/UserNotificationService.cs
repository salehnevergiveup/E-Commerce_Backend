// using System.Linq.Expressions;
// using System.Security.Claims;
// using PotatoTrade.DTO.Notification;
// using PotatoTrade.Repository.Notification;
// using PototoTrade.Data;
// using PototoTrade.Models.Notification;
// using PototoTrade.Repository.Users;
// using PototoTrade.Service.User;
// using PototoTrade.Service.Utilities.Response;

// namespace PotatoTrade.Service.Notification{
//     public class UserNotificationService{
//         private readonly NotificationRepository _notificationRepository;
//         private readonly UserAccountService _userAccountService;

//         private readonly NotificationService _notificationService;
//         private readonly DBC _context;



//         public UserNotificationService(NotificationRepository notificationRepository, UserAccountService userAccountService, NotificationService notificationService, DBC context)
//         {
//             _notificationRepository = notificationRepository;
//             _userAccountService = userAccountService;
//             _notificationService = notificationService;
//             _context = context;
//         }

//         public async Task<ResponseModel<bool>>  createBroadcastUserNotificationandSaveToDB(ClaimsPrincipal userClaims, Notifications broadcastedNotification ){
//             var response  = new ResponseModel<bool>{
//                 Success = false,
//                 Data = false,
//                 Message = "Failed to create and save noti to db"
//             };
//             try{
//                 var userId = int.Parse(userClaims.FindFirst(ClaimTypes.Name)?.Value);
//                 if (userId == 0)
//                 {
//                     response.Message = "Invalid Request";
//                     return response;
//                 }        

//                 var userIdsResponse = await _userAccountService.GetUserIdsByRoleId(3);

//                 // Check if the response is successful
//                 if (!userIdsResponse.Success)
//                 {
//                     response.Message = userIdsResponse.Message; // Return the failure message from the response
//                     return response;
//                 }

//                 // Access the actual user IDs from the response
//                 List<int> receiver_ids = userIdsResponse.Data;
//                 var notificationId = broadcastedNotification.Id;
//                 var isUserNotificationBroadcasted = await _notificationRepository.CreateBroadcastUserNotification(receiver_ids, notificationId);

//                 if (isUserNotificationBroadcasted == false)
//                 {
//                     response.Message = "User Notification entries not created";
//                     return response;
//                 }
//                 response.Success = true;
//                 response.Data = true;
//                 response.Message = "broadcast message created and saved successfully";
            
//             }catch(Exception e){
//                 response.Message = $"An error occurred: {e.Message}";
//             }
//         return response;
//         }

        
//     }
// }