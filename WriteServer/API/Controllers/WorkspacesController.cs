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

    [HttpPost("join")]
    public async Task<ActionResult<Workspace>> JoinWorkspace([FromBody] JoinWorkspaceDto dto) {
        var decomposed = dto.JoinCode.Split("\\");
        var workspaceId = Guid.Parse(decomposed[0]);
        PermissionLevel permissionLevel = (PermissionLevel)int.Parse(decomposed[1]);
        Console.WriteLine($"{dto.Username} joining {workspaceId} with pl {permissionLevel}");
        await Task.Delay(10);
        return Ok(new Workspace() {
            Username = "AnotherUser",
            WorkspaceId = workspaceId,
            WorkspaceName = "Joined workspace",
            OwnerUsername = dto.Username,
            Permission = permissionLevel,
            CreatedAt = DateTime.UtcNow.AddDays(-10.5)
        });
    }

    [HttpDelete("{workspaceId}/{username}")]
    public async Task<ActionResult> DeleteWorkspace(Guid workspaceId, string username) {
        Console.WriteLine($"{username} Deleting {workspaceId}");
        await Task.Delay(10);
        return Ok();
    }

    [HttpPut("")]
    public async Task<ActionResult> ChangeName([FromBody] ChangeWorkspaceNameDto dto) {
        Console.WriteLine($"{dto.UserUsername} changing {dto.OwnerUsername}'s workspace {dto.WorkspaceId} name to {dto.NewName} ");
        await Task.Delay(10);
        return Ok();
    }

    [HttpGet("{ownerUsername}/{workspaceId}")]
    public async Task<ActionResult<Workspace>> Refresh(string ownerUsername, Guid workspaceId) {
        Console.WriteLine($"refreshing workspace {workspaceId}");
        await Task.Delay(10);
        return Ok(new Workspace() {
            Username = "AnotherUser",
            WorkspaceId = workspaceId,
            WorkspaceName = "Joined workspace with changed name",
            OwnerUsername = ownerUsername,
            Permission = PermissionLevel.OWNER,
            CreatedAt = DateTime.UtcNow.AddDays(-10.5)
        });
    }

    [HttpDelete("users/{workspaceId}/{username}/{performer}")]
    public async Task<ActionResult> KickFromWorkspace(Guid workspaceId, string username, string performer) {
        Console.WriteLine($"Kicking {username} from {workspaceId} by {performer}");
        await Task.Delay(10);
        return Ok();
    }

    [HttpPut("users/{username}/{newPermLevel}/{performer}")]
    public async Task<ActionResult> ChangeUserPermLevel(string username, PermissionLevel newPermLevel, string performer) {
        Console.WriteLine($"Changing {username} permission to {newPermLevel} by {performer}");
        await Task.Delay(10);
        return Ok();
    }

    [HttpGet("users/{workspaceId}")]
    public async Task<ActionResult<List<UserInWorkspaceDto>>> GetUsersInWorkspace(Guid workspaceId) {
        Console.WriteLine($"Getting users for workspace {workspaceId}");
        await Task.Delay(10);
        var usersInWorkspace = new List<UserInWorkspaceDto>
        {
            new() {
                WorkspaceId = workspaceId,
                Username = "user1@example.com",
                Permission = PermissionLevel.VIEW_ONLY
            },
            new() {
                WorkspaceId = workspaceId,
                Username = "user2@example.com",
                Permission = PermissionLevel.EDIT
            },
            new() {
                WorkspaceId = workspaceId,
                Username = "user3@example.com",
                Permission = PermissionLevel.ADMIN
            },
            new() {
                WorkspaceId = workspaceId,
                Username = "editx@example.com",
                Permission = PermissionLevel.EDIT
            },
            new() {
                WorkspaceId = workspaceId,
                Username = "user4@example.com",
                Permission = PermissionLevel.VIEW_ONLY
            },
            new() {
                WorkspaceId = workspaceId,
                Username = "editor2@example.com",
                Permission = PermissionLevel.EDIT
            },
            new() {
                WorkspaceId = workspaceId,
                Username = "admin2@example.com",
                Permission = PermissionLevel.ADMIN
            },
            new() {
                WorkspaceId = workspaceId,
                Username = "owner2@example.com",
                Permission = PermissionLevel.OWNER
            }
        };

        return Ok(usersInWorkspace);
    }
}