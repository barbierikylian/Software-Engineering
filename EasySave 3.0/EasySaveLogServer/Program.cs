using System.IO;

var builder = WebApplication.CreateBuilder(args);
var app = builder.Build();

string logDir = Path.Combine(Directory.GetCurrentDirectory(), "logs");
if (!Directory.Exists(logDir)) Directory.CreateDirectory(logDir);

object _fileLock = new object();

app.MapPost("/api/logs", async (HttpContext context) =>
{
    using var reader = new StreamReader(context.Request.Body);
    string body = await reader.ReadToEndAsync();

    Console.WriteLine($"[LOG RECEIVED] {DateTime.Now:HH:mm:ss} : {body}");

    string date = DateTime.Now.ToString("yyyy-MM-dd");
    string ext = context.Request.ContentType == "application/xml" ? "xml" : "json";
    string logFile = Path.Combine(logDir, $"centralized_log_{date}.{ext}");

    lock (_fileLock)
    {
        using var writer = new StreamWriter(logFile, true);
        if (ext == "json" && new FileInfo(logFile).Length > 0)
        {
            writer.WriteLine(",");
        }
        writer.WriteLine(body);
    }
    return Results.Ok();
});

app.Run("http://0.0.0.0:8080");