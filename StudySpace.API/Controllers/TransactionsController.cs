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

        [HttpGet("{storeId}/total-amount")]
        public async Task<IActionResult> GetStoreTotalAmount(int storeId)
        {
            return Ok(await _transactionService.CalculateStoreIncome(storeId));
        }

        [HttpGet("total-amount")]
        public async Task<IActionResult> GetTotalAmount()
        {
            return Ok(await _transactionService.CalculateTotalTransaction());
        }

        [HttpGet("monthly-total")]
        public async Task<IActionResult> GetMonthlyTotal()
        {
            return Ok(await _transactionService.CalculateMonthlyTransactions());
        }
    }
}
