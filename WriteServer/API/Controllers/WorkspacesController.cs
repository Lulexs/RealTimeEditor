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
        try {
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
            var newWorkspace = new Workspace {
                WorkspaceId = workspaceId,
                WorkspaceName = dto.Name,
                OwnerUsername = dto.OwnerName,
                Username = dto.OwnerName,
                CreatedAt = createdAt,
                Permission = PermissionLevel.OWNER
            };
            _logger.LogInformation("Created workspace {} for user {}", newWorkspace.WorkspaceId, newWorkspace.Username);
            return Ok(newWorkspace);
        }
        catch (Exception ex) {
            _logger.LogError("Error during CreateWorkspace: {}", ex.Message);
            return StatusCode(500, "An error occurred while creating the workspace.");
        }
    }
    // TODO:
    [HttpGet("{username}")]
    public async Task<ActionResult<List<Workspace>>> GetUsersWorkspaces(string username) {
        try {
            var session = CassandraSessionManager.GetSession();

            var statement = await session.PrepareAsync(
                "SELECT workspaceid, workspacename, createrusername, permissionlevel, createdat " +
                "FROM workspaces_by_user WHERE username = ?"
            );
            var boundStatement = statement.Bind(username);

            var resultSet = await session.ExecuteAsync(boundStatement);


            var workspaces = new List<Workspace>();
            foreach (var row in resultSet) {
                workspaces.Add(new Workspace {
                    Username = username,
                    WorkspaceId = row.GetValue<Guid>("workspaceid"),
                    WorkspaceName = row.GetValue<string>("workspacename"),
                    OwnerUsername = row.GetValue<string>("createrusername"),
                    Permission = (PermissionLevel)row.GetValue<int>("permissionlevel"),
                    CreatedAt = row.GetValue<DateTime>("createdat")
                });
            }

            _logger.LogInformation("Retrieved {} workspaces for {}", workspaces.Count, username);
            return Ok(workspaces);
        }
        catch (Exception ex) {
            _logger.LogError("Error during GetUsersWorkspaces: {}", ex.Message);
            return StatusCode(500, "An error occurred while retrieving the user's workspaces.");
        }
    }
    // TODO:
    [HttpPost("join")]
    public async Task<ActionResult<Workspace>> JoinWorkspace([FromBody] JoinWorkspaceDto dto) {
        var decomposed = dto.JoinCode.Split("\\");
        var workspaceId = Guid.Parse(decomposed[0]);
        PermissionLevel permissionLevel = (PermissionLevel)int.Parse(decomposed[1]);

        var session = CassandraSessionManager.GetSession();
        var userAlreadyInWorkspace = await session.PrepareAsync(
            "SELECT workspaceid, username, permissionlevel FROM users_by_workspace WHERE workspaceid = ? AND username = ?"
        );
        var userAlreadyInWorkspaceBound = userAlreadyInWorkspace.Bind(workspaceId, dto.Username);
        var alreadyInWorkspaceRes = (await session.ExecuteAsync(userAlreadyInWorkspaceBound)).ToList();

        if (alreadyInWorkspaceRes.Count != 0) {
            return BadRequest("You are already in this workspace");
        }

        var anyUserInWorkspaceStatement = await session.PrepareAsync(
            "SELECT username FROM users_by_workspace WHERE workspaceid = ?"
        );
        var anyUserInWorkspaceStatementBound = anyUserInWorkspaceStatement.Bind(workspaceId);
        var res = (await session.ExecuteAsync(anyUserInWorkspaceStatementBound)).ToList();
        if (res.Count == 0) {
            return NotFound("Workspace doesn't exist");
        }
        var anyUser = res.First();
        var username = anyUser.GetValue<string>("username");

        var workspaceInfoStatement = await session.PrepareAsync(
            "SELECT workspacename, createrusername, createdat FROM workspaces_by_user WHERE username = ? AND workspaceid = ?"
        );
        var workspaceInfoStatementBound = workspaceInfoStatement.Bind(username, workspaceId);
        var workspaceInfoRow = (await session.ExecuteAsync(workspaceInfoStatementBound)).First();
        var name = workspaceInfoRow.GetValue<string>("workspacename");
        var ownerName = workspaceInfoRow.GetValue<string>("createrusername");
        var createdAt = workspaceInfoRow.GetValue<DateTime>("createdat");

        var workspacesByUserStatement = await session.PrepareAsync(
                "INSERT INTO workspaces_by_user (username, workspaceid, workspacename, createrusername, permissionlevel, createdat) " +
                "VALUES (?, ?, ?, ?, ?, ?)"
            );
        var workspacesByUserBound = workspacesByUserStatement.Bind(
            dto.Username, workspaceId, name, ownerName, (int)permissionLevel, createdAt
        );
        await session.ExecuteAsync(workspacesByUserBound);

        var usersByWorkspaceStatement = await session.PrepareAsync(
            "INSERT INTO users_by_workspace (workspaceid, username, permissionlevel) " +
            "VALUES (?, ?, ?)"
        );
        var usersByWorkspaceBound = usersByWorkspaceStatement.Bind(
            workspaceId, dto.Username, (int)permissionLevel
        );
        await session.ExecuteAsync(usersByWorkspaceBound);


        _logger.LogInformation("{} joining {} with pl {}", dto.Username, workspaceId, permissionLevel);
        return Ok(new Workspace() {
            Username = dto.Username,
            WorkspaceId = workspaceId,
            WorkspaceName = name,
            OwnerUsername = ownerName,
            Permission = permissionLevel,
            CreatedAt = createdAt
        });
    }
    // TODO:
    [HttpDelete("{workspaceId}/{username}")]
    public async Task<ActionResult> DeleteWorkspace(Guid workspaceId, string username) {
        try {
            var session = CassandraSessionManager.GetSession();

            // 1. First check if the workspace exists and verify ownership
            var workspaceCheckStatement = await session.PrepareAsync(
                "SELECT createrusername FROM workspaces_by_user " +
                "WHERE username = ? AND workspaceid = ?"
            );
            var workspaceCheckBound = workspaceCheckStatement.Bind(username, workspaceId);
            var workspaceResult = (await session.ExecuteAsync(workspaceCheckBound)).ToList();

            if (workspaceResult.Count == 0) {
                _logger.LogWarning("Workspace {WorkspaceId} not found for user {Username}", workspaceId, username);
                return NotFound($"Workspace with ID {workspaceId} not found.");
            }

            var creator = workspaceResult.First().GetValue<string>("createrusername");
            if (creator != username) {
                _logger.LogWarning("User {Username} attempted to delete workspace {WorkspaceId} owned by {Owner}",
                    username, workspaceId, creator);
                return Forbid("Only the workspace owner can delete it.");
            }

            // 2. Get all users in the workspace before deletion
            var getUsersStatement = await session.PrepareAsync(
                "SELECT username FROM users_by_workspace WHERE workspaceid = ?"
            );
            var getUsersBound = getUsersStatement.Bind(workspaceId);
            var workspaceUsers = (await session.ExecuteAsync(getUsersBound))
                .Select(row => row.GetValue<string>("username"))
                .ToList();

            // 3. Delete all documents and their updates
            var getDocumentsStatement = await session.PrepareAsync(
                "SELECT documentid FROM documents WHERE workspaceid = ?"
            );
            var getDocumentsBound = getDocumentsStatement.Bind(workspaceId);
            var documents = (await session.ExecuteAsync(getDocumentsBound))
                .Select(row => row.GetValue<Guid>("documentid"))
                .ToList();

            foreach (var documentId in documents) {
                // Delete document updates
                var deleteUpdatesStatement = await session.PrepareAsync(
                    "DELETE FROM updates_by_snapshot WHERE documentid = ?"
                );
                var deleteUpdatesBound = deleteUpdatesStatement.Bind(documentId);
                await session.ExecuteAsync(deleteUpdatesBound);
            }

            // 4. Delete documents
            var deleteDocumentsStatement = await session.PrepareAsync(
                "DELETE FROM documents WHERE workspaceid = ?"
            );
            var deleteDocumentsBound = deleteDocumentsStatement.Bind(workspaceId);
            await session.ExecuteAsync(deleteDocumentsBound);

            // 5. Delete workspace entries for all users
            foreach (var workspaceUser in workspaceUsers) {
                var deleteWorkspaceByUserStatement = await session.PrepareAsync(
                    "DELETE FROM workspaces_by_user WHERE username = ? AND workspaceid = ?"
                );
                var deleteWorkspaceByUserBound = deleteWorkspaceByUserStatement.Bind(workspaceUser, workspaceId);
                await session.ExecuteAsync(deleteWorkspaceByUserBound);
            }

            // 6. Delete all user entries for the workspace
            var deleteUsersStatement = await session.PrepareAsync(
                "DELETE FROM users_by_workspace WHERE workspaceid = ?"
            );
            var deleteUsersBound = deleteUsersStatement.Bind(workspaceId);
            await session.ExecuteAsync(deleteUsersBound);

            _logger.LogInformation("Workspace {WorkspaceId} successfully deleted by owner {Username}",
                workspaceId, username);

            return Ok($"Workspace {workspaceId} and all associated data successfully deleted.");
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
    // TODO:
    [HttpGet("{ownerUsername}/{workspaceId}")]
    public async Task<ActionResult<Workspace>> Refresh(string ownerUsername, Guid workspaceId) {
        try {
            var session = CassandraSessionManager.GetSession();
            var statement = await session.PrepareAsync(
                "SELECT workspacename, createrusername, permissionlevel, createdat FROM workspaces_by_user " +
                "WHERE username = ? AND workspaceid = ?"
            );
            ownerUsername = ownerUsername.Trim();

            var boundStatement = statement.Bind(ownerUsername, workspaceId);
            var resultSet = await session.ExecuteAsync(boundStatement);
            var rows = resultSet.ToList();

            if (rows.Count == 0) {
                _logger.LogWarning("Workspace with ID {WorkspaceId} and owner {OwnerUsername} not found.",
                    workspaceId, ownerUsername);
                return NotFound($"Workspace with ID {workspaceId} and owner {ownerUsername} not found.");
            }

            var row = rows.First();
            var workspace = new Workspace {
                WorkspaceId = workspaceId,
                WorkspaceName = row.GetValue<string>("workspacename"),
                OwnerUsername = row.GetValue<string>("createrusername"),
                Username = ownerUsername,
                Permission = (PermissionLevel)row.GetValue<int>("permissionlevel"),
                CreatedAt = row.GetValue<DateTime>("createdat")
            };

            _logger.LogInformation("Workspace {WorkspaceId} refreshed successfully.", workspaceId);
            return Ok(workspace);
        }
        catch (Exception ex) {
            _logger.LogError(ex, "Error during Refresh for workspace {WorkspaceId}", workspaceId);
            return StatusCode(500, "An error occurred while refreshing the workspace.");
        }
    }

    // TODO:
    [HttpGet("users/{workspaceId}")]
    public async Task<ActionResult<List<UserInWorkspaceDto>>> GetUsersInWorkspace(Guid workspaceId) {
        try {
            var session = CassandraSessionManager.GetSession();
            var statement = await session.PrepareAsync(
                "SELECT username, permissionlevel FROM users_by_workspace WHERE workspaceid = ?"
            );
            var boundStatement = statement.Bind(workspaceId);
            var resultSet = await session.ExecuteAsync(boundStatement);

            var usersInWorkspace = new List<UserInWorkspaceDto>();
            foreach (var row in resultSet) {
                usersInWorkspace.Add(new UserInWorkspaceDto {
                    WorkspaceId = workspaceId,
                    Username = row.GetValue<string>("username"),
                    Permission = (PermissionLevel)row.GetValue<int>("permissionlevel")
                });
                Console.WriteLine(row.GetValue<string>("username") + " " + row.GetValue<int>("permissionlevel"));
            }

            _logger.LogInformation("Retrieved {} users for workspace {}", usersInWorkspace.Count, workspaceId);
            return Ok(usersInWorkspace);
        }
        catch (Exception ex) {
            _logger.LogError("Error during GetUsersInWorkspace: {}", ex.Message);
            return StatusCode(500, "An error occurred while retrieving users in the workspace.");
        }
    }
}