using Microsoft.AspNetCore.Mvc;
using StudySpace.Service.BusinessModel;
using StudySpace.Service.Services;

namespace StudySpace.API.Controllers
{
    [Route("/Room")]
    [ApiController]
    public class RoomController : ControllerBase
    {
        private readonly IRoomService _roomService;

        public RoomController(IRoomService roomService)
        {
            _roomService = roomService;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll(int pageNumber, int pageSize)
        {
            var result = await _roomService.GetAll(pageNumber, pageSize);
            return Ok(result);
        }

        [HttpGet("Check")]
        public async Task<IActionResult> FilterRoom(int pageNumber, int pageSize, string space, string location, string room, int person)
        {
            var result = await _roomService.SearchRooms(pageNumber, pageSize, space, location, room, person);
            return Ok(result);
        }

        [HttpGet("detail/{id}")]
        public async Task<IActionResult> GetDetailRoom(int id)
        {
            return Ok(await _roomService.GetById(id));
        }

        [HttpPost]
        public async Task<IActionResult> CreateRoom(CreateRoomRequestModel model)
        {
            var result = await _roomService.Save(model);
            return Ok(result);
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteRoom(int id)
        {
            return Ok(await _roomService.DeleteById(id));
        }

        [HttpPut("unactive/{id}")]
        public async Task<IActionResult> UnactiveRoom(int id)
        {
            return Ok(await _roomService.UnactiveRoom(id));
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateRoom(int id, [FromForm] CreateRoomRequestModel model)
        {
            return Ok(await _roomService.Update(id, model));
        }

        [HttpGet("user/{id}")]
        public async Task<IActionResult> GetBookedRoomInUser(int id)
        {
            return Ok(await _roomService.GetAllBookedRoomInUser(id));
        }

        [HttpGet("supplier/{id}")]
        public async Task<IActionResult> GetBookedRoomInSup(int id)
        {
            return Ok(await _roomService.GetAllBookedRoomInSup(id));

        }
    }
}
