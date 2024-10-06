using Microsoft.AspNetCore.Mvc;
using StudySpace.Service.BusinessModel;
using StudySpace.Service.Services;

namespace StudySpace.API.Controllers
{
    [Route("[controller]")]
    [ApiController]
    public class BookingsController : Controller
    {
        private readonly IBookingService _bookingService;

        public BookingsController(IBookingService bookingService)
        {
            _bookingService = bookingService;
        }

        [HttpGet]
        public async Task<IActionResult> GetAllBookings()
        {
            return Ok(await _bookingService.GetAll());
        }

        [HttpGet("all/{supplierId}")]
        public async Task<IActionResult> GetALlBooingInSup([FromRoute] int supplierId)
        {
            return Ok(await _bookingService.GetAllBookingInSup(supplierId));
        }

        [HttpGet("details/{id}")]
        public async Task<IActionResult> GetBookingDetail(int id)
        {
            return Ok(await _bookingService.GetById(id));
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteById(int id)
        {
            return Ok(await _bookingService.DeleteById(id));
        }

        [HttpPost]
        public async Task<IActionResult> CreateBooking(CreateBookingRequestModel model)
        {
            return Ok(await _bookingService.Save(model));
        }
    }
}
