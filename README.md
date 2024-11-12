# IS202 - Kartverket rapport system
#### Gruppe 19
___
## System arkitektur
Løsningen tar i bruk en moderne og skalerbar pakke med programvare som integrerer ASP.NET Core MVC med MariaDB som relasjonel database system.
Systemet følger MVC mønsteret. Mønsteret handler om Modeller, Visninger og Kontrollere som blir brukt for å skape nettsider, behandle data og styre flyten av forespørseler.
I tillegg bruker systemet Entity Framework Core for å hente og lagre data, hvor den fungerer som et Objekt-relasjonelt kartleggings verktøy (ORM). Dette simplifiserer database samhandlinger.
Endelig er det MariaDB som sørger for en robust og høyt ytende database-maskin som støtter avansert indeksing og overførings-støtte. Dette medfører rask og pålitelig data lagring.
Denne arkitekturen utnytter .NET 8 sin ytelse, kapasitet på tvers av plattformer og effektive håndtering av nettforespørsler for å levere en skalerbar, vedlikeholdbar løsning.

* MariaDB Database 11.5.2
* ASP.NET Core MVC .NET 8
* Entity Framework Core 8.0.2
* Docker (Compose)

## Systemdrift instruksjon
### Kjør i docker med docker-compose
Bruker MariaDB 11.5.2 som database og ASP.NET Core med MVC mønster i Docker med hjelp av Docker-Compose fil.
1. Run `docker-compose up`
2. Open http://localhost:8080 in your browser

## Testing scenarier
1. tbd