using AppTemplate.Application.Data.Pagination;
using AppTemplate.Application.Repositories;
using AppTemplate.Application.Services.Roles;
using AppTemplate.Domain.Roles;
using Ardalis.Result;
using Moq;
using System.Linq.Expressions;

namespace AppTemplate.Application.Tests.Unit.Services.RolesServiceTests;

[Trait("Category", "Unit")]
public class RolesServiceUnitTests
{
    private readonly Mock<IRolesRepository> _rolesRepositoryMock;
    private readonly RolesService _service;

    public RolesServiceUnitTests()
    {
        _rolesRepositoryMock = new Mock<IRolesRepository>();
        _service = new RolesService(_rolesRepositoryMock.Object);
    }

    [Fact]
    public async Task AddAsync_CallsRepositoryAddAsync()
    {
        var role = Role.Create("TestRole", "TestDisplayName", Guid.NewGuid());
        await _service.AddAsync(role);
        _rolesRepositoryMock.Verify(r => r.AddAsync(role, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public void Delete_CallsRepositoryDelete()
    {
        var role = Role.Create("TestRole", "TestDisplayName", Guid.NewGuid());
        _service.Delete(role);
        _rolesRepositoryMock.Verify(r => r.Delete(role, true, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public void Update_CallsRepositoryUpdate()
    {
        var role = Role.Create("TestRole", "TestDisplayName", Guid.NewGuid());
        _service.Update(role);
        _rolesRepositoryMock.Verify(r => r.Update(role, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task GetAsync_ReturnsRole()
    {
        var role = Role.Create("TestRole", "TestDisplayName", Guid.NewGuid());
        _rolesRepositoryMock
            .Setup(r => r.GetAsync(It.IsAny<Expression<Func<Role, bool>>>(), false, null, true, It.IsAny<CancellationToken>()))
            .ReturnsAsync(role);

        var result = await _service.GetAsync(r => r.Id == role.Id);

        Assert.NotNull(result);
        Assert.Equal(role, result);
    }

    [Fact]
    public async Task GetAllAsync_ReturnsPaginatedList()
    {
        var paginatedList = new PaginatedList<Role>(new List<Role>(), 0, 0, 10);
        _rolesRepositoryMock
            .Setup(r => r.GetAllAsync(It.IsAny<int>(), It.IsAny<int>(), null, false, null, true, It.IsAny<CancellationToken>()))
            .ReturnsAsync(paginatedList);

        var result = await _service.GetAllAsync();

        Assert.NotNull(result);
        Assert.Equal(0, result.TotalCount);
    }

    [Fact]
    public async Task GetDefaultRole_ReturnsSuccess_WhenDefaultRoleExists()
    {
        // Arrange
        var defaultRole = Role.Create("Registered", "Registered User", Guid.NewGuid(), isDefault: true);
        
        _rolesRepositoryMock
            .Setup(r => r.GetAsync(
                It.IsAny<Expression<Func<Role, bool>>>(),
                false,
                null,
                false,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(defaultRole);

        // Act
        var result = await _service.GetDefaultRole();

        // Assert
        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Value);
        Assert.Equal(defaultRole, result.Value);
        
        _rolesRepositoryMock.Verify(
            r => r.GetAsync(
                It.Is<Expression<Func<Role, bool>>>(expr => expr != null),
                false,
                null,
                false,
                It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task GetDefaultRole_ReturnsError_WhenDefaultRoleDoesNotExist()
    {
        // Arrange
        _rolesRepositoryMock
            .Setup(r => r.GetAsync(
                It.IsAny<Expression<Func<Role, bool>>>(),
                false,
                null,
                false,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync((Role?)null);

        // Act
        var result = await _service.GetDefaultRole();

        // Assert
        Assert.False(result.IsSuccess);
        Assert.Equal(ResultStatus.Error, result.Status);
        Assert.Contains("Default role not found", result.Errors);
        Assert.Null(result.Value);
        
        _rolesRepositoryMock.Verify(
            r => r.GetAsync(
                It.Is<Expression<Func<Role, bool>>>(expr => expr != null),
                false,
                null,
                false,
                It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task GetDefaultRole_UsesCancellationToken()
    {
        // Arrange
        var cancellationTokenSource = new CancellationTokenSource();
        var cancellationToken = cancellationTokenSource.Token;
        var defaultRole = Role.Create("Registered", "Registered User", Guid.NewGuid(), isDefault: true);
        
        _rolesRepositoryMock
            .Setup(r => r.GetAsync(
                It.IsAny<Expression<Func<Role, bool>>>(),
                false,
                null,
                false,
                cancellationToken))
            .ReturnsAsync(defaultRole);

        // Act
        var result = await _service.GetDefaultRole(cancellationToken);

        // Assert
        Assert.True(result.IsSuccess);
        
        _rolesRepositoryMock.Verify(
            r => r.GetAsync(
                It.IsAny<Expression<Func<Role, bool>>>(),
                false,
                null,
                false,
                cancellationToken),
            Times.Once);
    }

    [Fact]
    public async Task GetAllAsync_PassesAllParametersCorrectly()
    {
        // Arrange
        var index = 2;
        var size = 20;
        Expression<Func<Role, bool>> predicate = r => r.Name.Value.Contains("Admin");
        var includeSoftDeleted = true;
        Func<IQueryable<Role>, IQueryable<Role>> include = q => q;
        var asNoTracking = false;
        var cancellationToken = new CancellationToken();

        var paginatedList = new PaginatedList<Role>(new List<Role>(), 0, index, size);
        _rolesRepositoryMock
            .Setup(r => r.GetAllAsync(index, size, predicate, includeSoftDeleted, include, asNoTracking, cancellationToken))
            .ReturnsAsync(paginatedList);

        // Act
        var result = await _service.GetAllAsync(index, size, predicate, includeSoftDeleted, include, asNoTracking, cancellationToken);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(index, result.PageIndex);
        Assert.Equal(size, result.PageSize);
        
        _rolesRepositoryMock.Verify(
            r => r.GetAllAsync(index, size, predicate, includeSoftDeleted, include, asNoTracking, cancellationToken),
            Times.Once);
    }

    [Fact]
    public async Task GetAsync_PassesAllParametersCorrectly()
    {
        // Arrange
        Expression<Func<Role, bool>> predicate = r => r.Id == Guid.NewGuid();
        var includeSoftDeleted = true;
        Func<IQueryable<Role>, IQueryable<Role>> include = q => q;
        var asNoTracking = false;
        var cancellationToken = new CancellationToken();
        
        var role = Role.Create("TestRole", "TestDisplayName", Guid.NewGuid());
        _rolesRepositoryMock
            .Setup(r => r.GetAsync(predicate, includeSoftDeleted, include, asNoTracking, cancellationToken))
            .ReturnsAsync(role);

        // Act
        var result = await _service.GetAsync(predicate, includeSoftDeleted, include, asNoTracking, cancellationToken);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(role, result);
        
        _rolesRepositoryMock.Verify(
            r => r.GetAsync(predicate, includeSoftDeleted, include, asNoTracking, cancellationToken),
            Times.Once);
    }
}
