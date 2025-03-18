using Application.DTOs;
using Microsoft.AspNetCore.Mvc;

namespace Api.Controllers;

/// <summary>
/// Health check API to monitor the system status.
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class HealthController : ControllerBase
{
    /// <summary>
    /// Checks the health of the system.
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(HealthStatusResponse), StatusCodes.Status200OK)]
    public IActionResult GetHealthStatus()
    {
        return Ok(new HealthStatusResponse { Status = "Healthy", Timestamp = DateTime.UtcNow });
    }
}
