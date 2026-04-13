# AuthController

**Bazowa ścieżka:** `api/auth`

## Endpointy

- `POST /api/auth/login` — logowanie e‑mail + hasło.
- `POST /api/auth/login-google` — logowanie Google (na podstawie e‑maila).
- `POST /api/auth/register` — rejestracja nowego klienta.
- `GET /api/auth/check-email?email=...` — sprawdzenie, czy e‑mail jest zajęty.
- `GET /api/auth/check-phone?phone=...` — sprawdzenie, czy numer telefonu jest zajęty.

## Główne pola wejściowe

### `POST /login`
- `email`
- `password`

### `POST /login-google`
- `email`

### `POST /register`
- `name`
- `surname`
- `email`
- `password`
- `phoneNumber`

## Typowe odpowiedzi

- `200 OK` — poprawna autoryzacja/rejestracja.
- `400 Bad Request` — brak danych lub nieprawidłowy format.
- `401 Unauthorized` — błędne dane logowania lub brak użytkownika.
