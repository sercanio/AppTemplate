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
                name: "permissions",
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
                    table.PrimaryKey("PK_permissions", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "roles",
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
                    table.PrimaryKey("PK_roles", x => x.Id);
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
                name: "users",
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
                    table.PrimaryKey("PK_users", x => x.Id);
                    table.ForeignKey(
                        name: "FK_users_AspNetUsers_identity_id",
                        column: x => x.identity_id,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "role_permission",
                columns: table => new
                {
                    RoleId = table.Column<Guid>(type: "uuid", nullable: false),
                    PermissionId = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_role_permission", x => new { x.RoleId, x.PermissionId });
                    table.ForeignKey(
                        name: "FK_role_permission_permissions_PermissionId",
                        column: x => x.PermissionId,
                        principalTable: "permissions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_role_permission_roles_RoleId",
                        column: x => x.RoleId,
                        principalTable: "roles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "role_user",
                columns: table => new
                {
                    RoleId = table.Column<Guid>(type: "uuid", nullable: false),
                    UserId = table.Column<Guid>(type: "uuid", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_role_user", x => new { x.RoleId, x.UserId });
                    table.ForeignKey(
                        name: "FK_role_user_roles_RoleId",
                        column: x => x.RoleId,
                        principalTable: "roles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_role_user_users_UserId",
                        column: x => x.UserId,
                        principalTable: "users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.InsertData(
                table: "AspNetUsers",
                columns: new[] { "Id", "AccessFailedCount", "ConcurrencyStamp", "Email", "EmailConfirmed", "LockoutEnabled", "LockoutEnd", "NormalizedEmail", "NormalizedUserName", "PasswordHash", "PhoneNumber", "PhoneNumberConfirmed", "SecurityStamp", "TwoFactorEnabled", "UserName" },
                values: new object[] { "b3398ff2-1b43-4af7-812d-eb4347eecbb8", 0, "6267cae9-20fe-48dd-982f-1a757f8d0252", "admin@example.com", true, false, null, "ADMIN@EXAMPLE.COM", "ADMIN", "AQAAAAIAAYagAAAAEMByb9Ia6Gx9Y6uJE6NaFKWolte0JiZ3ScPUme1bkTY94cEd7407w0QiM16vTcfoNg==", null, false, "7847bacf-e833-42ac-98e2-10a82044a338", false, "admin" });

            migrationBuilder.InsertData(
                table: "permissions",
                columns: new[] { "Id", "CreatedBy", "CreatedOnUtc", "DeletedOnUtc", "feature", "name", "UpdatedBy", "UpdatedOnUtc" },
                values: new object[,]
                {
                    { new Guid("0eeb5f27-10fd-430a-9257-a8457107141a"), "System", new DateTime(2025, 3, 24, 19, 13, 56, 57, DateTimeKind.Utc).AddTicks(2111), null, "permissions", "permissions:read", null, null },
                    { new Guid("25bb194c-ea15-4339-9f45-5a895c51b626"), "System", new DateTime(2025, 3, 24, 19, 13, 56, 57, DateTimeKind.Utc).AddTicks(2096), null, "users", "users:update", null, null },
                    { new Guid("3050d953-5dcf-4eb0-a18d-a3ce62a0dd3c"), "System", new DateTime(2025, 3, 24, 19, 13, 56, 57, DateTimeKind.Utc).AddTicks(2211), null, "auditlogs", "auditlogs:read", null, null },
                    { new Guid("33261a4a-c423-4876-8f15-e40068aea5ca"), "System", new DateTime(2025, 3, 24, 19, 13, 56, 57, DateTimeKind.Utc).AddTicks(468), null, "users", "users:read", null, null },
                    { new Guid("346d3cc6-ac81-42b1-8539-cd53f42b6566"), "System", new DateTime(2025, 3, 24, 19, 13, 56, 57, DateTimeKind.Utc).AddTicks(2107), null, "roles", "roles:update", null, null },
                    { new Guid("386e40e9-da38-4d2f-8d02-ac4cbaddf760"), "System", new DateTime(2025, 3, 24, 19, 13, 56, 57, DateTimeKind.Utc).AddTicks(2109), null, "roles", "roles:delete", null, null },
                    { new Guid("559dd4ec-4d2e-479d-a0a9-5229ecc04fb4"), "System", new DateTime(2025, 3, 24, 19, 13, 56, 57, DateTimeKind.Utc).AddTicks(2099), null, "users", "users:delete", null, null },
                    { new Guid("940c88ad-24fe-4d86-a982-fa5ea224edba"), "System", new DateTime(2025, 3, 24, 19, 13, 56, 57, DateTimeKind.Utc).AddTicks(2105), null, "roles", "roles:create", null, null },
                    { new Guid("9f79a54c-0b54-4de5-94b9-8582a5f32e78"), "System", new DateTime(2025, 3, 24, 19, 13, 56, 57, DateTimeKind.Utc).AddTicks(2090), null, "users", "users:create", null, null },
                    { new Guid("a03a127b-9a03-46a0-b709-b6919f2598be"), "System", new DateTime(2025, 3, 24, 19, 13, 56, 57, DateTimeKind.Utc).AddTicks(2214), null, "notifications", "notifications:read", null, null },
                    { new Guid("a5585e9e-ec65-431b-9bb9-9bbc1663ebb8"), "System", new DateTime(2025, 3, 24, 19, 13, 56, 57, DateTimeKind.Utc).AddTicks(2216), null, "notifications", "notifications:update", null, null },
                    { new Guid("d066e4ee-6af2-4857-bd40-b9b058fa2201"), "System", new DateTime(2025, 3, 24, 19, 13, 56, 57, DateTimeKind.Utc).AddTicks(2103), null, "roles", "roles:read", null, null }
                });

            migrationBuilder.InsertData(
                table: "roles",
                columns: new[] { "Id", "CreatedBy", "CreatedOnUtc", "DeletedOnUtc", "is_default", "name", "UpdatedBy", "UpdatedOnUtc" },
                values: new object[,]
                {
                    { new Guid("4b606d86-3537-475a-aa20-26aadd8f5cfd"), "System", new DateTime(2025, 3, 24, 19, 13, 56, 63, DateTimeKind.Utc).AddTicks(3999), null, false, "Admin", null, null },
                    { new Guid("5dc6ec47-5b7c-4c2b-86cd-3a671834e56e"), "System", new DateTime(2025, 3, 24, 19, 13, 56, 63, DateTimeKind.Utc).AddTicks(4654), null, true, "Registered", null, null }
                });

            migrationBuilder.InsertData(
                table: "role_permission",
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
                table: "users",
                columns: new[] { "Id", "CreatedBy", "CreatedOnUtc", "DeletedOnUtc", "identity_id", "UpdatedBy", "UpdatedOnUtc", "email_notification", "in_app_notification", "push_notification" },
                values: new object[] { new Guid("55c7f429-0916-4d84-8b76-d45185d89aa7"), "System", new DateTime(2025, 3, 24, 19, 13, 56, 53, DateTimeKind.Utc).AddTicks(577), null, "b3398ff2-1b43-4af7-812d-eb4347eecbb8", null, null, true, true, true });

            migrationBuilder.InsertData(
                table: "role_user",
                columns: new[] { "RoleId", "UserId" },
                values: new object[,]
                {
                    { new Guid("4b606d86-3537-475a-aa20-26aadd8f5cfd"), new Guid("55c7f429-0916-4d84-8b76-d45185d89aa7") },
                    { new Guid("5dc6ec47-5b7c-4c2b-86cd-3a671834e56e"), new Guid("55c7f429-0916-4d84-8b76-d45185d89aa7") }
                });

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
                name: "IX_role_permission_PermissionId",
                table: "role_permission",
                column: "PermissionId");

            migrationBuilder.CreateIndex(
                name: "IX_role_user_UserId",
                table: "role_user",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_users_identity_id",
                table: "users",
                column: "identity_id",
                unique: true);
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
                name: "outbox_messages");

            migrationBuilder.DropTable(
                name: "role_permission");

            migrationBuilder.DropTable(
                name: "role_user");

            migrationBuilder.DropTable(
                name: "AspNetRoles");

            migrationBuilder.DropTable(
                name: "permissions");

            migrationBuilder.DropTable(
                name: "roles");

            migrationBuilder.DropTable(
                name: "users");

            migrationBuilder.DropTable(
                name: "AspNetUsers");
        }
    }
}
