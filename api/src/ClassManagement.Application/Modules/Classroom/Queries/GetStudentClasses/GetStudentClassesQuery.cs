using ClassManagement.Application.Modules.Classroom.DTOs;

// Classes the student is an approved member of (S2-03).
public record GetStudentClassesQuery : BaseFilterQuery, IRequest<PaginatedResult<StudentClassDto>>;
