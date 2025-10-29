using PragueParking2.Core;
using PragueParking2.Data;
using Spectre.Console;

// Sökväg till projektmappen
var projectPath = Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, "..\\..\\.."));
var dataPath = Path.Combine(projectPath, "garage-data.json");
var configPath = Path.Combine(projectPath, "prices-and-config.json");

var fileService = new FileService(dataPath, configPath);

// Ensure config exists
if (!File.Exists(configPath))
{
    var defaultCfg = new Config();
    fileService.SaveConfig(defaultCfg);
}

var (garage, loadError) = fileService.LoadAll();
if (loadError is not null)
{
    AnsiConsole.MarkupLine($"[red]Kunde inte läsa data: {loadError.Replace("[", "[[").Replace("]", "]]")}[/]");
}

bool running = true;
while (running)
{
    AnsiConsole.Clear();
    RenderHeader();

    var choice = AnsiConsole.Prompt(
        new SelectionPrompt<string>()
            .Title("Välj ett [green]menyval[/]:")
            .PageSize(12)
            .AddChoices(new[] {
                "1) Parkera fordon",
                "2) Hämta ut fordon",
                "3) Flytta fordon",
                "4) Sök fordon",
                "5) Karta / översikt",
                "6) Läs om pris & konfig",
                "7) Avsluta"
            }));

    switch (choice[..1])
    {
        case "1":
            ParkVehicle(garage, fileService);
            break;
        case "2":
            CheckoutVehicle(garage, fileService);
            break;
        case "3":
            MoveVehicle(garage, fileService);
            break;
        case "4":
            FindVehicle(garage);
            break;
        case "5":
            RenderMap(garage);
            break;
        case "6":
            ReloadConfig(garage, fileService);
            break;
        case "7":
            running = false;
            break;
    }
}

static void RenderHeader()
{
    var rule = new Rule("[bold yellow]Prague Parking 2.0[/]");
    rule.Centered();
    AnsiConsole.Write(rule);
}

static void ParkVehicle(ParkingGarage garage, FileService fs)
{
    var type = AnsiConsole.Prompt(
        new SelectionPrompt<string>()
            .Title("Fordonstyp?")
            .AddChoices("CAR", "MC"));

    var regNo = AnsiConsole.Ask<string>("Ange registreringsnummer:");
    regNo = regNo.Trim().ToUpperInvariant();
    if (string.IsNullOrWhiteSpace(regNo) || regNo.Length > 10 || regNo.Contains(' '))
    {
        AnsiConsole.MarkupLine("[red]Ogiltigt registreringsnummer.[/]");
        AnsiConsole.MarkupLine("Tryck på valfri tangent...");
        Console.ReadKey();
        return;
    }

    Vehicle v = type == "MC" ? new Motorcycle(regNo) : new Car(regNo);
    if (garage.Park(v, out var spotNo))
    {
        fs.SaveGarage(garage);
        AnsiConsole.MarkupLine($"[green]{type} {regNo} parkerad på plats {spotNo}.[/]");
    }
    else
    {
        AnsiConsole.MarkupLine("[red]Kunde inte parkera. Ingen plats eller redan parkerad.[/]");
    }
    AnsiConsole.MarkupLine("Tryck på valfri tangent...");
    Console.ReadKey();
}

static void CheckoutVehicle(ParkingGarage garage, FileService fs)
{
    var regNo = AnsiConsole.Ask<string>("Ange registreringsnummer:");
    if (garage.Checkout(regNo, out var fee, out var msg))
    {
        fs.SaveGarage(garage);
        AnsiConsole.MarkupLine($"[green]{msg}[/]");
    }
    else
    {
        AnsiConsole.MarkupLine($"[red]{msg}[/]");
    }
    AnsiConsole.MarkupLine("Tryck på valfri tangent...");
    Console.ReadKey();
}

static void MoveVehicle(ParkingGarage garage, FileService fs)
{
    var regNo = AnsiConsole.Ask<string>("Ange registreringsnummer:");
    var to = AnsiConsole.Ask<int>("Flytta till platsnummer:");
    if (garage.Move(regNo, to, out var msg))
    {
        fs.SaveGarage(garage);
        AnsiConsole.MarkupLine($"[green]{msg}[/]");
    }
    else
    {
        AnsiConsole.MarkupLine($"[red]{msg}[/]");
    }
    AnsiConsole.MarkupLine("Tryck på valfri tangent...");
    Console.ReadKey();
}

static void FindVehicle(ParkingGarage garage)
{
    var regNo = AnsiConsole.Ask<string>("Ange registreringsnummer:");
    var result = garage.FindVehicle(regNo);
    if (result is null || result.Value.vehicle is null)
    {
        AnsiConsole.MarkupLine("[red]Fordonet finns inte i P-huset.[/]");
    }
    else
    {
        var (spot, vehicle) = result.Value;
        AnsiConsole.MarkupLine($"[green]Hittade {vehicle.Type} {vehicle.RegNo} på plats {spot!.Number}.[/]");
    }
    AnsiConsole.MarkupLine("Tryck på valfri tangent...");
    Console.ReadKey();
}

static void RenderMap(ParkingGarage garage)
{
    var table = new Table().Border(TableBorder.Rounded);
    table.AddColumn(new TableColumn("[bold]Plats[/]").Centered());
    table.AddColumn(new TableColumn("[bold]Beläggning[/]"));
    table.AddColumn(new TableColumn("[bold]Fordon[/]"));

    foreach (var spot in garage.Spots)
    {
        var pct = (int)Math.Round(100.0 * (spot.UsedUnits) / Math.Max(1, spot.CapacityUnits));
        var bar = new BarChart()
            .Width(20)
            .Label($"Plats {spot.Number}")
            .AddItem("Använd", pct);

        var status = pct switch
        {
            0 => "[grey]Tom[/]",
            50 => "[green]Delvis[/]",
            100 => "[red]Full[/]",
            _ => "[yellow]Felaktig[/]" // Should never happen with our vehicle sizes
        };

        var vehicles = spot.Vehicles.Count == 0
            ? "-"
            : string.Join(", ", spot.Vehicles.Select(v => $"{v.Type} {v.RegNo}"));

        table.AddRow($"{spot.Number}", status, vehicles);
    }

    AnsiConsole.Write(table);
    AnsiConsole.MarkupLine("Tryck på valfri tangent...");
    Console.ReadKey();
}

static void ReloadConfig(ParkingGarage garage, FileService fs)
{
    var (cfg, err) = fs.LoadConfig();
    if (err is not null)
    {
        AnsiConsole.MarkupLine($"[red]{err}[/]");
        Console.ReadKey();
        return;
    }

    // Apply rates and free minutes
    garage.HourlyRates.Clear();
    foreach (var kv in cfg.HourlyRates) garage.HourlyRates[kv.Key.ToUpperInvariant()] = kv.Value;

    // Spot count change only if expansion; shrinking is blocked for simplicity at G-level
    if (cfg.SpotCount > garage.Spots.Count)
    {
        for (int i = garage.Spots.Count + 1; i <= cfg.SpotCount; i++)
            garage.Spots.Add(new ParkingSpot { Number = i, CapacityUnits = cfg.SpotCapacityUnits });
    }

    fs.SaveGarage(garage);
    AnsiConsole.MarkupLine("[green]Konfiguration laddad.[/]");
    AnsiConsole.MarkupLine("Tryck på valfri tangent...");
    Console.ReadKey();
}