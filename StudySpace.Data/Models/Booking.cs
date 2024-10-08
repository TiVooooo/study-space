﻿// <auto-generated> This file has been auto generated by EF Core Power Tools. </auto-generated>
#nullable disable
using System;
using System.Collections.Generic;

namespace StudySpace.Data.Models;

public partial class Booking
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

    public virtual ICollection<Feedback> Feedbacks { get; set; } = new List<Feedback>();

    public virtual Room Room { get; set; }

    public virtual ICollection<Transaction> Transactions { get; set; } = new List<Transaction>();

    public virtual Account User { get; set; }
}