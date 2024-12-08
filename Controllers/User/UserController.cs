using Microsoft.AspNetCore.Mvc;
using PototoTrade.Controllers.CustomerController;
using PototoTrade.DTO.User;
using PototoTrade.Service.User;

namespace PototoTrade.Controllers.User
{
    [ApiController]
    [Route("api/users")]
    public class UserController : CustomerBaseController
    {
        private readonly UserAccountService _userService;

        public UserController(UserAccountService userAccountService)
        {
            _userService = userAccountService;
        }

        [HttpGet]
        public async Task<IActionResult> GetUserList()
        {
            return MakeResponse(await _userService.GetUserList(User));
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetUser(int id)
        {
            return MakeResponse(await _userService.GetUser(id, User));
        }

        [HttpPost]
        public async Task<IActionResult> createUser(CreateUserDTO createUser)
        {
            return MakeResponse(await _userService.CreateUser(createUser, User));
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteUser(int id)
        {
            return MakeResponse(await _userService.DeleteUser(id));
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateUser(int id, UpdateUserDTO updateUserDto)
        {
            return MakeResponse(await _userService.UpdateUser(id, updateUserDto, User));
        }

        [HttpGet("profile/{id}")]
        public async Task<IActionResult> ProfileUser(int id)
        {
           return  MakeResponse(await _userService.GetUserProfile(id));  
        }

    }
}