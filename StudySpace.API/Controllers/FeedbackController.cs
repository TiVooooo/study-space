using Microsoft.AspNetCore.Mvc;
using StudySpace.API.Model.RequestModel;
using StudySpace.Service.BusinessModel;
using StudySpace.Service.Services;

namespace StudySpace.API.Controllers
{
    [Route("/Feedback")]
    [ApiController]
    public class FeedbackController : ControllerBase
    {
        private readonly IFeedbackService _feedbackService;
        public FeedbackController(IFeedbackService feedbackService)
        {
            _feedbackService =  feedbackService;
        }

        [HttpPost]
        public async Task<IActionResult> CreateFeedback([FromForm] Service.BusinessModel.FeedbackRequestModel feedbackRequest)
        {
            var result = await _feedbackService.Save(feedbackRequest);
            return Ok(result);
        }

        [HttpGet("detail/room/{id}")]
        public async Task<IActionResult> GetDetailFeedback(int id, int pageNumber, int pageSize)
        {
            return Ok(await _feedbackService.GetFeedback(id, pageNumber, pageSize));
        }

        [HttpGet("all/supplier/{supplierId}")]
        public async Task<IActionResult> GetAllFeedbackOfStore([FromRoute]int supplierId)
        {
            return Ok(await _feedbackService.GetAllFeedbackOfStore(supplierId));
        }

        [HttpGet("all/room/{id}")]
        public async Task<IActionResult> GetAllFeedbacks(int id)
        {
            return Ok(await _feedbackService.GetAllImageFeedbackOfRoom(id));
        }

        [HttpGet("detail/{id}")]
        public async Task<IActionResult> GetFeedbackDetail(int id)
        {
            return Ok(await _feedbackService.GetById(id));
        }
    }

}
