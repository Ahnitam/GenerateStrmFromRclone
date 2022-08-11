using System.Net.Mime;
using GenerateStrmFromRclone.Manager;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace GenerateStrmFromRclone.Api
{
    [Authorize(Policy = "DefaultAuthorization")]
    [Route("GenerateStrmFromRclone")]
    [Produces(MediaTypeNames.Application.Json)]
    [ApiController]
    public class ApiController : ControllerBase
    {
        private readonly ILogger<ApiController> _logger;

        public ApiController(ILogger<ApiController> logger)
        {
            _logger = logger;
        }
        
        [HttpGet("getRcloneDrives")]
        [ProducesResponseType(typeof(List<Dictionary<string, dynamic?>>), StatusCodes.Status200OK)]
        [Produces(MediaTypeNames.Application.Json)]
        public IActionResult GetRcloneDrives([FromQuery] string rcloneRcUrl, [FromQuery] string? rcloneAuth)
        {
            return Ok(Rclone.GetRcloneDrives(rcloneRcUrl, rcloneAuth));   
        }

        [HttpGet("driveExists")]
        [ProducesResponseType(typeof(Dictionary<string, bool>), StatusCodes.Status200OK)]
        [Produces(MediaTypeNames.Application.Json)]
        public IActionResult DriveExist([FromQuery] string rcloneRcUrl, [FromQuery] string? rcloneAuth, [FromQuery] string drive, [FromQuery] string? drivePath)
        {
            return Ok(new Dictionary<string, bool> {
                { "exists", Rclone.CheckConfiguration(rcloneRcUrl, rcloneAuth, drive, drivePath) }
            });   
        }

        [HttpGet("folderExists")]
        [ProducesResponseType(typeof(Dictionary<string, bool>), StatusCodes.Status200OK)]
        [Produces(MediaTypeNames.Application.Json)]
        public IActionResult FolderExists([FromQuery] string folder)
        {
            try
            {
                return Ok(new Dictionary<string, bool> {
                    { "exists", Directory.Exists(folder) }
                });
            }
            catch (System.Exception)
            {
                return Ok(new Dictionary<string, bool> {
                    { "exists", false }
                });
            }
        }
    }
}