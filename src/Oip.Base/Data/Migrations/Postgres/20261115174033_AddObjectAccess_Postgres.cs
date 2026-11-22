using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace Oip.Base.Data.Migrations.Postgres
{
    /// <inheritdoc />
    public partial class AddObjectAccess_Postgres : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<long>(
                name: "AclSetId",
                schema: "oip",
                table: "ModuleInstance",
                type: "bigint",
                nullable: false,
                defaultValue: 0L,
                comment: "Identifier of the effective access control set of the instance.");

            migrationBuilder.CreateTable(
                name: "AccessObject",
                schema: "oip",
                columns: table => new
                {
                    ObjectTypeId = table.Column<int>(type: "integer", nullable: false, comment: "Identifier of the object type."),
                    ObjectId = table.Column<long>(type: "bigint", nullable: false, comment: "Identifier of the object within its type."),
                    ParentTypeId = table.Column<int>(type: "integer", nullable: true, comment: "Type of the parent object the rights are inherited from."),
                    ParentId = table.Column<long>(type: "bigint", nullable: true, comment: "Identifier of the parent object the rights are inherited from."),
                    OwnAclSetId = table.Column<long>(type: "bigint", nullable: true, comment: "Set with the rights assigned to the object explicitly."),
                    InheritanceBroken = table.Column<bool>(type: "boolean", nullable: false, comment: "When true the object does not inherit the rights of its parent."),
                    AclSetId = table.Column<long>(type: "bigint", nullable: false, comment: "Set with the effective rights: the inherited rights merged with the explicit ones."),
                    OwnerSubject = table.Column<string>(type: "character varying(300)", maxLength: 300, nullable: true, comment: "Subject of the user that owns the object.")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AccessObject", x => new { x.ObjectTypeId, x.ObjectId });
                },
                comment: "Securable object known to the access control: its place in the hierarchy and its access control sets.");

            migrationBuilder.CreateTable(
                name: "AccessObjectType",
                schema: "oip",
                columns: table => new
                {
                    ObjectTypeId = table.Column<int>(type: "integer", nullable: false, comment: "Identifier of the type.")
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Code = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false, comment: "Unique code of the type, for example acme.factory.")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AccessObjectType", x => x.ObjectTypeId);
                },
                comment: "Registered type of securable objects.");

            migrationBuilder.CreateTable(
                name: "AccessRight",
                schema: "oip",
                columns: table => new
                {
                    RightId = table.Column<int>(type: "integer", nullable: false, comment: "Identifier of the right.")
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Code = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false, comment: "Unique code of the right, for example approve.")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AccessRight", x => x.RightId);
                },
                comment: "Custom right of a service context. A code has one identifier in the whole context, so an inherited set means the\n            same right in a parent and in a child of another type.");

            migrationBuilder.CreateTable(
                name: "AclSet",
                schema: "oip",
                columns: table => new
                {
                    AclSetId = table.Column<long>(type: "bigint", nullable: false, comment: "Identifier of the set.")
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Hash = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false, comment: "SHA-256 of the normalized entries, used to reuse an existing set instead of creating a duplicate.")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AclSet", x => x.AclSetId);
                },
                comment: "Immutable set of access control entries shared by every object with the same effective or explicit rights.");

            migrationBuilder.CreateTable(
                name: "AccessObjectTypeRight",
                schema: "oip",
                columns: table => new
                {
                    ObjectTypeId = table.Column<int>(type: "integer", nullable: false, comment: "Identifier of the object type."),
                    RightId = table.Column<int>(type: "integer", nullable: false, comment: "Identifier of the right.")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AccessObjectTypeRight", x => new { x.ObjectTypeId, x.RightId });
                    table.ForeignKey(
                        name: "FK_AccessObjectTypeRight_AccessObjectType_ObjectTypeId",
                        column: x => x.ObjectTypeId,
                        principalSchema: "oip",
                        principalTable: "AccessObjectType",
                        principalColumn: "ObjectTypeId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_AccessObjectTypeRight_AccessRight_RightId",
                        column: x => x.RightId,
                        principalSchema: "oip",
                        principalTable: "AccessRight",
                        principalColumn: "RightId",
                        onDelete: ReferentialAction.Cascade);
                },
                comment: "Custom right available for an object type.");

            migrationBuilder.CreateTable(
                name: "AclSetEntry",
                schema: "oip",
                columns: table => new
                {
                    AclSetId = table.Column<long>(type: "bigint", nullable: false, comment: "Identifier of the set the entry belongs to."),
                    SubjectKey = table.Column<string>(type: "character varying(300)", maxLength: 300, nullable: false, comment: "Subject key, for example role:engineers, user:{sub}, client:{client_id}, everyone or owner."),
                    AllowMask = table.Column<int>(type: "integer", nullable: false, comment: "Bit mask of the granted standard rights: read, edit, delete and manage-acl."),
                    DenyMask = table.Column<int>(type: "integer", nullable: false, comment: "Bit mask of the denied standard rights. A denied right wins over a granted one.")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AclSetEntry", x => new { x.AclSetId, x.SubjectKey });
                    table.ForeignKey(
                        name: "FK_AclSetEntry_AclSet_AclSetId",
                        column: x => x.AclSetId,
                        principalSchema: "oip",
                        principalTable: "AclSet",
                        principalColumn: "AclSetId",
                        onDelete: ReferentialAction.Cascade);
                },
                comment: "Rights granted or denied to one subject within an access control set.");

            migrationBuilder.CreateTable(
                name: "AclSetEntryRight",
                schema: "oip",
                columns: table => new
                {
                    AclSetId = table.Column<long>(type: "bigint", nullable: false, comment: "Identifier of the set the entry belongs to."),
                    SubjectKey = table.Column<string>(type: "character varying(300)", maxLength: 300, nullable: false, comment: "Subject key of the entry."),
                    RightId = table.Column<int>(type: "integer", nullable: false, comment: "Identifier of the custom right."),
                    IsDeny = table.Column<bool>(type: "boolean", nullable: false, comment: "When true the right is denied, otherwise granted. A denied right wins over a granted one.")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AclSetEntryRight", x => new { x.AclSetId, x.SubjectKey, x.RightId, x.IsDeny });
                    table.ForeignKey(
                        name: "FK_AclSetEntryRight_AccessRight_RightId",
                        column: x => x.RightId,
                        principalSchema: "oip",
                        principalTable: "AccessRight",
                        principalColumn: "RightId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_AclSetEntryRight_AclSetEntry_AclSetId_SubjectKey",
                        columns: x => new { x.AclSetId, x.SubjectKey },
                        principalSchema: "oip",
                        principalTable: "AclSetEntry",
                        principalColumns: new[] { "AclSetId", "SubjectKey" },
                        onDelete: ReferentialAction.Cascade);
                },
                comment: "Custom right granted or denied to one subject within an access control set. The standard rights are kept in the\n            masks of , any other right is kept as a row, so a context has no limit of rights.");

            migrationBuilder.CreateIndex(
                name: "IX_ModuleInstance_AclSetId",
                schema: "oip",
                table: "ModuleInstance",
                column: "AclSetId");

            migrationBuilder.CreateIndex(
                name: "IX_AccessObject_AclSetId",
                schema: "oip",
                table: "AccessObject",
                column: "AclSetId");

            migrationBuilder.CreateIndex(
                name: "IX_AccessObject_ParentTypeId_ParentId",
                schema: "oip",
                table: "AccessObject",
                columns: new[] { "ParentTypeId", "ParentId" });

            migrationBuilder.CreateIndex(
                name: "IX_AccessObjectType_Code",
                schema: "oip",
                table: "AccessObjectType",
                column: "Code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_AccessObjectTypeRight_RightId",
                schema: "oip",
                table: "AccessObjectTypeRight",
                column: "RightId");

            migrationBuilder.CreateIndex(
                name: "IX_AccessRight_Code",
                schema: "oip",
                table: "AccessRight",
                column: "Code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_AclSet_Hash",
                schema: "oip",
                table: "AclSet",
                column: "Hash",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_AclSetEntry_SubjectKey_AclSetId",
                schema: "oip",
                table: "AclSetEntry",
                columns: new[] { "SubjectKey", "AclSetId" });

            migrationBuilder.CreateIndex(
                name: "IX_AclSetEntryRight_RightId_SubjectKey_AclSetId",
                schema: "oip",
                table: "AclSetEntryRight",
                columns: new[] { "RightId", "SubjectKey", "AclSetId" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AccessObject",
                schema: "oip");

            migrationBuilder.DropTable(
                name: "AccessObjectTypeRight",
                schema: "oip");

            migrationBuilder.DropTable(
                name: "AclSetEntryRight",
                schema: "oip");

            migrationBuilder.DropTable(
                name: "AccessObjectType",
                schema: "oip");

            migrationBuilder.DropTable(
                name: "AccessRight",
                schema: "oip");

            migrationBuilder.DropTable(
                name: "AclSetEntry",
                schema: "oip");

            migrationBuilder.DropTable(
                name: "AclSet",
                schema: "oip");

            migrationBuilder.DropIndex(
                name: "IX_ModuleInstance_AclSetId",
                schema: "oip",
                table: "ModuleInstance");

            migrationBuilder.DropColumn(
                name: "AclSetId",
                schema: "oip",
                table: "ModuleInstance");
        }
    }
}
