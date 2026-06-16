using ClassManagement.Domain.Modules.Questions.Enums;

// Teacher flips one of their questions Public/Private. PublicId bound from the route.
public record SetQuestionVisibilityCommand(Guid PublicId, QuestionVisibility Visibility) : IRequest;
