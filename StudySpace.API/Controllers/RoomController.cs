using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using StudySpace.Common;
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

        [HttpGet("without-paging")]
        public async Task<IActionResult> GetAll()
        {
            return Ok(await _roomService.GetAll());
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

        [HttpGet("detail/history/user/{roomId}")]
        public async Task<IActionResult> GetDetailBookedRoom(int roomId)
        {
            return Ok(await _roomService.GetDetailBookedRoomInUser(roomId));
        }

        [HttpPost]
        public async Task<IActionResult> CreateRoom([FromForm]CreateRoomRequestModel model)
        {
            var result = await _roomService.Save(model);
            return Ok(result);
        }

        /*
        [HttpDelete("test")]

        public async Task<IActionResult> Xoa (string url)
        {
            var result = _roomService.Xoa(url);
            return Ok(result);
        }
*/
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
        public async Task<IActionResult> UpdateRoom(int id, [FromForm] UpdateRoomModel model)
        {
            return Ok(await _roomService.Update(id, model));
        }

        [HttpGet("booked/user/{userId}")]
        public async Task<IActionResult> GetBookedRoomInUser(int userId)
        {
            return Ok(await _roomService.GetAllBookedRoomInUser(userId));
        }

        [HttpGet("booked/supplier/{supplierId}")]
        public async Task<IActionResult> GetBookedRoomInSup(int supplierId)
        {
            return Ok(await _roomService.GetAllBookedRoomInSup(supplierId));

        }

        [HttpGet("supplier/{supplierId}")]
        public async Task<IActionResult> GetAllRoom(int supplierId)
        {
            return Ok(await _roomService.GetAllRoomInSup(supplierId));
        }

        [HttpGet("filter")]
        public async Task<IActionResult> FilterRoom(
            string price
            , [FromQuery] Double[]? priceRange,
            [FromQuery] string[]? utilities,
            int pageNumber, int pageSize, string space, string location, string room, int person)
        {
            return Ok(await _roomService.FilterRoom(pageNumber,pageSize,price,priceRange,utilities,space,location,room,person));
        }

        [HttpGet("detail-by-store")]
        public async Task<IActionResult> GetRoomDetailByStore(int roomId, int storeId)
        {
            return Ok(await _roomService.GetRoomDetailInSup(storeId,roomId));
        }

    }
}
