using System;
using Microsoft.EntityFrameworkCore.Migrations;

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
                name: "AspNetUsers",
                columns: table => new
                {
                    Id = table.Column<string>(type: "text", nullable: false),
                    UserName = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                    NormalizedUserName = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: true),
                    Email = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
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
                    Id = table.Column<Guid>(type: "uuid", nullable: false, comment: "Unique identifier for the outbox message"),
                    OccurredOnUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, comment: "When the domain event occurred"),
                    Type = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false, comment: "Type of the domain event"),
                    Content = table.Column<string>(type: "jsonb", nullable: false, comment: "Serialized domain event content"),
                    ProcessedOnUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: true, comment: "When the message was processed (null if not processed)"),
                    Error = table.Column<string>(type: "text", nullable: true, comment: "Error details if processing failed")
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
                    CreatedOnUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedOnUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    DeletedOnUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Permissions", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "RefreshTokens",
                columns: table => new
                {
                    Token = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    UserId = table.Column<string>(type: "character varying(450)", maxLength: 450, nullable: false),
                    ExpiresAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    LastUsedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    IsRevoked = table.Column<bool>(type: "boolean", nullable: false),
                    RevokedReason = table.Column<string>(type: "text", nullable: true),
                    ReplacedByToken = table.Column<string>(type: "text", nullable: true),
                    IsCurrent = table.Column<bool>(type: "boolean", nullable: false),
                    DeviceName = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: true),
                    UserAgent = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    IpAddress = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    Platform = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    Browser = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    AccessTokenJti = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RefreshTokens", x => x.Token);
                });

            migrationBuilder.CreateTable(
                name: "Roles",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    name = table.Column<string>(type: "text", nullable: false),
                    display_name = table.Column<string>(type: "text", nullable: false),
                    is_default = table.Column<bool>(type: "boolean", nullable: false),
                    CreatedById = table.Column<Guid>(type: "uuid", nullable: true),
                    UpdatedById = table.Column<Guid>(type: "uuid", nullable: true),
                    DeletedById = table.Column<Guid>(type: "uuid", nullable: true),
                    CreatedOnUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedOnUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    DeletedOnUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Roles", x => x.Id);
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
                    user_changed_its_username = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    ProfilePictureUrl = table.Column<string>(type: "text", nullable: true),
                    biography = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                    created_by_id = table.Column<Guid>(type: "uuid", nullable: true),
                    updated_by_id = table.Column<Guid>(type: "uuid", nullable: true),
                    deleted_by_id = table.Column<Guid>(type: "uuid", nullable: true),
                    CreatedOnUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedOnUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    DeletedOnUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AppUsers", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AppUsers_AppUsers_created_by_id",
                        column: x => x.created_by_id,
                        principalTable: "AppUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_AppUsers_AppUsers_deleted_by_id",
                        column: x => x.deleted_by_id,
                        principalTable: "AppUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_AppUsers_AppUsers_updated_by_id",
                        column: x => x.updated_by_id,
                        principalTable: "AppUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_AppUsers_AspNetUsers_identity_id",
                        column: x => x.identity_id,
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
                    Title = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false, comment: "Notification title"),
                    Message = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: false, comment: "Notification message"),
                    Type = table.Column<int>(type: "integer", nullable: false, comment: "Notification type"),
                    IsRead = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false, comment: "Whether the notification has been read"),
                    RecipientId = table.Column<Guid>(type: "uuid", nullable: false, comment: "The ID of the user who owns this notification"),
                    CreatedOnUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, comment: "When the notification was created"),
                    UpdatedOnUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    DeletedOnUtc = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Notifications", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Notifications_AppUsers_RecipientId",
                        column: x => x.RecipientId,
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
                values: new object[] { "b3398ff2-1b43-4af7-812d-eb4347eecbb8", 0, "d035a7c6-4663-43f6-943f-7662b6612d13", "admin@example.com", true, false, null, "ADMIN@EXAMPLE.COM", "ADMIN", "AQAAAAIAAYagAAAAECsW7AdFFt31AoInippObrPExS3T6sqoTHyW92YwnM6nOKsqJ6zqUNwuXyHDxGNUTg==", null, false, "fixed-security-stamp-for-seeding", false, "admin" });

            migrationBuilder.InsertData(
                table: "Permissions",
                columns: new[] { "Id", "CreatedOnUtc", "DeletedOnUtc", "feature", "name", "UpdatedOnUtc" },
                values: new object[,]
                {
                    { new Guid("0eeb5f27-10fd-430a-9257-a8457107141a"), new DateTime(2025, 10, 7, 6, 2, 25, 361, DateTimeKind.Utc).AddTicks(2062), null, "permissions", "permissions:read", null },
                    { new Guid("1ff035a6-5d40-4a2d-aa9c-1d3182b3642e"), new DateTime(2025, 10, 7, 6, 2, 25, 361, DateTimeKind.Utc).AddTicks(2122), null, "titlefollows", "titlefollows:create", null },
                    { new Guid("22c1dbc9-bad6-4ebf-9c49-5577625f2b5f"), new DateTime(2025, 10, 7, 6, 2, 25, 361, DateTimeKind.Utc).AddTicks(2099), null, "userfollows", "userfollows:update", null },
                    { new Guid("25bb194c-ea15-4339-9f45-5a895c51b626"), new DateTime(2025, 10, 7, 6, 2, 25, 361, DateTimeKind.Utc).AddTicks(2052), null, "users", "users:update", null },
                    { new Guid("272c16b9-da69-4065-b849-6fb45c9ff281"), new DateTime(2025, 10, 7, 6, 2, 25, 361, DateTimeKind.Utc).AddTicks(2116), null, "titles", "titles:create", null },
                    { new Guid("3050d953-5dcf-4eb0-a18d-a3ce62a0dd3c"), new DateTime(2025, 10, 7, 6, 2, 25, 361, DateTimeKind.Utc).AddTicks(2063), null, "auditlogs", "auditlogs:read", null },
                    { new Guid("30bb7d98-1a11-4152-9481-9a9d5fd39041"), new DateTime(2025, 10, 7, 6, 2, 25, 361, DateTimeKind.Utc).AddTicks(2133), null, "entrybookmarks", "entrybookmarks:create", null },
                    { new Guid("33261a4a-c423-4876-8f15-e40068aea5ca"), new DateTime(2025, 10, 7, 6, 2, 25, 361, DateTimeKind.Utc).AddTicks(2044), null, "users", "users:read", null },
                    { new Guid("33ffe115-42c7-457a-8c63-1f8c5179bb5c"), new DateTime(2025, 10, 7, 6, 2, 25, 361, DateTimeKind.Utc).AddTicks(2143), null, "entryreports", "entryreports:delete", null },
                    { new Guid("346d3cc6-ac81-42b1-8539-cd53f42b6566"), new DateTime(2025, 10, 7, 6, 2, 25, 361, DateTimeKind.Utc).AddTicks(2059), null, "roles", "roles:update", null },
                    { new Guid("386e40e9-da38-4d2f-8d02-ac4cbaddf760"), new DateTime(2025, 10, 7, 6, 2, 25, 361, DateTimeKind.Utc).AddTicks(2060), null, "roles", "roles:delete", null },
                    { new Guid("481770d7-09f7-481a-83cd-f6aa808a072e"), new DateTime(2025, 10, 7, 6, 2, 25, 361, DateTimeKind.Utc).AddTicks(2149), null, "featuredentries", "featuredentries:delete", null },
                    { new Guid("48c15b07-004d-44eb-a348-8ae63327a4b8"), new DateTime(2025, 10, 7, 6, 2, 25, 361, DateTimeKind.Utc).AddTicks(2119), null, "titles", "titles:delete", null },
                    { new Guid("5341ddd6-5c42-477a-a155-33c51030f76b"), new DateTime(2025, 10, 7, 6, 2, 25, 361, DateTimeKind.Utc).AddTicks(2124), null, "titlefollows", "titlefollows:delete", null },
                    { new Guid("559dd4ec-4d2e-479d-a0a9-5229ecc04fb4"), new DateTime(2025, 10, 7, 6, 2, 25, 361, DateTimeKind.Utc).AddTicks(2053), null, "users", "users:delete", null },
                    { new Guid("5bc63b8d-2825-4f0e-aeae-234c7b2d930f"), new DateTime(2025, 10, 7, 6, 2, 25, 361, DateTimeKind.Utc).AddTicks(2131), null, "entrybookmarks", "entrybookmarks:read", null },
                    { new Guid("5be9a36e-c59c-4a9a-a800-15f4f76ea80b"), new DateTime(2025, 10, 7, 6, 2, 25, 361, DateTimeKind.Utc).AddTicks(2127), null, "entrylikes", "entrylikes:create", null },
                    { new Guid("5ca7e876-17bb-4b7a-a1d5-42ed0ee6baf3"), new DateTime(2025, 10, 7, 6, 2, 25, 361, DateTimeKind.Utc).AddTicks(2147), null, "featuredentries", "featuredentries:create", null },
                    { new Guid("6030de9b-595c-474f-99ff-b654ad062e19"), new DateTime(2025, 10, 7, 6, 2, 25, 361, DateTimeKind.Utc).AddTicks(2114), null, "titles", "titles:read", null },
                    { new Guid("6203c108-2c3d-4ed3-ab3c-b119e7a7491a"), new DateTime(2025, 10, 7, 6, 2, 25, 361, DateTimeKind.Utc).AddTicks(2055), null, "roles", "roles:admin", null },
                    { new Guid("638a9f7e-7bfe-4748-8947-f605c799d214"), new DateTime(2025, 10, 7, 6, 2, 25, 361, DateTimeKind.Utc).AddTicks(2112), null, "entries", "entries:delete", null },
                    { new Guid("70a3a380-6e15-4b0e-b8b5-67591dbafcfa"), new DateTime(2025, 10, 7, 6, 2, 25, 361, DateTimeKind.Utc).AddTicks(2148), null, "featuredentries", "featuredentries:update", null },
                    { new Guid("78f1b087-3b45-48d0-8e16-9f04a760c294"), new DateTime(2025, 10, 7, 6, 2, 25, 361, DateTimeKind.Utc).AddTicks(2070), null, "userfollows", "userfollows:create", null },
                    { new Guid("7b834e3d-ff31-416f-8b1e-ce1a7e9681e8"), new DateTime(2025, 10, 7, 6, 2, 25, 361, DateTimeKind.Utc).AddTicks(2105), null, "userfollows", "userfollows:delete", null },
                    { new Guid("8116c67b-7f82-41b5-b9c4-a91e042e9257"), new DateTime(2025, 10, 7, 6, 2, 25, 361, DateTimeKind.Utc).AddTicks(2109), null, "entries", "entries:create", null },
                    { new Guid("828736bc-1b6a-4b24-a55a-763fb6616970"), new DateTime(2025, 10, 7, 6, 2, 25, 361, DateTimeKind.Utc).AddTicks(2144), null, "featuredentries", "featuredentries:admin", null },
                    { new Guid("859de6a2-9975-4d49-99fb-0a99cb8b3474"), new DateTime(2025, 10, 7, 6, 2, 25, 361, DateTimeKind.Utc).AddTicks(2137), null, "entryreports", "entryreports:admin", null },
                    { new Guid("8694cd0d-8987-44b4-b823-3f2e7f023919"), new DateTime(2025, 10, 7, 6, 2, 25, 361, DateTimeKind.Utc).AddTicks(2136), null, "entrybookmarks", "entrybookmarks:delete", null },
                    { new Guid("8f97aeb9-a9fd-470f-bae9-c9f5f0534d23"), new DateTime(2025, 10, 7, 6, 2, 25, 361, DateTimeKind.Utc).AddTicks(2067), null, "statistics", "statistics:read", null },
                    { new Guid("9185d676-db9e-4a8e-8286-7f4ea78ab022"), new DateTime(2025, 10, 7, 6, 2, 25, 361, DateTimeKind.Utc).AddTicks(2113), null, "titles", "titles:admin", null },
                    { new Guid("940c88ad-24fe-4d86-a982-fa5ea224edba"), new DateTime(2025, 10, 7, 6, 2, 25, 361, DateTimeKind.Utc).AddTicks(2057), null, "roles", "roles:create", null },
                    { new Guid("9f79a54c-0b54-4de5-94b9-8582a5f32e78"), new DateTime(2025, 10, 7, 6, 2, 25, 361, DateTimeKind.Utc).AddTicks(2049), null, "users", "users:create", null },
                    { new Guid("a03a127b-9a03-46a0-b709-b6919f2598be"), new DateTime(2025, 10, 7, 6, 2, 25, 361, DateTimeKind.Utc).AddTicks(2065), null, "notifications", "notifications:read", null },
                    { new Guid("a393e161-dc5e-4f0c-9fb9-f3a901b48149"), new DateTime(2025, 10, 7, 6, 2, 25, 361, DateTimeKind.Utc).AddTicks(2141), null, "entryreports", "entryreports:update", null },
                    { new Guid("a5585e9e-ec65-431b-9bb9-9bbc1663ebb8"), new DateTime(2025, 10, 7, 6, 2, 25, 361, DateTimeKind.Utc).AddTicks(2066), null, "notifications", "notifications:update", null },
                    { new Guid("ac804e8f-abe7-4516-927f-045477dbe007"), new DateTime(2025, 10, 7, 6, 2, 25, 361, DateTimeKind.Utc).AddTicks(2123), null, "titlefollows", "titlefollows:update", null },
                    { new Guid("afe7776a-69a6-4134-96ea-a24829c67c9d"), new DateTime(2025, 10, 7, 6, 2, 25, 361, DateTimeKind.Utc).AddTicks(2134), null, "entrybookmarks", "entrybookmarks:update", null },
                    { new Guid("b11364e1-dc05-422f-982a-6f365c1825a8"), new DateTime(2025, 10, 7, 6, 2, 25, 361, DateTimeKind.Utc).AddTicks(2110), null, "entries", "entries:update", null },
                    { new Guid("b2fe4d1c-59b9-4161-8eab-04380b45fd5e"), new DateTime(2025, 10, 7, 6, 2, 25, 361, DateTimeKind.Utc).AddTicks(2129), null, "entrylikes", "entrylikes:update", null },
                    { new Guid("bb5cb9b7-75d0-4a9f-81d9-d02259b6ddf2"), new DateTime(2025, 10, 7, 6, 2, 25, 361, DateTimeKind.Utc).AddTicks(2130), null, "entrylikes", "entrylikes:delete", null },
                    { new Guid("c42c3f31-f94a-474d-a159-2c826c031e34"), new DateTime(2025, 10, 7, 6, 2, 25, 361, DateTimeKind.Utc).AddTicks(2140), null, "entryreports", "entryreports:create", null },
                    { new Guid("c8a25b63-74ee-4375-98c8-e64107bb6d76"), new DateTime(2025, 10, 7, 6, 2, 25, 361, DateTimeKind.Utc).AddTicks(163), null, "users", "users:admin", null },
                    { new Guid("cd552577-20c8-4e12-9685-a5c24ecd7fa8"), new DateTime(2025, 10, 7, 6, 2, 25, 361, DateTimeKind.Utc).AddTicks(2107), null, "entries", "entries:read", null },
                    { new Guid("d066e4ee-6af2-4857-bd40-b9b058fa2201"), new DateTime(2025, 10, 7, 6, 2, 25, 361, DateTimeKind.Utc).AddTicks(2056), null, "roles", "roles:read", null },
                    { new Guid("e033b219-f1c5-4c0c-b1f4-a756facb1819"), new DateTime(2025, 10, 7, 6, 2, 25, 361, DateTimeKind.Utc).AddTicks(2117), null, "titles", "titles:update", null },
                    { new Guid("e5f82d92-0610-4f63-ba1c-9bad3cbc09dd"), new DateTime(2025, 10, 7, 6, 2, 25, 361, DateTimeKind.Utc).AddTicks(2138), null, "entryreports", "entryreports:read", null },
                    { new Guid("ec733d3f-cf8b-475c-8af6-5881cdb65dbe"), new DateTime(2025, 10, 7, 6, 2, 25, 361, DateTimeKind.Utc).AddTicks(2120), null, "titlefollows", "titlefollows:read", null },
                    { new Guid("ee1be42d-4341-4ac5-9390-2ec71eb54239"), new DateTime(2025, 10, 7, 6, 2, 25, 361, DateTimeKind.Utc).AddTicks(2069), null, "userfollows", "userfollows:read", null },
                    { new Guid("f14a636e-6a91-4b3e-9ea4-d9bbe8c36872"), new DateTime(2025, 10, 7, 6, 2, 25, 361, DateTimeKind.Utc).AddTicks(2106), null, "entries", "entries:admin", null },
                    { new Guid("fa8626a7-e34f-48c3-8b14-64b6889a36fc"), new DateTime(2025, 10, 7, 6, 2, 25, 361, DateTimeKind.Utc).AddTicks(2145), null, "featuredentries", "featuredentries:read", null },
                    { new Guid("fbeb18d0-5e5f-4a38-aec7-6bb314408dc7"), new DateTime(2025, 10, 7, 6, 2, 25, 361, DateTimeKind.Utc).AddTicks(2126), null, "entrylikes", "entrylikes:read", null }
                });

            migrationBuilder.InsertData(
                table: "Roles",
                columns: new[] { "Id", "CreatedById", "CreatedOnUtc", "DeletedById", "DeletedOnUtc", "display_name", "is_default", "name", "UpdatedById", "UpdatedOnUtc" },
                values: new object[,]
                {
                    { new Guid("4b606d86-3537-475a-aa20-26aadd8f5cfd"), null, new DateTime(2025, 10, 7, 6, 2, 25, 367, DateTimeKind.Utc).AddTicks(6754), null, null, "yönetici", false, "Admin", null, null },
                    { new Guid("5dc6ec47-5b7c-4c2b-86cd-3a671834e56e"), null, new DateTime(2025, 10, 7, 6, 2, 25, 367, DateTimeKind.Utc).AddTicks(7254), null, null, "kayıtlı", true, "Registered", null, null }
                });

            migrationBuilder.InsertData(
                table: "AppUsers",
                columns: new[] { "Id", "biography", "created_by_id", "CreatedOnUtc", "deleted_by_id", "DeletedOnUtc", "identity_id", "ProfilePictureUrl", "updated_by_id", "UpdatedOnUtc", "email_notification", "in_app_notification", "push_notification" },
                values: new object[] { new Guid("55c7f429-0916-4d84-8b76-d45185d89aa7"), null, null, new DateTime(2025, 10, 7, 6, 2, 25, 351, DateTimeKind.Utc).AddTicks(8088), null, null, "b3398ff2-1b43-4af7-812d-eb4347eecbb8", null, null, null, true, true, true });

            migrationBuilder.InsertData(
                table: "RolePermission",
                columns: new[] { "PermissionId", "RoleId" },
                values: new object[,]
                {
                    { new Guid("0eeb5f27-10fd-430a-9257-a8457107141a"), new Guid("4b606d86-3537-475a-aa20-26aadd8f5cfd") },
                    { new Guid("1ff035a6-5d40-4a2d-aa9c-1d3182b3642e"), new Guid("4b606d86-3537-475a-aa20-26aadd8f5cfd") },
                    { new Guid("22c1dbc9-bad6-4ebf-9c49-5577625f2b5f"), new Guid("4b606d86-3537-475a-aa20-26aadd8f5cfd") },
                    { new Guid("25bb194c-ea15-4339-9f45-5a895c51b626"), new Guid("4b606d86-3537-475a-aa20-26aadd8f5cfd") },
                    { new Guid("272c16b9-da69-4065-b849-6fb45c9ff281"), new Guid("4b606d86-3537-475a-aa20-26aadd8f5cfd") },
                    { new Guid("3050d953-5dcf-4eb0-a18d-a3ce62a0dd3c"), new Guid("4b606d86-3537-475a-aa20-26aadd8f5cfd") },
                    { new Guid("30bb7d98-1a11-4152-9481-9a9d5fd39041"), new Guid("4b606d86-3537-475a-aa20-26aadd8f5cfd") },
                    { new Guid("33261a4a-c423-4876-8f15-e40068aea5ca"), new Guid("4b606d86-3537-475a-aa20-26aadd8f5cfd") },
                    { new Guid("33ffe115-42c7-457a-8c63-1f8c5179bb5c"), new Guid("4b606d86-3537-475a-aa20-26aadd8f5cfd") },
                    { new Guid("346d3cc6-ac81-42b1-8539-cd53f42b6566"), new Guid("4b606d86-3537-475a-aa20-26aadd8f5cfd") },
                    { new Guid("386e40e9-da38-4d2f-8d02-ac4cbaddf760"), new Guid("4b606d86-3537-475a-aa20-26aadd8f5cfd") },
                    { new Guid("481770d7-09f7-481a-83cd-f6aa808a072e"), new Guid("4b606d86-3537-475a-aa20-26aadd8f5cfd") },
                    { new Guid("48c15b07-004d-44eb-a348-8ae63327a4b8"), new Guid("4b606d86-3537-475a-aa20-26aadd8f5cfd") },
                    { new Guid("5341ddd6-5c42-477a-a155-33c51030f76b"), new Guid("4b606d86-3537-475a-aa20-26aadd8f5cfd") },
                    { new Guid("559dd4ec-4d2e-479d-a0a9-5229ecc04fb4"), new Guid("4b606d86-3537-475a-aa20-26aadd8f5cfd") },
                    { new Guid("5bc63b8d-2825-4f0e-aeae-234c7b2d930f"), new Guid("4b606d86-3537-475a-aa20-26aadd8f5cfd") },
                    { new Guid("5be9a36e-c59c-4a9a-a800-15f4f76ea80b"), new Guid("4b606d86-3537-475a-aa20-26aadd8f5cfd") },
                    { new Guid("5ca7e876-17bb-4b7a-a1d5-42ed0ee6baf3"), new Guid("4b606d86-3537-475a-aa20-26aadd8f5cfd") },
                    { new Guid("6030de9b-595c-474f-99ff-b654ad062e19"), new Guid("4b606d86-3537-475a-aa20-26aadd8f5cfd") },
                    { new Guid("6203c108-2c3d-4ed3-ab3c-b119e7a7491a"), new Guid("4b606d86-3537-475a-aa20-26aadd8f5cfd") },
                    { new Guid("638a9f7e-7bfe-4748-8947-f605c799d214"), new Guid("4b606d86-3537-475a-aa20-26aadd8f5cfd") },
                    { new Guid("70a3a380-6e15-4b0e-b8b5-67591dbafcfa"), new Guid("4b606d86-3537-475a-aa20-26aadd8f5cfd") },
                    { new Guid("78f1b087-3b45-48d0-8e16-9f04a760c294"), new Guid("4b606d86-3537-475a-aa20-26aadd8f5cfd") },
                    { new Guid("7b834e3d-ff31-416f-8b1e-ce1a7e9681e8"), new Guid("4b606d86-3537-475a-aa20-26aadd8f5cfd") },
                    { new Guid("8116c67b-7f82-41b5-b9c4-a91e042e9257"), new Guid("4b606d86-3537-475a-aa20-26aadd8f5cfd") },
                    { new Guid("828736bc-1b6a-4b24-a55a-763fb6616970"), new Guid("4b606d86-3537-475a-aa20-26aadd8f5cfd") },
                    { new Guid("859de6a2-9975-4d49-99fb-0a99cb8b3474"), new Guid("4b606d86-3537-475a-aa20-26aadd8f5cfd") },
                    { new Guid("8694cd0d-8987-44b4-b823-3f2e7f023919"), new Guid("4b606d86-3537-475a-aa20-26aadd8f5cfd") },
                    { new Guid("8f97aeb9-a9fd-470f-bae9-c9f5f0534d23"), new Guid("4b606d86-3537-475a-aa20-26aadd8f5cfd") },
                    { new Guid("9185d676-db9e-4a8e-8286-7f4ea78ab022"), new Guid("4b606d86-3537-475a-aa20-26aadd8f5cfd") },
                    { new Guid("940c88ad-24fe-4d86-a982-fa5ea224edba"), new Guid("4b606d86-3537-475a-aa20-26aadd8f5cfd") },
                    { new Guid("9f79a54c-0b54-4de5-94b9-8582a5f32e78"), new Guid("4b606d86-3537-475a-aa20-26aadd8f5cfd") },
                    { new Guid("a03a127b-9a03-46a0-b709-b6919f2598be"), new Guid("4b606d86-3537-475a-aa20-26aadd8f5cfd") },
                    { new Guid("a393e161-dc5e-4f0c-9fb9-f3a901b48149"), new Guid("4b606d86-3537-475a-aa20-26aadd8f5cfd") },
                    { new Guid("a5585e9e-ec65-431b-9bb9-9bbc1663ebb8"), new Guid("4b606d86-3537-475a-aa20-26aadd8f5cfd") },
                    { new Guid("ac804e8f-abe7-4516-927f-045477dbe007"), new Guid("4b606d86-3537-475a-aa20-26aadd8f5cfd") },
                    { new Guid("afe7776a-69a6-4134-96ea-a24829c67c9d"), new Guid("4b606d86-3537-475a-aa20-26aadd8f5cfd") },
                    { new Guid("b11364e1-dc05-422f-982a-6f365c1825a8"), new Guid("4b606d86-3537-475a-aa20-26aadd8f5cfd") },
                    { new Guid("b2fe4d1c-59b9-4161-8eab-04380b45fd5e"), new Guid("4b606d86-3537-475a-aa20-26aadd8f5cfd") },
                    { new Guid("bb5cb9b7-75d0-4a9f-81d9-d02259b6ddf2"), new Guid("4b606d86-3537-475a-aa20-26aadd8f5cfd") },
                    { new Guid("c42c3f31-f94a-474d-a159-2c826c031e34"), new Guid("4b606d86-3537-475a-aa20-26aadd8f5cfd") },
                    { new Guid("c8a25b63-74ee-4375-98c8-e64107bb6d76"), new Guid("4b606d86-3537-475a-aa20-26aadd8f5cfd") },
                    { new Guid("cd552577-20c8-4e12-9685-a5c24ecd7fa8"), new Guid("4b606d86-3537-475a-aa20-26aadd8f5cfd") },
                    { new Guid("d066e4ee-6af2-4857-bd40-b9b058fa2201"), new Guid("4b606d86-3537-475a-aa20-26aadd8f5cfd") },
                    { new Guid("e033b219-f1c5-4c0c-b1f4-a756facb1819"), new Guid("4b606d86-3537-475a-aa20-26aadd8f5cfd") },
                    { new Guid("e5f82d92-0610-4f63-ba1c-9bad3cbc09dd"), new Guid("4b606d86-3537-475a-aa20-26aadd8f5cfd") },
                    { new Guid("ec733d3f-cf8b-475c-8af6-5881cdb65dbe"), new Guid("4b606d86-3537-475a-aa20-26aadd8f5cfd") },
                    { new Guid("ee1be42d-4341-4ac5-9390-2ec71eb54239"), new Guid("4b606d86-3537-475a-aa20-26aadd8f5cfd") },
                    { new Guid("f14a636e-6a91-4b3e-9ea4-d9bbe8c36872"), new Guid("4b606d86-3537-475a-aa20-26aadd8f5cfd") },
                    { new Guid("fa8626a7-e34f-48c3-8b14-64b6889a36fc"), new Guid("4b606d86-3537-475a-aa20-26aadd8f5cfd") },
                    { new Guid("fbeb18d0-5e5f-4a38-aec7-6bb314408dc7"), new Guid("4b606d86-3537-475a-aa20-26aadd8f5cfd") },
                    { new Guid("1ff035a6-5d40-4a2d-aa9c-1d3182b3642e"), new Guid("5dc6ec47-5b7c-4c2b-86cd-3a671834e56e") },
                    { new Guid("22c1dbc9-bad6-4ebf-9c49-5577625f2b5f"), new Guid("5dc6ec47-5b7c-4c2b-86cd-3a671834e56e") },
                    { new Guid("272c16b9-da69-4065-b849-6fb45c9ff281"), new Guid("5dc6ec47-5b7c-4c2b-86cd-3a671834e56e") },
                    { new Guid("30bb7d98-1a11-4152-9481-9a9d5fd39041"), new Guid("5dc6ec47-5b7c-4c2b-86cd-3a671834e56e") },
                    { new Guid("33261a4a-c423-4876-8f15-e40068aea5ca"), new Guid("5dc6ec47-5b7c-4c2b-86cd-3a671834e56e") },
                    { new Guid("5bc63b8d-2825-4f0e-aeae-234c7b2d930f"), new Guid("5dc6ec47-5b7c-4c2b-86cd-3a671834e56e") },
                    { new Guid("5be9a36e-c59c-4a9a-a800-15f4f76ea80b"), new Guid("5dc6ec47-5b7c-4c2b-86cd-3a671834e56e") },
                    { new Guid("6030de9b-595c-474f-99ff-b654ad062e19"), new Guid("5dc6ec47-5b7c-4c2b-86cd-3a671834e56e") },
                    { new Guid("638a9f7e-7bfe-4748-8947-f605c799d214"), new Guid("5dc6ec47-5b7c-4c2b-86cd-3a671834e56e") },
                    { new Guid("78f1b087-3b45-48d0-8e16-9f04a760c294"), new Guid("5dc6ec47-5b7c-4c2b-86cd-3a671834e56e") },
                    { new Guid("8116c67b-7f82-41b5-b9c4-a91e042e9257"), new Guid("5dc6ec47-5b7c-4c2b-86cd-3a671834e56e") },
                    { new Guid("8694cd0d-8987-44b4-b823-3f2e7f023919"), new Guid("5dc6ec47-5b7c-4c2b-86cd-3a671834e56e") },
                    { new Guid("a03a127b-9a03-46a0-b709-b6919f2598be"), new Guid("5dc6ec47-5b7c-4c2b-86cd-3a671834e56e") },
                    { new Guid("a5585e9e-ec65-431b-9bb9-9bbc1663ebb8"), new Guid("5dc6ec47-5b7c-4c2b-86cd-3a671834e56e") },
                    { new Guid("ac804e8f-abe7-4516-927f-045477dbe007"), new Guid("5dc6ec47-5b7c-4c2b-86cd-3a671834e56e") },
                    { new Guid("afe7776a-69a6-4134-96ea-a24829c67c9d"), new Guid("5dc6ec47-5b7c-4c2b-86cd-3a671834e56e") },
                    { new Guid("b11364e1-dc05-422f-982a-6f365c1825a8"), new Guid("5dc6ec47-5b7c-4c2b-86cd-3a671834e56e") },
                    { new Guid("b2fe4d1c-59b9-4161-8eab-04380b45fd5e"), new Guid("5dc6ec47-5b7c-4c2b-86cd-3a671834e56e") },
                    { new Guid("bb5cb9b7-75d0-4a9f-81d9-d02259b6ddf2"), new Guid("5dc6ec47-5b7c-4c2b-86cd-3a671834e56e") },
                    { new Guid("cd552577-20c8-4e12-9685-a5c24ecd7fa8"), new Guid("5dc6ec47-5b7c-4c2b-86cd-3a671834e56e") },
                    { new Guid("e033b219-f1c5-4c0c-b1f4-a756facb1819"), new Guid("5dc6ec47-5b7c-4c2b-86cd-3a671834e56e") },
                    { new Guid("ec733d3f-cf8b-475c-8af6-5881cdb65dbe"), new Guid("5dc6ec47-5b7c-4c2b-86cd-3a671834e56e") },
                    { new Guid("ee1be42d-4341-4ac5-9390-2ec71eb54239"), new Guid("5dc6ec47-5b7c-4c2b-86cd-3a671834e56e") },
                    { new Guid("fbeb18d0-5e5f-4a38-aec7-6bb314408dc7"), new Guid("5dc6ec47-5b7c-4c2b-86cd-3a671834e56e") }
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
                name: "IX_AppUsers_created_by_id",
                table: "AppUsers",
                column: "created_by_id");

            migrationBuilder.CreateIndex(
                name: "IX_AppUsers_deleted_by_id",
                table: "AppUsers",
                column: "deleted_by_id");

            migrationBuilder.CreateIndex(
                name: "IX_AppUsers_identity_id",
                table: "AppUsers",
                column: "identity_id",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_AppUsers_updated_by_id",
                table: "AppUsers",
                column: "updated_by_id");

            migrationBuilder.CreateIndex(
                name: "IX_AspNetUserLogins_UserId",
                table: "AspNetUserLogins",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "EmailIndex",
                table: "AspNetUsers",
                column: "NormalizedEmail");

            migrationBuilder.CreateIndex(
                name: "IX_AspNetUsers_Email",
                table: "AspNetUsers",
                column: "Email",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_AspNetUsers_UserName",
                table: "AspNetUsers",
                column: "UserName",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "UserNameIndex",
                table: "AspNetUsers",
                column: "NormalizedUserName",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Notifications_CreatedOnUtc",
                table: "Notifications",
                column: "CreatedOnUtc",
                filter: "\"DeletedOnUtc\" IS NULL");

            migrationBuilder.CreateIndex(
                name: "IX_Notifications_RecipientId",
                table: "Notifications",
                column: "RecipientId",
                filter: "\"DeletedOnUtc\" IS NULL");

            migrationBuilder.CreateIndex(
                name: "IX_Notifications_RecipientId_IsRead",
                table: "Notifications",
                columns: new[] { "RecipientId", "IsRead" },
                filter: "\"DeletedOnUtc\" IS NULL")
                .Annotation("Npgsql:IndexInclude", new[] { "CreatedOnUtc", "Title" });

            migrationBuilder.CreateIndex(
                name: "IX_OutboxMessages_OccurredOnUtc",
                table: "outbox_messages",
                column: "OccurredOnUtc");

            migrationBuilder.CreateIndex(
                name: "IX_OutboxMessages_ProcessedOnUtc",
                table: "outbox_messages",
                column: "ProcessedOnUtc",
                filter: "\"ProcessedOnUtc\" IS NULL");

            migrationBuilder.CreateIndex(
                name: "IX_RefreshTokens_ExpiresAt",
                table: "RefreshTokens",
                column: "ExpiresAt");

            migrationBuilder.CreateIndex(
                name: "IX_RefreshTokens_IsRevoked",
                table: "RefreshTokens",
                column: "IsRevoked");

            migrationBuilder.CreateIndex(
                name: "IX_RefreshTokens_UserId",
                table: "RefreshTokens",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_RefreshTokens_UserId_IsRevoked",
                table: "RefreshTokens",
                columns: new[] { "UserId", "IsRevoked" });

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
                name: "AspNetUserLogins");

            migrationBuilder.DropTable(
                name: "AspNetUserTokens");

            migrationBuilder.DropTable(
                name: "Notifications");

            migrationBuilder.DropTable(
                name: "outbox_messages");

            migrationBuilder.DropTable(
                name: "RefreshTokens");

            migrationBuilder.DropTable(
                name: "RolePermission");

            migrationBuilder.DropTable(
                name: "RoleUser");

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
