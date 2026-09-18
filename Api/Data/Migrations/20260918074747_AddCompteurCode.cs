using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Api.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddCompteurCode : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "CompteursCode",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    ProjetId = table.Column<int>(type: "INTEGER", nullable: false),
                    Prefixe = table.Column<string>(type: "TEXT", maxLength: 10, nullable: false),
                    DernierNumero = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CompteursCode", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_CompteursCode_ProjetId_Prefixe",
                table: "CompteursCode",
                columns: new[] { "ProjetId", "Prefixe" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "CompteursCode");
        }
    }
}
