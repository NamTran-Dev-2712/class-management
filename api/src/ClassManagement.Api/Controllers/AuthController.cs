using MediatR;

[ApiController]
[Route("api/[controller]")]
public class AuthController : BaseApiController
{
    private readonly ISender _mediator;
    private readonly IConfiguration _configuration;

    public AuthController(ISender mediator, IConfiguration configuration)
    {
        _mediator = mediator;
        _configuration = configuration;
    }

    [HttpPost("register")]
    public async Task<IActionResult> Register(RegisterCommand command)
    {
        var userId = await _mediator.Send(command);
        return ApiCreated(
            routeName: null,
            routeValues: null,
            data: new { UserId = userId },
            message: "User registered successfully."
        );
    }
}
