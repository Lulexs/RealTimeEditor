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
    // TODO:
    [HttpPost("")]
    public async Task<ActionResult<Workspace>> CreateWorkspace([FromBody] WorkspaceDto dto) {
        try
        {
            var session = CassandraSessionManager.GetSession();
            var workspaceId = Guid.NewGuid();
            var createdAt = DateTime.UtcNow;

            //`workspaces_by_user`
            var workspacesByUserStatement = await session.PrepareAsync(
                "INSERT INTO workspaces_by_user (username, workspaceid, workspacename, createrusername, permissionlevel, createdat) " +
                "VALUES (?, ?, ?, ?, ?, ?)"
            );
            var workspacesByUserBound = workspacesByUserStatement.Bind(
                dto.OwnerName, workspaceId, dto.Name, dto.OwnerName, (int)PermissionLevel.OWNER, createdAt
            );
            await session.ExecuteAsync(workspacesByUserBound);

            //`users_by_workspace`
            var usersByWorkspaceStatement = await session.PrepareAsync(
                "INSERT INTO users_by_workspace (workspaceid, username, permissionlevel) " +
                "VALUES (?, ?, ?)"
            );
            var usersByWorkspaceBound = usersByWorkspaceStatement.Bind(
                workspaceId, dto.OwnerName, (int)PermissionLevel.OWNER
            );
            await session.ExecuteAsync(usersByWorkspaceBound);

            //Ne znam za front da li ti treba nov workspace ili sta
            var newWorkspace = new Workspace
            {
                WorkspaceId = workspaceId,
                WorkspaceName = dto.Name,
                OwnerUsername = dto.OwnerName,
                Username = dto.OwnerName,
                CreatedAt = createdAt,
                Permission = PermissionLevel.OWNER
            };
            _logger.LogInformation($"Created workspace {newWorkspace.WorkspaceId} for user {newWorkspace.Username}");
            return Ok(newWorkspace);
        }
        catch (Exception ex)
        {
            _logger.LogError($"Error during CreateWorkspace: {ex.Message}");
            return StatusCode(500, "An error occurred while creating the workspace.");
        }
    }
    // TODO:
    [HttpGet("{username}")]
    public async Task<ActionResult<List<Workspace>>> GetUsersWorkspaces(string username) {
        try
        {
            var session = CassandraSessionManager.GetSession();

            var statement = await session.PrepareAsync(
                "SELECT workspaceid, workspacename, createrusername, permissionlevel, createdat " +
                "FROM workspaces_by_user WHERE username = ?"
            );
            var boundStatement = statement.Bind(username);

            var resultSet = await session.ExecuteAsync(boundStatement);


            var workspaces = new List<Workspace>();
            foreach (var row in resultSet)
            {
                workspaces.Add(new Workspace
                {
                    Username = username,
                    WorkspaceId = row.GetValue<Guid>("workspaceid"),
                    WorkspaceName = row.GetValue<string>("workspacename"),
                    OwnerUsername = row.GetValue<string>("createrusername"),
                    Permission = (PermissionLevel)row.GetValue<int>("permissionlevel"),
                    CreatedAt = row.GetValue<DateTime>("createdat")
                });
            }

            if (workspaces.Count == 0)
            {
                _logger.LogWarning($"No workspaces found for user {username}");
                return NotFound($"No workspaces found for user '{username}'.");
            }

            _logger.LogInformation($"Retrieved {workspaces.Count} workspaces for {username}");
            return Ok(workspaces);
        }
        catch (Exception ex)
        {
            _logger.LogError($"Error during GetUsersWorkspaces: {ex.Message}");
            return StatusCode(500, "An error occurred while retrieving the user's workspaces.");
        }
    }
    // TODO: velja ne zna ovo da uradi.
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
    // TODO:
    [HttpDelete("{workspaceId}/{username}")]
    public async Task<ActionResult> DeleteWorkspace(Guid workspaceId, string username) {
        try
        {
            var session = CassandraSessionManager.GetSession();

            // Msm da treba provera ovako
            var ownerCheckStatement = await session.PrepareAsync(
                "SELECT createrusername FROM workspaces_by_user WHERE workspaceid = ?"
            );
            var ownerCheckBound = ownerCheckStatement.Bind(workspaceId);
            var ownerResult = await session.ExecuteAsync(ownerCheckBound);

            if (!ownerResult.Any())
            {
                return NotFound($"Workspace with ID {workspaceId} not found.");
            }

            var ownerUsername = ownerResult.First().GetValue<string>("createrusername");
            if (ownerUsername != username)
            {
                return Forbid("Only the owner of the workspace can delete it.");
            }

            // Brisanje  iz `users_by_workspace`
            var deleteUsersByWorkspaceStatement = await session.PrepareAsync(
                "DELETE FROM users_by_workspace WHERE workspaceid = ?"
            );
            var deleteUsersByWorkspaceBound = deleteUsersByWorkspaceStatement.Bind(workspaceId);
            await session.ExecuteAsync(deleteUsersByWorkspaceBound);

            // Brisanje iz `workspaces_by_user`
            var deleteWorkspacesByUserStatement = await session.PrepareAsync(
                "DELETE FROM workspaces_by_user WHERE workspaceid = ? AND username = ?"
            );
            var deleteWorkspacesByUserBound = deleteWorkspacesByUserStatement.Bind(workspaceId, username);
            await session.ExecuteAsync(deleteWorkspacesByUserBound);

            

            _logger.LogInformation($"{username} deleted workspace {workspaceId}");
            return Ok($"Workspace {workspaceId} deleted successfully.");
        }
        catch (Exception ex)
        {
            _logger.LogInformation($"Error during DeleteWorkspace: {ex.Message}");
            return StatusCode(500, "An error occurred while deleting the workspace.");
        }
    }

    [HttpPost("lock")]
    public async Task<ActionResult> AcquireLockForChangeDocumentName([FromBody] ChangeDocumentNameDto dto) {
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

    [HttpPost("lock-kick-and-change-perm-lvl-user")]
    public async Task<ActionResult> AcquireLockForKickChangePermLevelUser(Guid workspaceId, string username) {
        try
        {
            var resourceKey = $"{workspaceId}-{username}-kickChangePermLevel";
            await _lockLogic.LockResource(resourceKey); 

            return Ok();
        }
        catch (LockTakenException e)
        {
            _logger.LogInformation("Lock already taken for user {Username} in workspace {WorkspaceId}: {Message}", username, workspaceId, e.Message);
            return StatusCode(409, "Another admin is performing an administrative action on this user.");
        }
        catch (Exception e)
        {
            _logger.LogError("Error during acquire lock for kick/change permission level for user {Username} in workspace {WorkspaceId}: {Message}", username, workspaceId, e.Message);
            return StatusCode(500, "An error occurred while attempting to perform an administrative action on this user.");
        }
    }

    [HttpDelete("users/{workspaceId}/{username}/{performer}")]
    public async Task<ActionResult> KickFromWorkspace(Guid workspaceId, string username, string performer) {
         try
        {
            await _workspaceLogic.KickUserFromWorkspace(workspaceId, username, performer);
            return Ok($"User {username} has been successfully removed from workspace {workspaceId} by {performer}.");
        }
        catch (UnauthorizedAccessException e)
        {
            _logger.LogInformation("Kick operation unauthorized: {}", e.Message);
            return Forbid(e.Message);
        }
        catch (WorkspaceNotFoundException e)
        {
            _logger.LogInformation("Kick operation failed: {}", e.Message);
            return NotFound(e.Message);
        }
        catch (Exception e)
        {
            _logger.LogError("Error during KickFromWorkspace: {}", e.Message);
            return StatusCode(500, "An error occurred while kicking the user from the workspace.");
        }
    }

    [HttpPut("users/{username}/{newPermLevel}/{performer}")]
    public async Task<ActionResult> ChangeUserPermLevel(string username, PermissionLevel newPermLevel, string performer) {
        Console.WriteLine($"Changing {username} permission to {newPermLevel} by {performer}");
        await Task.Delay(10);
        return Ok();
    }
    // TODO:
    [HttpGet("{ownerUsername}/{workspaceId}")]
    public async Task<ActionResult<Workspace>> Refresh(string ownerUsername, Guid workspaceId) {
        try
        {
            var session = CassandraSessionManager.GetSession();
            var statement = await session.PrepareAsync(
                "SELECT workspacename, createrusername, permissionlevel, createdat FROM workspaces_by_user " +
                "WHERE workspaceid = ? AND username = ?"
            );
            var boundStatement = statement.Bind(workspaceId, ownerUsername);

            var resultSet = await session.ExecuteAsync(boundStatement);
            if (!resultSet.Any())
            {   
                _logger.LogWarning($"Workspace with ID {workspaceId} and owner {ownerUsername} not found.");
                return NotFound($"Workspace with ID {workspaceId} and owner {ownerUsername} not found.");
            }

            var row = resultSet.First();
            var workspace = new Workspace
            {
                WorkspaceId = workspaceId,
                WorkspaceName = row.GetValue<string>("workspacename"),
                OwnerUsername = row.GetValue<string>("createrusername"),
                Username = ownerUsername,
                Permission = (PermissionLevel)row.GetValue<int>("permissionlevel"),
                CreatedAt = row.GetValue<DateTime>("createdat")
            };

            _logger.LogInformation($"Workspace {workspaceId} refreshed successfully.");
            return Ok(workspace);
        }
        catch (Exception ex)
        {
            _logger.LogError($"Error during Refresh: {ex.Message}");
            return StatusCode(500, "An error occurred while refreshing the workspace.");
        }
    }

    // TODO:
    [HttpGet("users/{workspaceId}")]
    public async Task<ActionResult<List<UserInWorkspaceDto>>> GetUsersInWorkspace(Guid workspaceId) {
        try
        {
            var session = CassandraSessionManager.GetSession();
            var statement = await session.PrepareAsync(
                "SELECT username, permissionlevel FROM users_by_workspace WHERE workspaceid = ?"
            );
            var boundStatement = statement.Bind(workspaceId);
            var resultSet = await session.ExecuteAsync(boundStatement);

            var usersInWorkspace = new List<UserInWorkspaceDto>();
            foreach (var row in resultSet)
            {
                usersInWorkspace.Add(new UserInWorkspaceDto
                {
                    WorkspaceId = workspaceId,
                    Username = row.GetValue<string>("username"),
                    Permission = (PermissionLevel)row.GetValue<int>("permissionlevel")
                });
            }
            if (usersInWorkspace.Count == 0)
            {
                _logger.LogWarning($"No users found for workspace {workspaceId}");
                return NotFound($"No users found for workspace {workspaceId}.");
            }

            _logger.LogInformation($"Retrieved {usersInWorkspace.Count} users for workspace {workspaceId}");
            return Ok(usersInWorkspace);
        }
        catch (Exception ex)
        {
            _logger.LogError($"Error during GetUsersInWorkspace: {ex.Message}");
            return StatusCode(500, "An error occurred while retrieving users in the workspace.");
        }   
    }
}