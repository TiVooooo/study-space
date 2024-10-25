using Microsoft.AspNetCore.Mvc;
using Net.payOS.Types;
using StudySpace.Service.BusinessModel;
using StudySpace.Service.Helper;
using StudySpace.Service.Services;
using System.Text.Json;

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

        [HttpPost("customer")]
        public async Task<IActionResult> CreatePaymnetLinkPayOS(CreatePaymentRequest request)
        {
            return Ok(await _paymentService.CreatePaymentWithPayOS(request));
        }

        [HttpPost("supplier")]
        public async Task<IActionResult> CreatePaymentLinkPayOSSup(CreatePaymentSupplierRequest request)
        {
            return Ok(await _paymentService.CreatePaymentSupplierWithPayOS(request));
        }

        [HttpGet("{transactionId}")]
        public async Task<IActionResult> GetPayOSDetails(int transactionId)
        {
            return Ok(await _paymentService.GetPaymentDetailsPayOS(transactionId));
        }

        [HttpPost("cancelled/{transactionID}")]
        public async Task<IActionResult> CancelPayment(int transactionID, string reason)
        {
            return Ok(await _paymentService.CancelPayment(transactionID, reason));
        }

        [HttpPost("webhook")]
        public async Task PayOSWebhook([FromBody] WebhookType webhookData)
        {
            await _paymentService.ProcessWebhook(webhookData);
        }

    }
}
