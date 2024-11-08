using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PototoTrade.Controllers.CustomerController;
using PototoTrade.DTO.Role;
using PototoTrade.Service.Role;

//only for the super admin 
namespace PototoTrade.Controllers.Role
{
    [Route("api/roles")]
    [ApiController]
    [Authorize(Roles = "SuperAdmin" )]
    public class RoleController : CustomerBaseController
    {
        RoleService _roleService;

        public RoleController(RoleService roleService)
        {
            _roleService = roleService;
        }

        [HttpGet]
        public async Task<IActionResult> GetRolesList([FromQuery] bool includeUsers = true)
        {
            return MakeResponse(await _roleService.GetRolesList(User,includeUsers));
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetRole(int id)
        {
            return MakeResponse(await _roleService.GetRole(id, User));
        }


        [HttpPost]
        public async Task<IActionResult> CreateRole(CreateRoleDTO rolePer)
        {
            return MakeResponse(await _roleService.CreateRole(rolePer, User));
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteRole(int id)
        {
            return  MakeResponse(await _roleService.DeleteRole(id, User));
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateRole(int id, UpdateRoleDTO rolePer)
        {
            return MakeResponse(await _roleService.UpdateRole(id, rolePer, User)); 
        }
    }
}
