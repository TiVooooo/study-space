﻿// <auto-generated> This file has been auto generated by EF Core Power Tools. </auto-generated>
#nullable disable
using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;

namespace StudySpace.Data.Models;

public partial class EXE201_StudySpaceContext : DbContext
{
    public EXE201_StudySpaceContext(DbContextOptions<EXE201_StudySpaceContext> options)
        : base(options)
    {
    }

    public EXE201_StudySpaceContext()

    {
    }


    public virtual DbSet<Account> Accounts { get; set; }

    public virtual DbSet<Amity> Amities { get; set; }

    public virtual DbSet<Booking> Bookings { get; set; }

    public virtual DbSet<Feedback> Feedbacks { get; set; }

    public virtual DbSet<ImageFeedback> ImageFeedbacks { get; set; }

    public virtual DbSet<ImageRoom> ImageRooms { get; set; }

    public virtual DbSet<Package> Packages { get; set; }

    public virtual DbSet<Room> Rooms { get; set; }

    public virtual DbSet<Space> Spaces { get; set; }

    public virtual DbSet<Store> Stores { get; set; }

    public virtual DbSet<StorePackage> StorePackages { get; set; }

    public virtual DbSet<Transaction> Transactions { get; set; }

    public virtual DbSet<UserRole> UserRoles { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Account>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Account__3214EC272894645A");

            entity.ToTable("Account");

            entity.Property(e => e.Id)
                .ValueGeneratedNever()
                .HasColumnName("ID");
            entity.Property(e => e.Address).IsUnicode(false);
            entity.Property(e => e.AvatarUrl).IsUnicode(false);
            entity.Property(e => e.CreatedDate).HasColumnType("datetime");
            entity.Property(e => e.Email).IsUnicode(false);
            entity.Property(e => e.Gender).HasMaxLength(4);
            entity.Property(e => e.Password).IsUnicode(false);
            entity.Property(e => e.Phone).IsUnicode(false);
            entity.Property(e => e.RoleId).HasColumnName("RoleID");
        });

        modelBuilder.Entity<Amity>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Amities__3214EC27354EFA32");

            entity.Property(e => e.Id)
                .ValueGeneratedNever()
                .HasColumnName("ID");
            entity.Property(e => e.Description).HasColumnType("ntext");
            entity.Property(e => e.RoomId).HasColumnName("RoomID");

            entity.HasOne(d => d.Room).WithMany(p => p.Amities)
                .HasForeignKey(d => d.RoomId)
                .HasConstraintName("FK__Amities__RoomID__52593CB8");
        });

        modelBuilder.Entity<Booking>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Booking__3214EC27FFC2FA19");

            entity.ToTable("Booking");

            entity.Property(e => e.Id)
                .ValueGeneratedNever()
                .HasColumnName("ID");
            entity.Property(e => e.BookingDate).HasColumnType("datetime");
            entity.Property(e => e.EndTime).HasColumnType("datetime");
            entity.Property(e => e.Fee).HasColumnType("decimal(10, 2)");
            entity.Property(e => e.PaymentMethod).IsUnicode(false);
            entity.Property(e => e.RoomId).HasColumnName("RoomID");
            entity.Property(e => e.StartTime).HasColumnType("datetime");
            entity.Property(e => e.Status).IsUnicode(false);
            entity.Property(e => e.UserId).HasColumnName("UserID");

            entity.HasOne(d => d.Room).WithMany(p => p.Bookings)
                .HasForeignKey(d => d.RoomId)
                .HasConstraintName("FK__Booking__RoomID__440B1D61");

            entity.HasOne(d => d.User).WithMany(p => p.Bookings)
                .HasForeignKey(d => d.UserId)
                .HasConstraintName("FK__Booking__UserID__4316F928");
        });

        modelBuilder.Entity<Feedback>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Feedback__3214EC2787C6D057");

            entity.ToTable("Feedback");

            entity.Property(e => e.Id)
                .ValueGeneratedNever()
                .HasColumnName("ID");
            entity.Property(e => e.BookingId).HasColumnName("BookingID");
            entity.Property(e => e.ReviewDate).HasColumnType("datetime");
            entity.Property(e => e.ReviewText).HasColumnType("ntext");
            entity.Property(e => e.UserId).HasColumnName("UserID");

            entity.HasOne(d => d.Booking).WithMany(p => p.Feedbacks)
                .HasForeignKey(d => d.BookingId)
                .HasConstraintName("FK__Feedback__Bookin__5812160E");

            entity.HasOne(d => d.User).WithMany(p => p.Feedbacks)
                .HasForeignKey(d => d.UserId)
                .HasConstraintName("FK__Feedback__UserID__59063A47");
        });

        modelBuilder.Entity<ImageFeedback>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Image_Fe__3214EC27BD03540A");

            entity.ToTable("Image_Feedback");

            entity.Property(e => e.Id)
                .ValueGeneratedNever()
                .HasColumnName("ID");
            entity.Property(e => e.FeedbackId).HasColumnName("FeedbackID");

            entity.HasOne(d => d.Feedback).WithMany(p => p.ImageFeedbacks)
                .HasForeignKey(d => d.FeedbackId)
                .HasConstraintName("FK__Image_Fee__Feedb__5BE2A6F2");
        });

        modelBuilder.Entity<ImageRoom>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Image_Ro__3214EC27C3B9124C");

            entity.ToTable("Image_Room");

            entity.Property(e => e.Id)
                .ValueGeneratedNever()
                .HasColumnName("ID");
            entity.Property(e => e.RoomId).HasColumnName("RoomID");

            entity.HasOne(d => d.Room).WithMany(p => p.ImageRooms)
                .HasForeignKey(d => d.RoomId)
                .HasConstraintName("FK__Image_Roo__RoomI__5535A963");
        });

        modelBuilder.Entity<Package>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Package__3214EC27ED89A869");

            entity.ToTable("Package");

            entity.Property(e => e.Id)
                .ValueGeneratedNever()
                .HasColumnName("ID");
            entity.Property(e => e.Description).HasColumnType("ntext");
            entity.Property(e => e.Fee).HasColumnType("decimal(10, 2)");
        });

        modelBuilder.Entity<Room>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Room__3214EC2762129AF5");

            entity.ToTable("Room");

            entity.Property(e => e.Id)
                .ValueGeneratedNever()
                .HasColumnName("ID");
            entity.Property(e => e.Area).HasColumnType("decimal(10, 2)");
            entity.Property(e => e.Description).HasColumnType("ntext");
            entity.Property(e => e.PricePerHour).HasColumnType("decimal(10, 2)");
            entity.Property(e => e.StoreId).HasColumnName("StoreID");

            entity.HasOne(d => d.Store).WithMany(p => p.Rooms)
                .HasForeignKey(d => d.StoreId)
                .HasConstraintName("FK__Room__StoreID__403A8C7D");
        });

        modelBuilder.Entity<Space>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Space__3214EC279F3B94F9");

            entity.ToTable("Space");

            entity.Property(e => e.Id)
                .ValueGeneratedNever()
                .HasColumnName("ID");
            entity.Property(e => e.Description).HasColumnType("ntext");
        });

        modelBuilder.Entity<Store>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Store__3214EC2711210397");

            entity.ToTable("Store");

            entity.Property(e => e.Id)
                .ValueGeneratedNever()
                .HasColumnName("ID");
            entity.Property(e => e.CreateDate).HasColumnType("datetime");
            entity.Property(e => e.Description).HasColumnType("ntext");
            entity.Property(e => e.SpaceId).HasColumnName("SpaceID");

            entity.HasOne(d => d.Space).WithMany(p => p.Stores)
                .HasForeignKey(d => d.SpaceId)
                .HasConstraintName("FK__Store__SpaceID__3D5E1FD2");
        });

        modelBuilder.Entity<StorePackage>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Store_Pa__3214EC27E1DFA5B0");

            entity.ToTable("Store_Package");

            entity.Property(e => e.Id)
                .ValueGeneratedNever()
                .HasColumnName("ID");
            entity.Property(e => e.EndDate).HasColumnType("datetime");
            entity.Property(e => e.Fee).HasColumnType("decimal(10, 2)");
            entity.Property(e => e.PackageId).HasColumnName("PackageID");
            entity.Property(e => e.StartDate).HasColumnType("datetime");
            entity.Property(e => e.StoreId).HasColumnName("StoreID");

            entity.HasOne(d => d.Package).WithMany(p => p.StorePackages)
                .HasForeignKey(d => d.PackageId)
                .HasConstraintName("FK__Store_Pac__Packa__4F7CD00D");

            entity.HasOne(d => d.Store).WithMany(p => p.StorePackages)
                .HasForeignKey(d => d.StoreId)
                .HasConstraintName("FK__Store_Pac__Store__4E88ABD4");
        });

        modelBuilder.Entity<Transaction>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Transact__3214EC278CF29F10");

            entity.ToTable("Transaction");

            entity.Property(e => e.Id)
                .ValueGeneratedNever()
                .HasColumnName("ID");
            entity.Property(e => e.Amount).HasColumnType("decimal(10, 2)");
            entity.Property(e => e.BookingId).HasColumnName("BookingID");
            entity.Property(e => e.Date).HasColumnType("datetime");
            entity.Property(e => e.PackageId).HasColumnName("PackageID");
            entity.Property(e => e.StoreId).HasColumnName("StoreID");
            entity.Property(e => e.UserId).HasColumnName("UserID");

            entity.HasOne(d => d.Booking).WithMany(p => p.Transactions)
                .HasForeignKey(d => d.BookingId)
                .HasConstraintName("FK__Transacti__Booki__49C3F6B7");

            entity.HasOne(d => d.Package).WithMany(p => p.Transactions)
                .HasForeignKey(d => d.PackageId)
                .HasConstraintName("FK__Transacti__Packa__4AB81AF0");

            entity.HasOne(d => d.Store).WithMany(p => p.Transactions)
                .HasForeignKey(d => d.StoreId)
                .HasConstraintName("FK__Transacti__Store__4BAC3F29");

            entity.HasOne(d => d.User).WithMany(p => p.Transactions)
                .HasForeignKey(d => d.UserId)
                .HasConstraintName("FK__Transacti__Amoun__48CFD27E");
        });

        modelBuilder.Entity<UserRole>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__UserRole__3214EC27F4142748");

            entity.ToTable("UserRole");

            entity.Property(e => e.Id)
                .ValueGeneratedNever()
                .HasColumnName("ID");
            entity.Property(e => e.Description).HasColumnType("ntext");
            entity.Property(e => e.UserId).HasColumnName("UserID");

            entity.HasOne(d => d.User).WithMany(p => p.UserRoles)
                .HasForeignKey(d => d.UserId)
                .HasConstraintName("FK__UserRole__UserID__38996AB5");
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}