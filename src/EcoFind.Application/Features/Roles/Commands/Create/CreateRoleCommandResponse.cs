namespace EcoFind.Application.Features.Roles.Commands.Create;

public sealed record CreateRoleCommandResponse
{
    public Guid Id { get; init; }
    public string Name { get; init; } = string.Empty;

    public CreateRoleCommandResponse(Guid id, string name)
    {
        Id = id;
        Name = name;
    }

    private CreateRoleCommandResponse() { }
}
