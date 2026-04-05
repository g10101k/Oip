using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Oip.Discussions.Data.Migrations.SqlServer
{
    /// <inheritdoc />
    public partial class InitialMigration_SqlServer : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "discussions");

            migrationBuilder.CreateTable(
                name: "Comment",
                schema: "discussions",
                columns: table => new
                {
                    CommentId = table.Column<long>(type: "bigint", nullable: false, comment: "Unique identifier of the comment.")
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ObjectTypeId = table.Column<long>(type: "bigint", nullable: false, comment: "Identifier of the object type associated with the comment."),
                    ObjectId = table.Column<long>(type: "bigint", nullable: false, comment: "Identifier of the object associated with the comment."),
                    Content = table.Column<string>(type: "nvarchar(4000)", maxLength: 4000, nullable: false, comment: "Content of the comment."),
                    UserId = table.Column<long>(type: "bigint", nullable: false, comment: "Identifier of the user who created the comment."),
                    CreatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false, comment: "Date and time when the comment was created."),
                    UpdatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true, comment: "Date and time when the comment was last updated."),
                    DeletedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true, comment: "Date and time when the comment was deleted."),
                    IsDeleted = table.Column<bool>(type: "bit", nullable: false, defaultValue: false, comment: "Indicates whether the comment is deleted.")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Comment", x => new { x.ObjectTypeId, x.ObjectId, x.CommentId });
                    table.UniqueConstraint("AK_Comment_CommentId", x => x.CommentId);
                },
                comment: "Represents a comment entity with content, metadata, and related associations.");

            migrationBuilder.CreateTable(
                name: "DataProtectionKeys",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    FriendlyName = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Xml = table.Column<string>(type: "nvarchar(max)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DataProtectionKeys", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Attachment",
                schema: "discussions",
                columns: table => new
                {
                    AttachmentId = table.Column<long>(type: "bigint", nullable: false, comment: "Unique identifier of the attachment.")
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CommentId = table.Column<long>(type: "bigint", nullable: false, comment: "Identifier of the comment that contains this attachment."),
                    FileName = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false, comment: "Original name of the attached file."),
                    FilePath = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false, comment: "Physical path to the attached file in storage."),
                    FileType = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false, comment: "MIME type or file extension of the attachment."),
                    FileSize = table.Column<long>(type: "bigint", nullable: false, comment: "Size of the attached file in bytes."),
                    UploadedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false, comment: "Date and time when the attachment was uploaded."),
                    StorageFileId = table.Column<Guid>(type: "uniqueidentifier", nullable: false, comment: "Unique identifier for the file in the storage system.")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Attachment", x => x.AttachmentId);
                    table.ForeignKey(
                        name: "FK_Attachment_Comment_CommentId",
                        column: x => x.CommentId,
                        principalSchema: "discussions",
                        principalTable: "Comment",
                        principalColumn: "CommentId",
                        onDelete: ReferentialAction.Cascade);
                },
                comment: "Represents an attachment entity associated with a comment.");

            migrationBuilder.CreateTable(
                name: "CommentEditHistory",
                schema: "discussions",
                columns: table => new
                {
                    CommentEditHistoryId = table.Column<long>(type: "bigint", nullable: false, comment: "Unique identifier for the comment edit history record.")
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CommentId = table.Column<long>(type: "bigint", nullable: false, comment: "Identifier of the comment that was edited."),
                    OldContent = table.Column<string>(type: "nvarchar(4000)", maxLength: 4000, nullable: false, comment: "The content of the comment before the edit."),
                    NewContent = table.Column<string>(type: "nvarchar(4000)", maxLength: 4000, nullable: false, comment: "The content of the comment after the edit."),
                    EditedByUserId = table.Column<long>(type: "bigint", nullable: false, comment: "Identifier of the user who performed the edit."),
                    EditedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false, comment: "Date and time when the edit was performed."),
                    CommentEntityCommentId = table.Column<long>(type: "bigint", nullable: true),
                    CommentEntityObjectId = table.Column<long>(type: "bigint", nullable: true),
                    CommentEntityObjectTypeId = table.Column<long>(type: "bigint", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CommentEditHistory", x => x.CommentEditHistoryId);
                    table.ForeignKey(
                        name: "FK_CommentEditHistory_Comment_CommentEntityObjectTypeId_CommentEntityObjectId_CommentEntityCommentId",
                        columns: x => new { x.CommentEntityObjectTypeId, x.CommentEntityObjectId, x.CommentEntityCommentId },
                        principalSchema: "discussions",
                        principalTable: "Comment",
                        principalColumns: new[] { "ObjectTypeId", "ObjectId", "CommentId" });
                    table.ForeignKey(
                        name: "FK_CommentEditHistory_Comment_CommentId",
                        column: x => x.CommentId,
                        principalSchema: "discussions",
                        principalTable: "Comment",
                        principalColumn: "CommentId",
                        onDelete: ReferentialAction.Cascade);
                },
                comment: "Represents the edit history of a comment, tracking changes made to comment content.");

            migrationBuilder.CreateTable(
                name: "Mention",
                schema: "discussions",
                columns: table => new
                {
                    MentionId = table.Column<long>(type: "bigint", nullable: false, comment: "Unique identifier of the mention.")
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CommentId = table.Column<long>(type: "bigint", nullable: false, comment: "Identifier of the comment containing the mention."),
                    MentionedUserId = table.Column<long>(type: "bigint", nullable: false, comment: "Identifier of the user who was mentioned."),
                    Position = table.Column<int>(type: "int", nullable: false, comment: "Position of the mention within the comment text."),
                    CreatedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false, comment: "Date and time when the mention was created."),
                    CommentEntityCommentId = table.Column<long>(type: "bigint", nullable: true),
                    CommentEntityObjectId = table.Column<long>(type: "bigint", nullable: true),
                    CommentEntityObjectTypeId = table.Column<long>(type: "bigint", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Mention", x => x.MentionId);
                    table.ForeignKey(
                        name: "FK_Mention_Comment_CommentEntityObjectTypeId_CommentEntityObjectId_CommentEntityCommentId",
                        columns: x => new { x.CommentEntityObjectTypeId, x.CommentEntityObjectId, x.CommentEntityCommentId },
                        principalSchema: "discussions",
                        principalTable: "Comment",
                        principalColumns: new[] { "ObjectTypeId", "ObjectId", "CommentId" });
                    table.ForeignKey(
                        name: "FK_Mention_Comment_CommentId",
                        column: x => x.CommentId,
                        principalSchema: "discussions",
                        principalTable: "Comment",
                        principalColumn: "CommentId",
                        onDelete: ReferentialAction.Cascade);
                },
                comment: "Represents a mention entity where a user is mentioned in a comment.");

            migrationBuilder.CreateTable(
                name: "Reaction",
                schema: "discussions",
                columns: table => new
                {
                    ReactionId = table.Column<long>(type: "bigint", nullable: false, comment: "Unique identifier of the reaction.")
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CommentId = table.Column<long>(type: "bigint", nullable: false, comment: "Identifier of the comment that was reacted to."),
                    UserId = table.Column<long>(type: "bigint", nullable: false, comment: "Identifier of the user who created the reaction."),
                    EmojiCode = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false, comment: "Emoji code representing the reaction type (e.g., Like, Heart, etc.)."),
                    ReactedAt = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false, comment: "Timestamp when the reaction was created.")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Reaction", x => x.ReactionId);
                    table.ForeignKey(
                        name: "FK_Reaction_Comment_CommentId",
                        column: x => x.CommentId,
                        principalSchema: "discussions",
                        principalTable: "Comment",
                        principalColumn: "CommentId",
                        onDelete: ReferentialAction.Cascade);
                },
                comment: "Represents a reaction entity where a user reacts to a comment with an emoji.");

            migrationBuilder.CreateIndex(
                name: "IX_Attachment_CommentId",
                schema: "discussions",
                table: "Attachment",
                column: "CommentId");

            migrationBuilder.CreateIndex(
                name: "IX_Attachment_StorageFileId",
                schema: "discussions",
                table: "Attachment",
                column: "StorageFileId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Comment_CommentId",
                schema: "discussions",
                table: "Comment",
                column: "CommentId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Comment_CreatedAt",
                schema: "discussions",
                table: "Comment",
                column: "CreatedAt");

            migrationBuilder.CreateIndex(
                name: "IX_Comment_IsDeleted",
                schema: "discussions",
                table: "Comment",
                column: "IsDeleted");

            migrationBuilder.CreateIndex(
                name: "IX_Comment_ObjectTypeId_ObjectId",
                schema: "discussions",
                table: "Comment",
                columns: new[] { "ObjectTypeId", "ObjectId" });

            migrationBuilder.CreateIndex(
                name: "IX_Comment_UserId",
                schema: "discussions",
                table: "Comment",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_CommentEditHistory_CommentEntityObjectTypeId_CommentEntityObjectId_CommentEntityCommentId",
                schema: "discussions",
                table: "CommentEditHistory",
                columns: new[] { "CommentEntityObjectTypeId", "CommentEntityObjectId", "CommentEntityCommentId" });

            migrationBuilder.CreateIndex(
                name: "IX_CommentEditHistory_CommentId",
                schema: "discussions",
                table: "CommentEditHistory",
                column: "CommentId");

            migrationBuilder.CreateIndex(
                name: "IX_CommentEditHistory_EditedAt",
                schema: "discussions",
                table: "CommentEditHistory",
                column: "EditedAt");

            migrationBuilder.CreateIndex(
                name: "IX_CommentEditHistory_EditedByUserId",
                schema: "discussions",
                table: "CommentEditHistory",
                column: "EditedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_Mention_CommentEntityObjectTypeId_CommentEntityObjectId_CommentEntityCommentId",
                schema: "discussions",
                table: "Mention",
                columns: new[] { "CommentEntityObjectTypeId", "CommentEntityObjectId", "CommentEntityCommentId" });

            migrationBuilder.CreateIndex(
                name: "IX_Mention_CommentId",
                schema: "discussions",
                table: "Mention",
                column: "CommentId");

            migrationBuilder.CreateIndex(
                name: "IX_Mention_CommentId_Position",
                schema: "discussions",
                table: "Mention",
                columns: new[] { "CommentId", "Position" });

            migrationBuilder.CreateIndex(
                name: "IX_Mention_MentionedUserId",
                schema: "discussions",
                table: "Mention",
                column: "MentionedUserId");

            migrationBuilder.CreateIndex(
                name: "IX_Reaction_CommentId",
                schema: "discussions",
                table: "Reaction",
                column: "CommentId");

            migrationBuilder.CreateIndex(
                name: "IX_Reaction_CommentId_UserId",
                schema: "discussions",
                table: "Reaction",
                columns: new[] { "CommentId", "UserId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Reaction_EmojiCode",
                schema: "discussions",
                table: "Reaction",
                column: "EmojiCode");

            migrationBuilder.CreateIndex(
                name: "IX_Reaction_UserId",
                schema: "discussions",
                table: "Reaction",
                column: "UserId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Attachment",
                schema: "discussions");

            migrationBuilder.DropTable(
                name: "CommentEditHistory",
                schema: "discussions");

            migrationBuilder.DropTable(
                name: "DataProtectionKeys");

            migrationBuilder.DropTable(
                name: "Mention",
                schema: "discussions");

            migrationBuilder.DropTable(
                name: "Reaction",
                schema: "discussions");

            migrationBuilder.DropTable(
                name: "Comment",
                schema: "discussions");
        }
    }
}
