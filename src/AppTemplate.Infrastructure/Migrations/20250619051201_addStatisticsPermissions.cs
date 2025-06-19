using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AppTemplate.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class addStatisticsPermissions : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterDatabase()
                .Annotation("Npgsql:PostgresExtension:postgis", ",,");

            migrationBuilder.CreateTable(
                name: "Notifications",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Action = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false, comment: "The action that triggered this notification"),
                    UserName = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false, comment: "The username of the notification owner"),
                    Entity = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false, comment: "The entity type related to this notification"),
                    EntityId = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false, comment: "The ID of the related entity"),
                    Timestamp = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP", comment: "When the notification was created"),
                    Details = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: false, comment: "Detailed description of the notification"),
                    IsRead = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false, comment: "Whether the notification has been read"),
                    AdditionalData = table.Column<string>(type: "jsonb", nullable: true, comment: "Additional JSON data associated with the notification"),
                    UserId = table.Column<Guid>(type: "uuid", nullable: false, comment: "The ID of the user who owns this notification"),
                    CreatedBy = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                    UpdatedBy = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    CreatedOnUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedOnUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    DeletedOnUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Notifications", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Notifications_AppUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AppUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.UpdateData(
                table: "AppUsers",
                keyColumn: "Id",
                keyValue: new Guid("55c7f429-0916-4d84-8b76-d45185d89aa7"),
                column: "CreatedOnUtc",
                value: new DateTime(2025, 6, 19, 5, 12, 0, 209, DateTimeKind.Utc).AddTicks(6857));

            migrationBuilder.UpdateData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: "b3398ff2-1b43-4af7-812d-eb4347eecbb8",
                columns: new[] { "ConcurrencyStamp", "PasswordHash", "SecurityStamp" },
                values: new object[] { "ebed255d-a8a6-4b56-a94f-910d1df5a229", "AQAAAAIAAYagAAAAEDNrC/Cu+4ewaCRX0gtiR1e2k3Q+OEKP6MXguXvyNm4oedJW6rFuGNOHSxYDmNeMgw==", "58a601b1-51ee-4110-aa5f-f8ac7c55e72c" });

            migrationBuilder.UpdateData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: new Guid("0eeb5f27-10fd-430a-9257-a8457107141a"),
                column: "CreatedOnUtc",
                value: new DateTime(2025, 6, 19, 5, 12, 0, 232, DateTimeKind.Utc).AddTicks(9028));

            migrationBuilder.UpdateData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: new Guid("25bb194c-ea15-4339-9f45-5a895c51b626"),
                column: "CreatedOnUtc",
                value: new DateTime(2025, 6, 19, 5, 12, 0, 232, DateTimeKind.Utc).AddTicks(9015));

            migrationBuilder.UpdateData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: new Guid("3050d953-5dcf-4eb0-a18d-a3ce62a0dd3c"),
                column: "CreatedOnUtc",
                value: new DateTime(2025, 6, 19, 5, 12, 0, 232, DateTimeKind.Utc).AddTicks(9030));

            migrationBuilder.UpdateData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: new Guid("33261a4a-c423-4876-8f15-e40068aea5ca"),
                column: "CreatedOnUtc",
                value: new DateTime(2025, 6, 19, 5, 12, 0, 232, DateTimeKind.Utc).AddTicks(7612));

            migrationBuilder.UpdateData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: new Guid("346d3cc6-ac81-42b1-8539-cd53f42b6566"),
                column: "CreatedOnUtc",
                value: new DateTime(2025, 6, 19, 5, 12, 0, 232, DateTimeKind.Utc).AddTicks(9024));

            migrationBuilder.UpdateData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: new Guid("386e40e9-da38-4d2f-8d02-ac4cbaddf760"),
                column: "CreatedOnUtc",
                value: new DateTime(2025, 6, 19, 5, 12, 0, 232, DateTimeKind.Utc).AddTicks(9026));

            migrationBuilder.UpdateData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: new Guid("559dd4ec-4d2e-479d-a0a9-5229ecc04fb4"),
                column: "CreatedOnUtc",
                value: new DateTime(2025, 6, 19, 5, 12, 0, 232, DateTimeKind.Utc).AddTicks(9018));

            migrationBuilder.UpdateData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: new Guid("940c88ad-24fe-4d86-a982-fa5ea224edba"),
                column: "CreatedOnUtc",
                value: new DateTime(2025, 6, 19, 5, 12, 0, 232, DateTimeKind.Utc).AddTicks(9022));

            migrationBuilder.UpdateData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: new Guid("9f79a54c-0b54-4de5-94b9-8582a5f32e78"),
                column: "CreatedOnUtc",
                value: new DateTime(2025, 6, 19, 5, 12, 0, 232, DateTimeKind.Utc).AddTicks(9009));

            migrationBuilder.UpdateData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: new Guid("a03a127b-9a03-46a0-b709-b6919f2598be"),
                column: "CreatedOnUtc",
                value: new DateTime(2025, 6, 19, 5, 12, 0, 232, DateTimeKind.Utc).AddTicks(9032));

            migrationBuilder.UpdateData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: new Guid("a5585e9e-ec65-431b-9bb9-9bbc1663ebb8"),
                column: "CreatedOnUtc",
                value: new DateTime(2025, 6, 19, 5, 12, 0, 232, DateTimeKind.Utc).AddTicks(9034));

            migrationBuilder.UpdateData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: new Guid("d066e4ee-6af2-4857-bd40-b9b058fa2201"),
                column: "CreatedOnUtc",
                value: new DateTime(2025, 6, 19, 5, 12, 0, 232, DateTimeKind.Utc).AddTicks(9020));

            migrationBuilder.InsertData(
                table: "Permissions",
                columns: new[] { "Id", "CreatedBy", "CreatedOnUtc", "DeletedOnUtc", "feature", "name", "UpdatedBy", "UpdatedOnUtc" },
                values: new object[] { new Guid("8f97aeb9-a9fd-470f-bae9-c9f5f0534d23"), "System", new DateTime(2025, 6, 19, 5, 12, 0, 232, DateTimeKind.Utc).AddTicks(9036), null, "statistics", "statistics:read", null, null });

            migrationBuilder.UpdateData(
                table: "Roles",
                keyColumn: "Id",
                keyValue: new Guid("4b606d86-3537-475a-aa20-26aadd8f5cfd"),
                column: "CreatedOnUtc",
                value: new DateTime(2025, 6, 19, 5, 12, 0, 237, DateTimeKind.Utc).AddTicks(9336));

            migrationBuilder.UpdateData(
                table: "Roles",
                keyColumn: "Id",
                keyValue: new Guid("5dc6ec47-5b7c-4c2b-86cd-3a671834e56e"),
                column: "CreatedOnUtc",
                value: new DateTime(2025, 6, 19, 5, 12, 0, 238, DateTimeKind.Utc).AddTicks(77));

            migrationBuilder.CreateIndex(
                name: "IX_Notifications_AdditionalData",
                table: "Notifications",
                column: "AdditionalData",
                filter: "\"DeletedOnUtc\" IS NULL")
                .Annotation("Npgsql:IndexMethod", "gin");

            migrationBuilder.CreateIndex(
                name: "IX_Notifications_Entity_EntityId",
                table: "Notifications",
                columns: new[] { "Entity", "EntityId" },
                filter: "\"DeletedOnUtc\" IS NULL")
                .Annotation("Npgsql:IndexInclude", new[] { "UserId", "Timestamp" });

            migrationBuilder.CreateIndex(
                name: "IX_Notifications_Timestamp",
                table: "Notifications",
                column: "Timestamp",
                filter: "\"DeletedOnUtc\" IS NULL");

            migrationBuilder.CreateIndex(
                name: "IX_Notifications_Unread",
                table: "Notifications",
                columns: new[] { "UserId", "Timestamp" },
                filter: "\"IsRead\" = false AND \"DeletedOnUtc\" IS NULL")
                .Annotation("Npgsql:IndexInclude", new[] { "Details", "Action" });

            migrationBuilder.CreateIndex(
                name: "IX_Notifications_UserId",
                table: "Notifications",
                column: "UserId",
                filter: "\"DeletedOnUtc\" IS NULL");

            migrationBuilder.CreateIndex(
                name: "IX_Notifications_UserId_IsRead",
                table: "Notifications",
                columns: new[] { "UserId", "IsRead" },
                filter: "\"DeletedOnUtc\" IS NULL")
                .Annotation("Npgsql:IndexInclude", new[] { "Timestamp", "Details" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Notifications");

            migrationBuilder.DeleteData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: new Guid("8f97aeb9-a9fd-470f-bae9-c9f5f0534d23"));

            migrationBuilder.AlterDatabase()
                .OldAnnotation("Npgsql:PostgresExtension:postgis", ",,");

            migrationBuilder.UpdateData(
                table: "AppUsers",
                keyColumn: "Id",
                keyValue: new Guid("55c7f429-0916-4d84-8b76-d45185d89aa7"),
                column: "CreatedOnUtc",
                value: new DateTime(2025, 3, 26, 21, 55, 15, 423, DateTimeKind.Utc).AddTicks(3910));

            migrationBuilder.UpdateData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: "b3398ff2-1b43-4af7-812d-eb4347eecbb8",
                columns: new[] { "ConcurrencyStamp", "PasswordHash", "SecurityStamp" },
                values: new object[] { "b6b1015e-397e-4251-8559-aef7fc8a6a58", "AQAAAAIAAYagAAAAEHIU5u89fB0DtgDjmowFzlj1AyWVMP1Q6bz+zueZKex/vzQIIRFTQzSOsLy2veEhjA==", "5e24ddd3-fd6e-4cee-831d-82573724aa1f" });

            migrationBuilder.UpdateData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: new Guid("0eeb5f27-10fd-430a-9257-a8457107141a"),
                column: "CreatedOnUtc",
                value: new DateTime(2025, 3, 26, 21, 55, 15, 426, DateTimeKind.Utc).AddTicks(9083));

            migrationBuilder.UpdateData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: new Guid("25bb194c-ea15-4339-9f45-5a895c51b626"),
                column: "CreatedOnUtc",
                value: new DateTime(2025, 3, 26, 21, 55, 15, 426, DateTimeKind.Utc).AddTicks(9074));

            migrationBuilder.UpdateData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: new Guid("3050d953-5dcf-4eb0-a18d-a3ce62a0dd3c"),
                column: "CreatedOnUtc",
                value: new DateTime(2025, 3, 26, 21, 55, 15, 426, DateTimeKind.Utc).AddTicks(9084));

            migrationBuilder.UpdateData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: new Guid("33261a4a-c423-4876-8f15-e40068aea5ca"),
                column: "CreatedOnUtc",
                value: new DateTime(2025, 3, 26, 21, 55, 15, 426, DateTimeKind.Utc).AddTicks(7605));

            migrationBuilder.UpdateData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: new Guid("346d3cc6-ac81-42b1-8539-cd53f42b6566"),
                column: "CreatedOnUtc",
                value: new DateTime(2025, 3, 26, 21, 55, 15, 426, DateTimeKind.Utc).AddTicks(9080));

            migrationBuilder.UpdateData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: new Guid("386e40e9-da38-4d2f-8d02-ac4cbaddf760"),
                column: "CreatedOnUtc",
                value: new DateTime(2025, 3, 26, 21, 55, 15, 426, DateTimeKind.Utc).AddTicks(9081));

            migrationBuilder.UpdateData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: new Guid("559dd4ec-4d2e-479d-a0a9-5229ecc04fb4"),
                column: "CreatedOnUtc",
                value: new DateTime(2025, 3, 26, 21, 55, 15, 426, DateTimeKind.Utc).AddTicks(9075));

            migrationBuilder.UpdateData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: new Guid("940c88ad-24fe-4d86-a982-fa5ea224edba"),
                column: "CreatedOnUtc",
                value: new DateTime(2025, 3, 26, 21, 55, 15, 426, DateTimeKind.Utc).AddTicks(9078));

            migrationBuilder.UpdateData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: new Guid("9f79a54c-0b54-4de5-94b9-8582a5f32e78"),
                column: "CreatedOnUtc",
                value: new DateTime(2025, 3, 26, 21, 55, 15, 426, DateTimeKind.Utc).AddTicks(9069));

            migrationBuilder.UpdateData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: new Guid("a03a127b-9a03-46a0-b709-b6919f2598be"),
                column: "CreatedOnUtc",
                value: new DateTime(2025, 3, 26, 21, 55, 15, 426, DateTimeKind.Utc).AddTicks(9086));

            migrationBuilder.UpdateData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: new Guid("a5585e9e-ec65-431b-9bb9-9bbc1663ebb8"),
                column: "CreatedOnUtc",
                value: new DateTime(2025, 3, 26, 21, 55, 15, 426, DateTimeKind.Utc).AddTicks(9087));

            migrationBuilder.UpdateData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: new Guid("d066e4ee-6af2-4857-bd40-b9b058fa2201"),
                column: "CreatedOnUtc",
                value: new DateTime(2025, 3, 26, 21, 55, 15, 426, DateTimeKind.Utc).AddTicks(9077));

            migrationBuilder.UpdateData(
                table: "Roles",
                keyColumn: "Id",
                keyValue: new Guid("4b606d86-3537-475a-aa20-26aadd8f5cfd"),
                column: "CreatedOnUtc",
                value: new DateTime(2025, 3, 26, 21, 55, 15, 432, DateTimeKind.Utc).AddTicks(2657));

            migrationBuilder.UpdateData(
                table: "Roles",
                keyColumn: "Id",
                keyValue: new Guid("5dc6ec47-5b7c-4c2b-86cd-3a671834e56e"),
                column: "CreatedOnUtc",
                value: new DateTime(2025, 3, 26, 21, 55, 15, 432, DateTimeKind.Utc).AddTicks(3113));
        }
    }
}
