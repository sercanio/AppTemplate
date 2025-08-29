using AppTemplate.Core.Application.Abstractions.Clock;
using AppTemplate.Core.Domain.Abstractions;
using AppTemplate.Core.Infrastructure.Outbox;
using AppTemplate.Domain.AppUsers;
using AppTemplate.Domain.Notifications;
using AppTemplate.Domain.Roles;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Newtonsoft.Json;

namespace AppTemplate.Infrastructure;

public class ApplicationDbContext : IdentityDbContext<IdentityUser>, IUnitOfWork
{
  private readonly IDateTimeProvider _dateTimeProvider;
  private static readonly JsonSerializerSettings JsonSerializerSettings = new()
  {
    TypeNameHandling = TypeNameHandling.All
  };

  public DbSet<OutboxMessage> OutboxMessages { get; set; }
  public DbSet<AppUser> AppUsers { get; set; }
  public DbSet<Role> Roles { get; set; }
  public DbSet<Permission> Permissions { get; set; }
  public DbSet<Notification> Notifications { get; set; }

  public ApplicationDbContext(
      DbContextOptions<ApplicationDbContext> options,
      IDateTimeProvider dateTimeProvider)
      : base(options)
  {
    _dateTimeProvider = dateTimeProvider;
  }

  protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
  {
    optionsBuilder.ConfigureWarnings(warnings => warnings.Ignore(RelationalEventId.PendingModelChangesWarning));
  }

  protected override void OnModelCreating(ModelBuilder modelBuilder)
  {
    base.OnModelCreating(modelBuilder);

    var hasher = new PasswordHasher<IdentityUser>();
    var dummyUser = new IdentityUser();
    string password = "Passw0rd!";
    string hashedPassword = hasher.HashPassword(dummyUser, password);

    modelBuilder.Entity<IdentityUser>().HasData(
    new IdentityUser
    {
      Id = "b3398ff2-1b43-4af7-812d-eb4347eecbb8",
      UserName = "admin",
      NormalizedUserName = "ADMIN",
      Email = "admin@example.com",
      NormalizedEmail = "ADMIN@EXAMPLE.COM",
      EmailConfirmed = true,
      PasswordHash = hashedPassword,
      SecurityStamp = Guid.NewGuid().ToString("D")
    });

    modelBuilder.ApplyConfigurationsFromAssembly(typeof(ApplicationDbContext).Assembly);
  }

  public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
  {
    try
    {
      AddDomainEventsAsOutboxMessages();
      return await base.SaveChangesAsync(cancellationToken);
    }
    catch (DbUpdateConcurrencyException ex)
    {
      throw new Exception("Concurrency exception occurred.", ex);
    }
  }

  public void ClearChangeTracker()
  {
    ChangeTracker.Clear();
  }

  private void AddDomainEventsAsOutboxMessages()
  {
    var outboxMessages = new List<OutboxMessage>();

    // Handle Entity<Guid>
    var guidEntities = ChangeTracker
        .Entries<Entity<Guid>>()
        .Where(entry => entry.Entity.GetDomainEvents().Any())
        .ToList();

    // Handle Entity<int>
    var intEntities = ChangeTracker
        .Entries<Entity<int>>()
        .Where(entry => entry.Entity.GetDomainEvents().Any())
        .ToList();

    // Process Guid entities
    var guidOutboxMessages = guidEntities
        .SelectMany(entry =>
        {
          var events = entry.Entity.GetDomainEvents();
          entry.Entity.ClearDomainEvents();
          return events;
        })
        .Select(domainEvent => new OutboxMessage(
            Guid.NewGuid(),
            _dateTimeProvider.UtcNow,
            domainEvent.GetType().Name,
            JsonConvert.SerializeObject(domainEvent, JsonSerializerSettings)));

    // Process Int entities
    var intOutboxMessages = intEntities
        .SelectMany(entry =>
        {
          var events = entry.Entity.GetDomainEvents();
          entry.Entity.ClearDomainEvents();
          return events;
        })
        .Select(domainEvent => new OutboxMessage(
            Guid.NewGuid(),
            _dateTimeProvider.UtcNow,
            domainEvent.GetType().Name,
            JsonConvert.SerializeObject(domainEvent, JsonSerializerSettings)));

    outboxMessages.AddRange(guidOutboxMessages);
    outboxMessages.AddRange(intOutboxMessages);
    OutboxMessages.AddRange(outboxMessages);
  }
}
