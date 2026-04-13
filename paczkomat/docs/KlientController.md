# KlientController (ClientsController)

> Uwaga: klasa kontrolera nazywa się `ClientsController`, dlatego bazowa ścieżka to `api/clients`.

**Bazowa ścieżka:** `api/clients`

## Endpointy

- `GET /api/clients` — lista klientów (z relacjami: paczki, użytkownik).
- `GET /api/clients/{id}` — pojedynczy klient.
- `POST /api/clients` — utworzenie klienta.
- `PUT /api/clients/{id}` — aktualizacja klienta.
- `DELETE /api/clients/{id}` — usunięcie klienta.

## Typowe odpowiedzi

- `200 OK` / `201 Created` / `204 No Content` — sukces.
- `400 Bad Request` — niezgodność danych wejściowych (np. ID).
- `404 Not Found` — klient nie istnieje.
