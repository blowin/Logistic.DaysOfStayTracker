﻿// <auto-generated />
using System;
using Logistic.DaysOfStayTracker.Core;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

#nullable disable

namespace Logistic.DaysOfStayTracker.Migration.Migrations
{
    [DbContext(typeof(AppDbContext))]
    partial class AppDbContextModelSnapshot : ModelSnapshot
    {
        protected override void BuildModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder.HasAnnotation("ProductVersion", "6.0.3");

            modelBuilder.Entity("Logistic.DaysOfStayTracker.Core.DayOfStays.DayOfStay", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("TEXT");

                    b.Property<Guid>("DriverId")
                        .HasColumnType("TEXT");

                    b.Property<DateOnly>("EntryDate")
                        .HasColumnType("TEXT");

                    b.Property<DateOnly>("ExitDate")
                        .HasColumnType("TEXT");

                    b.HasKey("Id");

                    b.HasIndex("DriverId");

                    b.ToTable("DayOfStays");
                });

            modelBuilder.Entity("Logistic.DaysOfStayTracker.Core.Drivers.Driver", b =>
                {
                    b.Property<Guid>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("TEXT");

                    b.Property<string>("FirstName")
                        .IsRequired()
                        .HasMaxLength(32)
                        .HasColumnType("TEXT");

                    b.Property<string>("LastName")
                        .IsRequired()
                        .HasMaxLength(32)
                        .HasColumnType("TEXT");

                    b.Property<DateOnly?>("VisaExpiryDate")
                        .HasColumnType("TEXT");

                    b.HasKey("Id");

                    b.ToTable("Drivers");
                });

            modelBuilder.Entity("Logistic.DaysOfStayTracker.Core.DayOfStays.DayOfStay", b =>
                {
                    b.HasOne("Logistic.DaysOfStayTracker.Core.Drivers.Driver", null)
                        .WithMany()
                        .HasForeignKey("DriverId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();
                });
#pragma warning restore 612, 618
        }
    }
}
