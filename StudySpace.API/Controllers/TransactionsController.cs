using Microsoft.AspNetCore.Mvc;
using StudySpace.Service.Services;

namespace StudySpace.API.Controllers
{
    [Route("[controller]")]
    [ApiController]
    public class TransactionsController : ControllerBase
    {
        private readonly ITransactionService _transactionService;

        public TransactionsController(ITransactionService transactionService)
        {
            _transactionService ??= transactionService;
        }

        [HttpGet("user/{userId}")]
        public async Task<IActionResult> GetAllTransactionInUser([FromRoute]int userId)
        {
            return Ok(await _transactionService.GetAllTransactionInUser(userId));
        }


        [HttpGet("sup/{supId}")]
        public async Task<IActionResult> GetTransactionOfSup([FromRoute] int supId)
        {
            return Ok(await _transactionService.TransactionOfSupplier(supId));
        }

        [HttpGet("booking-room/{bookingID}")]
        public async Task<IActionResult> GetAllTransactionOfBooking([FromRoute] int bookingID)
        {
            return Ok(await _transactionService.GetTransactionOfBooking(bookingID));
        }

        [HttpGet]
        public async Task<IActionResult> GetAllTransaction()
        {
            return Ok(await _transactionService.GetAllTransaction());
        }

    }
}
