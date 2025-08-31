using AppTemplate.Core.Domain.Abstractions;

namespace AppTemplate.Domain.Roles;

public sealed class Permission : Entity<Guid>
{
  // users
  public static readonly Permission UsersAdmin = new(Guid.Parse("c8a25b63-74ee-4375-98c8-e64107bb6d76"), "users", "users:admin");
  public static readonly Permission UsersRead = new(Guid.Parse("33261a4a-c423-4876-8f15-e40068aea5ca"), "users", "users:read");
  public static readonly Permission UsersCreate = new(Guid.Parse("9f79a54c-0b54-4de5-94b9-8582a5f32e78"), "users", "users:create");
  public static readonly Permission UsersUpdate = new(Guid.Parse("25bb194c-ea15-4339-9f45-5a895c51b626"), "users", "users:update");
  public static readonly Permission UsersDelete = new(Guid.Parse("559dd4ec-4d2e-479d-a0a9-5229ecc04fb4"), "users", "users:delete");

  // roles
  public static readonly Permission RolesAdmin = new(Guid.Parse("6203c108-2c3d-4ed3-ab3c-b119e7a7491a"), "roles", "roles:admin");
  public static readonly Permission RolesRead = new(Guid.Parse("d066e4ee-6af2-4857-bd40-b9b058fa2201"), "roles", "roles:read");
  public static readonly Permission RolesCreate = new(Guid.Parse("940c88ad-24fe-4d86-a982-fa5ea224edba"), "roles", "roles:create");
  public static readonly Permission RolesUpdate = new(Guid.Parse("346d3cc6-ac81-42b1-8539-cd53f42b6566"), "roles", "roles:update");
  public static readonly Permission RolesDelete = new(Guid.Parse("386e40e9-da38-4d2f-8d02-ac4cbaddf760"), "roles", "roles:delete");

  // permissions
  public static readonly Permission PermissionsRead = new(Guid.Parse("0eeb5f27-10fd-430a-9257-a8457107141a"), "permissions", "permissions:read");

  // auditlogs
  public static readonly Permission AuditLogsRead = new(Guid.Parse("3050d953-5dcf-4eb0-a18d-a3ce62a0dd3c"), "auditlogs", "auditlogs:read");

  // notifications
  public static readonly Permission NotificationsRead = new(Guid.Parse("a03a127b-9a03-46a0-b709-b6919f2598be"), "notifications", "notifications:read");
  public static readonly Permission NotificationsUpdate = new(Guid.Parse("a5585e9e-ec65-431b-9bb9-9bbc1663ebb8"), "notifications", "notifications:update");

  // statistics
  public static readonly Permission StatisticsRead = new(Guid.Parse("8f97aeb9-a9fd-470f-bae9-c9f5f0534d23"), "statistics", "statistics:read");

  // userfollows
  public static readonly Permission UserFollowsRead = new(Guid.Parse("ee1be42d-4341-4ac5-9390-2ec71eb54239"), "userfollows", "userfollows:read");
  public static readonly Permission UserFollowsCreate = new(Guid.Parse("78f1b087-3b45-48d0-8e16-9f04a760c294"), "userfollows", "userfollows:create");
  public static readonly Permission UserFollowsUpdate = new(Guid.Parse("22c1dbc9-bad6-4ebf-9c49-5577625f2b5f"), "userfollows", "userfollows:update");
  public static readonly Permission UserFollowsDelete = new(Guid.Parse("7b834e3d-ff31-416f-8b1e-ce1a7e9681e8"), "userfollows", "userfollows:delete");

  // entries
  public static readonly Permission EntriesAdmin = new(Guid.Parse("f14a636e-6a91-4b3e-9ea4-d9bbe8c36872"), "entries", "entries:admin");
  public static readonly Permission EntriesRead = new(Guid.Parse("cd552577-20c8-4e12-9685-a5c24ecd7fa8"), "entries", "entries:read");
  public static readonly Permission EntriesCreate = new(Guid.Parse("8116c67b-7f82-41b5-b9c4-a91e042e9257"), "entries", "entries:create");
  public static readonly Permission EntriesUpdate = new(Guid.Parse("b11364e1-dc05-422f-982a-6f365c1825a8"), "entries", "entries:update");
  public static readonly Permission EntriesDelete = new(Guid.Parse("638a9f7e-7bfe-4748-8947-f605c799d214"), "entries", "entries:delete");

  // titles
  public static readonly Permission TitlesAdmin = new(Guid.Parse("9185d676-db9e-4a8e-8286-7f4ea78ab022"), "titles", "titles:admin");
  public static readonly Permission TitlesRead = new(Guid.Parse("6030de9b-595c-474f-99ff-b654ad062e19"), "titles", "titles:read");
  public static readonly Permission TitlesCreate = new(Guid.Parse("272c16b9-da69-4065-b849-6fb45c9ff281"), "titles", "titles:create");
  public static readonly Permission TitlesUpdate = new(Guid.Parse("e033b219-f1c5-4c0c-b1f4-a756facb1819"), "titles", "titles:update");
  public static readonly Permission TitlesDelete = new(Guid.Parse("48c15b07-004d-44eb-a348-8ae63327a4b8"), "titles", "titles:delete");

