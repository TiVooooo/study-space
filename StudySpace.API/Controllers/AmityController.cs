using Microsoft.AspNetCore.Mvc;
using StudySpace.Data.Models;
using StudySpace.Service.BusinessModel;
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

        [HttpPost]
        public async Task<IActionResult> CreateAmity([FromBody] CreateAmityRequestModel model)
        {
            return Ok(await _amityService.Save(model));
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateAmity(int id, CreateAmityRequestModel model)
        {
            return Ok(await _amityService.Update(id, model));
        }

        [HttpPut("status/{id}")]
        public async Task<IActionResult> UpdateStatusAmity(int id)
        {
            return Ok(await _amityService.UnactiveAmity(id));
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteAmity(int id)
        {
            return Ok(await _amityService.DeleteById(id));
        }

        [HttpGet("supplier/{supplierId}")]
        public async Task<IActionResult> GetAmitiesBySupplierId(int supplierId)
        {
            return Ok(await _amityService.GetBySupId(supplierId));
        }

        [HttpGet("detail/{id}")]
        public async Task<IActionResult> GetDetailById(int id)
        {
            return Ok(await _amityService.GetById(id));
        }

        [HttpPost("room/{roomID}/amity/{amityId}")]
        public async Task<IActionResult> AddRoomAmity([FromRoute] int roomID , [FromRoute] int amityId, int quantity)
        {
            return Ok(await _amityService.AddAmityToRoom(roomID,amityId, quantity));
        }

        [HttpDelete("room/{roomID}/amity/{amityId}")]
        public async Task<IActionResult> DeleteAmityInRoom([FromRoute] int roomID, [FromRoute] int amityId)
        {
            return Ok(await _amityService.DeleteAmityInRoom(roomID,amityId));
        }
    }
}
