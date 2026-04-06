---

# 📦 API: Packs

### Base URL

```
/api/packs
```

---

## 1️⃣ GET /api/packs

Pobiera listę paczek, z filtrowaniem i sortowaniem.

### Query parameters (opcjonalne)

| Parametr    | Typ    | Opis                                               |
| ----------- | ------ | -------------------------------------------------- |
| `delivered` | bool   | Filtruje paczki po statusie dostarczenia           |
| `klientId`  | int    | Filtruje paczki dla konkretnego klienta            |
| `sortBy`    | string | Pole do sortowania: `date`, `to_when`, `size`      |
| `sortDesc`  | bool   | Sortowanie malejące (`true`) lub rosnące (`false`) |

### Request

```http
GET /api/packs?delivered=false&klientId=3&sortBy=date&sortDesc=true
```

### Response (200 OK)

```json
[
  {
    "pack_id": 1,
    "size": "M",
    "klient_id": 3,
    "delivered": false,
    "date": "2025-11-01",
    "to_when": "2025-11-05",
    "klient": {
      "klient_id": 3,
      "user_id": 7
    },
    "boxes": [
      {
        "box_id": 10,
        "size": "M",
        "pack_id": 1
      }
    ]
  }
]
```

---

## 2️⃣ GET /api/packs/{id}

Pobiera jedną paczkę po `id`.

### Request

```http
GET /api/packs/1
```

### Response (200 OK)

```json
{
  "pack_id": 1,
  "size": "M",
  "klient_id": 3,
  "delivered": false,
  "date": "2025-11-01",
  "to_when": "2025-11-05",
  "klient": {
    "klient_id": 3,
    "user_id": 7
  },
  "boxes": [
    {
      "box_id": 10,
      "size": "M",
      "pack_id": 1
    }
  ]
}
```

### Response (404 Not Found)

```json
{
  "message": "Pack not found"
}
```

---

## 3️⃣ POST /api/packs

Tworzy nową paczkę.

### Request body

```json
{
  "size": "L",
  "klient_id": 5,
  "delivered": false,
  "date": "2026-02-23",
  "to_when": "2026-02-28"
}
```

### Response (201 Created)

```json
{
  "pack_id": 7,
  "size": "L",
  "klient_id": 5,
  "delivered": false,
  "date": "2026-02-23",
  "to_when": "2026-02-28",
  "klient": null,
  "boxes": []
}
```

---

## 4️⃣ PUT /api/packs/{id}

Aktualizuje paczkę o podanym `id`.

### Request body

```json
{
  "pack_id": 7,
  "size": "XL",
  "klient_id": 5,
  "delivered": true,
  "date": "2026-02-23",
  "to_when": "2026-03-01"
}
```

### Response (204 No Content)

* Brak treści w body, status `204` oznacza sukces.

### Response (400 Bad Request)

```json
{
  "message": "ID nie pasuje do paczki."
}
```

### Response (404 Not Found)

```json
{
  "message": "Pack not found"
}
```

---

## 5️⃣ DELETE /api/packs/{id}

Usuwa paczkę o podanym `id`.

### Request

```http
DELETE /api/packs/7
```

### Response (204 No Content)

* Paczka została usunięta, brak treści w body.

### Response (404 Not Found)

```json
{
  "message": "Pack not found"
}
```

---
