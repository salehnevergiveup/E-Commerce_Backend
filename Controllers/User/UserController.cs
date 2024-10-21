using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using PototoTrade.Service.User;

namespace PototoTrade.Controllers.User
{
    [ApiController]
    [Route("api/[controller]/public")]
    public class UserController : ControllerBase
    {
        private readonly IUserAccountService _userService;

        public UserController(IUserAccountService userAccountService)
        {
            _userService = userAccountService;
        }

        [HttpGet("getUserList")]
        public async Task<IActionResult> GetUserList()
        {
            var userList = await _userService.GetUserList();
            return Ok(userList);

        }
    }
}