  // titlefollows
  public static readonly Permission TitleFollowsRead = new(Guid.Parse("ec733d3f-cf8b-475c-8af6-5881cdb65dbe"), "titlefollows", "titlefollows:read");
  public static readonly Permission TitleFollowsCreate = new(Guid.Parse("1ff035a6-5d40-4a2d-aa9c-1d3182b3642e"), "titlefollows", "titlefollows:create");
  public static readonly Permission TitleFollowsUpdate = new(Guid.Parse("ac804e8f-abe7-4516-927f-045477dbe007"), "titlefollows", "titlefollows:update");
  public static readonly Permission TitleFollowsDelete = new(Guid.Parse("5341ddd6-5c42-477a-a155-33c51030f76b"), "titlefollows", "titlefollows:delete");

  // entrylikes
  public static readonly Permission EntryLikesRead = new(Guid.Parse("fbeb18d0-5e5f-4a38-aec7-6bb314408dc7"), "entrylikes", "entrylikes:read");
  public static readonly Permission EntryLikesCreate = new(Guid.Parse("5be9a36e-c59c-4a9a-a800-15f4f76ea80b"), "entrylikes", "entrylikes:create");
  public static readonly Permission EntryLikesUpdate = new(Guid.Parse("b2fe4d1c-59b9-4161-8eab-04380b45fd5e"), "entrylikes", "entrylikes:update");
  public static readonly Permission EntryLikesDelete = new(Guid.Parse("bb5cb9b7-75d0-4a9f-81d9-d02259b6ddf2"), "entrylikes", "entrylikes:delete");

  // entrybookmarks
  public static readonly Permission EntryBookmarksRead = new(Guid.Parse("5bc63b8d-2825-4f0e-aeae-234c7b2d930f"), "entrybookmarks", "entrybookmarks:read");
  public static readonly Permission EntryBookmarksCreate = new(Guid.Parse("30bb7d98-1a11-4152-9481-9a9d5fd39041"), "entrybookmarks", "entrybookmarks:create");
  public static readonly Permission EntryBookmarksUpdate = new(Guid.Parse("afe7776a-69a6-4134-96ea-a24829c67c9d"), "entrybookmarks", "entrybookmarks:update");
  public static readonly Permission EntryBookmarksDelete = new(Guid.Parse("8694cd0d-8987-44b4-b823-3f2e7f023919"), "entrybookmarks", "entrybookmarks:delete");

  // entryreports
  public static readonly Permission EntryReportsAdmin = new(Guid.Parse("859de6a2-9975-4d49-99fb-0a99cb8b3474"), "entryreports", "entryreports:admin");
  public static readonly Permission EntryReportsRead = new(Guid.Parse("e5f82d92-0610-4f63-ba1c-9bad3cbc09dd"), "entryreports", "entryreports:read");
  public static readonly Permission EntryReportsCreate = new(Guid.Parse("c42c3f31-f94a-474d-a159-2c826c031e34"), "entryreports", "entryreports:create");
  public static readonly Permission EntryReportsUpdate = new(Guid.Parse("a393e161-dc5e-4f0c-9fb9-f3a901b48149"), "entryreports", "entryreports:update");
  public static readonly Permission EntryReportsDelete = new(Guid.Parse("33ffe115-42c7-457a-8c63-1f8c5179bb5c"), "entryreports", "entryreports:delete");

  // featuredentries
  public static readonly Permission FeaturedEntriesAdmin = new(Guid.Parse("828736bc-1b6a-4b24-a55a-763fb6616970"), "featuredentries", "featuredentries:admin");
  public static readonly Permission FeaturedEntriesRead = new(Guid.Parse("fa8626a7-e34f-48c3-8b14-64b6889a36fc"), "featuredentries", "featuredentries:read");
  public static readonly Permission FeaturedEntriesCreate = new(Guid.Parse("5ca7e876-17bb-4b7a-a1d5-42ed0ee6baf3"), "featuredentries", "featuredentries:create");
  public static readonly Permission FeaturedEntriesUpdate = new(Guid.Parse("70a3a380-6e15-4b0e-b8b5-67591dbafcfa"), "featuredentries", "featuredentries:update");
  public static readonly Permission FeaturedEntriesDelete = new(Guid.Parse("481770d7-09f7-481a-83cd-f6aa808a072e"), "featuredentries", "featuredentries:delete");

  public Permission(Guid id, string feature, string name) : base(id)
  {
    Feature = feature;
    Name = name;
    Roles = new List<Role>([]);
  }

  public Permission()
  {
    Feature = string.Empty;
    Name = string.Empty;
    Roles = new List<Role>([]);
  }

  public string Feature { get; set; }
  public string Name { get; init; }
  public IList<Role> Roles { get; set; }

}
