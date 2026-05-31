using ClassManagement.Infrastructure.Persistence.DbContext;
using Microsoft.Extensions.Logging;

namespace ClassManagement.Infrastructure.Persistence.Seeds;

public static class SubjectSeeder
{
    private static readonly (string Name, string Description)[] DefaultSubjects =
    [
        ("Mathematics", "Algebra, Geometry, Calculus and more"),
        ("Physics", "Mechanics, Electromagnetism, Optics"),
        ("Chemistry", "Organic, Inorganic, Physical Chemistry"),
        ("Biology", "Cell Biology, Genetics, Ecology"),
        ("Literature", "Reading, Writing, Literary Analysis"),
        ("History", "World History, Vietnamese History"),
        ("English", "Grammar, Reading, Writing, Conversation"),
        ("Computer Science", "Programming, Algorithms, Data Structures"),
    ];

    public static async Task SeedAsync(ApplicationDbContext context, ILogger logger)
    {
        if (await context.Subjects.AnyAsync())
            return;

        var subjects = DefaultSubjects
            .Select(
                (s, i) =>
                    new Subject
                    {
                        Name = s.Name,
                        Description = s.Description,
                        IsActive = true,
                        DisplayOrder = i,
                        CreatedAt = DateTime.UtcNow,
                        UpdatedAt = DateTime.UtcNow,
                    }
            )
            .ToList();

        await context.Subjects.AddRangeAsync(subjects);
        await context.SaveChangesAsync();
        logger.LogInformation("Seeded {Count} subjects", subjects.Count);
    }
}
