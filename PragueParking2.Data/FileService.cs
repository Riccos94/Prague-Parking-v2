using System.Text.Json;
using PragueParking2.Core;

namespace PragueParking2.Data;

public sealed class FileService
{
    public string DataPath { get; }
    public string ConfigPath { get; }

    private readonly JsonSerializerOptions _opts = new()
    {
        WriteIndented = true,
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
    };

    public FileService(string dataPath, string configPath)
    {
        DataPath = dataPath;
        ConfigPath = configPath;
    }

    public (ParkingGarage garage, string? error) LoadAll()
    {
        try
        {
            var cfgJson = File.ReadAllText(ConfigPath);
            var cfg = JsonSerializer.Deserialize<Config>(cfgJson, _opts) ?? new();

            ParkingGarage garage;
            if (File.Exists(DataPath))
            {
                var json = File.ReadAllText(DataPath);
                var persisted = JsonSerializer.Deserialize<ParkingGarage>(json, _opts);
                garage = persisted ?? new ParkingGarage(cfg.SpotCount, cfg.SpotCapacityUnits);
                
                // If the persisted garage has no spots, reinitialize it
                if (garage.Spots.Count == 0)
                {
                    garage = new ParkingGarage(cfg.SpotCount, cfg.SpotCapacityUnits);
                }
            }
            else
            {
                garage = new ParkingGarage(cfg.SpotCount, cfg.SpotCapacityUnits);
                SaveGarage(garage);
            }

            // Apply pricing from config
            if (cfg.HourlyRates is not null && cfg.HourlyRates.Count > 0)
                foreach (var kv in cfg.HourlyRates) garage.HourlyRates[kv.Key.ToUpperInvariant()] = kv.Value;

            // Replace spots that have zero capacity with new spots with correct capacity
            for (int i = 0; i < garage.Spots.Count; i++)
            {
                var spot = garage.Spots[i];
                if (spot.CapacityUnits == 0)
                {
                    garage.Spots[i] = new ParkingSpot 
                    { 
                        Number = spot.Number, 
                        CapacityUnits = cfg.SpotCapacityUnits,
                        Vehicles = spot.Vehicles
                    };
                }
            }

            return (garage, null);
        }
        catch (Exception ex)
        {
            return (new ParkingGarage(100), ex.Message);
        }
    }

    public (Config cfg, string? error) LoadConfig()
    {
        try
        {
            var cfgJson = File.ReadAllText(ConfigPath);
            var cfg = System.Text.Json.JsonSerializer.Deserialize<Config>(cfgJson, _opts) ?? new();
            return (cfg, null);
        }
        catch (Exception ex)
        {
            return (new Config(), ex.Message);
        }
    }

    public void SaveGarage(ParkingGarage garage)
    {
        var json = JsonSerializer.Serialize(garage, _opts);
        File.WriteAllText(DataPath, json);
    }

    public void SaveConfig(Config cfg)
    {
        var json = JsonSerializer.Serialize(cfg, _opts);
        File.WriteAllText(ConfigPath, json);
    }
}

public sealed class Config
{
    public int SpotCount { get; init; } = 100;
    public int SpotCapacityUnits { get; init; } = 4; // default equals 1 CAR
    public int FreeMinutes { get; init; } = 10;

    public Dictionary<string, int> VehicleTypes { get; init; } = new()
    {
        { "CAR", 4 },
        { "MC", 2 }
    };

    public Dictionary<string, decimal> HourlyRates { get; init; } = new()
    {
        { "CAR", 20m },
        { "MC", 10m }
    };
}