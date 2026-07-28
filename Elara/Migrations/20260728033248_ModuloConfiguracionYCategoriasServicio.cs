using System;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ElaraMVC.Migrations
{
    /// <inheritdoc />
    public partial class ModuloConfiguracionYCategoriasServicio : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "Categoria",
                table: "Servicios",
                newName: "CategoriaServicioId");

            migrationBuilder.AddColumn<bool>(
                name: "TemaOscuro",
                table: "AspNetUsers",
                type: "tinyint(1)",
                nullable: false,
                defaultValue: false);

            migrationBuilder.CreateTable(
                name: "CategoriasServicio",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    Nombre = table.Column<string>(type: "varchar(255)", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Activo = table.Column<bool>(type: "tinyint(1)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CategoriasServicio", x => x.Id);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            // Semilla de las 4 categorías que existían como enum fijo (Manicura=0,
            // Pedicura=1, NailArt=2, Extras=3), con los mismos Id 1..4 que usa el
            // UPDATE de abajo para remapear los Servicios ya guardados.
            migrationBuilder.InsertData(
                table: "CategoriasServicio",
                columns: new[] { "Id", "Nombre", "Activo" },
                values: new object[,]
                {
                    { 1, "Manicura", true },
                    { 2, "Pedicura", true },
                    { 3, "Nail Art", true },
                    { 4, "Extras", true }
                });

            // RenameColumn (arriba) dejó el valor viejo del enum (0-3) tal cual en
            // CategoriaServicioId; lo remapeamos a los Id 1-4 recién insertados
            // antes de exigir la FK.
            migrationBuilder.Sql("UPDATE `Servicios` SET `CategoriaServicioId` = `CategoriaServicioId` + 1;");

            migrationBuilder.CreateTable(
                name: "ConfiguracionNegocio",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    NombreSalon = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    LogoUrl = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Direccion = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Telefono = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    EmailContacto = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Moneda = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ConfiguracionNegocio", x => x.Id);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "HorariosNegocio",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    ConfiguracionNegocioId = table.Column<int>(type: "int", nullable: false),
                    DiaSemana = table.Column<int>(type: "int", nullable: false),
                    Abierto = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    HoraApertura = table.Column<TimeSpan>(type: "time(6)", nullable: true),
                    HoraCierre = table.Column<TimeSpan>(type: "time(6)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_HorariosNegocio", x => x.Id);
                    table.ForeignKey(
                        name: "FK_HorariosNegocio_ConfiguracionNegocio_ConfiguracionNegocioId",
                        column: x => x.ConfiguracionNegocioId,
                        principalTable: "ConfiguracionNegocio",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateIndex(
                name: "IX_Servicios_CategoriaServicioId",
                table: "Servicios",
                column: "CategoriaServicioId");

            migrationBuilder.CreateIndex(
                name: "IX_CategoriasServicio_Nombre",
                table: "CategoriasServicio",
                column: "Nombre",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_HorariosNegocio_ConfiguracionNegocioId",
                table: "HorariosNegocio",
                column: "ConfiguracionNegocioId");

            migrationBuilder.AddForeignKey(
                name: "FK_Servicios_CategoriasServicio_CategoriaServicioId",
                table: "Servicios",
                column: "CategoriaServicioId",
                principalTable: "CategoriasServicio",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Servicios_CategoriasServicio_CategoriaServicioId",
                table: "Servicios");

            migrationBuilder.DropTable(
                name: "CategoriasServicio");

            migrationBuilder.DropTable(
                name: "HorariosNegocio");

            migrationBuilder.DropTable(
                name: "ConfiguracionNegocio");

            migrationBuilder.DropIndex(
                name: "IX_Servicios_CategoriaServicioId",
                table: "Servicios");

            migrationBuilder.DropColumn(
                name: "TemaOscuro",
                table: "AspNetUsers");

            migrationBuilder.Sql("UPDATE `Servicios` SET `CategoriaServicioId` = `CategoriaServicioId` - 1;");

            migrationBuilder.RenameColumn(
                name: "CategoriaServicioId",
                table: "Servicios",
                newName: "Categoria");
        }
    }
}
