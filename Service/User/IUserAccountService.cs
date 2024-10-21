using Microsoft.AspNetCore.Mvc;
using PototoTrade.Models;
using PototoTrade.Models.User;


namespace PototoTrade.Service.User
{
    public interface IUserAccountService
    {
        Task<ActionResult<List<UserAccount>>> GetUserList();
    }
}
