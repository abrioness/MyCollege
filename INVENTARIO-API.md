# API de Inventario (movimientos) – Requisitos para el backend

Para que en la app se reflejen las **entradas y salidas** en el reporte (Productos → Entradas y salidas), el backend debe exponer los siguientes endpoints.

## 1. POST api/Inventario

**Objetivo:** Guardar un movimiento (entrada o salida).

**Body (JSON):**
```json
{
  "idProducto": 1,
  "tipoMovimiento": 1,
  "cantidad": 10,
  "fechaMovimiento": "2025-02-08T12:00:00",
  "referenciaDocumento": "RECIBO-20005",
  "descripcion": "Venta recibo caja",
  "activo": true,
  "usuarioRegistro": 1,
  "fechaRegistro": "2025-02-08T12:00:00"
}
```

- `tipoMovimiento`: **1** = Entrada, **2** = Salida  
- `referenciaDocumento`: ej. `"RECIBO-20005"`, `"INGRESO-MANUAL"`, `"ALTA-PRODUCTO"`

**Respuesta:** 201 Created (o 200 OK).

---

## 2. GET api/Inventario

**Objetivo:** Listar movimientos para el reporte (con filtros opcionales).

**Query params (opcionales):**
- `idProducto` – filtrar por producto
- `desde` – fecha desde (yyyy-MM-dd)
- `hasta` – fecha hasta (yyyy-MM-dd)

**Ejemplo:** `GET api/Inventario?idProducto=5&desde=2025-02-01&hasta=2025-02-08`

**Respuesta (200 OK):** array JSON de movimientos, por ejemplo:
```json
[
  {
    "idInventario": 1,
    "idProducto": 5,
    "tipoMovimiento": 2,
    "cantidad": 2,
    "fechaMovimiento": "2025-02-08T10:30:00",
    "referenciaDocumento": "RECIBO-20005",
    "descripcion": "Venta recibo caja"
  }
]
```

---

## Cuándo llama la app a estos endpoints

| Acción en la app | Llamada |
|------------------|--------|
| Crear **producto nuevo** con stock inicial | POST Inventario (Entrada, referencia "ALTA-PRODUCTO") |
| **Ingreso** a producto existente (mismo código) | POST Inventario (Entrada, referencia "INGRESO-MANUAL") |
| **Venta** en PagoCaja (productos en el recibo) | POST Inventario por cada ítem (Salida, referencia "RECIBO-{id}") |
| Reporte **Entradas y salidas** | GET Inventario (con filtros opcionales) |

Si estos endpoints no existen o devuelven error, el producto y el stock se actualizan igual (StockActual), pero el reporte de movimientos quedará vacío hasta que el backend los implemente.
