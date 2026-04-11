using System.Text.Json;
using System.Text.Json.Serialization;

namespace Shared.Core.Caching;

/// <summary>
/// JSON converter factory for Result&lt;T&gt; that serializes only the inner value.
/// Since we only cache successful results, this avoids issues with Result's private constructors.
/// </summary>
public sealed class ResultJsonConverterFactory : JsonConverterFactory
{
    public override bool CanConvert(Type typeToConvert)
    {
        return typeToConvert.IsGenericType
               && typeToConvert.GetGenericTypeDefinition() == typeof(Result<>);
    }

    public override JsonConverter CreateConverter(Type typeToConvert, JsonSerializerOptions options)
    {
        var valueType = typeToConvert.GetGenericArguments()[0];
        var converterType = typeof(ResultJsonConverter<>).MakeGenericType(valueType);
        return (JsonConverter)Activator.CreateInstance(converterType)!;
    }
}

internal sealed class ResultJsonConverter<T> : JsonConverter<Result<T>>
{
    public override Result<T>? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        var value = JsonSerializer.Deserialize<T>(ref reader, options);
        if (value is null)
            return default;

        // Uses the implicit operator: public static implicit operator Result<TValue>(TValue value)
        return value;
    }

    public override void Write(Utf8JsonWriter writer, Result<T> value, JsonSerializerOptions options)
    {
        if (value.IsSuccess && value.Value is not null)
        {
            JsonSerializer.Serialize(writer, value.Value, options);
        }
        else
        {
            writer.WriteNullValue();
        }
    }
}
