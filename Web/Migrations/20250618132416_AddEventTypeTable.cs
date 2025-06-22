using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Web.Migrations
{
    /// <inheritdoc />
    public partial class AddEventTypeTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "EventTypeId",
                table: "Venue",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<bool>(
                name: "IsAvailable",
                table: "Venue",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<int>(
                name: "EventTypeId",
                table: "Event",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateTable(
                name: "EventTypes",
                columns: table => new
                {
                    EventTypeID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EventTypes", x => x.EventTypeID);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Venue_EventTypeId",
                table: "Venue",
                column: "EventTypeId");

            migrationBuilder.CreateIndex(
                name: "IX_Event_EventTypeId",
                table: "Event",
                column: "EventTypeId");

            migrationBuilder.AddForeignKey(
                name: "FK_Event_EventTypes_EventTypeId",
                table: "Event",
                column: "EventTypeId",
                principalTable: "EventTypes",
                principalColumn: "EventTypeID",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Venue_EventTypes_EventTypeId",
                table: "Venue",
                column: "EventTypeId",
                principalTable: "EventTypes",
                principalColumn: "EventTypeID",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Event_EventTypes_EventTypeId",
                table: "Event");

            migrationBuilder.DropForeignKey(
                name: "FK_Venue_EventTypes_EventTypeId",
                table: "Venue");

            migrationBuilder.DropTable(
                name: "EventTypes");

            migrationBuilder.DropIndex(
                name: "IX_Venue_EventTypeId",
                table: "Venue");

            migrationBuilder.DropIndex(
                name: "IX_Event_EventTypeId",
                table: "Event");

            migrationBuilder.DropColumn(
                name: "EventTypeId",
                table: "Venue");

            migrationBuilder.DropColumn(
                name: "IsAvailable",
                table: "Venue");

            migrationBuilder.DropColumn(
                name: "EventTypeId",
                table: "Event");
        }
    }
}
