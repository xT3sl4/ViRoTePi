# Dokumentacja projektu ViRoTePi

## Spis treści

1. [Opis projektu](#1-opis-projektu)
2. [Architektura systemu](#2-architektura-systemu)
3. [Wymagania i uruchomienie](#3-wymagania-i-uruchomienie)
4. [Baza danych](#4-baza-danych)
5. [Backend – API REST](#5-backend--api-rest)
6. [Frontend – aplikacja WPF](#6-frontend--aplikacja-wpf)
7. [Role użytkowników](#7-role-użytkowników)
8. [Diagramy UML](#8-diagramy-uml)

---

## 1. Opis projektu

**ViRoTePi** to desktopowa aplikacja do zarządzania paczkomatami, wzorowana na systemie InPost.  
Umożliwia nadawanie i odbiór paczek, zarządzanie kurierami oraz nadzór administracyjny nad całym systemem.

**Stos technologiczny:**

| Warstwa        | Technologia                    |
|----------------|-------------------------------|
| Backend (API)  | ASP.NET Core 8, Entity Framework Core |
| Frontend       | WPF (.NET Framework), C#      |
| Baza danych    | MariaDB 10.4                  |
| Mapa           | Leaflet.js (HTML + WebView2)  |
| Logowanie SSO  | Google OAuth2                 |

---

## 2. Architektura systemu

```
┌────────────────────────────┐
│   Frontend (WPF Desktop)   │
│  LoginPage / MainWindow /  │
│  AdminPanel / CourierPanel │
└────────────┬───────────────┘
             │  HTTP (localhost:5000)
             ▼
┌────────────────────────────┐
│   Backend (ASP.NET Core)   │
│  Controllers → EF Core     │
└────────────┬───────────────┘
             │
             ▼
┌────────────────────────────┐
│   Baza danych (MariaDB)    │
│   Schemat: inpost          │
└────────────────────────────┘
```

Komunikacja między frontendem a backendem odbywa się przez REST API (`ApiService.cs`).  
Mapa paczkomatów jest renderowana w oknie `MapPickerWindow` / `CourierPanel` za pomocą pliku `map.html` wczytanego w kontrolce WebView2.

---

## 3. Wymagania i uruchomienie

### 3.1 Wymagania

- **.NET 8 SDK** – backend  
- **.NET Framework 4.7.2+** lub .NET 6+ – frontend WPF  
- **MariaDB 10.4** (lub MySQL 8)  
- **phpMyAdmin** (opcjonalnie, do importu SQL)  
- **Visual Studio 2022** (zalecane)

### 3.2 Konfiguracja bazy danych

1. Uruchom serwer MariaDB (np. przez XAMPP).
2. Zaimportuj plik `inpost.sql` do MariaDB:
   ```sql
   CREATE DATABASE inpost CHARACTER SET utf8mb4 COLLATE utf8mb4_polish_ci;
   USE inpost;
   SOURCE /ścieżka/do/inpost.sql;
   ```
3. Sprawdź konfigurację połączenia w `paczkomat/appsettings.json`:
   ```json
   "ConnectionStrings": {
     "DefaultConnection": "Server=localhost;Database=inpost;User=root;Password=;"
   }
   ```
   Zmień `User` i `Password` jeśli serwer wymaga autoryzacji.

### 3.3 Uruchomienie backendu

```bash
cd paczkomat
dotnet run
```

API domyślnie uruchamia się na `http://localhost:5000`.

### 3.4 Uruchomienie frontendu

Otwórz `frontend/frontend.sln` w Visual Studio i uruchom projekt (`F5`).  
Aplikacja łączy się z API pod adresem skonfigurowanym w `ApiService.cs`.

---

## 4. Baza danych

### 4.1 Schemat tabel

#### `users` – wszyscy użytkownicy systemu

| Kolumna         | Typ           | Opis                          |
|-----------------|---------------|-------------------------------|
| `id`            | INT PK AI     | Identyfikator użytkownika     |
| `name`          | TEXT          | Imię                          |
| `surname`       | TEXT          | Nazwisko                      |
| `email`         | TEXT          | Adres e-mail (unikalny)       |
| `date_of_birth` | DATE          | Data urodzenia                |
| `password`      | TEXT          | Hasło (plain-text)            |
| `role`          | TEXT          | Rola: `klient`, `kurier`, `admin` |
| `phone_number`  | INT           | Numer telefonu (unikalny)     |
| `selfie`        | TEXT          | Nazwa pliku zdjęcia profilowego |
| `sex`           | TINYINT(1)    | Płeć (0=kobieta, 1=mężczyzna) |

#### `klients` – klienci (rozszerzenie `users`)

| Kolumna     | Typ       | Opis                    |
|-------------|-----------|-------------------------|
| `klient_id` | INT PK AI | Identyfikator klienta   |
| `user_id`   | INT FK    | → `users.id`            |
| `pack_id`   | INT       | (legacy, domyślnie 0)   |

#### `kuriers` – kurierzy (rozszerzenie `users`)

| Kolumna     | Typ       | Opis                              |
|-------------|-----------|-----------------------------------|
| `kurier_id` | INT PK AI | Identyfikator kuriera             |
| `user_id`   | INT FK    | → `users.id`                      |
| `state`     | TEXT      | Status: `available` / `unavailable` |

#### `admins` – administratorzy (rozszerzenie `users`)

| Kolumna    | Typ       | Opis                  |
|------------|-----------|-----------------------|
| `admin_id` | INT PK AI | Identyfikator admina  |
| `user_id`  | INT FK    | → `users.id`          |

#### `packs` – paczki

| Kolumna     | Typ            | Opis                              |
|-------------|----------------|-----------------------------------|
| `pack_id`   | INT PK AI      | Identyfikator paczki              |
| `size`      | TEXT           | Rozmiar: `S`, `M`, `L`           |
| `klient_id` | INT FK         | → `klients.klient_id`            |
| `delivered` | TINYINT(1)     | Czy dostarczono do paczkomatu     |
| `date`      | DATE           | Data nadania                      |
| `to_when`   | DATE           | Termin dostawy                    |
| `picked_up` | TINYINT(1)     | Czy odebrano przez klienta        |

#### `pending_packs` – paczki oczekujące na wydanie przez admina

| Kolumna               | Typ         | Opis                                  |
|-----------------------|-------------|---------------------------------------|
| `pending_id`          | INT PK AI   | Identyfikator                         |
| `sender_klient_id`    | INT FK      | → `klients.klient_id` (nadawca)       |
| `receiver_klient_id`  | INT FK      | → `klients.klient_id` (odbiorca)      |
| `size`                | TEXT        | Rozmiar: `S`, `M`, `L`               |
| `paczkomat_id`        | INT FK      | → `paczkomat.paczkomat_id`            |
| `created_at`          | DATETIME    | Data nadania                          |
| `status`              | VARCHAR(20) | Status: `waiting`, `released`, `rejected` |

#### `paczkomat` – paczkomaty

| Kolumna          | Typ       | Opis                       |
|------------------|-----------|----------------------------|
| `paczkomat_id`   | INT PK AI | Identyfikator paczkomatu   |
| `paczkomat_name` | TEXT      | Nazwa (np. `GDA001`)       |
| `address`        | TEXT      | Adres fizyczny             |

#### `boxs` – skrytki w paczkomatach

| Kolumna   | Typ       | Opis                     |
|-----------|-----------|--------------------------|
| `box_id`  | INT PK AI | Identyfikator skrytki    |
| `pack_id` | INT FK    | → `packs.pack_id`        |
| `size`    | TEXT      | Rozmiar skrytki          |

#### `kuriers_data` – przypisania kurierów do paczek

| Kolumna     | Typ    | Opis                    |
|-------------|--------|-------------------------|
| `kurier_id` | INT FK | → `kuriers.kurier_id`   |
| `pack_id`   | INT FK | → `packs.pack_id`       |

#### `paczkomat_data` – przypisania skrytek do paczkomatów

| Kolumna        | Typ    | Opis                         |
|----------------|--------|------------------------------|
| `paczkomat_id` | INT FK | → `paczkomat.paczkomat_id`  |
| `box_id`       | INT FK | → `boxs.box_id`              |

### 4.2 Relacje (klucze obce z CASCADE)

```
users ──< admins        (user_id)
users ──< klients       (user_id)
users ──< kuriers       (user_id)

klients ──< packs             (klient_id)
klients ──< pending_packs     (sender_klient_id)
klients ──< pending_packs     (receiver_klient_id)

packs ──< boxs               (pack_id)
packs ──< kuriers_data        (pack_id)
boxs  ──< paczkomat_data      (box_id)
paczkomat ──< paczkomat_data  (paczkomat_id)
paczkomat ──< pending_packs   (paczkomat_id)
kuriers ──< kuriers_data      (kurier_id)
```

---

## 5. Backend – API REST

Bazowy URL: `http://localhost:5000/api`

### 5.1 AuthController – `/api/auth`

#### `POST /api/auth/login`
Logowanie za pomocą e-maila i hasła.

**Ciało żądania:**
```json
{
  "Email": "jan1@mail.pl",
  "Password": "pass123"
}
```

**Odpowiedź (200 OK):**
```json
{
  "isAuthenticated": true,
  "role": "klient",
  "userId": 6,
  "klientId": 2,
  "kurierId": null,
  "selfie": "selfie5.jpg"
}
```

**Odpowiedź (401 Unauthorized):** `{ "isAuthenticated": false }`

---

#### `POST /api/auth/login-google`
Logowanie przez Google OAuth2 (weryfikacja e-maila).

**Ciało żądania:**
```json
{ "Email": "user@gmail.com" }
```

**Odpowiedź:** jak w `/login`.

---

#### `POST /api/auth/register`
Rejestracja nowego konta klienta.

**Ciało żądania:**
```json
{
  "Name": "Anna",
  "Surname": "Kowalska",
  "Email": "anna@mail.pl",
  "Password": "tajnehaslo",
  "PhoneNumber": "600100200"
}
```

**Odpowiedź (200 OK):**
```json
{ "message": "Konto zostało utworzone pomyślnie.", "userId": 13, "klientId": 7 }
```

---

#### `GET /api/auth/check-email?email=...`
Sprawdza, czy e-mail jest już zajęty.  
**Odpowiedź:** `{ "exists": true/false }`

#### `GET /api/auth/check-phone?phone=...`
Sprawdza, czy numer telefonu jest już zajęty.  
**Odpowiedź:** `{ "exists": true/false }`

---

### 5.2 PacksController – `/api/packs`

#### `GET /api/packs`
Lista paczek z opcjonalnym filtrowaniem.

**Parametry query:**
| Parametr    | Typ    | Opis                              |
|-------------|--------|-----------------------------------|
| `size`      | string | Filtr rozmiaru: `S`, `M`, `L`    |
| `delivered` | bool   | Filtr statusu dostawy             |
| `klientId`  | int    | ID klienta                        |
| `userId`    | int    | ID użytkownika (alternatywa)      |
| `sort`      | string | `date_asc`, `date_desc`, `size_asc`, `size_desc` |

**Odpowiedź (200 OK):** tablica obiektów `PackDto`:
```json
[
  {
    "packId": 11,
    "size": "S",
    "delivered": "W drodze",
    "date": "2026-04-09",
    "klientName": "Gdańsk ul. Długa 1",
    "pickedUp": false,
    "paczkomatName": ""
  }
]
```

> Pole `klientName` przechowuje adres paczkomatu (nazwa historyczna pola).

---

#### `GET /api/packs/{id}`
Szczegóły pojedynczej paczki.

---

#### `GET /api/packs/kurier/{kurierId}`
Paczki przypisane do kuriera.  
Opcjonalny parametr query: `?delivered=false`

---

#### `GET /api/packs/history?klientId=...`
Historia odebranych paczek klienta (`picked_up = true`), posortowana malejąco.

---

#### `GET /api/packs/delivered`
Wszystkie dostarczone paczki (`delivered = true`).

---

#### `GET /api/packs/clients-list`
Lista wszystkich klientów (id, imię, nazwisko, e-mail, telefon, selfie).

---

#### `POST /api/packs/send`
Nadanie paczki klient→klient.

**Ciało żądania:**
```json
{
  "Size": "M",
  "ReceiverPhone": "555555555",
  "ReceiverEmail": "nadawca@mail.pl",
  "PaczkomatName": "GDA002"
}
```

Tworzy rekord `pending_pack` ze statusem `waiting`. Administrator musi ją zatwierdzić.

**Odpowiedź (200 OK):**
```json
{
  "message": "Paczka została przyjęta i oczekuje na wydanie przez admina.",
  "pendingId": 3,
  "receiverName": "Tomasz Kamiński",
  "locker": "GDA002"
}
```

---

#### `PUT /api/packs/{id}/deliver`
Oznacza paczkę jako dostarczoną do paczkomatu (`delivered = true`).  
Wywołuje kurier po fizycznym wrzuceniu paczki.

---

#### `PUT /api/packs/{id}/pickup?klientId=...`
Klient odbiera paczkę (`picked_up = true`).  
Walidacja: paczka musi być dostarczona i należeć do klienta.

---

#### `POST /api/packs`
Tworzy nową paczkę (zapis bezpośredni, admin/kurier).

#### `PUT /api/packs/{id}`
Aktualizacja paczki.

#### `DELETE /api/packs/{id}`
Usuwa paczkę.

---

### 5.3 AdminController – `/api/admin`

#### `GET /api/admin/packs`
Wszystkie paczki nieodebrane z informacją o przypisaniu kuriera.

#### `GET /api/admin/packs/unassigned`
Paczki bez przypisanego kuriera.

#### `GET /api/admin/kuriers`
Lista kurierów z liczbą aktywnych paczek.

**Odpowiedź:**
```json
[
  {
    "kurierId": 1,
    "name": "Piotr Wiśniewski",
    "email": "piotr3@mail.pl",
    "state": "available",
    "assignedPacksCount": 4
  }
]
```

#### `GET /api/admin/stats`
Statystyki systemu.

**Odpowiedź:**
```json
{
  "totalPacks": 18,
  "undeliveredPacks": 10,
  "deliveredPacks": 8,
  "unassignedPacks": 3,
  "totalKuriers": 3,
  "activeKuriers": 2,
  "pendingPacks": 0
}
```

#### `GET /api/admin/pending-packs`
Lista paczek oczekujących na wydanie (`status = waiting`).

#### `POST /api/admin/assign-pack`
Przypisuje kuriera do paczki.

**Ciało żądania:**
```json
{ "PackId": 5, "KurierId": 2 }
```

#### `DELETE /api/admin/unassign-pack/{packId}`
Usuwa przypisanie kuriera od paczki.

#### `DELETE /api/admin/delete-pack/{packId}`
Usuwa paczkę wraz z powiązanymi rekordami (box, paczkomat_data, kurier).

#### `POST /api/admin/release-pack/{pendingId}`
Zatwierdza oczekującą paczkę:
- Tworzy rekord `pack`
- Tworzy rekord `box`
- Przypisuje skrytkę do paczkomatu
- Zmienia status `pending_pack` na `released`

#### `DELETE /api/admin/reject-pack/{pendingId}`
Odrzuca oczekującą paczkę (status → `rejected`).

#### `PUT /api/admin/kurier/{kurierId}/toggle-state`
Przełącza dostępność kuriera między `available` a `unavailable`.

---

### 5.4 UsersController – `/api/users`

#### `GET /api/users`
Lista wszystkich użytkowników (ID, imię, nazwisko, e-mail, rola, telefon).

#### `GET /api/users/{id}`
Dane konkretnego użytkownika.

#### `POST /api/users`
Tworzenie użytkownika przez administratora (rola wymagana).  
Automatycznie tworzy powiązany rekord `klient`, `kurier` lub `admin`.

**Ciało żądania:**
```json
{
  "Name": "Nowy",
  "Surname": "Użytkownik",
  "Email": "nowy@mail.pl",
  "Password": "haslo",
  "Role": "kurier",
  "PhoneNumber": "612345678"
}
```

#### `PUT /api/users/{id}`
Edycja użytkownika. Przy zmianie roli automatycznie usuwa stary rekord roli i tworzy nowy.  
Wszystkie pola opcjonalne — przekazane wartości są aktualizowane, brak pola = bez zmian.

#### `DELETE /api/users/{id}`
Usuwa użytkownika (powiązane rekordy usuwane przez CASCADE).

---

### 5.5 ClientsController – `/api/clients`

Operacje CRUD na encji `klient` (używane wewnętrznie).

| Metoda | Endpoint             | Opis                   |
|--------|----------------------|------------------------|
| GET    | `/api/clients`       | Lista klientów         |
| GET    | `/api/clients/{id}`  | Jeden klient           |
| POST   | `/api/clients`       | Dodaj klienta          |
| PUT    | `/api/clients/{id}`  | Zaktualizuj klienta    |
| DELETE | `/api/clients/{id}`  | Usuń klienta           |

---

## 6. Frontend – aplikacja WPF

### 6.1 Okna aplikacji

#### `LoginPage` – logowanie
- Logowanie e-mail + hasło (wywołuje `POST /api/auth/login`)
- Logowanie przez Google OAuth2 (wywołuje `POST /api/auth/login-google`)
- Przekierowanie do odpowiedniego panelu zależnie od roli
- Link do rejestracji

#### `RegisterWindow` – rejestracja
- Formularz rejestracji nowego klienta
- Walidacja unikalności e-maila i telefonu w czasie rzeczywistym
- Obsługa rejestracji przez Google

#### `MainWindow` – panel klienta
Dostępne sekcje (wybierane przez przyciski w bocznym menu):

| Sekcja              | Opis                                           |
|---------------------|------------------------------------------------|
| **Moje paczki**     | Lista paczek klienta ze statusem i lokalizacją |
| **Nadaj paczkę**    | Formularz nadania paczki do klienta             |
| **Historia**        | Lista odebranych paczek                         |
| **Wyślij do klienta** | Wysyłanie paczki do innego klienta (po numerze telefonu) |
| **Mapa**            | Przeglądanie mapy paczkomatów (Leaflet)         |
| **Odbierz paczkę**  | Odbiór doręczonej paczki                        |
| **Pomoc**           | Okno z filmami instruktażowymi                  |
| **Edycja profilu**  | Zmiana danych osobowych i hasła                 |

#### `AdminPanel` – panel administratora
Zakładki:

| Zakładka                  | Opis                                                |
|---------------------------|-----------------------------------------------------|
| **Statystyki**            | Liczby: paczki, kurierzy, oczekujące                |
| **Paczki**                | Przeglądanie wszystkich paczek, usuwanie            |
| **Oczekujące paczki**     | Zatwierdzanie i odrzucanie `pending_pack`           |
| **Przypisz kuriera**      | Przypisanie / odpisanie kuriera do paczki           |
| **Dostarczone paczki**    | Historia dostarczonych paczek                       |
| **Zarządzanie użytkownikami** | Lista, dodawanie, edycja, usuwanie użytkowników |

#### `CourierPanel` – panel kuriera
- Lista przypisanych paczek do dostarczenia
- Mapa z markerami paczek (Leaflet, `map.html`)
- Przycisk dostarczenia paczki (zmiana `delivered = true`)
- Przełącznik dostępności (`available` / `unavailable`)

#### `HelpWindow` – pomoc
- Odtwarzacz wideo z filmami instruktażowymi
- Filmy skanowane automatycznie z folderu `Videos/`
- Przyciski: poprzedni / następny, play, pause, stop

#### `MapPickerWindow` – wybór paczkomatu
- Mapa Leaflet w WebView2
- Kliknięcie na marker → wybór paczkomatu
- Wybrana nazwa paczkomatu zwracana do wywołującego okna

#### `ProfileEditWindow` – edycja profilu
- Zmiana imienia, nazwiska, e-maila, hasła, telefonu
- Wywołuje `PUT /api/users/{id}`

#### `UserEditDialog` – edycja użytkownika (admin)
- Formularz edycji danych użytkownika przez administratora
- Możliwość zmiany roli

### 6.2 `ApiService.cs`

Centralna klasa do komunikacji z API REST. Konfiguracja bazowego URL w konstruktorze.  
Udostępnia metody asynchroniczne dla każdej operacji API.

---

## 7. Role użytkowników

| Rola    | Opis                                                                 |
|---------|----------------------------------------------------------------------|
| `klient`  | Nadaje i odbiera paczki, przegląda historię, edytuje profil        |
| `kurier`  | Dostarcza paczki do paczkomatów, zarządza swoją dostępnością       |
| `admin`   | Zarządza całym systemem: paczki, kurierzy, użytkownicy, statystyki |

### Możliwości niezalogowanego gościa

- Logowanie i rejestracja
- Nadanie paczki (tworzenie `pending_pack`)
- Przeglądanie mapy paczkomatów

---

## 8. Diagramy UML

Diagramy UML projektu dostępne są w katalogu `docs/`:

- **`docs/diagrams.html`** – interaktywna strona HTML z diagramami (otwórz w przeglądarce)
- **`docs/diagrams_class.mermaid`** – diagram klas (encje DB + okna WPF)
- **`docs/diagrams_usecase.mermaid`** – diagram przypadków użycia

### Diagram klas – skrót

```
user ──< klient ──< pack ──< box ──< paczkomat_data >── paczkomat
user ──< kurier ──< kuriers_data >── pack
user ──< admin
klient ──< pending_pack >── paczkomat
```

### Przypadki użycia – aktorzy

| Aktor         | Główne przypadki użycia                                     |
|---------------|-------------------------------------------------------------|
| Gość          | Logowanie, rejestracja, nadanie paczki, mapa               |
| Klient        | Wszystko Gościa + odbiór, historia, edycja profilu, pomoc  |
| Kurier        | Logowanie, przeglądanie paczek, dostarczenie, zmiana stanu |
| Administrator | Logowanie, statystyki, zarządzanie paczkami i użytkownikami|
