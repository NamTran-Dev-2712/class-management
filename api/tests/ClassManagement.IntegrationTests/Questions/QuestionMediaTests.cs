using ClassManagement.Application.Common.Constants;

namespace ClassManagement.IntegrationTests.Questions;

// MVP-9: attaching media to questions (attachment list + per-option image) and validating ownership.
public class QuestionMediaTests(ApiFactory factory) : IClassFixture<ApiFactory>
{
    private readonly ApiFactory _factory = factory;

    private Task<HttpClient> TeacherAsync() =>
        AuthHelper.CreateRoleClientAsync(_factory, ApplicationRoles.Teacher);

    [Fact]
    public async Task Create_WithAttachmentAndOptionImage_DetailReturnsMedia()
    {
        var teacher = await TeacherAsync();
        var (attachmentId, _) = await MediaHelper.UploadAsync(teacher);
        var (optionImageId, _) = await MediaHelper.UploadAsync(teacher);

        var payload = new
        {
            subjectId = (string?)null,
            type = "SingleChoice",
            content = QuestionsApi.UniqueContent(),
            difficulty = "Medium",
            suggestedPoint = 1.0,
            visibility = "Private",
            explanation = (string?)null,
            options = new object[]
            {
                new
                {
                    content = "Alpha",
                    isCorrect = true,
                    mediaPublicId = optionImageId,
                },
                new { content = "Beta", isCorrect = false },
            },
            tags = Array.Empty<string>(),
            attachments = new object[]
            {
                new
                {
                    mediaPublicId = attachmentId,
                    role = "Attachment",
                    displayOrder = 0,
                },
            },
        };

        var create = await QuestionsApi.CreateRawAsync(teacher, payload);
        create.StatusCode.Should().Be(HttpStatusCode.Created);
        var publicId = (await AuthHelper.ReadDataAsync(create)).GetProperty("publicId").GetString();

        var detail = await teacher.GetAsync($"/api/teacher/questions/{publicId}");
        detail.StatusCode.Should().Be(HttpStatusCode.OK);
        var data = await AuthHelper.ReadDataAsync(detail);

        var media = data.GetProperty("media");
        media.GetArrayLength().Should().Be(1);
        media[0].GetProperty("mediaPublicId").GetGuid().Should().Be(attachmentId);
        media[0].GetProperty("kind").GetString().Should().Be("Image");
        media[0].GetProperty("url").GetString().Should().NotBeNullOrEmpty();

        var firstOption = data.GetProperty("options")[0];
        firstOption.GetProperty("mediaPublicId").GetGuid().Should().Be(optionImageId);
        firstOption.GetProperty("mediaUrl").GetString().Should().NotBeNullOrEmpty();
    }

    [Fact]
    public async Task Create_WithMediaOwnedByAnotherTeacher_Returns400()
    {
        var owner = await TeacherAsync();
        var (foreignMediaId, _) = await MediaHelper.UploadAsync(owner);

        var other = await TeacherAsync();
        var payload = BuildAttachmentPayload(foreignMediaId);

        var create = await QuestionsApi.CreateRawAsync(other, payload);
        create.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task Create_WithPendingMedia_Returns400()
    {
        var teacher = await TeacherAsync();
        // Presign only — never confirmed, so the asset stays Pending and is not a valid reference.
        var presign = await MediaHelper.PresignAsync(teacher, "x.png", "image/png", 1024);
        var pendingId = presign.GetProperty("mediaPublicId").GetGuid();

        var create = await QuestionsApi.CreateRawAsync(teacher, BuildAttachmentPayload(pendingId));
        create.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    private static object BuildAttachmentPayload(Guid mediaPublicId) =>
        new
        {
            subjectId = (string?)null,
            type = "SingleChoice",
            content = QuestionsApi.UniqueContent(),
            difficulty = "Medium",
            suggestedPoint = 1.0,
            visibility = "Private",
            explanation = (string?)null,
            options = new object[]
            {
                new { content = "Alpha", isCorrect = true },
                new { content = "Beta", isCorrect = false },
            },
            tags = Array.Empty<string>(),
            attachments = new object[]
            {
                new
                {
                    mediaPublicId,
                    role = "Attachment",
                    displayOrder = 0,
                },
            },
        };
}
