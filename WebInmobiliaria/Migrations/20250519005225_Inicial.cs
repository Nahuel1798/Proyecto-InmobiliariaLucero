using System;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace WebInmobiliaria.Migrations
{
    /// <inheritdoc />
    public partial class Inicial : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterDatabase()
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "Inquilinos",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    Dni = table.Column<string>(type: "varchar(20)", maxLength: 20, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    NombreCompleto = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Telefono = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Email = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Inquilinos", x => x.Id);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "Propietarios",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    Dni = table.Column<string>(type: "varchar(20)", maxLength: 20, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Apellido = table.Column<string>(type: "varchar(50)", maxLength: 50, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Nombre = table.Column<string>(type: "varchar(50)", maxLength: 50, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Telefono = table.Column<string>(type: "longtext", nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Email = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Propietarios", x => x.Id);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "TiposInmueble",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    Descripcion = table.Column<string>(type: "varchar(50)", maxLength: 50, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TiposInmueble", x => x.Id);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "Usuarios",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    Email = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Clave = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Rol = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Avatar = table.Column<string>(type: "varchar(255)", maxLength: 255, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Usuarios", x => x.Id);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "Inmuebles",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    Direccion = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Uso = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    TipoInmuebleId = table.Column<int>(type: "int", nullable: false),
                    Ambientes = table.Column<int>(type: "int", nullable: false),
                    Latitud = table.Column<double>(type: "double", nullable: false),
                    Longitud = table.Column<double>(type: "double", nullable: false),
                    Precio = table.Column<decimal>(type: "decimal(65,30)", nullable: false),
                    Estado = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    PropietarioId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Inmuebles", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Inmuebles_Propietarios_PropietarioId",
                        column: x => x.PropietarioId,
                        principalTable: "Propietarios",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Inmuebles_TiposInmueble_TipoInmuebleId",
                        column: x => x.TipoInmuebleId,
                        principalTable: "TiposInmueble",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "Contratos",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    InquilinoId = table.Column<int>(type: "int", nullable: false),
                    InmuebleId = table.Column<int>(type: "int", nullable: false),
                    FechaInicio = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    FechaFin = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    MontoMensual = table.Column<decimal>(type: "decimal(65,30)", nullable: false),
                    FechaTerminacionAnticipada = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    MontoMulta = table.Column<decimal>(type: "decimal(65,30)", nullable: true),
                    UsuarioAltaId = table.Column<int>(type: "int", nullable: false),
                    UsuarioBajaId = table.Column<int>(type: "int", nullable: true),
                    InmuebleId1 = table.Column<int>(type: "int", nullable: true),
                    InquilinoId1 = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Contratos", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Contratos_Inmuebles_InmuebleId",
                        column: x => x.InmuebleId,
                        principalTable: "Inmuebles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Contratos_Inmuebles_InmuebleId1",
                        column: x => x.InmuebleId1,
                        principalTable: "Inmuebles",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_Contratos_Inquilinos_InquilinoId",
                        column: x => x.InquilinoId,
                        principalTable: "Inquilinos",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Contratos_Inquilinos_InquilinoId1",
                        column: x => x.InquilinoId1,
                        principalTable: "Inquilinos",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_Contratos_Usuarios_UsuarioAltaId",
                        column: x => x.UsuarioAltaId,
                        principalTable: "Usuarios",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Contratos_Usuarios_UsuarioBajaId",
                        column: x => x.UsuarioBajaId,
                        principalTable: "Usuarios",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "Pagos",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    FechaPago = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    Importe = table.Column<decimal>(type: "decimal(65,30)", nullable: false),
                    ContratoId = table.Column<int>(type: "int", nullable: false),
                    NumeroPeriodo = table.Column<int>(type: "int", nullable: false),
                    Concepto = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Observaciones = table.Column<string>(type: "varchar(250)", maxLength: 250, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Anulado = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    UsuarioAltaId = table.Column<int>(type: "int", nullable: false),
                    UsuarioAnulacionId = table.Column<int>(type: "int", nullable: true),
                    ContratoId1 = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Pagos", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Pagos_Contratos_ContratoId",
                        column: x => x.ContratoId,
                        principalTable: "Contratos",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Pagos_Contratos_ContratoId1",
                        column: x => x.ContratoId1,
                        principalTable: "Contratos",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_Pagos_Usuarios_UsuarioAltaId",
                        column: x => x.UsuarioAltaId,
                        principalTable: "Usuarios",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Pagos_Usuarios_UsuarioAnulacionId",
                        column: x => x.UsuarioAnulacionId,
                        principalTable: "Usuarios",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateIndex(
                name: "IX_Contratos_InmuebleId",
                table: "Contratos",
                column: "InmuebleId");

            migrationBuilder.CreateIndex(
                name: "IX_Contratos_InmuebleId1",
                table: "Contratos",
                column: "InmuebleId1");

            migrationBuilder.CreateIndex(
                name: "IX_Contratos_InquilinoId",
                table: "Contratos",
                column: "InquilinoId");

            migrationBuilder.CreateIndex(
                name: "IX_Contratos_InquilinoId1",
                table: "Contratos",
                column: "InquilinoId1");

            migrationBuilder.CreateIndex(
                name: "IX_Contratos_UsuarioAltaId",
                table: "Contratos",
                column: "UsuarioAltaId");

            migrationBuilder.CreateIndex(
                name: "IX_Contratos_UsuarioBajaId",
                table: "Contratos",
                column: "UsuarioBajaId");

            migrationBuilder.CreateIndex(
                name: "IX_Inmuebles_PropietarioId",
                table: "Inmuebles",
                column: "PropietarioId");

            migrationBuilder.CreateIndex(
                name: "IX_Inmuebles_TipoInmuebleId",
                table: "Inmuebles",
                column: "TipoInmuebleId");

            migrationBuilder.CreateIndex(
                name: "IX_Pagos_ContratoId",
                table: "Pagos",
                column: "ContratoId");

            migrationBuilder.CreateIndex(
                name: "IX_Pagos_ContratoId1",
                table: "Pagos",
                column: "ContratoId1");

            migrationBuilder.CreateIndex(
                name: "IX_Pagos_UsuarioAltaId",
                table: "Pagos",
                column: "UsuarioAltaId");

            migrationBuilder.CreateIndex(
                name: "IX_Pagos_UsuarioAnulacionId",
                table: "Pagos",
                column: "UsuarioAnulacionId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Pagos");

            migrationBuilder.DropTable(
                name: "Contratos");

            migrationBuilder.DropTable(
                name: "Inmuebles");

            migrationBuilder.DropTable(
                name: "Inquilinos");

            migrationBuilder.DropTable(
                name: "Usuarios");

            migrationBuilder.DropTable(
                name: "Propietarios");

            migrationBuilder.DropTable(
                name: "TiposInmueble");
        }
    }
}
