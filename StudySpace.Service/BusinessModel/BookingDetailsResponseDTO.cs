using StudySpace.Data.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StudySpace.Service.BusinessModel
{
    public class BookingDetailsResponseDTO
    {
        public int Id { get; set; }

        public int? UserId { get; set; }

        public int? RoomId { get; set; }

        public DateTime? StartTime { get; set; }

        public DateTime? EndTime { get; set; }

        public string Status { get; set; }

        public double? Fee { get; set; }

        public DateTime? BookingDate { get; set; }

        public string PaymentMethod { get; set; }

        public bool? Checkin { get; set; }

        public string Note { get; set; }

        public List<int> FeedbackIds { get; set; }

        public List<int> TransactionId { get; set; }
    }
}
