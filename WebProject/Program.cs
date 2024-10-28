using Microsoft.AspNetCore.DataProtection.Repositories;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using StackExchange.Redis;
using WebProject.Hubs;
using WebProject.Models;

namespace WebProject
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);
            string connectionString = builder.Configuration.GetConnectionString("DatabaseConnection")
                ?? throw new InvalidOperationException("Connection string not found.");

            builder.Services.AddControllersWithViews();
            builder.Services.AddDbContext<ProblemDbContext>(options => options.UseSqlServer(connectionString));

            builder.Services.AddDefaultIdentity<IdentityUser>().AddEntityFrameworkStores<ProblemDbContext>();
            builder.Services.AddScoped<IProblemRepository, ProblemRepository>();
            builder.Services.AddSignalR(options =>
            {
                options.EnableDetailedErrors = true;
                options.KeepAliveInterval = TimeSpan.FromSeconds(15);
                options.ClientTimeoutInterval = TimeSpan.FromSeconds(30);
            }).AddStackExchangeRedis(redisOptions =>
            {
                redisOptions.ConnectionFactory = async writer =>
                {
                    var configuration = ConfigurationOptions.Parse(
                        builder.Configuration.GetConnectionString("Redis") ?? "localhost:6379");

                    var connection = await ConnectionMultiplexer.ConnectAsync(configuration, writer);
                    connection.ConnectionFailed += (_, e) =>
                    {
                        Console.WriteLine("Connection to Redis failed.");
                    };

                    if (!connection.IsConnected)
                    {
                        Console.WriteLine("Did not connect to Redis.");
                    }

                    return connection;
                };
            });


            var app = builder.Build();

            if (!app.Environment.IsDevelopment())
            {
                app.UseExceptionHandler("/Home/Error");
                app.UseHsts();
            }

            app.UseHttpsRedirection();
            app.UseStaticFiles();

            app.UseRouting();

            app.UseAuthentication();
            app.UseAuthorization();

            app.MapControllerRoute(
                name: "default",
                pattern: "{controller=Home}/{action=Index}/{id?}");

            app.MapHub<ProblemHub>("/problemHub");
            app.MapRazorPages();
            app.Run();
        }
    }
}
