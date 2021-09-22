using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace TPaperOrders.Migrations
{
    public partial class SecondMigration : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_EdiOrder_Delivery_DeliveryId",
                table: "EdiOrder");

            migrationBuilder.DropTable(
                name: "Client");

            migrationBuilder.DropTable(
                name: "Delivery");

            migrationBuilder.DropTable(
                name: "Inventory");

            migrationBuilder.DropTable(
                name: "Product");

            migrationBuilder.DropIndex(
                name: "IX_EdiOrder_DeliveryId",
                table: "EdiOrder");

            migrationBuilder.EnsureSchema(
                name: "dbo");

            migrationBuilder.RenameTable(
                name: "EdiOrder",
                newName: "EdiOrder",
                newSchema: "dbo");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameTable(
                name: "EdiOrder",
                schema: "dbo",
                newName: "EdiOrder");

            migrationBuilder.CreateTable(
                name: "Client",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    EdiClientId = table.Column<int>(type: "int", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Client", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Delivery",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ClientId = table.Column<int>(type: "int", nullable: false),
                    EdiOrderId = table.Column<int>(type: "int", nullable: false),
                    EdiOrderId1 = table.Column<int>(type: "int", nullable: true),
                    Notes = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Number = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    ProductCode = table.Column<int>(type: "int", nullable: false),
                    ProductId = table.Column<int>(type: "int", nullable: false)
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

            migrationBuilder.CreateTable(
                name: "Inventory",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Number = table.Column<int>(type: "int", nullable: false),
                    ProductId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Inventory", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Product",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ExternalCode = table.Column<int>(type: "int", nullable: false),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Product", x => x.Id);
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
                name: "IX_EdiOrder_DeliveryId",
                table: "EdiOrder",
                column: "DeliveryId");

            migrationBuilder.CreateIndex(
                name: "IX_Delivery_EdiOrderId1",
                table: "Delivery",
                column: "EdiOrderId1");

            migrationBuilder.AddForeignKey(
                name: "FK_EdiOrder_Delivery_DeliveryId",
                table: "EdiOrder",
                column: "DeliveryId",
                principalTable: "Delivery",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
