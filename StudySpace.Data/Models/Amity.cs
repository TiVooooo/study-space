﻿// <auto-generated> This file has been auto generated by EF Core Power Tools. </auto-generated>
#nullable disable
using System;
using System.Collections.Generic;

namespace StudySpace.Data.Models;

public partial class Amity
{
    public int Id { get; set; }

    public string Name { get; set; }

    public string Type { get; set; }

    public bool? Status { get; set; }

    public int? Quantity { get; set; }

    public string Description { get; set; }

    public virtual ICollection<RoomAmity> RoomAmities { get; set; } = new List<RoomAmity>();
}