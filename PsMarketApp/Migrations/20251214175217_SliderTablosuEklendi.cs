using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PsMarketApp.Migrations
{
    /// <inheritdoc />
    public partial class SliderTablosuEklendi : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Products_Slider_SliderId",
                table: "Products");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Slider",
                table: "Slider");

            migrationBuilder.RenameTable(
                name: "Slider",
                newName: "Sliders");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Sliders",
                table: "Sliders",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Products_Sliders_SliderId",
                table: "Products",
                column: "SliderId",
                principalTable: "Sliders",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Products_Sliders_SliderId",
                table: "Products");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Sliders",
                table: "Sliders");

            migrationBuilder.RenameTable(
                name: "Sliders",
                newName: "Slider");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Slider",
                table: "Slider",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Products_Slider_SliderId",
                table: "Products",
                column: "SliderId",
                principalTable: "Slider",
                principalColumn: "Id");
        }
    }
}
