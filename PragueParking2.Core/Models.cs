using System.Text.Json.Serialization;

namespace PragueParking2.Core;

[JsonConverter(typeof(VehicleJsonConverter))]
public abstract class Vehicle
{
    public string RegNo { get; init; } = string.Empty;
    public string Type { get; init; } = string.Empty; // "CAR", "MC"
    public int SizeUnits { get; init; }               // e.g., CAR=4, MC=2
    public DateTime CheckInUtc { get; init; } = DateTime.UtcNow;
}

public sealed class Car : Vehicle
{
    public Car(string regNo, int sizeUnits = 4)
    {
        RegNo = regNo.ToUpperInvariant();
        Type = "CAR";
        SizeUnits = sizeUnits;
    }
}

public sealed class Motorcycle : Vehicle
{
    public Motorcycle(string regNo, int sizeUnits = 2)
    {
        RegNo = regNo.ToUpperInvariant();
        Type = "MC";
        SizeUnits = sizeUnits;
    }
}

public sealed class ParkingSpot
{
    public int Number { get; init; }                 // 1-based
    public int CapacityUnits { get; init; } = 4;     // default spot capacity
    public List<Vehicle> Vehicles { get; init; } = new();

    [JsonIgnore]
    public int UsedUnits => Vehicles.Sum(v => v.SizeUnits);

    [JsonIgnore]
    public int FreeUnits => CapacityUnits - UsedUnits;

    public bool CanFit(Vehicle v) => v.SizeUnits <= FreeUnits;
}

public sealed class ParkingGarage
{
    public List<ParkingSpot> Spots { get; init; } = new();
    public Dictionary<string, decimal> HourlyRates { get; init; } = new() { { "CAR", 20m }, { "MC", 10m } };
    public int FreeMinutes { get; init; } = 10;

    public ParkingGarage() { }

    public ParkingGarage(int spotCount, int spotCapacityUnits = 4)
    {
        for (int i = 1; i <= spotCount; i++)
        {
            Spots.Add(new ParkingSpot { Number = i, CapacityUnits = spotCapacityUnits });
        }
    }

    public ParkingSpot? FindSpotFor(Vehicle v)
        => Spots.FirstOrDefault(s => s.CanFit(v));

    public bool Park(Vehicle v, out int spotNumber)
    {
        var spot = FindSpotFor(v);
        if (spot is null) { spotNumber = -1; return false; }
        // Prevent duplicates
        if (FindVehicle(v.RegNo) is null)
        {
            spot.Vehicles.Add(v);
            spotNumber = spot.Number;
            return true;
        }
        spotNumber = -1; return false;
    }

    public (ParkingSpot? spot, Vehicle? vehicle)? FindVehicle(string regNo)
    {
        regNo = regNo.ToUpperInvariant();
        foreach (var s in Spots)
        {
            var veh = s.Vehicles.FirstOrDefault(v => v.RegNo.Equals(regNo, StringComparison.OrdinalIgnoreCase));
            if (veh is not null) return (s, veh);
        }
        return null;
    }

    public bool Move(string regNo, int toSpotNumber, out string message)
    {
        var result = FindVehicle(regNo);
        if (result is null || result.Value.spot is null || result.Value.vehicle is null) { message = "Fordon hittades inte."; return false; }
        var (fromSpot, vehicle) = result.Value;
        var toSpot = Spots.FirstOrDefault(s => s.Number == toSpotNumber);
        if (toSpot is null) { message = "Målplats finns inte."; return false; }
        if (!toSpot.CanFit(vehicle)) { message = "Målplats saknar utrymme."; return false; }
        if (ReferenceEquals(fromSpot, toSpot)) { message = "Redan på platsen."; return false; }
        fromSpot.Vehicles.Remove(vehicle);
        toSpot.Vehicles.Add(vehicle);
        message = $"Flyttade {vehicle.Type} {vehicle.RegNo} till plats {toSpot.Number}.";
        return true;
    }

    public bool Checkout(string regNo, out decimal fee, out string message)
    {
        fee = 0m;
        var result = FindVehicle(regNo);
        if (result is null || result.Value.spot is null || result.Value.vehicle is null) { message = "Fordon hittades inte."; return false; }
        var (spot, vehicle) = result.Value;

        var parkedMinutes = (int)Math.Max(0, (DateTime.UtcNow - vehicle.CheckInUtc).TotalMinutes);
        if (parkedMinutes <= FreeMinutes)
        {
            fee = 0m;
        }
        else
        {
            var hours = (int)Math.Ceiling((parkedMinutes - FreeMinutes) / 60.0);
            var rate = HourlyRates.TryGetValue(vehicle.Type, out var r) ? r : 0m;
            fee = rate * hours;
        }

        spot.Vehicles.Remove(vehicle);
        message = $"Uttagen {vehicle.Type} {vehicle.RegNo}. Avgift: {fee} CZK.";
        return true;
    }
}