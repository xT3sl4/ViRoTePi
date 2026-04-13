# PacksController

**Bazowa ścieżka:** `api/packs`

## Endpointy odczytu

- `GET /api/packs` — lista paczek z filtrowaniem:
  - `size`
  - `delivered`
  - `klientId`
  - `userId`
  - `sort` (`date_asc`, `date_desc`, `size_asc`, `size_desc`)
- `GET /api/packs/{id}` — szczegóły paczki.
- `GET /api/packs/kurier/{kurierId}` — paczki przypisane do kuriera (`delivered` opcjonalnie).
- `GET /api/packs/history` — historia odebranych paczek klienta (`klientId` lub `userId`).
- `GET /api/packs/delivered` — lista paczek doręczonych.
- `GET /api/packs/clients-list` — lista klientów (na potrzeby nadawania paczek).

## Endpointy operacyjne

- `PUT /api/packs/{id}/deliver` — oznaczenie paczki jako doręczonej.
- `PUT /api/packs/{id}/pickup` — odbiór paczki przez klienta (`klientId` opcjonalnie jako walidacja właściciela).
- `POST /api/packs` — utworzenie paczki.
- `PUT /api/packs/{id}` — aktualizacja paczki.
- `DELETE /api/packs/{id}` — usunięcie paczki.
- `POST /api/packs/send` — utworzenie paczki oczekującej na akceptację administratora.

## Główne pola wejściowe dla `POST /api/packs/send`

- `size` (`S`, `M`, `L`)
- `receiverPhone`
- `receiverEmail` *(w implementacji używany do identyfikacji nadawcy)*
- `paczkomatName`

## Typowe odpowiedzi

- `200 OK` / `201 Created` / `204 No Content` — sukces.
- `400 Bad Request` — błąd walidacji lub nieprawidłowy stan.
- `404 Not Found` — brak paczki lub innego zasobu.
