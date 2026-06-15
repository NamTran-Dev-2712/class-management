namespace ClassManagement.IntegrationTests.Classroom;

// Shared request helpers for the classroom endpoints, mirroring the style of SubjectTests.
internal static class ClassroomApi
{
    public static string UniqueName() => $"Class_{Guid.NewGuid():N}";

    public static async Task<string> CreateClassAsync(
        HttpClient teacher,
        string? name = null,
        string? subjectName = "General",
        string? subjectId = null,
        string? description = null
    )
    {
        name ??= UniqueName();
        var resp = await teacher.PostAsJsonAsync(
            "/api/teacher/classes",
            new
            {
                name,
                description,
                subjectId,
                subjectName,
                coverImageUrl = (string?)null,
            }
        );
        resp.StatusCode.Should().Be(HttpStatusCode.Created);
        var data = await AuthHelper.ReadDataAsync(resp);
        return data.GetProperty("publicId").GetString()!;
    }

    public static async Task<JsonElement> GetTeacherClassAsync(HttpClient teacher, string classId)
    {
        var resp = await teacher.GetAsync($"/api/teacher/classes/{classId}");
        resp.StatusCode.Should().Be(HttpStatusCode.OK);
        return await AuthHelper.ReadDataAsync(resp);
    }

    public static async Task<string> GetInviteCodeAsync(HttpClient teacher, string classId) =>
        (await GetTeacherClassAsync(teacher, classId)).GetProperty("inviteCode").GetString()!;

    public static Task<HttpResponseMessage> JoinAsync(HttpClient student, string inviteCode) =>
        student.PostAsJsonAsync("/api/student/classes/join", new { inviteCode });

    // Returns the membership publicId of the (single) member matching the status filter.
    public static async Task<string> GetFirstMemberIdAsync(
        HttpClient teacher,
        string classId,
        string status
    )
    {
        var resp = await teacher.GetAsync(
            $"/api/teacher/classes/{classId}/members?status={status}&pageSize=50"
        );
        resp.StatusCode.Should().Be(HttpStatusCode.OK);
        var data = await AuthHelper.ReadDataAsync(resp);
        return data.GetProperty("items")[0].GetProperty("publicId").GetString()!;
    }

    // Fetches an active seeded/admin subject (publicId + name) for catalog-subject scenarios.
    public static async Task<(string PublicId, string Name)> GetAnActiveSubjectAsync(
        HttpClient anyClient
    )
    {
        var resp = await anyClient.GetAsync("/api/subjects?pageSize=1&isActive=true&sortBy=name");
        resp.StatusCode.Should().Be(HttpStatusCode.OK);
        var item = (await AuthHelper.ReadDataAsync(resp)).GetProperty("items")[0];
        return (item.GetProperty("publicId").GetString()!, item.GetProperty("name").GetString()!);
    }
}
