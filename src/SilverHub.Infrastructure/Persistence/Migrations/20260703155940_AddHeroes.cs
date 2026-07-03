using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SilverHub.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddHeroes : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "heroes",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    slug = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    display_name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    canonical_name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    rarity = table.Column<string>(type: "text", nullable: false),
                    faction = table.Column<string>(type: "text", nullable: false),
                    equip_type = table.Column<string>(type: "text", nullable: false),
                    @class = table.Column<string>(name: "class", type: "text", nullable: false),
                    moon_type = table.Column<string>(type: "text", nullable: false),
                    damage_type = table.Column<string>(type: "text", nullable: false),
                    boudoir = table.Column<bool>(type: "boolean", nullable: false),
                    limited = table.Column<bool>(type: "boolean", nullable: false),
                    friendship_max = table.Column<int>(type: "integer", nullable: false),
                    release_date = table.Column<DateOnly>(type: "date", nullable: true),
                    breakdown_markdown = table.Column<string>(type: "text", nullable: true),
                    has_resonantia = table.Column<bool>(type: "boolean", nullable: false),
                    resonantia_traits_json = table.Column<string>(type: "jsonb", nullable: true),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_heroes", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "synergies",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    slug = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    icon_key = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    icon2_key = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    description2_md = table.Column<string>(type: "text", nullable: true),
                    icon3_key = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    description3_md = table.Column<string>(type: "text", nullable: true),
                    created_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_synergies", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "hero_images",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    hero_id = table.Column<Guid>(type: "uuid", nullable: false),
                    kind = table.Column<string>(type: "text", nullable: false),
                    image_key = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                    variant_name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    sort_order = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_hero_images", x => x.id);
                    table.ForeignKey(
                        name: "FK_hero_images_heroes_hero_id",
                        column: x => x.hero_id,
                        principalTable: "heroes",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "hero_preferred_artifacts",
                columns: table => new
                {
                    hero_id = table.Column<Guid>(type: "uuid", nullable: false),
                    artifact_name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    artifact_slug = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    sort_order = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_hero_preferred_artifacts", x => new { x.hero_id, x.artifact_name });
                    table.ForeignKey(
                        name: "FK_hero_preferred_artifacts_heroes_hero_id",
                        column: x => x.hero_id,
                        principalTable: "heroes",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "hero_skills",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    hero_id = table.Column<Guid>(type: "uuid", nullable: false),
                    skill_type = table.Column<string>(type: "text", nullable: false),
                    name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    icon_key = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    description_md = table.Column<string>(type: "text", nullable: false),
                    cost = table.Column<int>(type: "integer", nullable: true),
                    values = table.Column<string>(type: "jsonb", nullable: true),
                    buffs = table.Column<string>(type: "jsonb", nullable: true),
                    debuffs = table.Column<string>(type: "jsonb", nullable: true),
                    sort_order = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_hero_skills", x => x.id);
                    table.ForeignKey(
                        name: "FK_hero_skills_heroes_hero_id",
                        column: x => x.hero_id,
                        principalTable: "heroes",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "hero_tags",
                columns: table => new
                {
                    hero_id = table.Column<Guid>(type: "uuid", nullable: false),
                    tag = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_hero_tags", x => new { x.hero_id, x.tag });
                    table.ForeignKey(
                        name: "FK_hero_tags_heroes_hero_id",
                        column: x => x.hero_id,
                        principalTable: "heroes",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "synergy_members",
                columns: table => new
                {
                    synergy_id = table.Column<Guid>(type: "uuid", nullable: false),
                    hero_id = table.Column<Guid>(type: "uuid", nullable: false),
                    role = table.Column<string>(type: "text", nullable: false),
                    sort_order = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_synergy_members", x => new { x.synergy_id, x.hero_id });
                    table.ForeignKey(
                        name: "FK_synergy_members_heroes_hero_id",
                        column: x => x.hero_id,
                        principalTable: "heroes",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_synergy_members_synergies_synergy_id",
                        column: x => x.synergy_id,
                        principalTable: "synergies",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_hero_images_hero_id_kind_sort_order",
                table: "hero_images",
                columns: new[] { "hero_id", "kind", "sort_order" });

            migrationBuilder.CreateIndex(
                name: "IX_hero_preferred_artifacts_hero_id_sort_order",
                table: "hero_preferred_artifacts",
                columns: new[] { "hero_id", "sort_order" });

            migrationBuilder.CreateIndex(
                name: "IX_hero_skills_hero_id_skill_type_sort_order",
                table: "hero_skills",
                columns: new[] { "hero_id", "skill_type", "sort_order" });

            migrationBuilder.CreateIndex(
                name: "IX_hero_tags_tag",
                table: "hero_tags",
                column: "tag");

            migrationBuilder.CreateIndex(
                name: "IX_heroes_slug",
                table: "heroes",
                column: "slug",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_synergies_slug",
                table: "synergies",
                column: "slug",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_synergy_members_hero_id",
                table: "synergy_members",
                column: "hero_id");

            migrationBuilder.CreateIndex(
                name: "IX_synergy_members_synergy_id_role_sort_order",
                table: "synergy_members",
                columns: new[] { "synergy_id", "role", "sort_order" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "hero_images");

            migrationBuilder.DropTable(
                name: "hero_preferred_artifacts");

            migrationBuilder.DropTable(
                name: "hero_skills");

            migrationBuilder.DropTable(
                name: "hero_tags");

            migrationBuilder.DropTable(
                name: "synergy_members");

            migrationBuilder.DropTable(
                name: "heroes");

            migrationBuilder.DropTable(
                name: "synergies");
        }
    }
}
