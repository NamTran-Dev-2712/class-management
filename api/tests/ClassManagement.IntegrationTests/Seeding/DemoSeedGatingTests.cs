using ClassManagement.Infrastructure.Persistence.DbContext;
using ClassManagement.Infrastructure.Persistence.Seeds.Demo;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace ClassManagement.IntegrationTests.Seeding;

// The end-to-end demo dataset is dev-only: gated by IHostEnvironment.IsDevelopment() AND Seed:DemoData.
// ApiFactory forces Seed:DemoData=false, so the test host must never contain demo rows. These tests are
// the guard that the gate stays off in the test/CI environment.
public class DemoSeedGatingTests(ApiFactory factory) : IClassFixture<ApiFactory>
{
    private readonly ApiFactory _factory = factory;

    [Fact]
    public async Task DemoSeed_IsDisabledInTestHost_NoMarkerUser()
    {
        var exists = await _factory.WithScopeAsync(sp =>
        {
            var context = sp.GetRequiredService<ApplicationDbContext>();
            return context.Users.AnyAsync(u => u.Email == DemoDataSeeder.MarkerEmail);
        });

        exists.Should().BeFalse();
    }

    [Fact]
    public async Task DemoSeed_IsDisabledInTestHost_NoDemoClasses()
    {
        var exists = await _factory.WithScopeAsync(sp =>
        {
            var context = sp.GetRequiredService<ApplicationDbContext>();
            return context.Classes.AnyAsync(c => c.InviteCode.StartsWith("DEMO"));
        });

        exists.Should().BeFalse();
    }
}
