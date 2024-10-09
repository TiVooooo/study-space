using Microsoft.AspNetCore.Mvc;
using StudySpace.Service.Services;

namespace StudySpace.API.Controllers
{
    [Route("Space")]
    [ApiController]
    public class SpaceController : ControllerBase
    {
        private readonly ISpaceService _spaceService;
        public SpaceController(ISpaceService spaceService)
        {
            _spaceService = spaceService;
        }

        [HttpGet("popular")]
        public async Task<IActionResult> GetAllPopularSpace()
        {
            var resutl = await _spaceService.GetSpacePopular();
            return Ok(resutl);

        }


        [HttpGet()]
        public async Task<IActionResult> GetAll()
        {
            var resutl = await _spaceService.GetAll();
            return Ok(resutl);

        }

        [HttpGet("name")]
        public async Task<IActionResult> GetALlSpaceName()
        {
            return Ok(await _spaceService.GetAllSpace());
        }
    }
}
