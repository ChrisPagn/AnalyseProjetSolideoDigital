using System.Security.Claims;
using Api.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using Shared.Dtos.Auth;

namespace Api.Controllers;

[ApiController]
[Route("api/auth")]
public class AuthController(AuthService authService) : ControllerBase
{
    [HttpPost("login")]
    [EnableRateLimiting("login")]
    public async Task<ActionResult<CurrentUserDto>> Login(LoginRequestDto dto)
    {
        var resultat = await authService.ConnecterAsync(dto);

        return resultat switch
        {
            ResultatConnexion.Succes => Ok(new CurrentUserDto(true, dto.Email)),
            ResultatConnexion.CompteBloque => StatusCode(StatusCodes.Status423Locked,
                "Compte temporairement bloqué suite à plusieurs tentatives échouées."),
            _ => Unauthorized("Identifiants invalides.")
        };
    }

    [HttpPost("logout")]
    [Authorize]
    public async Task<IActionResult> Logout()
    {
        await authService.DeconnecterAsync();
        return NoContent();
    }

    [HttpGet("me")]
    public ActionResult<CurrentUserDto> Me()
    {
        var estAuthentifie = User.Identity?.IsAuthenticated ?? false;
        var email = User.FindFirstValue(ClaimTypes.Email);
        return Ok(new CurrentUserDto(estAuthentifie, email));
    }
}
