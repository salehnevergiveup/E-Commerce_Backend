using System;
using Microsoft.AspNetCore.Mvc;
using PototoTrade.Service.Utilities.Response;

namespace PototoTrade.Controllers.CustomerController;

public class CustomerBaseController : ControllerBase
{
    public IActionResult MakeResponse<T>(ResponseModel<T> response)
    {
        return response.Success ? Ok(response) : BadRequest(response);
    }
}
