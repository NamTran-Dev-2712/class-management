namespace ClassManagement.Infrastructure.Persistence.Seeds.Demo;

// Mutable carrier passed between the demo sub-seeders so each step can reference rows created by the
// previous one (with their generated long ids + loaded child collections) without re-querying.
internal sealed class DemoSeedState
{
    public List<Subject> Subjects { get; set; } = [];
    public List<ApplicationUser> Teachers { get; } = [];
    public List<ApplicationUser> Students { get; } = [];

    // Classes carry their Memberships collection loaded; Questions carry Options + Tags; Exams carry
    // Questions; published Assignments carry Snapshot → Questions → Options.
    public List<Class> Classes { get; } = [];
    public List<Question> Questions { get; } = [];
    public List<Exam> Exams { get; } = [];
    public List<Assignment> Assignments { get; } = [];

    public IEnumerable<Question> QuestionsOf(long teacherId) =>
        Questions.Where(q => q.TeacherId == teacherId);

    public IEnumerable<Class> ClassesOf(long teacherId) =>
        Classes.Where(c => c.OwnerId == teacherId);

    public IEnumerable<Exam> ExamsOf(long teacherId) => Exams.Where(e => e.TeacherId == teacherId);
}
