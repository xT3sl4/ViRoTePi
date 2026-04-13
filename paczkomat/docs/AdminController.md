# AdminController

**Bazowa ścieżka:** `api/admin`

## Endpointy paczek i kurierów

- `GET /api/admin/packs` — wszystkie niedoręczone paczki (z informacją o przypisaniu kuriera).
- `GET /api/admin/packs/unassigned` — paczki bez przypisanego kuriera.
- `GET /api/admin/kuriers` — lista kurierów i ich obciążenie.
- `POST /api/admin/assign-pack` — przypisanie paczki do kuriera (`packId`, `kurierId`).
- `DELETE /api/admin/unassign-pack/{packId}` — usunięcie przypisania kuriera.
- `DELETE /api/admin/delete-pack/{packId}` — usunięcie paczki wraz z powiązaniami.
- `PUT /api/admin/kurier/{kurierId}/toggle-state` — zmiana statusu kuriera (`available` / `unavailable`).

## Endpointy statystyk i paczek oczekujących

- `GET /api/admin/stats` — statystyki systemowe (paczki, kurierzy, oczekujące).
- `GET /api/admin/pending-packs` — paczki oczekujące na wydanie (`status = waiting`).
- `POST /api/admin/release-pack/{pendingId}` — wydanie paczki oczekującej do systemu głównego.
- `DELETE /api/admin/reject-pack/{pendingId}` — odrzucenie paczki oczekującej.

## Typowe odpowiedzi

- `200 OK` — sukces operacji.
- `400 Bad Request` — niepoprawny stan biznesowy (np. paczka już wydana).
- `404 Not Found` — brak wskazanego zasobu.
