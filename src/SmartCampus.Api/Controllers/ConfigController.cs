using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using SmartCampus.Configuration;

namespace SmartCampus.Api.Controllers;

[ApiController]
[Route("api/v1/config")]
public sealed class ConfigController(IOptions<BrandingSettings> brandingSettings) : ControllerBase
{
    [HttpGet("branding")]
    public ActionResult<BrandingSettings> GetBranding() => Ok(brandingSettings.Value);
}
