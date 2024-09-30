using Microsoft.AspNetCore.Mvc;
using StudySpace.Service.Services;

namespace StudySpace.API.Controllers
{
    [Route("/Amity")]
    [ApiController]
    public class AmityController : ControllerBase
    {
        private readonly IAmityService _amityService;

        public AmityController(IAmityService amityService)
        {
            _amityService = amityService;
        }

        [HttpGet("name")]
        public async Task<IActionResult> GetAll()
        {
            var result = await _amityService.GetAllAmities();
            return Ok(result);
        }
    }
}
