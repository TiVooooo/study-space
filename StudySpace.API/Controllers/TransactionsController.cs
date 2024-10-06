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

        [HttpGet("store/{storeId}/total-amount")]
        public async Task<IActionResult> GetStoreTotalAmount(int storeId)
        {
            return Ok(await _transactionService.CalculateStoreIncome(storeId));
        }

        [HttpGet("user/{userId}")]
        public async Task<IActionResult> GetAllTransactionInUser([FromRoute]int userId)
        {
            return Ok(await _transactionService.GetAllTransactionInUser(userId));
        }

    }
}
