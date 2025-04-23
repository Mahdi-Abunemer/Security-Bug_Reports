using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

[Authorize]
public class SecureController : Controller
{
    [Authorize]
    [HttpGet("/secure")]
    public IActionResult Index() => View();
}
