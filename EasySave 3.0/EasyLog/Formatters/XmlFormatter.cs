using System.Globalization;
using System.Reflection;
using System.Xml.Linq;

namespace EasyLog
{
    public class XmlFormatter : IFormatter
    {
        public string Serialize(LogModel logModel)
        {
            XElement root = new XElement("LogEntry");

            PropertyInfo[] properties = typeof(LogModel).GetProperties();

            foreach (PropertyInfo property in properties)
            {
                object? value = property.GetValue(logModel);
                if (value == null) continue;

                root.Add(new XElement(property.Name, FormatValue(value)));
            }

            return root.ToString();
        }

        public string FileExtension => "xml";

        private static string FormatValue(object value)
        {
            return value switch
            {
                IFormattable formattable => formattable.ToString(null, CultureInfo.InvariantCulture),
                _ => value.ToString() ?? string.Empty
            };
        }
    }
}