using System;
using System.IO;
using System.Xml.Linq;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;

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
    bool isXml = context.Request.ContentType != null && context.Request.ContentType.Contains("xml");
    string ext = isXml ? "xml" : "json";
    string logFile = Path.Combine(logDir, $"centralized_log_{date}.{ext}");

    lock (_fileLock)
    {
        if (isXml)
        {
            try
            {
                XDocument doc;
                if (!File.Exists(logFile) || new FileInfo(logFile).Length == 0)
                    doc = new XDocument(new XElement("Logs"));
                else
                    doc = XDocument.Parse(File.ReadAllText(logFile));

                doc.Root?.Add(XElement.Parse(body));
                File.WriteAllText(logFile, "<?xml version=\"1.0\" encoding=\"utf-8\"?>\n" + doc.ToString());
            }
            catch
            {
                File.AppendAllText(logFile, body + Environment.NewLine);
            }
        }
        else
        {
            bool addComma = File.Exists(logFile) && new FileInfo(logFile).Length > 0;
            using var writer = new StreamWriter(logFile, true);
            if (addComma) writer.WriteLine(",");
            writer.WriteLine(body);
        }
    }
    return Results.Ok();
});

app.Run("http://0.0.0.0:8080");