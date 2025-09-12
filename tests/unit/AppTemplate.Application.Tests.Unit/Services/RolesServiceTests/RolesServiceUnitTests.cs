using AppTemplate.Application.Data.Pagination;
using AppTemplate.Application.Repositories;
using AppTemplate.Application.Services.Roles;
using AppTemplate.Domain.Roles;
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
        _rolesRepositoryMock.Verify(r => r.AddAsync(role), Times.Once);
    }

    [Fact]
    public void Delete_CallsRepositoryDelete()
    {
        var role = Role.Create("TestRole", "TestDisplayName", Guid.NewGuid());
        _service.Delete(role);
        _rolesRepositoryMock.Verify(r => r.Delete(role, true), Times.Once);
    }

    [Fact]
    public void Update_CallsRepositoryUpdate()
    {
        var role = Role.Create("TestRole", "TestDisplayName", Guid.NewGuid());
        _service.Update(role);
        _rolesRepositoryMock.Verify(r => r.Update(role), Times.Once);
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
}
