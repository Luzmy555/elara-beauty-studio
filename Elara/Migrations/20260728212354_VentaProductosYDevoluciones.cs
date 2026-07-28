using System;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ElaraMVC.Migrations
{
    /// <inheritdoc />
    public partial class VentaProductosYDevoluciones : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<decimal>(
                name: "PrecioVenta",
                table: "Productos",
                type: "decimal(10,2)",
                precision: 10,
                scale: 2,
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<int>(
                name: "FacturaDetalleId",
                table: "MovimientosInventario",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ComprobanteTransferenciaUrl",
                table: "Facturas",
                type: "longtext",
                nullable: true)
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.AlterColumn<int>(
                name: "ServicioId",
                table: "FacturaDetalles",
                type: "int",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AlterColumn<int>(
                name: "EmpleadoId",
                table: "FacturaDetalles",
                type: "int",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AddColumn<int>(
                name: "ProductoId",
                table: "FacturaDetalles",
                type: "int",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "Devoluciones",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    NumeroDevolucion = table.Column<string>(type: "varchar(20)", maxLength: 20, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    FacturaDetalleId = table.Column<int>(type: "int", nullable: false),
                    Cantidad = table.Column<int>(type: "int", nullable: false),
                    MontoReembolsado = table.Column<decimal>(type: "decimal(10,2)", precision: 10, scale: 2, nullable: false),
                    Motivo = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    MetodoReembolso = table.Column<int>(type: "int", nullable: false),
                    Fecha = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    ProcesadoPorUserId = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Devoluciones", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Devoluciones_FacturaDetalles_FacturaDetalleId",
                        column: x => x.FacturaDetalleId,
                        principalTable: "FacturaDetalles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateIndex(
                name: "IX_MovimientosInventario_FacturaDetalleId",
                table: "MovimientosInventario",
                column: "FacturaDetalleId");

            migrationBuilder.CreateIndex(
                name: "IX_FacturaDetalles_ProductoId",
                table: "FacturaDetalles",
                column: "ProductoId");

            migrationBuilder.CreateIndex(
                name: "IX_Devoluciones_FacturaDetalleId",
                table: "Devoluciones",
                column: "FacturaDetalleId");

            migrationBuilder.AddForeignKey(
                name: "FK_FacturaDetalles_Productos_ProductoId",
                table: "FacturaDetalles",
                column: "ProductoId",
                principalTable: "Productos",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_MovimientosInventario_FacturaDetalles_FacturaDetalleId",
                table: "MovimientosInventario",
                column: "FacturaDetalleId",
                principalTable: "FacturaDetalles",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_FacturaDetalles_Productos_ProductoId",
                table: "FacturaDetalles");

            migrationBuilder.DropForeignKey(
                name: "FK_MovimientosInventario_FacturaDetalles_FacturaDetalleId",
                table: "MovimientosInventario");

            migrationBuilder.DropTable(
                name: "Devoluciones");

            migrationBuilder.DropIndex(
                name: "IX_MovimientosInventario_FacturaDetalleId",
                table: "MovimientosInventario");

            migrationBuilder.DropIndex(
                name: "IX_FacturaDetalles_ProductoId",
                table: "FacturaDetalles");

            migrationBuilder.DropColumn(
                name: "PrecioVenta",
                table: "Productos");

            migrationBuilder.DropColumn(
                name: "FacturaDetalleId",
                table: "MovimientosInventario");

            migrationBuilder.DropColumn(
                name: "ComprobanteTransferenciaUrl",
                table: "Facturas");

            migrationBuilder.DropColumn(
                name: "ProductoId",
                table: "FacturaDetalles");

            migrationBuilder.AlterColumn<int>(
                name: "ServicioId",
                table: "FacturaDetalles",
                type: "int",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);

            migrationBuilder.AlterColumn<int>(
                name: "EmpleadoId",
                table: "FacturaDetalles",
                type: "int",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);
        }
    }
}
