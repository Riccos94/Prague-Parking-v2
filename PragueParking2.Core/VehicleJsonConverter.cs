using System.Text.Json;
using System.Text.Json.Serialization;

namespace PragueParking2.Core;

public class VehicleJsonConverter : JsonConverter<Vehicle>
{
    public override Vehicle? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        if (reader.TokenType != JsonTokenType.StartObject)
        {
            throw new JsonException();
        }

        string? regNo = null;
        string? type = null;
        int sizeUnits = 0;
        DateTime checkInUtc = DateTime.UtcNow;

        while (reader.Read())
        {
            if (reader.TokenType == JsonTokenType.EndObject)
            {
                if (string.IsNullOrEmpty(regNo) || string.IsNullOrEmpty(type))
                {
                    throw new JsonException("Missing required fields");
                }

                return type.ToUpperInvariant() switch
                {
                    "CAR" => new Car(regNo, sizeUnits) { CheckInUtc = checkInUtc },
                    "MC" => new Motorcycle(regNo, sizeUnits) { CheckInUtc = checkInUtc },
                    _ => throw new JsonException($"Unknown vehicle type: {type}")
                };
            }

            if (reader.TokenType != JsonTokenType.PropertyName)
            {
                throw new JsonException();
            }

            string propertyName = reader.GetString()!;
            reader.Read();

            switch (propertyName.ToLower())
            {
                case "regno":
                    regNo = reader.GetString();
                    break;
                case "type":
                    type = reader.GetString();
                    break;
                case "sizeunits":
                    sizeUnits = reader.GetInt32();
                    break;
                case "checkinutc":
                    checkInUtc = reader.GetDateTime();
                    break;
            }
        }

        throw new JsonException();
    }

    public override void Write(Utf8JsonWriter writer, Vehicle value, JsonSerializerOptions options)
    {
        writer.WriteStartObject();
        writer.WriteString("regNo", value.RegNo);
        writer.WriteString("type", value.Type);
        writer.WriteNumber("sizeUnits", value.SizeUnits);
        writer.WriteString("checkInUtc", value.CheckInUtc);
        writer.WriteEndObject();
    }
}