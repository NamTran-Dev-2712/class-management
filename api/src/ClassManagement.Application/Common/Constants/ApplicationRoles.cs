namespace ClassManagement.Application.Common.Constants;

public static class ApplicationRoles
{
    public const string Admin = "Admin";
    public const string Teacher = "Teacher";
    public const string Student = "Student";

    public static readonly string[] All = [Admin, Teacher, Student];

    // Roles an admin can assign/manage through user management (admins are out of scope).
    public static readonly string[] Manageable = [Teacher, Student];
}
