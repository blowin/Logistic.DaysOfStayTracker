using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Logistic.DaysOfStayTracker.Migration.Migrations
{
    public partial class init : Microsoft.EntityFrameworkCore.Migrations.Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Countries",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    Name = table.Column<string>(type: "TEXT", maxLength: 64, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Countries", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Drivers",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    FirstName = table.Column<string>(type: "TEXT", maxLength: 32, nullable: false),
                    LastName = table.Column<string>(type: "TEXT", maxLength: 32, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Drivers", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "DayOfStays",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    EntryDate = table.Column<DateOnly>(type: "TEXT", nullable: false),
                    ExitDate = table.Column<DateOnly>(type: "TEXT", nullable: false),
                    DriverId = table.Column<Guid>(type: "TEXT", nullable: false),
                    EntryCountryId = table.Column<Guid>(type: "TEXT", nullable: false),
                    ExitCountryId = table.Column<Guid>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DayOfStays", x => x.Id);
                    table.ForeignKey(
                        name: "FK_DayOfStays_Countries_EntryCountryId",
                        column: x => x.EntryCountryId,
                        principalTable: "Countries",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_DayOfStays_Countries_ExitCountryId",
                        column: x => x.ExitCountryId,
                        principalTable: "Countries",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_DayOfStays_Drivers_DriverId",
                        column: x => x.DriverId,
                        principalTable: "Drivers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.InsertData(
                table: "Countries",
                columns: new[] { "Id", "Name" },
                values: new object[] { new Guid("874c46ca-e44b-4f8e-8c69-f8c26b01cad8"), "Польша" });

            migrationBuilder.InsertData(
                table: "Countries",
                columns: new[] { "Id", "Name" },
                values: new object[] { new Guid("9714975e-99a2-490f-8135-56f152d3afd3"), "Литва" });

            migrationBuilder.InsertData(
                table: "Countries",
                columns: new[] { "Id", "Name" },
                values: new object[] { new Guid("b2b4a9ec-5788-414b-a018-60602649bc83"), "Эстония" });

            migrationBuilder.InsertData(
                table: "Countries",
                columns: new[] { "Id", "Name" },
                values: new object[] { new Guid("eb675cb9-e115-4f25-82f6-2330e84a6f32"), "Латвия" });

            migrationBuilder.CreateIndex(
                name: "IX_DayOfStays_DriverId",
                table: "DayOfStays",
                column: "DriverId");

            migrationBuilder.CreateIndex(
                name: "IX_DayOfStays_EntryCountryId",
                table: "DayOfStays",
                column: "EntryCountryId");

            migrationBuilder.CreateIndex(
                name: "IX_DayOfStays_ExitCountryId",
                table: "DayOfStays",
                column: "ExitCountryId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "DayOfStays");

            migrationBuilder.DropTable(
                name: "Countries");

            migrationBuilder.DropTable(
                name: "Drivers");
        }
    }
}
