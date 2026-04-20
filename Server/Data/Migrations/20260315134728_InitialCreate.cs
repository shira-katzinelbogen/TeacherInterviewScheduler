using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SchedulingService.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "InterviewSlots",
                columns: table => new
                {
                    InterviewSlotID = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    JobID = table.Column<long>(type: "bigint", nullable: false),
                    TimeStart = table.Column<DateTime>(type: "datetime2", nullable: false),
                    TimeEnd = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Place = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    InterviewType = table.Column<int>(type: "int", nullable: false),
                    SlotStatus = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_InterviewSlots", x => x.InterviewSlotID);
                });

            migrationBuilder.CreateTable(
                name: "StudentAvailability",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    StudentId = table.Column<long>(type: "bigint", nullable: false),
                    StartTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    EndTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Status = table.Column<int>(type: "int", nullable: false),
                    ReasonStudent = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    ReasonStatus = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_StudentAvailability", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ScheduledInterview",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    StudentId = table.Column<long>(type: "bigint", nullable: false),
                    InterviewSlotID = table.Column<long>(type: "bigint", nullable: false),
                    InterviewStatus = table.Column<int>(type: "int", nullable: false),
                    Comments = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ScheduledInterview", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ScheduledInterview_InterviewSlots_InterviewSlotID",
                        column: x => x.InterviewSlotID,
                        principalTable: "InterviewSlots",
                        principalColumn: "InterviewSlotID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ScheduledInterview_InterviewSlotID",
                table: "ScheduledInterview",
                column: "InterviewSlotID");

            migrationBuilder.CreateIndex(
                name: "IX_ScheduledInterview_StudentId",
                table: "ScheduledInterview",
                column: "StudentId");

            migrationBuilder.CreateIndex(
                name: "IX_StudentAvailability_StudentId",
                table: "StudentAvailability",
                column: "StudentId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ScheduledInterview");

            migrationBuilder.DropTable(
                name: "StudentAvailability");

            migrationBuilder.DropTable(
                name: "InterviewSlots");
        }
    }
}
