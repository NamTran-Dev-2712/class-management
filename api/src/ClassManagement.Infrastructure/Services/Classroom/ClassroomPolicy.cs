using ClassManagement.Infrastructure.Configuration;
using Microsoft.Extensions.Options;

namespace ClassManagement.Infrastructure.Services.Classroom;

// Exposes classroom tunables (bound from appsettings) to Application handlers via IClassroomPolicy.
public sealed class ClassroomPolicy : IClassroomPolicy
{
    private readonly ClassroomOptions _options;

    public ClassroomPolicy(IOptions<ClassroomOptions> options)
    {
        _options = options.Value;
    }

    public int MaxClassesPerTeacher => _options.MaxClassesPerTeacher;
}
