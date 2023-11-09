namespace WakeOnLANServer;

public class Program
{
    public static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

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
