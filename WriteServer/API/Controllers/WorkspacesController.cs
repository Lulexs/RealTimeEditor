using ApplicationLogic;
using ApplicationLogic.Dtos;
using ApplicationLogic.Exceptions;
using Models;

namespace API.Controllers;

[ApiController]
[Route("[controller]")]
public class WorkspacesController : ControllerBase {

    private readonly WorkspaceLogic _workspaceLogic;
    private readonly LockLogic _lockLogic;
    private readonly ILogger<WorkspacesController> _logger;

    public WorkspacesController(WorkspaceLogic workspaceLogic, LockLogic lockLogic, ILogger<WorkspacesController> logger) {
        _lockLogic = lockLogic;
        _workspaceLogic = workspaceLogic;
        _logger = logger;
    }

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
                    WorkspaceId = Guid.Parse("bb4f9ca1-41ec-469c-bbc8-666666666666"),
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

    [HttpPost("lock")]
    public async Task<ActionResult> AcquireLockForChangeDocumentName([FromBody] ChangeDocumentNameDto dto) {
        try {
            var resourceKey = $"{dto.WorkspaceId}-changeWorkspaceName";
            await _lockLogic.LockResource(resourceKey);
            return Ok();
        }
        catch (LockTakenException e) {
            _logger.LogInformation("{}", e.Message);
            return StatusCode(409, "Another user is changing the workspace name");
        }
        catch (Exception e) {
            _logger.LogError("Error during acquire lock for change workspace name: {}", e.Message);
            return StatusCode(500, "An error occurred while attempting to change the workspace name.");
        }
    }

    [HttpPut("")]
    public async Task<ActionResult> ChangeDocumentName([FromBody] ChangeWorkspaceNameDto dto) {
        try {
            await _workspaceLogic.ChangeWorkspaceName(dto);
            return Ok();
        }
        catch (WorkspaceNotFoundException e) {
            _logger.LogInformation("Change workspace name failed due to {}", e.Message);
            return NotFound(e.Message);
        }
        catch (Exception e) {
            _logger.LogError("Error during ChangeWorkspaceName: {}", e.Message);
            return StatusCode(500, "An error occurred while changing the workspace name.");
        }
    }

    // [HttpPost("lock-kick-and-change-perm-lvl-user")]
    // public async Task<ActionResult> AcquireLockForKickChangePermLevelUser(Guid workspaceId, string username) {
    //     var resourceKey = $"{workspaceId}-{username}-kickChangePermLevel";
    //     await using (var redLock = await _redLockManager.GetFactory().CreateLockAsync(
    //         resourceKey,
    //         TimeSpan.FromSeconds(10))) {
    //         if (!redLock.IsAcquired) {
    //             return StatusCode(409, "Another admin is doing adminitrative lock over this user");
    //         }
    //         return Ok();
    //     }
    // }

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