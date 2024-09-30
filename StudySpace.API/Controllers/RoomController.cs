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

        [HttpGet("available")]
        public async Task<IActionResult> FilterRoom(int pageNumber, int pageSize, string space, string location, string room, int person)
        {
            var result = await _roomService.SearchRooms(pageNumber, pageSize, space, location, room, person);
            return Ok(result);
        }

        [HttpGet("detail/{roomId}")]
        public async Task<IActionResult> GetDetailRoom(int roomId)
        {
            return Ok(await _roomService.GetById(roomId));
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

        [HttpPut("status/{id}")]
        public async Task<IActionResult> UnactiveRoom(int id)
        {
            return Ok(await _roomService.UnactiveRoom(id));
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateRoom(int id, [FromForm] CreateRoomRequestModel model)
        {
            return Ok(await _roomService.Update(id, model));
        }

        [HttpGet("user/{userId}")]
        public async Task<IActionResult> GetBookedRoomInUser(int userId)
        {
            return Ok(await _roomService.GetAllBookedRoomInUser(userId));
        }

        [HttpGet("supplier/{supplierId}")]
        public async Task<IActionResult> GetBookedRoomInSup(int supplierId)
        {
            return Ok(await _roomService.GetAllBookedRoomInSup(supplierId));

        }


        [HttpGet("filter/{price}")]
        public async Task<IActionResult> FilterRoom(
            [FromRoute] string price
            , [FromQuery] Double[]? priceRange,
            [FromQuery] string[]? utilities)
        {
            return Ok(await _roomService.FilterRoom(price,priceRange,utilities));
        }

    }
}
