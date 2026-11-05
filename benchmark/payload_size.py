"""Порівняння розміру і швидкості серіалізації: JSON vs Protobuf.

Запуск (з кореня репозиторію):
    pip install protobuf
    protoc -I api --python_out=benchmark api/service.proto
    python benchmark/payload_size.py

Сутність для порівняння - бронювання (Reservation) з 4 місцями
(максимум за NFR-11), сумою, статусом і двома часовими мітками.
"""
import json
import timeit
from datetime import datetime, timezone

from google.protobuf.timestamp_pb2 import Timestamp

import service_pb2 as pb

SEATS = [
    ("3f8a2c1d-6e4b-4a9f-b2d7-1c5e8f0a9b22", 12, 412.5),
    ("7a1b9e3c-2d4f-4b8a-9c6e-0f1d2e3a4b55", 13, 430.0),
    ("c2d9f4a1-8b3e-4f6c-9a7d-5e1b2c3d4e66", 14, 447.5),
    ("e5a7b3c9-1d2f-4e8a-b6c4-9f0a1b2c3d77", 15, 465.0),
]
RESERVED_UNTIL = datetime(2026, 11, 20, 10, 10, tzinfo=timezone.utc)
CREATED_AT = datetime(2026, 11, 20, 10, 0, tzinfo=timezone.utc)


def make_json_payload() -> bytes:
    data = {
        "id": "0e7c2a91-4b6d-4f3e-8a2c-91d5b7e4f003",
        "eventId": "9c4e1a7b-2f3d-4e6a-8b1c-7d2e5f9a3c01",
        "userId": "a1b2c3d4-e5f6-4a7b-8c9d-0e1f2a3b4c5d",
        "status": "ACTIVE",
        "seats": [
            {"seatId": sid, "sectorName": "Фан-зона A", "row": 5, "number": num,
             "priceKopecks": 120000, "x": x, "y": 188.0}
            for sid, num, x in SEATS
        ],
        "insurance": True,
        "totalKopecks": 480000,
        "currency": "UAH",
        "reservedUntil": RESERVED_UNTIL.isoformat().replace("+00:00", "Z"),
        "createdAt": CREATED_AT.isoformat().replace("+00:00", "Z"),
    }
    # compact JSON без пробілів, UTF-8
    return json.dumps(data, ensure_ascii=False, separators=(",", ":")).encode("utf-8")


def make_proto_message() -> pb.Reservation:
    msg = pb.Reservation(
        id="0e7c2a91-4b6d-4f3e-8a2c-91d5b7e4f003",
        event_id="9c4e1a7b-2f3d-4e6a-8b1c-7d2e5f9a3c01",
        user_id="a1b2c3d4-e5f6-4a7b-8c9d-0e1f2a3b4c5d",
        status=pb.RESERVATION_STATUS_ACTIVE,
        insurance=True,
        total_kopecks=480000,
        currency="UAH",
    )
    for sid, num, x in SEATS:
        msg.seats.add(seat_id=sid, sector_name="Фан-зона A", row=5, number=num,
                      price_kopecks=120000, x=x, y=188.0)
    until, created = Timestamp(), Timestamp()
    until.FromDatetime(RESERVED_UNTIL)
    created.FromDatetime(CREATED_AT)
    msg.reserved_until.CopyFrom(until)
    msg.created_at.CopyFrom(created)
    return msg


def main() -> None:
    json_bytes = make_json_payload()
    msg = make_proto_message()
    proto_bytes = msg.SerializeToString()

    j, p = len(json_bytes), len(proto_bytes)
    print(f"JSON (compact, UTF-8): {j} байт")
    print(f"Protobuf:              {p} байт")
    print(f"Коефіцієнт стиснення:  {j / p:.2f}x")
    print(f"Економія трафіку:      {(1 - p / j) * 100:.1f}%")

    n = 100_000
    data_dict = json.loads(json_bytes)
    t_json_ser = timeit.timeit(lambda: json.dumps(data_dict, ensure_ascii=False, separators=(",", ":")).encode(), number=n)
    t_json_de = timeit.timeit(lambda: json.loads(json_bytes), number=n)
    t_pb_ser = timeit.timeit(msg.SerializeToString, number=n)
    t_pb_de = timeit.timeit(lambda: pb.Reservation.FromString(proto_bytes), number=n)
    print(f"\nЧас на {n} операцій (секунди):")
    print(f"JSON серіалізація:       {t_json_ser:.3f}")
    print(f"JSON десеріалізація:     {t_json_de:.3f}")
    print(f"Protobuf серіалізація:   {t_pb_ser:.3f}")
    print(f"Protobuf десеріалізація: {t_pb_de:.3f}")


if __name__ == "__main__":
    main()
