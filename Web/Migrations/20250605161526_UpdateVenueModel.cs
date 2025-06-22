using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Web.Migrations
{
    /// <inheritdoc />
    public partial class UpdateVenueModel : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Booking_Event_EventId1",
                table: "Booking");

            migrationBuilder.DropForeignKey(
                name: "FK_Booking_Venue_VenueId1",
                table: "Booking");

            migrationBuilder.DropForeignKey(
                name: "FK_Event_Venue_VenueId1",
                table: "Event");

            migrationBuilder.DropIndex(
                name: "IX_Event_VenueId1",
                table: "Event");

            migrationBuilder.DropIndex(
                name: "IX_Booking_EventId1",
                table: "Booking");

            migrationBuilder.DropIndex(
                name: "IX_Booking_VenueId1",
                table: "Booking");

            migrationBuilder.DropColumn(
                name: "VenueId1",
                table: "Event");

            migrationBuilder.DropColumn(
                name: "EventId1",
                table: "Booking");

            migrationBuilder.DropColumn(
                name: "VenueId1",
                table: "Booking");

            migrationBuilder.AlterColumn<string>(
                name: "ImageUrl",
                table: "Venue",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Description",
                table: "Event",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "ImageUrl",
                table: "Venue",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AlterColumn<string>(
                name: "Description",
                table: "Event",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AddColumn<int>(
                name: "VenueId1",
                table: "Event",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "EventId1",
                table: "Booking",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "VenueId1",
                table: "Booking",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Event_VenueId1",
                table: "Event",
                column: "VenueId1");

            migrationBuilder.CreateIndex(
                name: "IX_Booking_EventId1",
                table: "Booking",
                column: "EventId1");

            migrationBuilder.CreateIndex(
                name: "IX_Booking_VenueId1",
                table: "Booking",
                column: "VenueId1");

            migrationBuilder.AddForeignKey(
                name: "FK_Booking_Event_EventId1",
                table: "Booking",
                column: "EventId1",
                principalTable: "Event",
                principalColumn: "EventId");

            migrationBuilder.AddForeignKey(
                name: "FK_Booking_Venue_VenueId1",
                table: "Booking",
                column: "VenueId1",
                principalTable: "Venue",
                principalColumn: "VenueId");

            migrationBuilder.AddForeignKey(
                name: "FK_Event_Venue_VenueId1",
                table: "Event",
                column: "VenueId1",
                principalTable: "Venue",
                principalColumn: "VenueId");
        }
    }
}
