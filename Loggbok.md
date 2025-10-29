# Loggbok – Prague Parking V2

### 2025-10-21
- Började med att kolla igenom kraven för V2. Det var en hel del att ta in med OOP, JSON-hantering och det nya UI-biblioteket Spectre.Console.
- Tog min tid med att skapa ett bra start- ville verkligen få grundstrukturen rätt från början.

### 2025-10-22
- Fokuserade på Core-delen med fordonsklasser. Valde att göra Vehicle som abstrakt basklass - kändes som ett bra sätt att hantera de olika fordonstyperna.
- Brottades lite med hur parkeringsplatsernas storlek skulle hanteras. Landade i en lösning med "units" som känns flexibel.

### 2025-10-23
- Stor utmaning idag: JSON-serialisering. Stötte på problem med att spara och läsa fordonsdata eftersom Vehicle är en abstrakt klass.
- Efter mycket felsökning implementerade en egen JsonConverter - det löste problemet med serialisering av fordon.
- Lärt mig mycket om hur JSON.NET hanterar arv och abstrakta klasser.

### 2025-10-24
- Första mötet med Spectre.Console - vilken uppgradering från vanlig console.write!
- Fastnade ett tag på hur tabeller skulle formateras för översiktskartan. Experimenterade med olika layouts.
- Fick kämpa lite med att få färgkodningen rätt i kartan - ville göra det tydligt hur belagd varje plats är.

### 2025-10-25
- Upptäckte att fordonsdata försvann vid omstart - visade sig att filerna sparades i fel mapp.
- Tog hjälp av AI och nu sparas allt i projektmappen istället för build-mappen.
- La till reload av konfiguration - bra för testning

### 2025-10-26
- Började med tester - insåg att jag borde gjort detta tidigare i processen.
- Hittade faktiskt några edge cases när jag skrev testerna, särskilt kring gratisperioden.
- Städade upp i koden och dokumenterade - alltid överraskande hur mycket små förbättringar man kan hitta.

### 2025-10-27 - 2025-10-28
- Genomförde omfattande testning av alla funktioner.
- Stötte på och fixade ett oväntat problem där nånstans i koden pga att jag höll på felsöka med AI så skapades demo vechichle
- Förbättrade användargränssnittet baserat på egen användning - lade till tydligare felmeddelanden.

