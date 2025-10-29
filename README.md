# Prague Parking V2 

Parkeringshanteringssystem med konsolgrönssnitt byggt med Spectre.Console, JSON-lagring och konfigurering.

## Installation & Körning
1. Se till att .NET SDK 8 är installerat
2. Klona/ladda ner projektet
3. Öppna en terminal i projektmappen
4. Kör `dotnet run --project PragueParking2.App`

Alternativt via Visual Studio:
1. Öppna `PragueParkingV2.sln` i Visual Studio 2022/2025
2. Stöll in `PragueParking2.App` som startup-projekt
3. Tryck F5 / Ctrl+F5

## Funktioner
- Parkera fordon (bil eller MC)
- Hämta ut fordon (med automatisk avgiftsber�kning)
- Flytta fordon mellan platser
- Sök efter fordon via registreringsnummer
- Visa översiktskarta över parkeringen
- Ladda om priser och konfiguration

## Filstruktur
- `PragueParking2.App`  Användargrnssnitt och programmets startpunkt
  - `prices-and-config.json` - Konfiguration f�r priser och parkeringsplatser
  - `garage-data.json` - Sparad data över parkerade fordon
- `PragueParking2.Core`  Domänmodeller och logik
  - Vehicle (abstrakt basklass)
  - Car och Motorcycle (fordonstyper)
  - ParkingSpot (parkeringsplats)
  - ParkingGarage (parkeringshus)
- `PragueParking2.Data`  Datahantering (fil/JSON)
- `PragueParking2.Tests`  Enhetstester

## Priser och Regler
- Bil (CAR): 20 CZK per timme
- Motorcykel (MC): 10 CZK per timme
- Första 10 minuterna är gratis
- Påbörjad timme räknas som hel timme
- Bil tar 4 enheter, MC tar 2 enheter
- Varje parkeringsplats har 4 enheter totalt

## Konfiguration
Priser och parkeringshusets inställningar kan ändras i `prices-and-config.json`:
- `spotCount`: Antal parkeringsplatser
- `spotCapacityUnits`: Antal enheter per parkeringsplats
- `freeMinutes`: Antal gratisminuter
- `hourlyRates`: Priser per timme f�r olika fordonstyper

## Datalagring
All parkeringsdata sparas automatiskt i `garage-data.json` och löses in vid programstart. Data persisterar mellan omstarter av programmet.
