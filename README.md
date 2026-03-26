# SOS100 🚀

Ett mikrotjänstbaserat system byggt i .NET och C# för att hantera utlåning, användare och kataloger. Projektet innehåller ett webbaserat användargränssnitt samt flera fristående API:er för att separera ansvarsområden som lån, påminnelser, rapportering och användarhantering.

## 🏗️ Systemarkitektur och Tjänster

Projektet är uppdelat i flera olika mikrotjänster och en webbklient:

* **`SOS100-MVC`** - Frontend-applikationen byggd med ASP.NET Core MVC. Detta är användargränssnittet som pratar med de underliggande API:erna (HTML, CSS, JavaScript, C#).
* **`KatalogApi`** - Hanterar systemets katalog (till exempel böcker, produkter eller utrustning).
* **`SOS100-LoanAPI`** - Ansvarar för logiken kring utlåning, returer och att hålla koll på aktiva lån.
* **`UserService`** - Hanterar systemets användare.
* **`ReminderApi`** - Tjänst för att hantera och skicka ut påminnelser, exempelvis för lån som förfaller.
* **`ReportApi`** - Genererar rapporter och statistik baserat på data från systemet.
