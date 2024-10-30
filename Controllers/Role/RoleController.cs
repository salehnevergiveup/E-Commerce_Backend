using Microsoft.AspNetCore.Mvc;
using PototoTrade.DTO.Role;
using PototoTrade.Service.Role;

//only for the super admin 
namespace PototoTrade.Controllers.Role
{
    [Route("api/[controller]/public")]
    [ApiController]
    // [Authorize(Roles = "SuperAdmin" )]
    public class RoleController : ControllerBase
    {
        RoleService _roleService;

        public RoleController(RoleService roleService)
        {
            _roleService = roleService;
        }

        [HttpGet]
        public async Task<IActionResult> GetRolesList()
        {
            var response = await _roleService.GetRolesList(User);
            return response.Success ? Ok(response) : BadRequest(response);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetRole(int id)
        {
            var response = await _roleService.GetRole(id, User);
            return response.Success ? Ok(response) : BadRequest(response);
        }


        [HttpPost]
        public async Task<IActionResult> CreateRole(RoleAdminPermissionCreate rolePer)
        {
            var response = await _roleService.CreateRole(rolePer, User);

            return response.Success ? Ok(response) : BadRequest(response);
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteRole(int id)
        {
            var response = await _roleService.DeleteRole(id, User);
            return response.Success ? Ok(response) : BadRequest(response);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateRole(int id, UpdateViewRoleAdminPermission rolePer)
        {
            var response = await _roleService.UpdateRole(id, rolePer, User);

            return response.Success ? Ok(response) : BadRequest(response);

        }
    }
}
