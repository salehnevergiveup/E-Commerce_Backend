using Microsoft.AspNetCore.Mvc;
using PototoTrade.Models;


namespace PototoTrade.Service.User
{
    public interface IUserAccountService
    {
        Task<ActionResult<List<UserAccount>>> GetUserList();
    }
}
