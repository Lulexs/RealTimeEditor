using ApplicationLogic.Dtos;
using ApplicationLogic;
using ApplicationLogic.Exceptions;

namespace API.Controllers;

[ApiController]
[Route("[controller]")]
public class UsersController : ControllerBase {

    private readonly ILogger<UsersController> _logger;
    private readonly UserLogic _userLogic;

    public UsersController(ILogger<UsersController> logger, UserLogic userLogic) {
        _logger = logger;
        _userLogic = userLogic;
    }

    [HttpPost("login")]
    public async Task<ActionResult<UserDto>> Login([FromBody] LoginDto dto) {
        try {
            if (dto == null || string.IsNullOrWhiteSpace(dto.Username) || string.IsNullOrWhiteSpace(dto.Password) || string.IsNullOrWhiteSpace(dto.Region)) {
                return BadRequest("Invalid login data provided");
            }

            var userDto = await _userLogic.LoginUserAsync(dto);

            return Ok(userDto);
        }
        catch(UserNotFoundException e) {
            return NotFound(e.Message);
        }
        catch(InvalidUserPasswordException e) {
            return Unauthorized(e.Message);
        }
        catch(Exception ex){
            _logger.LogError(ex, "Unexpected error while logging in user");
            return StatusCode(500, "Unexpected error while logging in user");
        }
    }

    [HttpPost("register")]
    public async Task<ActionResult> Register([FromBody] RegisterDto regDto) {
        await Task.Run(() => { });
        Console.WriteLine($"Registering user: {regDto.Username}/{regDto.Password}");
        return Ok();
    }
}