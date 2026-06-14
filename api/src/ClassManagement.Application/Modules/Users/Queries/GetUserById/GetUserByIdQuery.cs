public record GetUserByIdQuery(Guid PublicId) : IRequest<UserDetailDto>;
