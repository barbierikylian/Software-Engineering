using System;
using System.IO;
using System.Xml.Linq;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;

WebApplicationBuilder builder = WebApplication.CreateBuilder(args);
WebApplication app = builder.Build();

string logDir = Path.Combine(Directory.GetCurrentDirectory(), "logs");
if (Directory.Exists(logDir) == false)
{
    Directory.CreateDirectory(logDir);
}

object _fileLock = new object();

app.MapPost("/api/logs", async (HttpContext context) =>
{
    string body;
    using (StreamReader reader = new StreamReader(context.Request.Body))
    {
        body = await reader.ReadToEndAsync();
    }

    Console.WriteLine("[LOG RECEIVED] " + DateTime.Now.ToString("HH:mm:ss") + " : " + body);

    string date = DateTime.Now.ToString("yyyy-MM-dd");

    bool isXml = false;
    if (context.Request.ContentType != null)
    {
        if (context.Request.ContentType.Contains("xml"))
        {
            isXml = true;
        }
    }

    string ext = "json";
    if (isXml)
    {
        ext = "xml";
    }

    string logFile = Path.Combine(logDir, "centralized_log_" + date + "." + ext);

    lock (_fileLock)
    {
        if (isXml)
        {
            try
            {
                XDocument doc;
                bool fileExists = File.Exists(logFile);
                long fileLength = 0;

                if (fileExists)
                {
                    FileInfo fileInfo = new FileInfo(logFile);
                    fileLength = fileInfo.Length;
                }

                if (fileExists == false || fileLength == 0)
                {
                    XElement rootElement = new XElement("Logs");
                    doc = new XDocument(rootElement);
                }
                else
                {
                    string fileContent = File.ReadAllText(logFile);
                    doc = XDocument.Parse(fileContent);
                }

                XElement newElement = XElement.Parse(body);
                if (doc.Root != null)
                {
                    doc.Root.Add(newElement);
                }

                string finalXml = "<?xml version=\"1.0\" encoding=\"utf-8\"?>\n" + doc.ToString();
                File.WriteAllText(logFile, finalXml);
            }
            catch
            {
                File.AppendAllText(logFile, body + Environment.NewLine);
            }
        }
        else
        {
            bool addComma = false;
            if (File.Exists(logFile))
            {
                FileInfo fileInfo = new FileInfo(logFile);
                if (fileInfo.Length > 0)
                {
                    addComma = true;
                }
            }

            using (StreamWriter writer = new StreamWriter(logFile, true))
            {
                if (addComma)
                {
                    writer.WriteLine(",");
                }
                writer.WriteLine(body);
            }
        }
    }

    return Results.Ok();
});

app.Run("http://0.0.0.0:8080");