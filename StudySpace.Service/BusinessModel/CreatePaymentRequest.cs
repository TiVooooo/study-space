﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StudySpace.Service.BusinessModel
{
    public class CreatePaymentRequest
    {
        public int BookingId { get; set; }
        public int Amount { get; set; }
        public string Description { get; set; }

    }
}
