using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Logistic.DaysOfStayTracker.Migration.Migrations
{
    public partial class add_visa_expiry_date : Microsoft.EntityFrameworkCore.Migrations.Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateOnly>(
                name: "VisaExpiryDate",
                table: "Drivers",
                type: "TEXT",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "VisaExpiryDate",
                table: "Drivers");
        }
    }
}
