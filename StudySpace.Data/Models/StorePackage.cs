﻿// <auto-generated> This file has been auto generated by EF Core Power Tools. </auto-generated>
#nullable disable
using System;
using System.Collections.Generic;

namespace StudySpace.Data.Models;

public partial class StorePackage
{
    public int Id { get; set; }

    public int? StoreId { get; set; }

    public int? PackageId { get; set; }

    public DateTime? StartDate { get; set; }

    public DateTime? EndDate { get; set; }

    public decimal? Fee { get; set; }

    public double? Duration { get; set; }

    public bool? Status { get; set; }

    public virtual Package Package { get; set; }

    public virtual Store Store { get; set; }
}