using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AppTemplate.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddDeviceInfoToRefreshTokens : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Browser",
                table: "RefreshTokens",
                type: "character varying(50)",
                maxLength: 50,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "DeviceName",
                table: "RefreshTokens",
                type: "character varying(200)",
                maxLength: 200,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "IpAddress",
                table: "RefreshTokens",
                type: "character varying(50)",
                maxLength: 50,
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsCurrent",
                table: "RefreshTokens",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<DateTime>(
                name: "LastUsedAt",
                table: "RefreshTokens",
                type: "timestamp with time zone",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<string>(
                name: "Platform",
                table: "RefreshTokens",
                type: "character varying(50)",
                maxLength: 50,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "UserAgent",
                table: "RefreshTokens",
                type: "character varying(500)",
                maxLength: 500,
                nullable: true);

            migrationBuilder.UpdateData(
                table: "AppUsers",
                keyColumn: "Id",
                keyValue: new Guid("55c7f429-0916-4d84-8b76-d45185d89aa7"),
                column: "CreatedOnUtc",
                value: new DateTime(2025, 9, 29, 7, 48, 5, 953, DateTimeKind.Utc).AddTicks(5373));

            migrationBuilder.UpdateData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: "b3398ff2-1b43-4af7-812d-eb4347eecbb8",
                columns: new[] { "ConcurrencyStamp", "PasswordHash" },
                values: new object[] { "d4d9cb6c-0521-4808-bd1d-4ef96f920a03", "AQAAAAIAAYagAAAAEO7jKU2GXd4/F80aBla6mT6tb5Y9NLB3/DiFkW587+29E3XBxRCrEOJCk+M4JXqycA==" });

            migrationBuilder.UpdateData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: new Guid("0eeb5f27-10fd-430a-9257-a8457107141a"),
                column: "CreatedOnUtc",
                value: new DateTime(2025, 9, 29, 7, 48, 5, 963, DateTimeKind.Utc).AddTicks(9289));

            migrationBuilder.UpdateData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: new Guid("1ff035a6-5d40-4a2d-aa9c-1d3182b3642e"),
                column: "CreatedOnUtc",
                value: new DateTime(2025, 9, 29, 7, 48, 5, 963, DateTimeKind.Utc).AddTicks(9350));

            migrationBuilder.UpdateData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: new Guid("22c1dbc9-bad6-4ebf-9c49-5577625f2b5f"),
                column: "CreatedOnUtc",
                value: new DateTime(2025, 9, 29, 7, 48, 5, 963, DateTimeKind.Utc).AddTicks(9332));

            migrationBuilder.UpdateData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: new Guid("25bb194c-ea15-4339-9f45-5a895c51b626"),
                column: "CreatedOnUtc",
                value: new DateTime(2025, 9, 29, 7, 48, 5, 963, DateTimeKind.Utc).AddTicks(9276));

            migrationBuilder.UpdateData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: new Guid("272c16b9-da69-4065-b849-6fb45c9ff281"),
                column: "CreatedOnUtc",
                value: new DateTime(2025, 9, 29, 7, 48, 5, 963, DateTimeKind.Utc).AddTicks(9345));

            migrationBuilder.UpdateData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: new Guid("3050d953-5dcf-4eb0-a18d-a3ce62a0dd3c"),
                column: "CreatedOnUtc",
                value: new DateTime(2025, 9, 29, 7, 48, 5, 963, DateTimeKind.Utc).AddTicks(9290));

            migrationBuilder.UpdateData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: new Guid("30bb7d98-1a11-4152-9481-9a9d5fd39041"),
                column: "CreatedOnUtc",
                value: new DateTime(2025, 9, 29, 7, 48, 5, 963, DateTimeKind.Utc).AddTicks(9361));

            migrationBuilder.UpdateData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: new Guid("33261a4a-c423-4876-8f15-e40068aea5ca"),
                column: "CreatedOnUtc",
                value: new DateTime(2025, 9, 29, 7, 48, 5, 963, DateTimeKind.Utc).AddTicks(9269));

            migrationBuilder.UpdateData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: new Guid("33ffe115-42c7-457a-8c63-1f8c5179bb5c"),
                column: "CreatedOnUtc",
                value: new DateTime(2025, 9, 29, 7, 48, 5, 963, DateTimeKind.Utc).AddTicks(9371));

            migrationBuilder.UpdateData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: new Guid("346d3cc6-ac81-42b1-8539-cd53f42b6566"),
                column: "CreatedOnUtc",
                value: new DateTime(2025, 9, 29, 7, 48, 5, 963, DateTimeKind.Utc).AddTicks(9286));

            migrationBuilder.UpdateData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: new Guid("386e40e9-da38-4d2f-8d02-ac4cbaddf760"),
                column: "CreatedOnUtc",
                value: new DateTime(2025, 9, 29, 7, 48, 5, 963, DateTimeKind.Utc).AddTicks(9287));

            migrationBuilder.UpdateData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: new Guid("481770d7-09f7-481a-83cd-f6aa808a072e"),
                column: "CreatedOnUtc",
                value: new DateTime(2025, 9, 29, 7, 48, 5, 963, DateTimeKind.Utc).AddTicks(9378));

            migrationBuilder.UpdateData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: new Guid("48c15b07-004d-44eb-a348-8ae63327a4b8"),
                column: "CreatedOnUtc",
                value: new DateTime(2025, 9, 29, 7, 48, 5, 963, DateTimeKind.Utc).AddTicks(9347));

            migrationBuilder.UpdateData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: new Guid("5341ddd6-5c42-477a-a155-33c51030f76b"),
                column: "CreatedOnUtc",
                value: new DateTime(2025, 9, 29, 7, 48, 5, 963, DateTimeKind.Utc).AddTicks(9353));

            migrationBuilder.UpdateData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: new Guid("559dd4ec-4d2e-479d-a0a9-5229ecc04fb4"),
                column: "CreatedOnUtc",
                value: new DateTime(2025, 9, 29, 7, 48, 5, 963, DateTimeKind.Utc).AddTicks(9278));

            migrationBuilder.UpdateData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: new Guid("5bc63b8d-2825-4f0e-aeae-234c7b2d930f"),
                column: "CreatedOnUtc",
                value: new DateTime(2025, 9, 29, 7, 48, 5, 963, DateTimeKind.Utc).AddTicks(9360));

            migrationBuilder.UpdateData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: new Guid("5be9a36e-c59c-4a9a-a800-15f4f76ea80b"),
                column: "CreatedOnUtc",
                value: new DateTime(2025, 9, 29, 7, 48, 5, 963, DateTimeKind.Utc).AddTicks(9356));

            migrationBuilder.UpdateData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: new Guid("5ca7e876-17bb-4b7a-a1d5-42ed0ee6baf3"),
                column: "CreatedOnUtc",
                value: new DateTime(2025, 9, 29, 7, 48, 5, 963, DateTimeKind.Utc).AddTicks(9375));

            migrationBuilder.UpdateData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: new Guid("6030de9b-595c-474f-99ff-b654ad062e19"),
                column: "CreatedOnUtc",
                value: new DateTime(2025, 9, 29, 7, 48, 5, 963, DateTimeKind.Utc).AddTicks(9343));

            migrationBuilder.UpdateData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: new Guid("6203c108-2c3d-4ed3-ab3c-b119e7a7491a"),
                column: "CreatedOnUtc",
                value: new DateTime(2025, 9, 29, 7, 48, 5, 963, DateTimeKind.Utc).AddTicks(9280));

            migrationBuilder.UpdateData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: new Guid("638a9f7e-7bfe-4748-8947-f605c799d214"),
                column: "CreatedOnUtc",
                value: new DateTime(2025, 9, 29, 7, 48, 5, 963, DateTimeKind.Utc).AddTicks(9340));

            migrationBuilder.UpdateData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: new Guid("70a3a380-6e15-4b0e-b8b5-67591dbafcfa"),
                column: "CreatedOnUtc",
                value: new DateTime(2025, 9, 29, 7, 48, 5, 963, DateTimeKind.Utc).AddTicks(9377));

            migrationBuilder.UpdateData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: new Guid("78f1b087-3b45-48d0-8e16-9f04a760c294"),
                column: "CreatedOnUtc",
                value: new DateTime(2025, 9, 29, 7, 48, 5, 963, DateTimeKind.Utc).AddTicks(9330));

            migrationBuilder.UpdateData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: new Guid("7b834e3d-ff31-416f-8b1e-ce1a7e9681e8"),
                column: "CreatedOnUtc",
                value: new DateTime(2025, 9, 29, 7, 48, 5, 963, DateTimeKind.Utc).AddTicks(9333));

            migrationBuilder.UpdateData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: new Guid("8116c67b-7f82-41b5-b9c4-a91e042e9257"),
                column: "CreatedOnUtc",
                value: new DateTime(2025, 9, 29, 7, 48, 5, 963, DateTimeKind.Utc).AddTicks(9338));

            migrationBuilder.UpdateData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: new Guid("828736bc-1b6a-4b24-a55a-763fb6616970"),
                column: "CreatedOnUtc",
                value: new DateTime(2025, 9, 29, 7, 48, 5, 963, DateTimeKind.Utc).AddTicks(9372));

            migrationBuilder.UpdateData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: new Guid("859de6a2-9975-4d49-99fb-0a99cb8b3474"),
                column: "CreatedOnUtc",
                value: new DateTime(2025, 9, 29, 7, 48, 5, 963, DateTimeKind.Utc).AddTicks(9366));

            migrationBuilder.UpdateData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: new Guid("8694cd0d-8987-44b4-b823-3f2e7f023919"),
                column: "CreatedOnUtc",
                value: new DateTime(2025, 9, 29, 7, 48, 5, 963, DateTimeKind.Utc).AddTicks(9364));

            migrationBuilder.UpdateData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: new Guid("8f97aeb9-a9fd-470f-bae9-c9f5f0534d23"),
                column: "CreatedOnUtc",
                value: new DateTime(2025, 9, 29, 7, 48, 5, 963, DateTimeKind.Utc).AddTicks(9327));

            migrationBuilder.UpdateData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: new Guid("9185d676-db9e-4a8e-8286-7f4ea78ab022"),
                column: "CreatedOnUtc",
                value: new DateTime(2025, 9, 29, 7, 48, 5, 963, DateTimeKind.Utc).AddTicks(9342));

            migrationBuilder.UpdateData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: new Guid("940c88ad-24fe-4d86-a982-fa5ea224edba"),
                column: "CreatedOnUtc",
                value: new DateTime(2025, 9, 29, 7, 48, 5, 963, DateTimeKind.Utc).AddTicks(9284));

            migrationBuilder.UpdateData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: new Guid("9f79a54c-0b54-4de5-94b9-8582a5f32e78"),
                column: "CreatedOnUtc",
                value: new DateTime(2025, 9, 29, 7, 48, 5, 963, DateTimeKind.Utc).AddTicks(9275));

            migrationBuilder.UpdateData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: new Guid("a03a127b-9a03-46a0-b709-b6919f2598be"),
                column: "CreatedOnUtc",
                value: new DateTime(2025, 9, 29, 7, 48, 5, 963, DateTimeKind.Utc).AddTicks(9324));

            migrationBuilder.UpdateData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: new Guid("a393e161-dc5e-4f0c-9fb9-f3a901b48149"),
                column: "CreatedOnUtc",
                value: new DateTime(2025, 9, 29, 7, 48, 5, 963, DateTimeKind.Utc).AddTicks(9370));

            migrationBuilder.UpdateData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: new Guid("a5585e9e-ec65-431b-9bb9-9bbc1663ebb8"),
                column: "CreatedOnUtc",
                value: new DateTime(2025, 9, 29, 7, 48, 5, 963, DateTimeKind.Utc).AddTicks(9325));

            migrationBuilder.UpdateData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: new Guid("ac804e8f-abe7-4516-927f-045477dbe007"),
                column: "CreatedOnUtc",
                value: new DateTime(2025, 9, 29, 7, 48, 5, 963, DateTimeKind.Utc).AddTicks(9352));

            migrationBuilder.UpdateData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: new Guid("afe7776a-69a6-4134-96ea-a24829c67c9d"),
                column: "CreatedOnUtc",
                value: new DateTime(2025, 9, 29, 7, 48, 5, 963, DateTimeKind.Utc).AddTicks(9363));

            migrationBuilder.UpdateData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: new Guid("b11364e1-dc05-422f-982a-6f365c1825a8"),
                column: "CreatedOnUtc",
                value: new DateTime(2025, 9, 29, 7, 48, 5, 963, DateTimeKind.Utc).AddTicks(9339));

            migrationBuilder.UpdateData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: new Guid("b2fe4d1c-59b9-4161-8eab-04380b45fd5e"),
                column: "CreatedOnUtc",
                value: new DateTime(2025, 9, 29, 7, 48, 5, 963, DateTimeKind.Utc).AddTicks(9357));

            migrationBuilder.UpdateData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: new Guid("bb5cb9b7-75d0-4a9f-81d9-d02259b6ddf2"),
                column: "CreatedOnUtc",
                value: new DateTime(2025, 9, 29, 7, 48, 5, 963, DateTimeKind.Utc).AddTicks(9358));

            migrationBuilder.UpdateData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: new Guid("c42c3f31-f94a-474d-a159-2c826c031e34"),
                column: "CreatedOnUtc",
                value: new DateTime(2025, 9, 29, 7, 48, 5, 963, DateTimeKind.Utc).AddTicks(9368));

            migrationBuilder.UpdateData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: new Guid("c8a25b63-74ee-4375-98c8-e64107bb6d76"),
                column: "CreatedOnUtc",
                value: new DateTime(2025, 9, 29, 7, 48, 5, 963, DateTimeKind.Utc).AddTicks(7243));

            migrationBuilder.UpdateData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: new Guid("cd552577-20c8-4e12-9685-a5c24ecd7fa8"),
                column: "CreatedOnUtc",
                value: new DateTime(2025, 9, 29, 7, 48, 5, 963, DateTimeKind.Utc).AddTicks(9336));

            migrationBuilder.UpdateData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: new Guid("d066e4ee-6af2-4857-bd40-b9b058fa2201"),
                column: "CreatedOnUtc",
                value: new DateTime(2025, 9, 29, 7, 48, 5, 963, DateTimeKind.Utc).AddTicks(9282));

            migrationBuilder.UpdateData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: new Guid("e033b219-f1c5-4c0c-b1f4-a756facb1819"),
                column: "CreatedOnUtc",
                value: new DateTime(2025, 9, 29, 7, 48, 5, 963, DateTimeKind.Utc).AddTicks(9346));

            migrationBuilder.UpdateData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: new Guid("e5f82d92-0610-4f63-ba1c-9bad3cbc09dd"),
                column: "CreatedOnUtc",
                value: new DateTime(2025, 9, 29, 7, 48, 5, 963, DateTimeKind.Utc).AddTicks(9367));

            migrationBuilder.UpdateData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: new Guid("ec733d3f-cf8b-475c-8af6-5881cdb65dbe"),
                column: "CreatedOnUtc",
                value: new DateTime(2025, 9, 29, 7, 48, 5, 963, DateTimeKind.Utc).AddTicks(9349));

            migrationBuilder.UpdateData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: new Guid("ee1be42d-4341-4ac5-9390-2ec71eb54239"),
                column: "CreatedOnUtc",
                value: new DateTime(2025, 9, 29, 7, 48, 5, 963, DateTimeKind.Utc).AddTicks(9328));

            migrationBuilder.UpdateData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: new Guid("f14a636e-6a91-4b3e-9ea4-d9bbe8c36872"),
                column: "CreatedOnUtc",
                value: new DateTime(2025, 9, 29, 7, 48, 5, 963, DateTimeKind.Utc).AddTicks(9335));

            migrationBuilder.UpdateData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: new Guid("fa8626a7-e34f-48c3-8b14-64b6889a36fc"),
                column: "CreatedOnUtc",
                value: new DateTime(2025, 9, 29, 7, 48, 5, 963, DateTimeKind.Utc).AddTicks(9374));

            migrationBuilder.UpdateData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: new Guid("fbeb18d0-5e5f-4a38-aec7-6bb314408dc7"),
                column: "CreatedOnUtc",
                value: new DateTime(2025, 9, 29, 7, 48, 5, 963, DateTimeKind.Utc).AddTicks(9354));

            migrationBuilder.UpdateData(
                table: "Roles",
                keyColumn: "Id",
                keyValue: new Guid("4b606d86-3537-475a-aa20-26aadd8f5cfd"),
                column: "CreatedOnUtc",
                value: new DateTime(2025, 9, 29, 7, 48, 5, 969, DateTimeKind.Utc).AddTicks(5568));

            migrationBuilder.UpdateData(
                table: "Roles",
                keyColumn: "Id",
                keyValue: new Guid("5dc6ec47-5b7c-4c2b-86cd-3a671834e56e"),
                column: "CreatedOnUtc",
                value: new DateTime(2025, 9, 29, 7, 48, 5, 969, DateTimeKind.Utc).AddTicks(6102));

            migrationBuilder.CreateIndex(
                name: "IX_RefreshTokens_UserId_IsRevoked",
                table: "RefreshTokens",
                columns: new[] { "UserId", "IsRevoked" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_RefreshTokens_UserId_IsRevoked",
                table: "RefreshTokens");

            migrationBuilder.DropColumn(
                name: "Browser",
                table: "RefreshTokens");

            migrationBuilder.DropColumn(
                name: "DeviceName",
                table: "RefreshTokens");

            migrationBuilder.DropColumn(
                name: "IpAddress",
                table: "RefreshTokens");

            migrationBuilder.DropColumn(
                name: "IsCurrent",
                table: "RefreshTokens");

            migrationBuilder.DropColumn(
                name: "LastUsedAt",
                table: "RefreshTokens");

            migrationBuilder.DropColumn(
                name: "Platform",
                table: "RefreshTokens");

            migrationBuilder.DropColumn(
                name: "UserAgent",
                table: "RefreshTokens");

            migrationBuilder.UpdateData(
                table: "AppUsers",
                keyColumn: "Id",
                keyValue: new Guid("55c7f429-0916-4d84-8b76-d45185d89aa7"),
                column: "CreatedOnUtc",
                value: new DateTime(2025, 9, 24, 10, 28, 19, 379, DateTimeKind.Utc).AddTicks(5462));

            migrationBuilder.UpdateData(
                table: "AspNetUsers",
                keyColumn: "Id",
                keyValue: "b3398ff2-1b43-4af7-812d-eb4347eecbb8",
                columns: new[] { "ConcurrencyStamp", "PasswordHash" },
                values: new object[] { "78c2c6c5-0ea5-4a9c-bf2a-32559e976b32", "AQAAAAIAAYagAAAAEEhyjJlhyX4oMohEF3oTkbjTd4zZ9n6ubdIfe966KCb7Qc6nGFQEojqLK8GvTh4b5g==" });

            migrationBuilder.UpdateData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: new Guid("0eeb5f27-10fd-430a-9257-a8457107141a"),
                column: "CreatedOnUtc",
                value: new DateTime(2025, 9, 24, 10, 28, 19, 388, DateTimeKind.Utc).AddTicks(2743));

            migrationBuilder.UpdateData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: new Guid("1ff035a6-5d40-4a2d-aa9c-1d3182b3642e"),
                column: "CreatedOnUtc",
                value: new DateTime(2025, 9, 24, 10, 28, 19, 388, DateTimeKind.Utc).AddTicks(2811));

            migrationBuilder.UpdateData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: new Guid("22c1dbc9-bad6-4ebf-9c49-5577625f2b5f"),
                column: "CreatedOnUtc",
                value: new DateTime(2025, 9, 24, 10, 28, 19, 388, DateTimeKind.Utc).AddTicks(2793));

            migrationBuilder.UpdateData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: new Guid("25bb194c-ea15-4339-9f45-5a895c51b626"),
                column: "CreatedOnUtc",
                value: new DateTime(2025, 9, 24, 10, 28, 19, 388, DateTimeKind.Utc).AddTicks(2734));

            migrationBuilder.UpdateData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: new Guid("272c16b9-da69-4065-b849-6fb45c9ff281"),
                column: "CreatedOnUtc",
                value: new DateTime(2025, 9, 24, 10, 28, 19, 388, DateTimeKind.Utc).AddTicks(2805));

            migrationBuilder.UpdateData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: new Guid("3050d953-5dcf-4eb0-a18d-a3ce62a0dd3c"),
                column: "CreatedOnUtc",
                value: new DateTime(2025, 9, 24, 10, 28, 19, 388, DateTimeKind.Utc).AddTicks(2765));

            migrationBuilder.UpdateData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: new Guid("30bb7d98-1a11-4152-9481-9a9d5fd39041"),
                column: "CreatedOnUtc",
                value: new DateTime(2025, 9, 24, 10, 28, 19, 388, DateTimeKind.Utc).AddTicks(2821));

            migrationBuilder.UpdateData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: new Guid("33261a4a-c423-4876-8f15-e40068aea5ca"),
                column: "CreatedOnUtc",
                value: new DateTime(2025, 9, 24, 10, 28, 19, 388, DateTimeKind.Utc).AddTicks(2729));

            migrationBuilder.UpdateData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: new Guid("33ffe115-42c7-457a-8c63-1f8c5179bb5c"),
                column: "CreatedOnUtc",
                value: new DateTime(2025, 9, 24, 10, 28, 19, 388, DateTimeKind.Utc).AddTicks(2830));

            migrationBuilder.UpdateData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: new Guid("346d3cc6-ac81-42b1-8539-cd53f42b6566"),
                column: "CreatedOnUtc",
                value: new DateTime(2025, 9, 24, 10, 28, 19, 388, DateTimeKind.Utc).AddTicks(2741));

            migrationBuilder.UpdateData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: new Guid("386e40e9-da38-4d2f-8d02-ac4cbaddf760"),
                column: "CreatedOnUtc",
                value: new DateTime(2025, 9, 24, 10, 28, 19, 388, DateTimeKind.Utc).AddTicks(2742));

            migrationBuilder.UpdateData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: new Guid("481770d7-09f7-481a-83cd-f6aa808a072e"),
                column: "CreatedOnUtc",
                value: new DateTime(2025, 9, 24, 10, 28, 19, 388, DateTimeKind.Utc).AddTicks(2837));

            migrationBuilder.UpdateData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: new Guid("48c15b07-004d-44eb-a348-8ae63327a4b8"),
                column: "CreatedOnUtc",
                value: new DateTime(2025, 9, 24, 10, 28, 19, 388, DateTimeKind.Utc).AddTicks(2808));

            migrationBuilder.UpdateData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: new Guid("5341ddd6-5c42-477a-a155-33c51030f76b"),
                column: "CreatedOnUtc",
                value: new DateTime(2025, 9, 24, 10, 28, 19, 388, DateTimeKind.Utc).AddTicks(2813));

            migrationBuilder.UpdateData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: new Guid("559dd4ec-4d2e-479d-a0a9-5229ecc04fb4"),
                column: "CreatedOnUtc",
                value: new DateTime(2025, 9, 24, 10, 28, 19, 388, DateTimeKind.Utc).AddTicks(2735));

            migrationBuilder.UpdateData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: new Guid("5bc63b8d-2825-4f0e-aeae-234c7b2d930f"),
                column: "CreatedOnUtc",
                value: new DateTime(2025, 9, 24, 10, 28, 19, 388, DateTimeKind.Utc).AddTicks(2820));

            migrationBuilder.UpdateData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: new Guid("5be9a36e-c59c-4a9a-a800-15f4f76ea80b"),
                column: "CreatedOnUtc",
                value: new DateTime(2025, 9, 24, 10, 28, 19, 388, DateTimeKind.Utc).AddTicks(2816));

            migrationBuilder.UpdateData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: new Guid("5ca7e876-17bb-4b7a-a1d5-42ed0ee6baf3"),
                column: "CreatedOnUtc",
                value: new DateTime(2025, 9, 24, 10, 28, 19, 388, DateTimeKind.Utc).AddTicks(2834));

            migrationBuilder.UpdateData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: new Guid("6030de9b-595c-474f-99ff-b654ad062e19"),
                column: "CreatedOnUtc",
                value: new DateTime(2025, 9, 24, 10, 28, 19, 388, DateTimeKind.Utc).AddTicks(2804));

            migrationBuilder.UpdateData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: new Guid("6203c108-2c3d-4ed3-ab3c-b119e7a7491a"),
                column: "CreatedOnUtc",
                value: new DateTime(2025, 9, 24, 10, 28, 19, 388, DateTimeKind.Utc).AddTicks(2737));

            migrationBuilder.UpdateData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: new Guid("638a9f7e-7bfe-4748-8947-f605c799d214"),
                column: "CreatedOnUtc",
                value: new DateTime(2025, 9, 24, 10, 28, 19, 388, DateTimeKind.Utc).AddTicks(2801));

            migrationBuilder.UpdateData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: new Guid("70a3a380-6e15-4b0e-b8b5-67591dbafcfa"),
                column: "CreatedOnUtc",
                value: new DateTime(2025, 9, 24, 10, 28, 19, 388, DateTimeKind.Utc).AddTicks(2836));

            migrationBuilder.UpdateData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: new Guid("78f1b087-3b45-48d0-8e16-9f04a760c294"),
                column: "CreatedOnUtc",
                value: new DateTime(2025, 9, 24, 10, 28, 19, 388, DateTimeKind.Utc).AddTicks(2787));

            migrationBuilder.UpdateData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: new Guid("7b834e3d-ff31-416f-8b1e-ce1a7e9681e8"),
                column: "CreatedOnUtc",
                value: new DateTime(2025, 9, 24, 10, 28, 19, 388, DateTimeKind.Utc).AddTicks(2795));

            migrationBuilder.UpdateData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: new Guid("8116c67b-7f82-41b5-b9c4-a91e042e9257"),
                column: "CreatedOnUtc",
                value: new DateTime(2025, 9, 24, 10, 28, 19, 388, DateTimeKind.Utc).AddTicks(2799));

            migrationBuilder.UpdateData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: new Guid("828736bc-1b6a-4b24-a55a-763fb6616970"),
                column: "CreatedOnUtc",
                value: new DateTime(2025, 9, 24, 10, 28, 19, 388, DateTimeKind.Utc).AddTicks(2832));

            migrationBuilder.UpdateData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: new Guid("859de6a2-9975-4d49-99fb-0a99cb8b3474"),
                column: "CreatedOnUtc",
                value: new DateTime(2025, 9, 24, 10, 28, 19, 388, DateTimeKind.Utc).AddTicks(2825));

            migrationBuilder.UpdateData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: new Guid("8694cd0d-8987-44b4-b823-3f2e7f023919"),
                column: "CreatedOnUtc",
                value: new DateTime(2025, 9, 24, 10, 28, 19, 388, DateTimeKind.Utc).AddTicks(2824));

            migrationBuilder.UpdateData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: new Guid("8f97aeb9-a9fd-470f-bae9-c9f5f0534d23"),
                column: "CreatedOnUtc",
                value: new DateTime(2025, 9, 24, 10, 28, 19, 388, DateTimeKind.Utc).AddTicks(2785));

            migrationBuilder.UpdateData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: new Guid("9185d676-db9e-4a8e-8286-7f4ea78ab022"),
                column: "CreatedOnUtc",
                value: new DateTime(2025, 9, 24, 10, 28, 19, 388, DateTimeKind.Utc).AddTicks(2803));

            migrationBuilder.UpdateData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: new Guid("940c88ad-24fe-4d86-a982-fa5ea224edba"),
                column: "CreatedOnUtc",
                value: new DateTime(2025, 9, 24, 10, 28, 19, 388, DateTimeKind.Utc).AddTicks(2739));

            migrationBuilder.UpdateData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: new Guid("9f79a54c-0b54-4de5-94b9-8582a5f32e78"),
                column: "CreatedOnUtc",
                value: new DateTime(2025, 9, 24, 10, 28, 19, 388, DateTimeKind.Utc).AddTicks(2733));

            migrationBuilder.UpdateData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: new Guid("a03a127b-9a03-46a0-b709-b6919f2598be"),
                column: "CreatedOnUtc",
                value: new DateTime(2025, 9, 24, 10, 28, 19, 388, DateTimeKind.Utc).AddTicks(2782));

            migrationBuilder.UpdateData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: new Guid("a393e161-dc5e-4f0c-9fb9-f3a901b48149"),
                column: "CreatedOnUtc",
                value: new DateTime(2025, 9, 24, 10, 28, 19, 388, DateTimeKind.Utc).AddTicks(2829));

            migrationBuilder.UpdateData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: new Guid("a5585e9e-ec65-431b-9bb9-9bbc1663ebb8"),
                column: "CreatedOnUtc",
                value: new DateTime(2025, 9, 24, 10, 28, 19, 388, DateTimeKind.Utc).AddTicks(2783));

            migrationBuilder.UpdateData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: new Guid("ac804e8f-abe7-4516-927f-045477dbe007"),
                column: "CreatedOnUtc",
                value: new DateTime(2025, 9, 24, 10, 28, 19, 388, DateTimeKind.Utc).AddTicks(2812));

            migrationBuilder.UpdateData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: new Guid("afe7776a-69a6-4134-96ea-a24829c67c9d"),
                column: "CreatedOnUtc",
                value: new DateTime(2025, 9, 24, 10, 28, 19, 388, DateTimeKind.Utc).AddTicks(2822));

            migrationBuilder.UpdateData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: new Guid("b11364e1-dc05-422f-982a-6f365c1825a8"),
                column: "CreatedOnUtc",
                value: new DateTime(2025, 9, 24, 10, 28, 19, 388, DateTimeKind.Utc).AddTicks(2800));

            migrationBuilder.UpdateData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: new Guid("b2fe4d1c-59b9-4161-8eab-04380b45fd5e"),
                column: "CreatedOnUtc",
                value: new DateTime(2025, 9, 24, 10, 28, 19, 388, DateTimeKind.Utc).AddTicks(2817));

            migrationBuilder.UpdateData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: new Guid("bb5cb9b7-75d0-4a9f-81d9-d02259b6ddf2"),
                column: "CreatedOnUtc",
                value: new DateTime(2025, 9, 24, 10, 28, 19, 388, DateTimeKind.Utc).AddTicks(2818));

            migrationBuilder.UpdateData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: new Guid("c42c3f31-f94a-474d-a159-2c826c031e34"),
                column: "CreatedOnUtc",
                value: new DateTime(2025, 9, 24, 10, 28, 19, 388, DateTimeKind.Utc).AddTicks(2828));

            migrationBuilder.UpdateData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: new Guid("c8a25b63-74ee-4375-98c8-e64107bb6d76"),
                column: "CreatedOnUtc",
                value: new DateTime(2025, 9, 24, 10, 28, 19, 388, DateTimeKind.Utc).AddTicks(1563));

            migrationBuilder.UpdateData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: new Guid("cd552577-20c8-4e12-9685-a5c24ecd7fa8"),
                column: "CreatedOnUtc",
                value: new DateTime(2025, 9, 24, 10, 28, 19, 388, DateTimeKind.Utc).AddTicks(2797));

            migrationBuilder.UpdateData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: new Guid("d066e4ee-6af2-4857-bd40-b9b058fa2201"),
                column: "CreatedOnUtc",
                value: new DateTime(2025, 9, 24, 10, 28, 19, 388, DateTimeKind.Utc).AddTicks(2738));

            migrationBuilder.UpdateData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: new Guid("e033b219-f1c5-4c0c-b1f4-a756facb1819"),
                column: "CreatedOnUtc",
                value: new DateTime(2025, 9, 24, 10, 28, 19, 388, DateTimeKind.Utc).AddTicks(2807));

            migrationBuilder.UpdateData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: new Guid("e5f82d92-0610-4f63-ba1c-9bad3cbc09dd"),
                column: "CreatedOnUtc",
                value: new DateTime(2025, 9, 24, 10, 28, 19, 388, DateTimeKind.Utc).AddTicks(2826));

            migrationBuilder.UpdateData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: new Guid("ec733d3f-cf8b-475c-8af6-5881cdb65dbe"),
                column: "CreatedOnUtc",
                value: new DateTime(2025, 9, 24, 10, 28, 19, 388, DateTimeKind.Utc).AddTicks(2809));

            migrationBuilder.UpdateData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: new Guid("ee1be42d-4341-4ac5-9390-2ec71eb54239"),
                column: "CreatedOnUtc",
                value: new DateTime(2025, 9, 24, 10, 28, 19, 388, DateTimeKind.Utc).AddTicks(2786));

            migrationBuilder.UpdateData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: new Guid("f14a636e-6a91-4b3e-9ea4-d9bbe8c36872"),
                column: "CreatedOnUtc",
                value: new DateTime(2025, 9, 24, 10, 28, 19, 388, DateTimeKind.Utc).AddTicks(2796));

            migrationBuilder.UpdateData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: new Guid("fa8626a7-e34f-48c3-8b14-64b6889a36fc"),
                column: "CreatedOnUtc",
                value: new DateTime(2025, 9, 24, 10, 28, 19, 388, DateTimeKind.Utc).AddTicks(2833));

            migrationBuilder.UpdateData(
                table: "Permissions",
                keyColumn: "Id",
                keyValue: new Guid("fbeb18d0-5e5f-4a38-aec7-6bb314408dc7"),
                column: "CreatedOnUtc",
                value: new DateTime(2025, 9, 24, 10, 28, 19, 388, DateTimeKind.Utc).AddTicks(2815));

            migrationBuilder.UpdateData(
                table: "Roles",
                keyColumn: "Id",
                keyValue: new Guid("4b606d86-3537-475a-aa20-26aadd8f5cfd"),
                column: "CreatedOnUtc",
                value: new DateTime(2025, 9, 24, 10, 28, 19, 392, DateTimeKind.Utc).AddTicks(9846));

            migrationBuilder.UpdateData(
                table: "Roles",
                keyColumn: "Id",
                keyValue: new Guid("5dc6ec47-5b7c-4c2b-86cd-3a671834e56e"),
                column: "CreatedOnUtc",
                value: new DateTime(2025, 9, 24, 10, 28, 19, 393, DateTimeKind.Utc).AddTicks(353));
        }
    }
}
