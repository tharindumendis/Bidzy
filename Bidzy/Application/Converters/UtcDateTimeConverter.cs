namespace Bidzy.Application.Converters
{
    using System.Text.Json;
    using System.Text.Json.Serialization;

    public class UtcDateTimeConverter : JsonConverter<DateTime>
    {

        public override DateTime Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            // Parse incoming date strings as UTC
            return DateTime.Parse(reader.GetString()!).ToUniversalTime();
        }

        public override void Write(Utf8JsonWriter writer, DateTime value, JsonSerializerOptions options)
        {
            // Always serialize as UTC with 'Z' suffix
            writer.WriteStringValue(value.ToString("yyyy-MM-dd'T'HH:mm:ss.fff'Z'"));
        }
    }
}
