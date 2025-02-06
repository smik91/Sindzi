using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

namespace Sindzi.API.Controllers;

[ApiController]
[Route("[controller]")]
public class EmptyController : ControllerBase
{
    [HttpGet("test")]
    public async Task<IActionResult> Get()
    {
        return Ok(1);
    }
}
