using ApplicationLogic;
using ApplicationLogic.Dtos;
using ApplicationLogic.Exceptions;
using Models;
using Persistence.WorkspaceRepository; // visak
using Persistence;

namespace API.Controllers;

[ApiController]
[Route("[controller]")]
public class WorkspacesController : ControllerBase {

    private readonly WorkspaceLogic _workspaceLogic;
    private readonly LockLogic _lockLogic;
    private readonly ILogger<WorkspacesController> _logger;

    private readonly WorkspaceRepositoryCassandra _wsRepoCass; // visak

    public WorkspacesController(WorkspaceLogic workspaceLogic, LockLogic lockLogic, ILogger<WorkspacesController> logger, WorkspaceRepositoryCassandra wsRepoCass) {
        _lockLogic = lockLogic;
        _workspaceLogic = workspaceLogic;
        _logger = logger;
        _wsRepoCass = wsRepoCass; // visak
    }

    [HttpPost("")]
    public async Task<ActionResult<Workspace>> CreateWorkspace([FromBody] WorkspaceDto dto) {
        try {
            var workspace = await _workspaceLogic.CreateWorkspace(dto);

            _logger.LogInformation("Created workspace {} for user {}", workspace.WorkspaceId, workspace.Username);
            return Ok(workspace);
        }
        catch (Exception ex) {
            _logger.LogError("Error during CreateWorkspace: {}", ex.Message);
            return StatusCode(500, "An error occurred while creating the workspace.");
        }
    }

    [HttpGet("{username}")]
    public async Task<ActionResult<List<Workspace>>> GetUsersWorkspaces(string username) {
        try {

            var workspaces = await _workspaceLogic.GetUserWorkspaces(username);

            _logger.LogInformation("Retrieved {} workspaces for {}", workspaces.Count, username);
            return Ok(workspaces);
        }
        catch (Exception ex) {
            _logger.LogError("Error during GetUsersWorkspaces: {}", ex.Message);
            return StatusCode(500, "An error occurred while retrieving the user's workspaces.");
        }
    }

    [HttpPost("join")]
    public async Task<ActionResult<Workspace>> JoinWorkspace([FromBody] JoinWorkspaceDto dto) {

        try {
            var workspace = await _workspaceLogic.AddUserToWorkspace(dto);

            _logger.LogInformation("{} joining {} with pl {}", dto.Username, workspace.WorkspaceId, workspace.Permission);
            return Ok(workspace);
        }
        catch (WorkspaceNotFoundException ex) {
            _logger.LogWarning(ex, "Workspace not found");
            return NotFound("Workspace not found");
        }
        catch (Exception ex) {
            _logger.LogError(ex, "Error joining workspace");
            return StatusCode(500, "An error occurred while joining the workspace.");
        }
    }

    [HttpDelete("{workspaceId}/{username}")]
    public async Task<ActionResult> DeleteWorkspace(Guid workspaceId, string username) {
        try {

            await _workspaceLogic.DeleteWorkspace(workspaceId, username);

            _logger.LogInformation("Workspace {WorkspaceId} successfully deleted by owner {Username}",
                workspaceId, username);

            return Ok($"Workspace {workspaceId} and all associated data successfully deleted.");
        }
        catch (WorkspaceNotFoundException ex) {
            _logger.LogWarning(ex, "Workspace {WorkspaceId} not found for user {Username}", workspaceId, username);
            return NotFound($"Workspace with ID {workspaceId} not found.");
        }
        catch (UnauthorizedAccessException ex) {
            _logger.LogWarning(ex, "User {Username} attempted to delete someone else's workspace ({WorkspaceId})",
                username, workspaceId);
            return Forbid("Only the workspace owner can delete it.");
        }
        catch (Exception ex) {
            _logger.LogError(ex, "Error deleting workspace {WorkspaceId} by user {Username}",
                workspaceId, username);
            return StatusCode(500, "An error occurred while deleting the workspace.");
        }
    }

    [HttpPost("lock")]
    public async Task<ActionResult> AcquireLockForChangeWorkspaceName([FromBody] ChangeWorkspaceNameDto dto) {
        try {
            var resourceKey = $"{dto.WorkspaceId}-changeWorkspaceName";
            await _lockLogic.LockResource(resourceKey);
            return Ok();
        }
        catch (LockTakenException e) {
            _logger.LogInformation("Lock already taken for {WorkspaceId}: {Message} ", dto.WorkspaceId, e.Message);
            return StatusCode(409, "Another user is changing the workspace name");
        }
        catch (Exception e) {
            _logger.LogError("Error during acquire lock for change workspace name: {}", e.Message);
            return StatusCode(500, "An error occurred while attempting to change the workspace name.");
        }
    }

