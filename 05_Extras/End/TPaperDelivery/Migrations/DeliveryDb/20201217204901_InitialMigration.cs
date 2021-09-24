using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace TPaperDelivery.Migrations.DeliveryDb
{
    public partial class InitialMigration : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "delivery");

            migrationBuilder.CreateTable(
                name: "Client",
                schema: "delivery",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(nullable: true),
                    EdiClientId = table.Column<int>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Client", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Delivery",
                schema: "delivery",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Number = table.Column<decimal>(nullable: false),
                    ClientId = table.Column<int>(nullable: false),
                    EdiOrderId = table.Column<int>(nullable: false),
                    ProductCode = table.Column<int>(nullable: false),
                    ProductId = table.Column<int>(nullable: false),
                    Notes = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Delivery", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Inventory",
                schema: "delivery",
                columns: table => new
                {
                    Id = table.Column<Guid>(nullable: false),
                    Number = table.Column<int>(nullable: false),
                    ProductId = table.Column<int>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Inventory", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Product",
                schema: "delivery",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(nullable: true),
                    ExternalCode = table.Column<int>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Product", x => x.Id);
                });

            migrationBuilder.InsertData(
                schema: "delivery",
                table: "Client",
                columns: new[] { "Id", "EdiClientId", "Name" },
                values: new object[] { 1, 1, "Test client" });

            migrationBuilder.InsertData(
                schema: "delivery",
                table: "Product",
                columns: new[] { "Id", "ExternalCode", "Name" },
                values: new object[] { 1, 1, "Sample" });

            migrationBuilder.CreateIndex(
                name: "IX_Delivery_EdiOrderId",
                schema: "delivery",
                table: "Delivery",
                column: "EdiOrderId",
                unique: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Client",
                schema: "delivery");

            migrationBuilder.DropTable(
                name: "Delivery",
                schema: "delivery");

            migrationBuilder.DropTable(
                name: "Inventory",
                schema: "delivery");

            migrationBuilder.DropTable(
                name: "Product",
                schema: "delivery");
        }
    }
}
