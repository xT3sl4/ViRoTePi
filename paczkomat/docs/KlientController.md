
---

# 📄 Klient API

## **GET /api/klients**

Pobierz wszystkich klientów.

**Request:** brak body

**Response 200 OK:**

```json
[
  {
    "klient_id": 1,
    "user_id": 5,
    "pack_id": null,
    "packs": [
      {
        "pack_id": 12,
        "size": "M",
        "delivered": false,
        "date": "2026-02-23",
        "to_when": "2026-02-25"
      }
    ],
    "user": {
      "user_id": 5,
      "name": "Jan Kowalski",
      "email": "jan@example.com"
    }
  }
]
```

---

## **GET /api/klients/{id}**

Pobierz jednego klienta po ID.

**Request:** brak body

**Response 200 OK:**

```json
{
  "klient_id": 1,
  "user_id": 5,
  "pack_id": null,
  "packs": [],
  "user": {
    "user_id": 5,
    "name": "Jan Kowalski",
    "email": "jan@example.com"
  }
}
```

---

## **POST /api/klients**

Dodaj nowego klienta.

**Request Body:**

```json
{
  "user_id": 5
}
```

* `user_id` – wymagane, ID istniejącego użytkownika do powiązania z klientem.

**Response 201 Created:**

```json
{
  "klient_id": 10,
  "user_id": 5,
  "pack_id": null,
  "packs": [],
  "user": {
    "user_id": 5,
    "name": "Jan Kowalski",
    "email": "jan@example.com"
  }
}
```

---

## **PUT /api/klients/{id}**

Aktualizuj klienta.

**Request Body:**

```json
{
  "user_id": 7
}
```

**Response 200 OK:**

```json
{
  "klient_id": 10,
  "user_id": 7,
  "pack_id": null,
  "packs": [],
  "user": {
    "user_id": 7,
    "name": "Anna Nowak",
    "email": "anna@example.com"
  }
}
```

---

## **DELETE /api/klients/{id}**

Usuń klienta po ID.

**Request:** brak body

**Response 204 No Content:** brak body

---

