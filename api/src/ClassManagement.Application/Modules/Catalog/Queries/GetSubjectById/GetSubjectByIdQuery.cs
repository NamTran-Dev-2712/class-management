public record GetSubjectByIdQuery(Guid PublicId) : IRequest<SubjectDto>;
