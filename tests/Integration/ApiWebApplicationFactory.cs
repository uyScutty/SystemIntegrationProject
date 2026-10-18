using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using VoiceVideoService.Data;
using VoiceVideoService.Messages;
using VoiceVideoService.Tests.Fakes;

namespace VoiceVideoService.Tests.Integration;

public sealed class ApiWebApplicationFactory : WebApplicationFactory<Program>
{
    private readonly SqliteConnection _connection = new("DataSource=:memory:");

    public ApiWebApplicationFactory()
    {
        _connection.Open();
    }

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Testing");

        builder.ConfigureServices(services =>
        {
            services.RemoveAll<DbContextOptions<VoiceVideoDbContext>>();
            services.RemoveAll<IDbContextOptionsConfiguration<VoiceVideoDbContext>>();
            services.RemoveAll<IMessageClient>();

            services.AddDbContext<VoiceVideoDbContext>(options =>
                options.UseSqlite(_connection));
            services.AddSingleton<IMessageClient, FakeMessageClient>();
        });
    }

    protected override void Dispose(bool disposing)
    {
        base.Dispose(disposing);

        if (disposing)
        {
            _connection.Dispose();
        }
    }
}
