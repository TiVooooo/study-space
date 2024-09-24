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

       /* [HttpPost]
        public async Task<IActionResult> CreateFeedback([FromForm] FeedbackModel feedbackRequest)
        {
            //var feedback = _mapper.Map<FeedbackRequestModel>(feedbackRequest);
            var result = await _feedbackService.Save(feedbackRequest);
            return Ok(result);
        }*/
    }

}
