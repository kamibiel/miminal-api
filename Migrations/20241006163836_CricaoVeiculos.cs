using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace mininal_api.Migrations
{
    /// <inheritdoc />
    public partial class CricaoVeiculos : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Veiculos",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    Nome = table.Column<string>(type: "varchar(150)", maxLength: 150, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Marca = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    Ano = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Veiculos", x => x.Id);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.UpdateData(
                table: "Administradores",
                keyColumn: "Id",
                keyValue: 1,
                column: "Senha",
                value: "$2a$11$mtgEKlQc3aQF4DxJxzLQc.yLuP.XpDQn88Pp10Bg4bH9HxUrUgQ.C");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Veiculos");

            migrationBuilder.UpdateData(
                table: "Administradores",
                keyColumn: "Id",
                keyValue: 1,
                column: "Senha",
                value: "$2a$11$I8sofkmmv6vJqRMrPuVGnuhnI24qiCrL4zrUv56BCIgI9b8TT4.BC");
        }
    }
}
