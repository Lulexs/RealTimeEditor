using ApplicationLogic.Dtos;
using Models;

namespace API.Controllers;

[ApiController]
[Route("[controller]")]
public class WorkspacesController : ControllerBase {

    [HttpPost("")]
    public async Task<ActionResult<Workspace>> CreateWorkspace([FromBody] WorkspaceDto dto) {
        Console.WriteLine($"{dto.Name}/{dto.OwnerName}");
        await Task.Delay(10);
        return Ok(new Workspace() { Username = dto.Name, OwnerUsername = dto.OwnerName, WorkspaceName = dto.Name, CreatedAt = DateTime.Now, WorkspaceId = Guid.NewGuid(), Permission = PermissionLevel.OWNER });
    }

    [HttpGet("{username}")]
    public async Task<ActionResult<List<Workspace>>> GetUsersWorkspaces(string username) {
        Console.WriteLine($"Getting workspaces for {username}");

        var workspaces = new List<Workspace>
        {
                new() {
                    Username = username,
                    WorkspaceId = Guid.NewGuid(),
                    WorkspaceName = "Project Alpha",
                    OwnerUsername = username,
                    Permission = PermissionLevel.ADMIN,
                    CreatedAt = DateTime.UtcNow.AddDays(-10)
                },
                new() {
                    Username = username,
                    WorkspaceId = Guid.NewGuid(),
                    WorkspaceName = "Project Beta",
                    OwnerUsername = "AnotherUser",
                    Permission = PermissionLevel.EDIT,
                    CreatedAt = DateTime.UtcNow.AddDays(-30)
                }
            };

        await Task.Delay(10);
        return Ok(workspaces);
    }

}