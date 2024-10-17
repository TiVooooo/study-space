using Microsoft.AspNetCore.Mvc;
using StudySpace.Service.BusinessModel;
using StudySpace.Service.Services;

namespace StudySpace.API.Controllers
{
    [Route("[controller]")]
    [ApiController]
    public class PaymentsController : ControllerBase
    {
        public readonly IPaymentService _paymentService;

        public PaymentsController(IPaymentService paymentService)
        {
            _paymentService = paymentService;
        }

        [HttpPost]
        public async Task<IActionResult> CreatePaymnetLinkPayOS(CreatePaymentRequest request)
        {
            return Ok(await _paymentService.CreatePaymentWithPayOS(request));
        }

        [HttpGet("{transactionId}")]
        public async Task<IActionResult> GetPayOSDetails(int transactionId)
        {
            return Ok(await _paymentService.GetPaymentDetailsPayOS(transactionId));
        }
    }
}
