using AppTemplate.Application.Authentication.Models;
using AppTemplate.Application.Services.Clock;
using AppTemplate.Domain;
using AppTemplate.Domain.AppUsers;
using AppTemplate.Domain.Notifications;
using AppTemplate.Domain.OutboxMessages;
using AppTemplate.Domain.Roles;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Newtonsoft.Json;
using System.Reflection.Emit;

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
  public DbSet<RefreshToken> RefreshTokens { get; set; }

  public ApplicationDbContext(
      DbContextOptions<ApplicationDbContext> options,
      IDateTimeProvider dateTimeProvider)
      : base(options)
  {
    _dateTimeProvider = dateTimeProvider;
  }

  protected override void OnModelCreating(ModelBuilder builder)
  {
    base.OnModelCreating(builder);

    builder.Entity<IdentityUser>(b =>
    {
      b.Property(u => u.UserName)
          .IsRequired();
      b.HasIndex(u => u.UserName)
          .IsUnique();

      b.Property(u => u.Email)
          .IsRequired();
      b.HasIndex(u => u.Email)
          .IsUnique();
    });

    var hasher = new PasswordHasher<IdentityUser>();
    var dummyUser = new IdentityUser();
    string password = "Passw0rd!";
    string hashedPassword = hasher.HashPassword(dummyUser, password);

    builder.Entity<IdentityUser>().HasData(
    new IdentityUser
    {
      Id = "b3398ff2-1b43-4af7-812d-eb4347eecbb8",
      UserName = "admin",
      NormalizedUserName = "ADMIN",
      Email = "admin@example.com",
      NormalizedEmail = "ADMIN@EXAMPLE.COM",
      EmailConfirmed = true,
      PasswordHash = hashedPassword,
      SecurityStamp = "fixed-security-stamp-for-seeding" // <-- Use a fixed value instead of Guid.NewGuid()
    });

    builder.Entity<RefreshToken>(entity =>
    {
      entity.HasKey(e => e.Token);
      entity.Property(e => e.Token).HasMaxLength(200);
      entity.Property(e => e.UserId).HasMaxLength(450);
      entity.Property(e => e.DeviceName).HasMaxLength(200);
      entity.Property(e => e.UserAgent).HasMaxLength(500);
      entity.Property(e => e.IpAddress).HasMaxLength(50);
      entity.Property(e => e.Platform).HasMaxLength(50);
      entity.Property(e => e.Browser).HasMaxLength(50);
      entity.HasIndex(e => e.UserId);
      entity.HasIndex(e => e.ExpiresAt);
      entity.HasIndex(e => e.IsRevoked);
      entity.HasIndex(e => new { e.UserId, e.IsRevoked });
    });

    builder.ApplyConfigurationsFromAssembly(typeof(ApplicationDbContext).Assembly);
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
