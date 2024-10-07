using Microsoft.AspNetCore.Mvc;
using StudySpace.Service.Services;

namespace StudySpace.API.Controllers
{
    [Route("[controller]")]
    [ApiController]
    public class DashboardsController : ControllerBase
    {
        private readonly IDashboardService _dashService;

        public DashboardsController(IDashboardService dashService)
        {
            _dashService ??= dashService;
        }

        [HttpGet]
        public async Task<IActionResult> GetAllDashboard()
        {
            return Ok(await _dashService.CalculateAll());
        }

        [HttpGet("store/{storeId}")]
        public async Task<IActionResult> GetStoreTotalAmount(int storeId)
        {
            return Ok(await _dashService.CalculateStoreIncome(storeId));
        }
    }
}
