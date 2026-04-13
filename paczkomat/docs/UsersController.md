# UsersController

**Bazowa ścieżka:** `api/users`

## Endpointy

- `GET /api/users` — lista użytkowników.
- `GET /api/users/{id}` — szczegóły użytkownika.
- `POST /api/users` — utworzenie użytkownika (z rolą).
- `PUT /api/users/{id}` — aktualizacja użytkownika.
- `DELETE /api/users/{id}` — usunięcie użytkownika.

## Główne pola wejściowe

### `POST /api/users`
- `name`
- `surname`
- `email`
- `password`
- `role` (`klient`, `kurier`, `admin`)
- `phoneNumber` (opcjonalnie)

### `PUT /api/users/{id}`
- `name` (opcjonalnie)
- `surname` (opcjonalnie)
- `email` (opcjonalnie)
- `password` (opcjonalnie)
- `role` (opcjonalnie)
- `phoneNumber` (opcjonalnie)

## Typowe odpowiedzi

- `200 OK` — sukces operacji.
- `400 Bad Request` — walidacja danych nie przeszła.
- `404 Not Found` — użytkownik nie istnieje.
