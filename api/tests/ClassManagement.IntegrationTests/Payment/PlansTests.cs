namespace ClassManagement.IntegrationTests.Payment;

// MVP-8 Slice 1 — public pricing. Verifies the seeded plans surface anonymously through GET /api/plans.
public class PlansTests(ApiFactory factory) : IClassFixture<ApiFactory>
{
    private readonly ApiFactory _factory = factory;

    [Fact]
    public async Task GetPlans_Anonymous_ReturnsSeededPlans()
    {
        var client = _factory.CreateClientWithCookies();

        var response = await client.GetAsync("/api/plans");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var data = await AuthHelper.ReadDataAsync(response);
        data.ValueKind.Should().Be(JsonValueKind.Array);

        var names = data.EnumerateArray().Select(p => p.GetProperty("name").GetString()).ToList();
        names.Should().Contain("Free");
        names.Should().Contain("Pro");

        // Free plan exposes finite limits; Pro is unlimited (null limits).
        var free = data.EnumerateArray().First(p => p.GetProperty("name").GetString() == "Free");
        free.GetProperty("priceVnd").GetInt64().Should().Be(0);
        free.GetProperty("maxClasses").GetInt32().Should().BeGreaterThan(0);

        var proMonthly = data.EnumerateArray()
            .First(p =>
                p.GetProperty("name").GetString() == "Pro"
                && p.GetProperty("billingCycle").GetString() == "Monthly"
            );
        proMonthly.GetProperty("priceVnd").GetInt64().Should().BeGreaterThan(0);
        // Pro is unlimited: maxClasses is null (the serializer omits null properties).
        var hasProLimit =
            proMonthly.TryGetProperty("maxClasses", out var proMax)
            && proMax.ValueKind != JsonValueKind.Null;
        hasProLimit.Should().BeFalse();
    }
}
