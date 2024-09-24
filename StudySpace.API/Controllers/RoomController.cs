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
        public async Task<IActionResult> Index(int pageNumber, int pageSize)
        {
            var result = await _roomService.GetAll(pageNumber,pageSize);
            return Ok(result);
        }
    }
}
