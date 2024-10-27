using Microsoft.AspNetCore.Mvc;
using PototoTrade.DTO.User;
using PototoTrade.Service.User;

namespace PototoTrade.Controllers.User
{
    [ApiController]
    [Route("api/[controller]/public")]
    // [Authorize(Roles = "Admin, SuperAdmin" )]
    public class UserController : ControllerBase
    {
        private readonly UserAccountService _userService;

        public UserController(UserAccountService userAccountService)
        {
            _userService = userAccountService;
        }

        [HttpGet]
        public async Task<IActionResult> GetUserList()
        {
            var response = await _userService.GetUserList(User);
            return response.Success ? Ok(response) : NotFound(response);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetUser(int id)
        {
            var response = await _userService.GetUser(id, User);
            return response.Success ? Ok(response) : NotFound(response);
        }

        [HttpPost]
        public async Task<IActionResult> createUser(CreateNewAccount createUser)
        {
            var response = await _userService.CreateUser(createUser, User);
            return response.Success ? Ok(response) : BadRequest(response);
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteUser(int id)
        {
            var response = await _userService.DeleteUser(id);
            return response.Success ? Ok(response) : NotFound(response);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateUser(int id, UpdateUserAccountDTO updateUserDto)
        {
            var response = await _userService.updateUser(id, updateUserDto, User);
            return response.Success ? Ok(response) : NotFound(response);
        }


    }
}