using ApplicationLogic.Dtos;
using Models;

namespace API.Controllers;

[ApiController]
[Route("[controller]")]
public class WorkspacesController : ControllerBase {

    [HttpPost("")]
    public async Task<ActionResult<Workspace>> CreateWorkspace([FromBody] WorkspaceDto dto) {
        Console.WriteLine($"{dto.Name}/{dto.OwnerName}");
        await Task.Run(() => { });
        return Ok(new Workspace() { Username = dto.Name, OwnerUsername = dto.OwnerName, WorkspaceName = dto.Name, CreatedAt = DateTime.Now, WorkspaceId = Guid.NewGuid(), Permission = PermissionLevel.OWNER });
    }

}