using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Api.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddLienPolymorpheInformationRegistre : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "EntiteReferenceId",
                table: "InformationsRegistre",
                type: "INTEGER",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "EntiteType",
                table: "InformationsRegistre",
                type: "TEXT",
                maxLength: 30,
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_InformationsRegistre_EntiteType_EntiteReferenceId",
                table: "InformationsRegistre",
                columns: new[] { "EntiteType", "EntiteReferenceId" });

            migrationBuilder.AddCheckConstraint(
                name: "CK_InformationRegistre_LienPolymorpheCoherent",
                table: "InformationsRegistre",
                sql: "(\"EntiteType\" IS NULL AND \"EntiteReferenceId\" IS NULL) OR (\"EntiteType\" IS NOT NULL AND \"EntiteReferenceId\" IS NOT NULL)");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_InformationsRegistre_EntiteType_EntiteReferenceId",
                table: "InformationsRegistre");

            migrationBuilder.DropCheckConstraint(
                name: "CK_InformationRegistre_LienPolymorpheCoherent",
                table: "InformationsRegistre");

            migrationBuilder.DropColumn(
                name: "EntiteReferenceId",
                table: "InformationsRegistre");

            migrationBuilder.DropColumn(
                name: "EntiteType",
                table: "InformationsRegistre");
        }
    }
}
