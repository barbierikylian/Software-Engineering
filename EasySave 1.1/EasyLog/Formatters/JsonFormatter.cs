using System.Text.Json;
using System.Text.Json.Serialization;

namespace EasyLog
{
   
    public class JsonFormatter : IFormatter
    {
        private readonly JsonSerializerOptions _options;

        public JsonFormatter()
        {
            _options = new JsonSerializerOptions
            {
                WriteIndented = true,
                DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
            };
        }

        public string Serialize(LogModel logModel)
        {
            return JsonSerializer.Serialize(logModel, _options);
        }

        public string FileExtension => "json";
    }
}