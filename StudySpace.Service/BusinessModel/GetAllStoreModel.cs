﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StudySpace.Service.BusinessModel
{
    public class GetAllStoreModel
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public string Email { get; set; }
        public string Phone { get; set; }
        public string Address { get; set; }
        public DateTime? OpenTime { get; set; }
        public DateTime? CloseTime { get; set; }
        public bool? IsOverNight { get; set; }
        public string? Status { get; set; }
        public int TotalBookings { get; set; }
        public int TotalTransactions { get; set; }
        public int TotalBookingsInMonth { get; set; }
        public int TotalTransactionsInMonth { get; set; }
        public int TotalRooms { get; set; }
        public double StarAverage { get; set; }
    }
}