    [HttpPut("")]
    public async Task<ActionResult> ChangeWorkspaceName([FromBody] ChangeWorkspaceNameDto dto) {
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

    [HttpPost("lockKickChangePermLevel")]
    public async Task<ActionResult> AcquireLockForKickChangePermLevelUser([FromBody] KickUserDto dto) {
        try {
            var resourceKey = $"{dto.WorkspaceId}-kickChangePermLevel";
            await _lockLogic.LockResource(resourceKey);

            return Ok();
        }
        catch (LockTakenException e) {
            _logger.LogInformation("Lock already taken for user workspace {WorkspaceId}: {Message}", dto.WorkspaceId, e.Message);
            return StatusCode(409, "Another admin is performing an administrative action.");
        }
        catch (Exception e) {
            _logger.LogError("Error during acquire lock for kick/change permission level in workspace {WorkspaceId}: {Message}", dto.WorkspaceId, e.Message);
            return StatusCode(500, "An error occurred while attempting to perform an administrative action.");
        }
    }

    [HttpDelete("users/{workspaceId}/{username}/{performer}")]
    public async Task<ActionResult> KickFromWorkspace(Guid workspaceId, string username, string performer) {
        try {
            await _workspaceLogic.KickUserFromWorkspace(workspaceId, username, performer);
            return Ok($"User {username} has been successfully removed from workspace {workspaceId} by {performer}.");
        }
        catch (UnauthorizedAccessException e) {
            _logger.LogInformation("Kick operation unauthorized: {}", e.Message);
            return Forbid(e.Message);
        }
        catch (WorkspaceNotFoundException e) {
            _logger.LogInformation("Kick operation failed: {}", e.Message);
            return NotFound(e.Message);
        }
        catch (Exception e) {
            _logger.LogError("Error during KickFromWorkspace: {}", e.Message);
            return StatusCode(500, "An error occurred while kicking the user from the workspace.");
        }
    }

    [HttpPut("users/{workspaceId}/{username}/{newPermLevel}/{performer}")]
    public async Task<ActionResult> ChangeUserPermLevel(Guid workspaceId, string username, PermissionLevel newPermLevel, string performer) {
        try {
            await _workspaceLogic.ChangeUserPermission(workspaceId, username, newPermLevel, performer);
            return Ok($"User {username}'s permission level has been successfully changed to {newPermLevel} by {performer}.");
        }
        catch (UnauthorizedAccessException e) {
            _logger.LogInformation("Permission change unauthorized: {}", e.Message);
            return Forbid(e.Message);
        }
        catch (WorkspaceNotFoundException e) {
            _logger.LogInformation("Permission change failed: {}", e.Message);
            return NotFound(e.Message);
        }
        catch (Exception e) {
            _logger.LogError("Error during ChangeUserPermLevel: {}", e.Message);
            return StatusCode(500, "An error occurred while changing the user's permission level.");
        }
    }

    [HttpGet("{ownerUsername}/{workspaceId}")]
    public async Task<ActionResult<Workspace>> Refresh(string ownerUsername, Guid workspaceId) {
        try {
            var workspace = await _workspaceLogic.GetWorkspaceByUserAndId(ownerUsername.Trim(), workspaceId);

            if (workspace == null) {
                _logger.LogWarning("Workspace with ID {WorkspaceId} and owner {OwnerUsername} not found.",
                    workspaceId, ownerUsername);
                return NotFound($"Workspace with ID {workspaceId} and owner {ownerUsername} not found.");
            }

            _logger.LogInformation("Workspace {WorkspaceId} refreshed successfully.", workspaceId);
            return Ok(workspace);
        }
        catch (Exception ex) {
            _logger.LogError(ex, "Error during Refresh for workspace {WorkspaceId}", workspaceId);
            return StatusCode(500, "An error occurred while refreshing the workspace.");
        }
    }

    [HttpGet("users/{workspaceId}")]
    public async Task<ActionResult<List<UserInWorkspaceDto>>> GetUsersInWorkspace(Guid workspaceId) {
        try {

            var usersInWorkspace = await _workspaceLogic.GetUsersInWorkspace(workspaceId);

            _logger.LogInformation("Retrieved {} users for workspace {}", usersInWorkspace.Count, workspaceId);
            return Ok(usersInWorkspace);
        }
        catch (Exception ex) {
            _logger.LogError("Error during GetUsersInWorkspace: {}", ex.Message);
            return StatusCode(500, "An error occurred while retrieving users in the workspace.");
        }
    }
}