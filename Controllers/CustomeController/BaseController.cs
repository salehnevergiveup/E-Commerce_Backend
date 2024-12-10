using System;
using Microsoft.AspNetCore.Mvc;
using PototoTrade.Service.Utilities.Response;

namespace PototoTrade.Controllers.CustomController;

public class CustomBaseController : ControllerBase
{
    public IActionResult MakeResponse<T>(ResponseModel<T> response)
    {
        return response.Success ? Ok(response) : BadRequest(response);
    }
}
