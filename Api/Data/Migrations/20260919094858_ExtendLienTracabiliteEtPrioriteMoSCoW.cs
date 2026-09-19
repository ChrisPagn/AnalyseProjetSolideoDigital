using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Api.Data.Migrations
{
    /// <inheritdoc />
    public partial class ExtendLienTracabiliteEtPrioriteMoSCoW : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropCheckConstraint(
                name: "CK_LienTracabilite_AuMoinsUnLien",
                table: "LiensTracabilite");

            migrationBuilder.AddColumn<int>(
                name: "InformationRegistreId",
                table: "LiensTracabilite",
                type: "INTEGER",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_LiensTracabilite_InformationRegistreId",
                table: "LiensTracabilite",
                column: "InformationRegistreId");

            migrationBuilder.AddCheckConstraint(
                name: "CK_LienTracabilite_AuMoinsUnLien",
                table: "LiensTracabilite",
                sql: "\"ProblemeId\" IS NOT NULL OR \"FonctionnaliteId\" IS NOT NULL OR \"EntiteId\" IS NOT NULL OR \"CritereAcceptationId\" IS NOT NULL OR \"InformationRegistreId\" IS NOT NULL");

            migrationBuilder.AddForeignKey(
                name: "FK_LiensTracabilite_InformationsRegistre_InformationRegistreId",
                table: "LiensTracabilite",
                column: "InformationRegistreId",
                principalTable: "InformationsRegistre",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_LiensTracabilite_InformationsRegistre_InformationRegistreId",
                table: "LiensTracabilite");

            migrationBuilder.DropIndex(
                name: "IX_LiensTracabilite_InformationRegistreId",
                table: "LiensTracabilite");

            migrationBuilder.DropCheckConstraint(
                name: "CK_LienTracabilite_AuMoinsUnLien",
                table: "LiensTracabilite");

            migrationBuilder.DropColumn(
                name: "InformationRegistreId",
                table: "LiensTracabilite");

            migrationBuilder.AddCheckConstraint(
                name: "CK_LienTracabilite_AuMoinsUnLien",
                table: "LiensTracabilite",
                sql: "\"ProblemeId\" IS NOT NULL OR \"FonctionnaliteId\" IS NOT NULL OR \"EntiteId\" IS NOT NULL OR \"CritereAcceptationId\" IS NOT NULL");
        }
    }
}
