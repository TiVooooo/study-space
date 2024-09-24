﻿// <auto-generated> This file has been auto generated by EF Core Power Tools. </auto-generated>
#nullable disable
using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;

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

    public static string GetConnectionString(string connectionStringName)
    {
        var config = new ConfigurationBuilder()
            .SetBasePath(AppDomain.CurrentDomain.BaseDirectory)
            .AddJsonFile("appsettings.json")
            .Build();

        string connectionString = config.GetConnectionString(connectionStringName);
        return connectionString;
    }
    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        => optionsBuilder.UseSqlServer(GetConnectionString("DefaultConnection"));

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
            entity.HasKey(e => e.Id).HasName("PK__Account__3214EC27F7265F3D");

            entity.ToTable("Account");

            entity.Property(e => e.Id).HasColumnName("ID");
            entity.Property(e => e.AvatarUrl).IsUnicode(false);
            entity.Property(e => e.CreatedDate).HasColumnType("datetime");
            entity.Property(e => e.Dob)
                .HasColumnType("datetime")
                .HasColumnName("DOB");
            entity.Property(e => e.Email).IsUnicode(false);
            entity.Property(e => e.Gender).HasMaxLength(4);
            entity.Property(e => e.Password).IsUnicode(false);
            entity.Property(e => e.Phone).IsUnicode(false);
            entity.Property(e => e.RoleId).HasColumnName("RoleID");

            entity.HasOne(d => d.Role).WithMany(p => p.Accounts)
                .HasForeignKey(d => d.RoleId)
                .HasConstraintName("FK__Account__RoleID__398D8EEE");
        });

        modelBuilder.Entity<Amity>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Amities__3214EC27354802C8");

            entity.Property(e => e.Id).HasColumnName("ID");
            entity.Property(e => e.Description).HasColumnType("ntext");
            entity.Property(e => e.RoomId).HasColumnName("RoomID");

            entity.HasOne(d => d.Room).WithMany(p => p.Amities)
                .HasForeignKey(d => d.RoomId)
                .HasConstraintName("FK__Amities__RoomID__5441852A");
        });

        modelBuilder.Entity<Booking>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Booking__3214EC275C6A0580");

            entity.ToTable("Booking");

            entity.Property(e => e.Id).HasColumnName("ID");
            entity.Property(e => e.BookingDate).HasColumnType("datetime");
            entity.Property(e => e.EndTime).HasColumnType("datetime");
            entity.Property(e => e.Note).HasColumnType("ntext");
            entity.Property(e => e.RoomId).HasColumnName("RoomID");
            entity.Property(e => e.StartTime).HasColumnType("datetime");
            entity.Property(e => e.Status).HasDefaultValueSql("((1))");
            entity.Property(e => e.UserId).HasColumnName("UserID");

            entity.HasOne(d => d.Room).WithMany(p => p.Bookings)
                .HasForeignKey(d => d.RoomId)
                .HasConstraintName("FK__Booking__RoomID__45F365D3");

            entity.HasOne(d => d.User).WithMany(p => p.Bookings)
                .HasForeignKey(d => d.UserId)
                .HasConstraintName("FK__Booking__UserID__44FF419A");
        });

        modelBuilder.Entity<Feedback>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Feedback__3214EC2756161DAC");

            entity.ToTable("Feedback");

            entity.Property(e => e.Id).HasColumnName("ID");
            entity.Property(e => e.BookingId).HasColumnName("BookingID");
            entity.Property(e => e.ReviewDate).HasColumnType("datetime");
            entity.Property(e => e.ReviewText).HasColumnType("ntext");
            entity.Property(e => e.UserId).HasColumnName("UserID");

            entity.HasOne(d => d.Booking).WithMany(p => p.Feedbacks)
                .HasForeignKey(d => d.BookingId)
                .HasConstraintName("FK__Feedback__Bookin__59FA5E80");

            entity.HasOne(d => d.User).WithMany(p => p.Feedbacks)
                .HasForeignKey(d => d.UserId)
                .HasConstraintName("FK__Feedback__UserID__5AEE82B9");
        });

        modelBuilder.Entity<ImageFeedback>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Image_Fe__3214EC2790971DC9");

            entity.ToTable("Image_Feedback");

            entity.Property(e => e.Id).HasColumnName("ID");
            entity.Property(e => e.FeedbackId).HasColumnName("FeedbackID");

            entity.HasOne(d => d.Feedback).WithMany(p => p.ImageFeedbacks)
                .HasForeignKey(d => d.FeedbackId)
                .HasConstraintName("FK__Image_Fee__Feedb__5DCAEF64");
        });

        modelBuilder.Entity<ImageRoom>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Image_Ro__3214EC279F000FE6");

            entity.ToTable("Image_Room");

            entity.Property(e => e.Id).HasColumnName("ID");
            entity.Property(e => e.RoomId).HasColumnName("RoomID");

            entity.HasOne(d => d.Room).WithMany(p => p.ImageRooms)
                .HasForeignKey(d => d.RoomId)
                .HasConstraintName("FK__Image_Roo__RoomI__571DF1D5");
        });

        modelBuilder.Entity<Package>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Package__3214EC27DC558C29");

            entity.ToTable("Package");

            entity.Property(e => e.Id).HasColumnName("ID");
            entity.Property(e => e.Description).HasColumnType("ntext");
        });

        modelBuilder.Entity<Room>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Room__3214EC27AD849216");

            entity.ToTable("Room");

            entity.Property(e => e.Id).HasColumnName("ID");
            entity.Property(e => e.Description).HasColumnType("ntext");
            entity.Property(e => e.HouseRule).HasColumnType("ntext");
            entity.Property(e => e.SpaceId).HasColumnName("SpaceID");
            entity.Property(e => e.StoreId).HasColumnName("StoreID");

            entity.HasOne(d => d.Space).WithMany(p => p.Rooms)
                .HasForeignKey(d => d.SpaceId)
                .HasConstraintName("FK__Room__SpaceID__412EB0B6");

            entity.HasOne(d => d.Store).WithMany(p => p.Rooms)
                .HasForeignKey(d => d.StoreId)
                .HasConstraintName("FK__Room__StoreID__403A8C7D");
        });

        modelBuilder.Entity<Space>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Space__3214EC27C1D649B2");

            entity.ToTable("Space");

            entity.Property(e => e.Id).HasColumnName("ID");
            entity.Property(e => e.Description).HasColumnType("ntext");
        });

        modelBuilder.Entity<Store>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Store__3214EC272B50A3A1");

            entity.ToTable("Store");

            entity.Property(e => e.Id).HasColumnName("ID");
            entity.Property(e => e.CloseTime).HasColumnType("datetime");
            entity.Property(e => e.CreateDate).HasColumnType("datetime");
            entity.Property(e => e.Description).HasColumnType("ntext");
            entity.Property(e => e.IsOverNight).HasColumnName("isOverNight");
            entity.Property(e => e.OpenTime).HasColumnType("datetime");
            entity.Property(e => e.ThumbnailUrl).IsUnicode(false);
        });

        modelBuilder.Entity<StorePackage>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Store_Pa__3214EC2731F6228E");

            entity.ToTable("Store_Package");

            entity.Property(e => e.Id).HasColumnName("ID");
            entity.Property(e => e.EndDate).HasColumnType("datetime");
            entity.Property(e => e.PackageId).HasColumnName("PackageID");
            entity.Property(e => e.StartDate).HasColumnType("datetime");
            entity.Property(e => e.StoreId).HasColumnName("StoreID");

            entity.HasOne(d => d.Package).WithMany(p => p.StorePackages)
                .HasForeignKey(d => d.PackageId)
                .HasConstraintName("FK__Store_Pac__Packa__5165187F");

            entity.HasOne(d => d.Store).WithMany(p => p.StorePackages)
                .HasForeignKey(d => d.StoreId)
                .HasConstraintName("FK__Store_Pac__Store__5070F446");
        });

        modelBuilder.Entity<Transaction>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__Transact__3214EC2703CA26AA");

            entity.ToTable("Transaction");

            entity.Property(e => e.Id).HasColumnName("ID");
            entity.Property(e => e.BookingId).HasColumnName("BookingID");
            entity.Property(e => e.Date).HasColumnType("datetime");
            entity.Property(e => e.PackageId).HasColumnName("PackageID");
            entity.Property(e => e.StoreId).HasColumnName("StoreID");
            entity.Property(e => e.UserId).HasColumnName("UserID");

            entity.HasOne(d => d.Booking).WithMany(p => p.Transactions)
                .HasForeignKey(d => d.BookingId)
                .HasConstraintName("FK__Transacti__Booki__4BAC3F29");

            entity.HasOne(d => d.Package).WithMany(p => p.Transactions)
                .HasForeignKey(d => d.PackageId)
                .HasConstraintName("FK__Transacti__Packa__4CA06362");

            entity.HasOne(d => d.Store).WithMany(p => p.Transactions)
                .HasForeignKey(d => d.StoreId)
                .HasConstraintName("FK__Transacti__Store__4D94879B");

            entity.HasOne(d => d.User).WithMany(p => p.Transactions)
                .HasForeignKey(d => d.UserId)
                .HasConstraintName("FK__Transacti__UserI__4AB81AF0");
        });

        modelBuilder.Entity<UserRole>(entity =>
        {
            entity.HasKey(e => e.Id).HasName("PK__UserRole__3214EC27700BE186");

            entity.ToTable("UserRole");

            entity.Property(e => e.Id).HasColumnName("ID");
            entity.Property(e => e.Description).HasColumnType("ntext");
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}