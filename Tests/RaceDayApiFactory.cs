
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using RaceDay.API.Data;
using System.Text;

namespace RaceDay.Test
{
    public class RaceDayApiFactory : WebApplicationFactory<Program>
    {
        private readonly string _databaseName = Guid.NewGuid().ToString();

        protected override void ConfigureWebHost(IWebHostBuilder builder)
        {
            builder.ConfigureAppConfiguration((context, config) =>
            {
                config.AddInMemoryCollection(
                    new Dictionary<string, string?>
                    {
                        ["Jwt:Key"] = "TestOnly_RaceDayAPI_IntegrationTest_Key_2026!",
                        ["Jwt:Issuer"] = "RaceDayAPI",
                        ["Jwt:Audience"] = "RaceDayAPIUsers"
                    });
            });

            builder.ConfigureServices(services =>
            {
                services.RemoveAll<RaceDayContext>();
                services.RemoveAll<DbContextOptions<RaceDayContext>>();

                services.AddDbContext<RaceDayContext>(options =>
                    options.UseInMemoryDatabase(_databaseName));

                services.PostConfigure<JwtBearerOptions>(
                    JwtBearerDefaults.AuthenticationScheme,
                    options =>
                    {
                        var key = "TestOnly_RaceDayAPI_IntegrationTest_Key_2026!";

                        options.TokenValidationParameters.ValidateIssuer = true;
                        options.TokenValidationParameters.ValidIssuer = "RaceDayAPI";

                        options.TokenValidationParameters.ValidateAudience = true;
                        options.TokenValidationParameters.ValidAudience = "RaceDayAPIUsers";

                        options.TokenValidationParameters.ValidateIssuerSigningKey = true;
                        options.TokenValidationParameters.IssuerSigningKey =
                            new SymmetricSecurityKey(Encoding.UTF8.GetBytes(key));

                        options.TokenValidationParameters.ValidateLifetime = true;
                    });
            });
        }

        public void InitializeDatabase()
        {
            using var scope = Services.CreateScope();

            var db = scope.ServiceProvider
                .GetRequiredService<RaceDayContext>();

            db.Database.EnsureCreated();
        }
    }
}
