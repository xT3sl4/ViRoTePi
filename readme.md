# ViRoTePi — dokumentacja projektu

ViRoTePi to system do obsługi paczek i paczkomatów, składający się z:
- **backendu API** w ASP.NET Core (`/paczkomat`),
- **aplikacji desktopowej WPF** (`/frontend`),
- **skryptu bazy danych** (`/inpost.sql`),
- **diagramów i materiałów projektowych** (`/docs`).

## Zakres funkcjonalny

System obsługuje trzy główne role:
- **Klient** — przegląda i odbiera paczki, nadaje paczki do innych klientów.
- **Kurier** — obsługuje paczki przypisane przez administratora i doręcza je do paczkomatów.
- **Administrator** — zarządza użytkownikami, kurierami, statystykami i paczkami oczekującymi.

## Struktura repozytorium

- `/paczkomat` — API, kontrolery, modele EF Core, konfiguracja połączenia z bazą.
- `/paczkomat/docs` — dokumentacja endpointów API.
- `/frontend` — aplikacja desktopowa WPF (.NET Framework 4.7.2).
- `/docs` — diagramy przypadków użycia i klas.
- `/inpost.sql` — skrypt SQL do utworzenia/uzupełnienia bazy danych.

## Wymagania środowiskowe

### Backend (`/paczkomat`)
- .NET SDK 8+
- MySQL/MariaDB
- Baza danych `inpost`

### Frontend (`/frontend`)
- Windows + Visual Studio (projekt WPF na **.NET Framework 4.7.2**)
- Pakiety NuGet z katalogu `frontend/packages`

## Konfiguracja bazy danych

1. Uruchom MySQL/MariaDB.
2. Zaimportuj skrypt `/home/runner/work/ViRoTePi/ViRoTePi/inpost.sql`.
3. Zweryfikuj connection string w:
   `/home/runner/work/ViRoTePi/ViRoTePi/paczkomat/appsettings.json`

Domyślnie:

```json
"DefaultConnection": "Server=localhost;Database=inpost;User=root;Password=;"
```

## Uruchamianie backendu API

```bash
cd /home/runner/work/ViRoTePi/ViRoTePi/paczkomat
dotnet run
```

W trybie developerskim Swagger jest dostępny pod adresem:
- `https://localhost:7272/swagger`
- `http://localhost:5165/swagger`

## Uruchamianie frontendu

Frontend należy uruchamiać na Windows (Visual Studio):
1. Otwórz `frontend/frontend.sln`.
2. Przywróć pakiety NuGet.
3. Uruchom projekt `frontend`.

Aplikacja komunikuje się z API pod adresem bazowym:
- `http://localhost:7272/` (zdefiniowane w `frontend/Services/ApiService.cs`)

## Dokumentacja API

Pełna dokumentacja endpointów znajduje się w katalogu:
- `/home/runner/work/ViRoTePi/ViRoTePi/paczkomat/docs/README.md`

## Diagramy

Materiały analityczne i projektowe:
- `/home/runner/work/ViRoTePi/ViRoTePi/docs/diagrams_usecase.mermaid`
- `/home/runner/work/ViRoTePi/ViRoTePi/docs/diagrams_class.mermaid`
- `/home/runner/work/ViRoTePi/ViRoTePi/docs/diagrams.html`

## Autorzy

- Robert Zając
- Piotr Szwarc
- Victor Moskwa
- Jan Wróblewski
