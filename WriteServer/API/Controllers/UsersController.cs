using ApplicationLogic.Dtos;
using ApplicationLogic;
using ApplicationLogic.Exceptions;
using ApplicationLogic.Utilities;

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
            _logger.LogInformation("User {} attempting to log in", dto.Username);

            if (dto == null || string.IsNullOrWhiteSpace(dto.Username) || string.IsNullOrWhiteSpace(dto.Password) || string.IsNullOrWhiteSpace(dto.Region)) {
                return BadRequest("Invalid login data provided");
            }

            var userDto = await _userLogic.LoginUserAsync(dto);

            _logger.LogInformation("User {} successfully logged in", dto.Username);
            return Ok(userDto);
        }
        catch (UserNotFoundException e) {
            _logger.LogInformation("User {} failed to login because no such account exists", dto.Username);
            return NotFound(e.Message);
        }
        catch (InvalidUserPasswordException e) {
            _logger.LogInformation("User {} failed to login because wrong password", dto.Username);
            return Unauthorized(e.Message);
        }
        catch (Exception ex) {
            _logger.LogError(ex, "Unexpected error while logging in user");
            return StatusCode(500, "Unexpected error while logging in user");
        }
    }

    [HttpPost("register")]
    public async Task<ActionResult> Register([FromBody] RegisterDto regDto) {
        try{
            _logger.LogInformation("User {} attempting to register", regDto.Username);

            if (regDto == null ||
                string.IsNullOrWhiteSpace(regDto.Username) ||
                string.IsNullOrWhiteSpace(regDto.Password) ||
                string.IsNullOrWhiteSpace(regDto.Region)  ||
                string.IsNullOrWhiteSpace(regDto.Avatar) ||
                string.IsNullOrWhiteSpace(regDto.Email)) {
                throw new InvalidRegisterDataException("Invalid registration data provided, all fields are required for registration");         
                }

            await _userLogic.RegisterUserAsync(regDto);

            _logger.LogInformation("User {} successfully registered", regDto.Username);

            return Ok("User successfully registered");

        }
        catch(UserAlreadyExistsException e){
            _logger.LogInformation("User {} failed to register because account already exists", regDto.Username);
            return Conflict(e.Message);
        }
        catch(InvalidRegisterDataException e){
            _logger.LogInformation("User {} failed to register because invalid data provided", regDto.Username);
            return BadRequest(e.Message);
        }
        catch(Exception ex){
            _logger.LogError(ex, "Unexpected error while registering user");
            return StatusCode(500, "Unexpected error while registering user");
        }
    }
}