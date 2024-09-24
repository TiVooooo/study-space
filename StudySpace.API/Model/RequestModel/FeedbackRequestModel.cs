namespace StudySpace.API.Model.RequestModel
{
    public class FeedbackRequestModel
    {
        public int UserId { get; set; }

        public int BookingId { get; set; }

        public int Rating { get; set; }

        public string? ReviewText { get; set; }

        public DateTime ReviewDate { get; set; }

        public List<IFormFile> Files { get; set; }
    }
}
