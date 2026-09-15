using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SecondBrain.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddConceptRelations : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "concept_relations",
                columns: table => new
                {
                    SourceConceptId = table.Column<Guid>(type: "uuid", nullable: false),
                    TargetConceptId = table.Column<Guid>(type: "uuid", nullable: false),
                    Type = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_concept_relations", x => new { x.SourceConceptId, x.TargetConceptId, x.Type });
                    table.ForeignKey(
                        name: "FK_concept_relations_concepts_SourceConceptId",
                        column: x => x.SourceConceptId,
                        principalTable: "concepts",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_concept_relations_concepts_TargetConceptId",
                        column: x => x.TargetConceptId,
                        principalTable: "concepts",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_concept_relations_TargetConceptId",
                table: "concept_relations",
                column: "TargetConceptId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "concept_relations");
        }
    }
}
