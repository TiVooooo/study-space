﻿// <auto-generated> This file has been auto generated by EF Core Power Tools. </auto-generated>
#nullable disable
using System;
using System.Collections.Generic;

namespace StudySpace.Data.Models;

public partial class Transaction
{
    public int Id { get; set; }

    public int? UserId { get; set; }

    public int? BookingId { get; set; }

    public int? PackageId { get; set; }

    public int? StoreId { get; set; }

    public DateTime? Date { get; set; }

    public double? Amount { get; set; }

    public virtual Booking Booking { get; set; }

    public virtual Package Package { get; set; }

    public virtual Store Store { get; set; }

    public virtual Account User { get; set; }
}