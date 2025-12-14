using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PsMarketApp.Migrations
{
    /// <inheritdoc />
    public partial class SliderSistemi : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "SliderId",
                table: "Products",
                type: "INTEGER",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "Slider",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Baslik = table.Column<string>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Slider", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Products_SliderId",
                table: "Products",
                column: "SliderId");

            migrationBuilder.AddForeignKey(
                name: "FK_Products_Slider_SliderId",
                table: "Products",
                column: "SliderId",
                principalTable: "Slider",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Products_Slider_SliderId",
                table: "Products");

            migrationBuilder.DropTable(
                name: "Slider");

            migrationBuilder.DropIndex(
                name: "IX_Products_SliderId",
                table: "Products");

            migrationBuilder.DropColumn(
                name: "SliderId",
                table: "Products");
        }
    }
}
