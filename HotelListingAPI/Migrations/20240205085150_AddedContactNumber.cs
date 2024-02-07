using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace HotelListingAPI.Migrations
{
    /// <inheritdoc />
    public partial class AddedContactNumber : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: "29f7223e-e013-4fe4-b3db-fad765ae6bba");

            migrationBuilder.DeleteData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: "f3973deb-8eff-4a2b-9692-ff6722d3673c");

            migrationBuilder.AddColumn<string>(
                name: "ContactNumber",
                table: "AspNetUsers",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.InsertData(
                table: "AspNetRoles",
                columns: new[] { "Id", "ConcurrencyStamp", "Name", "NormalizedName" },
                values: new object[,]
                {
                    { "4690bd56-a55b-4033-840c-4486e906e0eb", null, "User", "USER" },
                    { "e48c218c-20df-495f-bbc5-4b2400b7ba68", null, "Administrator", "ADMINISTRATOR" }
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: "4690bd56-a55b-4033-840c-4486e906e0eb");

            migrationBuilder.DeleteData(
                table: "AspNetRoles",
                keyColumn: "Id",
                keyValue: "e48c218c-20df-495f-bbc5-4b2400b7ba68");

            migrationBuilder.DropColumn(
                name: "ContactNumber",
                table: "AspNetUsers");

            migrationBuilder.InsertData(
                table: "AspNetRoles",
                columns: new[] { "Id", "ConcurrencyStamp", "Name", "NormalizedName" },
                values: new object[,]
                {
                    { "29f7223e-e013-4fe4-b3db-fad765ae6bba", null, "Administrator", "ADMINISTRATOR" },
                    { "f3973deb-8eff-4a2b-9692-ff6722d3673c", null, "User", "USER" }
                });
        }
    }
}
