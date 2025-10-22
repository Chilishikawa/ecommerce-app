using ECommerceApp.Api.Services;
using Microsoft.AspNetCore.Mvc;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly JwtService _jwtService;

    public AuthController(JwtService jwtService)
    {
        _jwtService = jwtService;
    }

    // Endpoint de prueba para generar un JWT
    [HttpPost("login-test")]
    public IActionResult LoginTest()
    {
        // Genera un token de prueba
        var token = _jwtService.GenerateToken("1", "test@example.com");
        return Ok(new { token });
    }
}
