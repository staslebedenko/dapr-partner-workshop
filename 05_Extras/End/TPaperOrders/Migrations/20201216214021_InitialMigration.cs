using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace TPaperOrders.Migrations
{
    public partial class InitialMigration : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Client",
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
                name: "Inventory",
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

            migrationBuilder.CreateTable(
                name: "EdiOrder",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ClientId = table.Column<int>(nullable: false),
                    Quantity = table.Column<decimal>(nullable: false),
                    ProductCode = table.Column<int>(nullable: false),
                    Notes = table.Column<string>(nullable: true),
                    DeliveryId = table.Column<int>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EdiOrder", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Delivery",
                columns: table => new
                {
                    Id = table.Column<int>(nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Number = table.Column<decimal>(nullable: false),
                    ClientId = table.Column<int>(nullable: false),
                    EdiOrderId = table.Column<int>(nullable: false),
                    ProductCode = table.Column<int>(nullable: false),
                    ProductId = table.Column<int>(nullable: false),
                    Notes = table.Column<string>(nullable: true),
                    EdiOrderId1 = table.Column<int>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Delivery", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Delivery_EdiOrder_EdiOrderId1",
                        column: x => x.EdiOrderId1,
                        principalTable: "EdiOrder",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.InsertData(
                table: "Client",
                columns: new[] { "Id", "EdiClientId", "Name" },
                values: new object[] { 1, 1, "Test client" });

            migrationBuilder.InsertData(
                table: "Product",
                columns: new[] { "Id", "ExternalCode", "Name" },
                values: new object[] { 1, 1, "Sample" });

            migrationBuilder.CreateIndex(
                name: "IX_Delivery_EdiOrderId1",
                table: "Delivery",
                column: "EdiOrderId1");

            migrationBuilder.CreateIndex(
                name: "IX_EdiOrder_DeliveryId",
                table: "EdiOrder",
                column: "DeliveryId");

            migrationBuilder.AddForeignKey(
                name: "FK_EdiOrder_Delivery_DeliveryId",
                table: "EdiOrder",
                column: "DeliveryId",
                principalTable: "Delivery",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Delivery_EdiOrder_EdiOrderId1",
                table: "Delivery");

            migrationBuilder.DropTable(
                name: "Client");

            migrationBuilder.DropTable(
                name: "Inventory");

            migrationBuilder.DropTable(
                name: "Product");

            migrationBuilder.DropTable(
                name: "EdiOrder");

            migrationBuilder.DropTable(
                name: "Delivery");
        }
    }
}
