using System.Net;
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
        [HttpGet("getNetworksInterfaces")]
        [ProducesResponseType(typeof(List<Dictionary<string, dynamic?>>), StatusCodes.Status200OK)]
        [Produces(MediaTypeNames.Application.Json)]
        public IActionResult GetNetworksInterfaces()
        {
            List<Dictionary<string, dynamic?>> interfaces = new List<Dictionary<string, dynamic?>>();
            try
            {
                foreach (var ip in Dns.GetHostEntry(Dns.GetHostName()).AddressList)
                {
                    interfaces.Add(new Dictionary<string, dynamic?> {
                        { "address", ip.ToString() },
                        { "addressFamily", ip.AddressFamily.ToString() }
                    });
                }
            }
            catch (Exception e)
            {
                _logger.LogError(e.ToString(), e);
            }
            
            return Ok(interfaces);
        }

        [HttpGet("getRcloneDrives")]
        [ProducesResponseType(typeof(List<Dictionary<string, dynamic?>>), StatusCodes.Status200OK)]
        [Produces(MediaTypeNames.Application.Json)]
        public IActionResult GetRcloneDrives([FromQuery] string? rcloneRcPort)
        {
            if (rcloneRcPort != null)
            {
                return Ok(Rclone.GetRcloneDrives(rcloneRcPort));
            }else {
                return BadRequest();
            }   
        }

        [HttpGet("driveExists")]
        [ProducesResponseType(typeof(Dictionary<string, bool>), StatusCodes.Status200OK)]
        [Produces(MediaTypeNames.Application.Json)]
        public IActionResult DriveExist([FromQuery] string drive, [FromQuery] string? drivePath, [FromQuery] string? rcloneRcPort)
        {
            if(rcloneRcPort != null){
                return Ok(new Dictionary<string, bool> {
                    { "exists", Rclone.CheckConfiguration(rcloneRcPort, drive, drivePath) }
                });
            } else {
                return BadRequest();
            }   
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

        [HttpGet("fileExists")]
        [ProducesResponseType(typeof(Dictionary<string, bool>), StatusCodes.Status200OK)]
        [Produces(MediaTypeNames.Application.Json)]
        public IActionResult FileExists([FromQuery] string file)
        {
            try
            {
                return Ok(new Dictionary<string, bool> {
                    { "exists", System.IO.File.Exists(file) }
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