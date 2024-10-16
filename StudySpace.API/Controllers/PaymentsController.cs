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
    }
}
