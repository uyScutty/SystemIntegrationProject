
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.Data.Sqlite;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using VoiceVideoService.Data;

namespace VoiceVideoService.Tests.Integration;

public class VoiceVideoWebApplicationFactory
    : WebApplicationFactory<Program>
{
    private readonly SqliteConnection _connection =
        new("DataSource=:memory:");

    public VoiceVideoWebApplicationFactory()
    {
        _connection.Open();
    }

    protected override void ConfigureWebHost(
        IWebHostBuilder builder)
    {
        builder.UseEnvironment("Testing");

        builder.ConfigureServices(services =>
        {
            services.RemoveAll<DbContextOptions<VoiceVideoDbContext>>();
            services.RemoveAll<IDbContextOptionsConfiguration<VoiceVideoDbContext>>();

            services.AddDbContext<VoiceVideoDbContext>(options =>
                options.UseSqlite(_connection));
        });
    }

    public async Task InitializeDatabaseAsync()
    {
        // Sørger for, at test-applikationen er startet
        _ = CreateClient();

        using var scope = Services.CreateScope();

        var context = scope.ServiceProvider
            .GetRequiredService<VoiceVideoDbContext>();

        await context.Database.EnsureCreatedAsync();
    }

    protected override void Dispose(bool disposing)
    {
        base.Dispose(disposing);

        if (disposing)
            _connection.Dispose();
    }
}
