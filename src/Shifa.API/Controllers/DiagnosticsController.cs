using Microsoft.AspNetCore.Mvc;

namespace Shifa.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class DiagnosticsController : ControllerBase
{
    [HttpGet("test-exception")]
    public IActionResult TestException()
    {
        throw new Exception("Sensitive internal error.");
    }
}