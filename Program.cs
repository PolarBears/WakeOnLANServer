using System.Runtime.InteropServices;

namespace WakeOnLANServer;

public class Program
{
    public static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        builder.Host.UseSystemd();

        builder.Services.AddControllers(); ;
        builder.Services.AddSingleton<DevicesInstance>();

        var app = builder.Build();

        app.UseDefaultFiles();
        app.UseStaticFiles();

        app.UseRouting();
        app.MapControllers();

        app.Run();
    }
}
