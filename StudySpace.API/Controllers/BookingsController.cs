using Microsoft.AspNetCore.Mvc;
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

        [HttpGet("all/{supplierId}")]
        public async Task<IActionResult> GetALlBooingInSup([FromRoute] int supplierId)
        {
            return Ok(await _bookingService.GetAllBookingInSup(supplierId));
        }
    }
}
