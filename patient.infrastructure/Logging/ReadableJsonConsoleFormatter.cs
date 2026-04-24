using System.Text.Json;
using System.Text.Json.Serialization;
using Serilog.Events;
using Serilog.Formatting;

namespace patient.infrastructure.Logging;

public sealed class ReadableJsonConsoleFormatter : ITextFormatter
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
    };

    private static readonly HashSet<string> ReservedProperties =
    [
        "SourceContext",
        "TraceId",
        "SpanId",
        "RequestId",
        "ConnectionId",
        "RequestPath",
        "RequestMethod",
        "StatusCode",
        "Elapsed",
        "ClientIp",
        "UserId"
    ];

    public void Format(LogEvent logEvent, TextWriter output)
    {
        var payload = new Dictionary<string, object?>(StringComparer.Ordinal)
        {
            ["timestamp"] = logEvent.Timestamp.UtcDateTime.ToString("O"),
            ["level"] = logEvent.Level.ToString(),
            ["message"] = logEvent.RenderMessage()
        };

        AddKnownProperty(payload, logEvent, "SourceContext", "source");
        AddKnownProperty(payload, logEvent, "TraceId", "traceId");
        AddKnownProperty(payload, logEvent, "SpanId", "spanId");
        AddKnownProperty(payload, logEvent, "RequestId", "requestId");
        AddKnownProperty(payload, logEvent, "ConnectionId", "connectionId");
        AddKnownProperty(payload, logEvent, "RequestPath", "endpoint");
        AddKnownProperty(payload, logEvent, "RequestMethod", "method");
        AddKnownProperty(payload, logEvent, "StatusCode", "statusCode");
        AddKnownProperty(payload, logEvent, "ClientIp", "ip");
        AddKnownProperty(payload, logEvent, "UserId", "userId");

        if (logEvent.Properties.TryGetValue("Elapsed", out var elapsedValue))
        {
            var rawElapsed = ToPlainObject(elapsedValue);
            if (rawElapsed is double elapsedDouble)
                payload["elapsedMs"] = Math.Round(elapsedDouble, 1);
            else if (rawElapsed is float elapsedFloat)
                payload["elapsedMs"] = Math.Round(elapsedFloat, 1);
            else
                payload["elapsedMs"] = rawElapsed;
        }

        foreach (var property in logEvent.Properties)
        {
            if (ReservedProperties.Contains(property.Key))
                continue;

            payload[ToCamelCase(property.Key)] = ToPlainObject(property.Value);
        }

        if (logEvent.Exception is not null)
            payload["exception"] = logEvent.Exception.ToString();

        output.WriteLine(JsonSerializer.Serialize(payload, JsonOptions));
    }

    private static void AddKnownProperty(
        IDictionary<string, object?> payload,
        LogEvent logEvent,
        string sourceName,
        string targetName)
    {
        if (!logEvent.Properties.TryGetValue(sourceName, out var value))
            return;

        payload[targetName] = ToPlainObject(value);
    }

    private static object? ToPlainObject(LogEventPropertyValue value) =>
        value switch
        {
            ScalarValue scalar => scalar.Value,
            SequenceValue sequence => sequence.Elements.Select(ToPlainObject).ToArray(),
            StructureValue structure => structure.Properties.ToDictionary(
                property => ToCamelCase(property.Name),
                property => ToPlainObject(property.Value)),
            DictionaryValue dictionary => dictionary.Elements.ToDictionary(
                pair => ToPlainObject(pair.Key)?.ToString() ?? string.Empty,
                pair => ToPlainObject(pair.Value)),
            _ => value.ToString()
        };

    private static string ToCamelCase(string value)
    {
        if (string.IsNullOrWhiteSpace(value) || char.IsLower(value[0]))
            return value;

        return char.ToLowerInvariant(value[0]) + value[1..];
    }
}
