// Copyright (c) Microsoft Corporation.
// Licensed under the MIT license.

namespace CustomerSupportServiceSample.Events
{
    public class PrimitiveDictionaryConverter : JsonConverter<Dictionary<string, object>>
    {
        public override Dictionary<string, object>? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            var doc = JsonDocument.ParseValue(ref reader);
            var objenum = doc.RootElement.EnumerateObject();
            var result = new Dictionary<string, object>();
            while (objenum.MoveNext())
            {
                var value = ConvertJsonElement(objenum.Current.Value);
                if (value != null)
                {
                    result.Add(objenum.Current.Name, value);
                }
            }

            return result;
        }

        public override void Write(Utf8JsonWriter writer, Dictionary<string, object> value, JsonSerializerOptions options)
        {
            throw new NotImplementedException();
        }

        private static object? ConvertJsonElement(JsonElement? element)
        {
            switch (element?.ValueKind)
            {
                case JsonValueKind.Object:
                    throw new JsonException("Nested objects are not supported");
                case JsonValueKind.Array:
                    throw new JsonException("Arrays are not supported");
                case JsonValueKind.String:
                    return element?.GetString();
                case JsonValueKind.Number:
                    return element?.GetDecimal();
                case JsonValueKind.False:
                    return false;
                case JsonValueKind.True:
                    return true;
            }

            return null;
        }
    }
}
