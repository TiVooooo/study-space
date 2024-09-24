using Microsoft.AspNetCore.Mvc;
using StudySpace.Service.Services;

namespace StudySpace.API.Controllers
{
    [Route("/room")]
    [ApiController]
    public class RoomController : ControllerBase
    {
        private readonly IRoomService _roomService;

        public RoomController(IRoomService roomService) // Change here
        {
            _roomService = roomService; // Use the injected service directly
        }

        [HttpGet]
        public async Task<IActionResult> GetAll(int pageNumber, int pageSize)
        {
            var result = await _roomService.GetAll(pageNumber,pageSize);
            return Ok(result);
        }

        [HttpGet("Check")]
        public async Task<IActionResult> FilterRoom(int pageNumber, int pageSize, string space, string location, string room, int person)
        {
            var result = await _roomService.SearchRooms(pageNumber, pageSize, space, location, room, person);
            return Ok(result);
        }
    }
}
