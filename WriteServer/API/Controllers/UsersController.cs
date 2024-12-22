using ApplicationLogic.Dtos;

namespace API.Controllers;

[ApiController]
[Route("[controller]")]
public class UsersController : ControllerBase {
    [HttpPost("login")]
    public async Task<ActionResult<UserDto>> Login([FromBody] LoginDto dto) {
        await Task.Run(() => { });
        return Ok(new UserDto() {
            Region = dto.Region,
            Username = dto.Username,
            Avatar = "https://raw.githubusercontent.com/mantinedev/mantine/master/.demo/avatars/avatar-5.png"
        });
    }

    [HttpPost("register")]
    public async Task<ActionResult> Register([FromBody] RegisterDto regDto) {
        await Task.Run(() => { });
        Console.WriteLine($"Registering user: {regDto.Username}/{regDto.Password}");
        return Ok();
    }
}