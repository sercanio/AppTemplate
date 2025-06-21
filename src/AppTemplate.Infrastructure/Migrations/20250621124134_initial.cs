using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace AppTemplate.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class initial : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterDatabase()
                .Annotation("Npgsql:PostgresExtension:postgis", ",,");

            migrationBuilder.CreateTable(
                name: "AspNetRoles",
                columns: table => new
                {
                    Id = table.Column<string>(type: "text", nullable: false),
                    Name = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    NormalizedName = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    ConcurrencyStamp = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetRoles", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "AspNetUsers",
                columns: table => new
                {
                    Id = table.Column<string>(type: "text", nullable: false),
                    UserName = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    NormalizedUserName = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    Email = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    NormalizedEmail = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    EmailConfirmed = table.Column<bool>(type: "boolean", nullable: false),
                    PasswordHash = table.Column<string>(type: "text", nullable: true),
                    SecurityStamp = table.Column<string>(type: "text", nullable: true),
                    ConcurrencyStamp = table.Column<string>(type: "text", nullable: true),
                    PhoneNumber = table.Column<string>(type: "text", nullable: true),
                    PhoneNumberConfirmed = table.Column<bool>(type: "boolean", nullable: false),
                    TwoFactorEnabled = table.Column<bool>(type: "boolean", nullable: false),
                    LockoutEnd = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                    LockoutEnabled = table.Column<bool>(type: "boolean", nullable: false),
                    AccessFailedCount = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetUsers", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "outbox_messages",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    OccurredOnUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    Type = table.Column<string>(type: "text", nullable: false),
                    Content = table.Column<string>(type: "jsonb", nullable: false),
                    ProcessedOnUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    Error = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_outbox_messages", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Permissions",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    feature = table.Column<string>(type: "text", nullable: false),
                    name = table.Column<string>(type: "text", nullable: false),
                    CreatedBy = table.Column<string>(type: "text", nullable: false),
                    UpdatedBy = table.Column<string>(type: "text", nullable: true),
                    CreatedOnUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedOnUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    DeletedOnUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Permissions", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Roles",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    name = table.Column<string>(type: "text", nullable: false),
                    is_default = table.Column<bool>(type: "boolean", nullable: false),
                    CreatedBy = table.Column<string>(type: "text", nullable: false),
                    UpdatedBy = table.Column<string>(type: "text", nullable: true),
                    CreatedOnUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedOnUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    DeletedOnUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Roles", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "AspNetRoleClaims",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    RoleId = table.Column<string>(type: "text", nullable: false),
                    ClaimType = table.Column<string>(type: "text", nullable: true),
                    ClaimValue = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetRoleClaims", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AspNetRoleClaims_AspNetRoles_RoleId",
                        column: x => x.RoleId,
                        principalTable: "AspNetRoles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AppUsers",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    in_app_notification = table.Column<bool>(type: "boolean", nullable: false),
                    email_notification = table.Column<bool>(type: "boolean", nullable: false),
                    push_notification = table.Column<bool>(type: "boolean", nullable: false),
                    identity_id = table.Column<string>(type: "text", nullable: false),
                    CreatedBy = table.Column<string>(type: "text", nullable: false),
                    UpdatedBy = table.Column<string>(type: "text", nullable: true),
                    CreatedOnUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedOnUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    DeletedOnUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AppUsers", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AppUsers_AspNetUsers_identity_id",
                        column: x => x.identity_id,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AspNetUserClaims",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    UserId = table.Column<string>(type: "text", nullable: false),
                    ClaimType = table.Column<string>(type: "text", nullable: true),
                    ClaimValue = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetUserClaims", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AspNetUserClaims_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AspNetUserLogins",
                columns: table => new
                {
                    LoginProvider = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    ProviderKey = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    ProviderDisplayName = table.Column<string>(type: "text", nullable: true),
                    UserId = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetUserLogins", x => new { x.LoginProvider, x.ProviderKey });
                    table.ForeignKey(
                        name: "FK_AspNetUserLogins_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AspNetUserRoles",
                columns: table => new
                {
                    UserId = table.Column<string>(type: "text", nullable: false),
                    RoleId = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetUserRoles", x => new { x.UserId, x.RoleId });
                    table.ForeignKey(
                        name: "FK_AspNetUserRoles_AspNetRoles_RoleId",
                        column: x => x.RoleId,
                        principalTable: "AspNetRoles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_AspNetUserRoles_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AspNetUserTokens",
                columns: table => new
                {
                    UserId = table.Column<string>(type: "text", nullable: false),
                    LoginProvider = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    Name = table.Column<string>(type: "character varying(128)", maxLength: 128, nullable: false),
                    Value = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AspNetUserTokens", x => new { x.UserId, x.LoginProvider, x.Name });
                    table.ForeignKey(
                        name: "FK_AspNetUserTokens_AspNetUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "RolePermission",
                columns: table => new
                {
                    RoleId = table.Column<Guid>(type: "uuid", nullable: false),
                    PermissionId = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RolePermission", x => new { x.RoleId, x.PermissionId });
                    table.ForeignKey(
                        name: "FK_RolePermission_Permissions_PermissionId",
                        column: x => x.PermissionId,
                        principalTable: "Permissions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_RolePermission_Roles_RoleId",
                        column: x => x.RoleId,
                        principalTable: "Roles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

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

            migrationBuilder.CreateTable(
                name: "RoleUser",
                columns: table => new
                {
                    RoleId = table.Column<Guid>(type: "uuid", nullable: false),
                    UserId = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RoleUser", x => new { x.RoleId, x.UserId });
                    table.ForeignKey(
                        name: "FK_RoleUser_AppUsers_UserId",
                        column: x => x.UserId,
                        principalTable: "AppUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_RoleUser_Roles_RoleId",
                        column: x => x.RoleId,
                        principalTable: "Roles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.InsertData(
                table: "AspNetUsers",
                columns: new[] { "Id", "AccessFailedCount", "ConcurrencyStamp", "Email", "EmailConfirmed", "LockoutEnabled", "LockoutEnd", "NormalizedEmail", "NormalizedUserName", "PasswordHash", "PhoneNumber", "PhoneNumberConfirmed", "SecurityStamp", "TwoFactorEnabled", "UserName" },
                values: new object[] { "b3398ff2-1b43-4af7-812d-eb4347eecbb8", 0, "fe973c9b-2f60-4ccc-8984-c640617ba9e2", "admin@example.com", true, false, null, "ADMIN@EXAMPLE.COM", "ADMIN", "AQAAAAIAAYagAAAAEHxV2BVA02rzy9naT7MX6uJ/56BWWw/5TD5wmxu/gXU4a6KFXj9UJdWT4SJl81Dg+w==", null, false, "9f7a3705-708c-4598-824a-21c2a5486c9f", false, "admin" });

            migrationBuilder.InsertData(
                table: "Permissions",
                columns: new[] { "Id", "CreatedBy", "CreatedOnUtc", "DeletedOnUtc", "feature", "name", "UpdatedBy", "UpdatedOnUtc" },
                values: new object[,]
                {
                    { new Guid("0eeb5f27-10fd-430a-9257-a8457107141a"), "System", new DateTime(2025, 6, 21, 12, 41, 32, 593, DateTimeKind.Utc).AddTicks(2186), null, "permissions", "permissions:read", null, null },
                    { new Guid("25bb194c-ea15-4339-9f45-5a895c51b626"), "System", new DateTime(2025, 6, 21, 12, 41, 32, 593, DateTimeKind.Utc).AddTicks(2170), null, "users", "users:update", null, null },
                    { new Guid("3050d953-5dcf-4eb0-a18d-a3ce62a0dd3c"), "System", new DateTime(2025, 6, 21, 12, 41, 32, 593, DateTimeKind.Utc).AddTicks(2188), null, "auditlogs", "auditlogs:read", null, null },
                    { new Guid("33261a4a-c423-4876-8f15-e40068aea5ca"), "System", new DateTime(2025, 6, 21, 12, 41, 32, 592, DateTimeKind.Utc).AddTicks(9419), null, "users", "users:read", null, null },
                    { new Guid("346d3cc6-ac81-42b1-8539-cd53f42b6566"), "System", new DateTime(2025, 6, 21, 12, 41, 32, 593, DateTimeKind.Utc).AddTicks(2182), null, "roles", "roles:update", null, null },
                    { new Guid("386e40e9-da38-4d2f-8d02-ac4cbaddf760"), "System", new DateTime(2025, 6, 21, 12, 41, 32, 593, DateTimeKind.Utc).AddTicks(2184), null, "roles", "roles:delete", null, null },
                    { new Guid("559dd4ec-4d2e-479d-a0a9-5229ecc04fb4"), "System", new DateTime(2025, 6, 21, 12, 41, 32, 593, DateTimeKind.Utc).AddTicks(2172), null, "users", "users:delete", null, null },
                    { new Guid("8f97aeb9-a9fd-470f-bae9-c9f5f0534d23"), "System", new DateTime(2025, 6, 21, 12, 41, 32, 593, DateTimeKind.Utc).AddTicks(2195), null, "statistics", "statistics:read", null, null },
                    { new Guid("940c88ad-24fe-4d86-a982-fa5ea224edba"), "System", new DateTime(2025, 6, 21, 12, 41, 32, 593, DateTimeKind.Utc).AddTicks(2179), null, "roles", "roles:create", null, null },
                    { new Guid("9f79a54c-0b54-4de5-94b9-8582a5f32e78"), "System", new DateTime(2025, 6, 21, 12, 41, 32, 593, DateTimeKind.Utc).AddTicks(2161), null, "users", "users:create", null, null },
                    { new Guid("a03a127b-9a03-46a0-b709-b6919f2598be"), "System", new DateTime(2025, 6, 21, 12, 41, 32, 593, DateTimeKind.Utc).AddTicks(2191), null, "notifications", "notifications:read", null, null },
                    { new Guid("a5585e9e-ec65-431b-9bb9-9bbc1663ebb8"), "System", new DateTime(2025, 6, 21, 12, 41, 32, 593, DateTimeKind.Utc).AddTicks(2193), null, "notifications", "notifications:update", null, null },
                    { new Guid("d066e4ee-6af2-4857-bd40-b9b058fa2201"), "System", new DateTime(2025, 6, 21, 12, 41, 32, 593, DateTimeKind.Utc).AddTicks(2175), null, "roles", "roles:read", null, null }
                });

            migrationBuilder.InsertData(
                table: "Roles",
                columns: new[] { "Id", "CreatedBy", "CreatedOnUtc", "DeletedOnUtc", "is_default", "name", "UpdatedBy", "UpdatedOnUtc" },
                values: new object[,]
                {
                    { new Guid("4b606d86-3537-475a-aa20-26aadd8f5cfd"), "System", new DateTime(2025, 6, 21, 12, 41, 32, 599, DateTimeKind.Utc).AddTicks(1141), null, false, "Admin", null, null },
                    { new Guid("5dc6ec47-5b7c-4c2b-86cd-3a671834e56e"), "System", new DateTime(2025, 6, 21, 12, 41, 32, 599, DateTimeKind.Utc).AddTicks(2261), null, true, "Registered", null, null }
                });

            migrationBuilder.InsertData(
                table: "AppUsers",
                columns: new[] { "Id", "CreatedBy", "CreatedOnUtc", "DeletedOnUtc", "identity_id", "UpdatedBy", "UpdatedOnUtc", "email_notification", "in_app_notification", "push_notification" },
                values: new object[] { new Guid("55c7f429-0916-4d84-8b76-d45185d89aa7"), "System", new DateTime(2025, 6, 21, 12, 41, 32, 553, DateTimeKind.Utc).AddTicks(2966), null, "b3398ff2-1b43-4af7-812d-eb4347eecbb8", null, null, true, true, true });

            migrationBuilder.InsertData(
                table: "RolePermission",
                columns: new[] { "PermissionId", "RoleId" },
                values: new object[,]
                {
                    { new Guid("0eeb5f27-10fd-430a-9257-a8457107141a"), new Guid("4b606d86-3537-475a-aa20-26aadd8f5cfd") },
                    { new Guid("25bb194c-ea15-4339-9f45-5a895c51b626"), new Guid("4b606d86-3537-475a-aa20-26aadd8f5cfd") },
                    { new Guid("3050d953-5dcf-4eb0-a18d-a3ce62a0dd3c"), new Guid("4b606d86-3537-475a-aa20-26aadd8f5cfd") },
                    { new Guid("33261a4a-c423-4876-8f15-e40068aea5ca"), new Guid("4b606d86-3537-475a-aa20-26aadd8f5cfd") },
                    { new Guid("346d3cc6-ac81-42b1-8539-cd53f42b6566"), new Guid("4b606d86-3537-475a-aa20-26aadd8f5cfd") },
                    { new Guid("386e40e9-da38-4d2f-8d02-ac4cbaddf760"), new Guid("4b606d86-3537-475a-aa20-26aadd8f5cfd") },
                    { new Guid("559dd4ec-4d2e-479d-a0a9-5229ecc04fb4"), new Guid("4b606d86-3537-475a-aa20-26aadd8f5cfd") },
                    { new Guid("940c88ad-24fe-4d86-a982-fa5ea224edba"), new Guid("4b606d86-3537-475a-aa20-26aadd8f5cfd") },
                    { new Guid("9f79a54c-0b54-4de5-94b9-8582a5f32e78"), new Guid("4b606d86-3537-475a-aa20-26aadd8f5cfd") },
                    { new Guid("a03a127b-9a03-46a0-b709-b6919f2598be"), new Guid("4b606d86-3537-475a-aa20-26aadd8f5cfd") },
                    { new Guid("a5585e9e-ec65-431b-9bb9-9bbc1663ebb8"), new Guid("4b606d86-3537-475a-aa20-26aadd8f5cfd") },
                    { new Guid("d066e4ee-6af2-4857-bd40-b9b058fa2201"), new Guid("4b606d86-3537-475a-aa20-26aadd8f5cfd") },
                    { new Guid("33261a4a-c423-4876-8f15-e40068aea5ca"), new Guid("5dc6ec47-5b7c-4c2b-86cd-3a671834e56e") },
                    { new Guid("a03a127b-9a03-46a0-b709-b6919f2598be"), new Guid("5dc6ec47-5b7c-4c2b-86cd-3a671834e56e") },
                    { new Guid("a5585e9e-ec65-431b-9bb9-9bbc1663ebb8"), new Guid("5dc6ec47-5b7c-4c2b-86cd-3a671834e56e") }
                });

            migrationBuilder.InsertData(
                table: "RoleUser",
                columns: new[] { "RoleId", "UserId" },
                values: new object[,]
                {
                    { new Guid("4b606d86-3537-475a-aa20-26aadd8f5cfd"), new Guid("55c7f429-0916-4d84-8b76-d45185d89aa7") },
                    { new Guid("5dc6ec47-5b7c-4c2b-86cd-3a671834e56e"), new Guid("55c7f429-0916-4d84-8b76-d45185d89aa7") }
                });

            migrationBuilder.CreateIndex(
                name: "IX_AppUsers_identity_id",
                table: "AppUsers",
                column: "identity_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_AspNetRoleClaims_RoleId",
                table: "AspNetRoleClaims",
                column: "RoleId");

            migrationBuilder.CreateIndex(
                name: "RoleNameIndex",
                table: "AspNetRoles",
                column: "NormalizedName",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_AspNetUserClaims_UserId",
                table: "AspNetUserClaims",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_AspNetUserLogins_UserId",
                table: "AspNetUserLogins",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_AspNetUserRoles_RoleId",
                table: "AspNetUserRoles",
                column: "RoleId");

            migrationBuilder.CreateIndex(
                name: "EmailIndex",
                table: "AspNetUsers",
                column: "NormalizedEmail");

            migrationBuilder.CreateIndex(
                name: "UserNameIndex",
                table: "AspNetUsers",
                column: "NormalizedUserName",
                unique: true);

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

            migrationBuilder.CreateIndex(
                name: "IX_RolePermission_PermissionId",
                table: "RolePermission",
                column: "PermissionId");

            migrationBuilder.CreateIndex(
                name: "IX_RoleUser_UserId",
                table: "RoleUser",
                column: "UserId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AspNetRoleClaims");

            migrationBuilder.DropTable(
                name: "AspNetUserClaims");

            migrationBuilder.DropTable(
                name: "AspNetUserLogins");

            migrationBuilder.DropTable(
                name: "AspNetUserRoles");

            migrationBuilder.DropTable(
                name: "AspNetUserTokens");

            migrationBuilder.DropTable(
                name: "Notifications");

            migrationBuilder.DropTable(
                name: "outbox_messages");

            migrationBuilder.DropTable(
                name: "RolePermission");

            migrationBuilder.DropTable(
                name: "RoleUser");

            migrationBuilder.DropTable(
                name: "AspNetRoles");

            migrationBuilder.DropTable(
                name: "Permissions");

            migrationBuilder.DropTable(
                name: "AppUsers");

            migrationBuilder.DropTable(
                name: "Roles");

            migrationBuilder.DropTable(
                name: "AspNetUsers");
        }
    }
}
