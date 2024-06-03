using Microsoft.AspNetCore.Mvc;
using Unflow.WebApi.Example.Business;
using Unflow.WebApi.Example.Database.Entities;

namespace Unflow.WebApi.Example.Controllers;

[Route("test")]
public class UsersController: ControllerBase
{
    private UserService _userService; 
    
    public UsersController(UserService userService)
    {
        _userService = userService;
    }
    
    [HttpGet("1")]
    public async Task<User> Get1(CancellationToken ct)
    {
        return await _userService.CreateUserAsync(ct);
    }
}