# Guía: Estado de Rifa y Mensualidad

## ✅ Implementación Realizada

Se han agregado dos columnas en la vista `EstadoCuenta.cshtml`:

1. **Pago Rifa**: Muestra "Solvente" o "Insolvente" según si tiene pagados ambos semestres del año actual
2. **Pago Mensualidad**: Muestra "Solvente" o "Insolvente" según si tiene pagado el mes anterior al actual

## 🔍 Cómo Funciona

### Columna "Pago Rifa"

**Lógica:**
- Verifica si el alumno tiene pagos de rifa para **ambos semestres** del año actual
- **Semestre 1**: Enero-Junio (meses 1-6)
- **Semestre 2**: Julio-Diciembre (meses 7-12)
- **Solvente**: Si tiene pagos de rifa en ambos semestres del año actual
- **Insolvente**: Si falta el pago de cualquier semestre

**Código:**
```csharp
// Busca pagos de rifa del semestre 1 (meses 1-6)
tieneRifaSemestre1 = Model.pagos.Any(p => 
    p.IdAlumno == alumno.IdAlumno && 
    p.IdTipoMovimiento == tipoRifa.IdTipoMovimiento &&
    p.IdMes >= 1 && p.IdMes <= 6 &&
    p.Anyo == añoActual &&
    p.Activo == true);

// Busca pagos de rifa del semestre 2 (meses 7-12)
tieneRifaSemestre2 = Model.pagos.Any(p => 
    p.IdAlumno == alumno.IdAlumno && 
    p.IdTipoMovimiento == tipoRifa.IdTipoMovimiento &&
    p.IdMes >= 7 && p.IdMes <= 12 &&
    p.Anyo == añoActual &&
    p.Activo == true);
```

### Columna "Pago Mensualidad"

**Lógica:**
- Si estamos en **diciembre (mes 12)**: Debe tener pagado **noviembre (mes 11)**
- Si estamos en cualquier otro mes: Debe tener pagado el **mes anterior**
- **Solvente**: Si tiene el pago del mes requerido
- **Insolvente**: Si no tiene el pago del mes requerido

**Ejemplos:**
- Si estamos en **diciembre**: Debe tener pagado **noviembre** → Si no tiene → "Insolvente"
- Si estamos en **enero**: Debe tener pagado **diciembre del año anterior**
- Si estamos en **marzo**: Debe tener pagado **febrero**

**Código:**
```csharp
int mesActual = DateTime.Now.Month;
int mesRequerido = mesActual == 12 ? 11 : (mesActual - 1);

tieneMensualidadAlDia = Model.pagos.Any(p => 
    p.IdAlumno == alumno.IdAlumno && 
    p.IdTipoMovimiento == tipoMensualidad.IdTipoMovimiento &&
    p.IdMes == mesRequerido &&
    p.Anyo == añoActualMensualidad &&
    p.Activo == true);
```

## ⚙️ Ajustes Necesarios

### 1. Identificar el IdTipoMovimiento para "Rifa"

El código actual busca tipos de movimiento que contengan "rifa" en el concepto:

```csharp
var tipoRifa = Model.tipoMovimiento.FirstOrDefault(tm => 
    tm.Concepto.ToLower().Contains("rifa"));
```

**Si tu base de datos tiene otro nombre**, ajusta el filtro:

```csharp
// Ejemplo: Si se llama "Rifa Anual"
var tipoRifa = Model.tipoMovimiento.FirstOrDefault(tm => 
    tm.Concepto.ToLower().Contains("rifa"));

// O si conoces el ID exacto:
var tipoRifa = Model.tipoMovimiento.FirstOrDefault(tm => 
    tm.IdTipoMovimiento == 3); // Ajusta el ID según tu BD
```

### 2. Identificar el IdTipoMovimiento para "Mensualidad"

El código actual busca tipos de movimiento que contengan "mensualidad" o "mensual":

```csharp
var tipoMensualidad = Model.tipoMovimiento.FirstOrDefault(tm => 
    tm.Concepto.ToLower().Contains("mensualidad") || 
    tm.Concepto.ToLower().Contains("mensual"));
```

**Si tu base de datos tiene otro nombre**, ajusta el filtro:

```csharp
// Ejemplo: Si se llama "Colegiatura Mensual"
var tipoMensualidad = Model.tipoMovimiento.FirstOrDefault(tm => 
    tm.Concepto.ToLower().Contains("colegiatura") || 
    tm.Concepto.ToLower().Contains("mensualidad"));

// O si conoces el ID exacto:
var tipoMensualidad = Model.tipoMovimiento.FirstOrDefault(tm => 
    tm.IdTipoMovimiento == 1); // Ajusta el ID según tu BD
```

## 🔍 Cómo Verificar los Tipos de Movimiento

Para saber qué `IdTipoMovimiento` corresponde a cada concepto, puedes:

1. **Consultar tu base de datos:**
   ```sql
   SELECT IdTipoMovimiento, Concepto 
   FROM TblCatTipoMovimiento 
   WHERE Activo = 1
   ```

2. **Agregar un debug temporal en la vista:**
   ```csharp
   @foreach(var tipo in Model.tipoMovimiento)
   {
       <p>ID: @tipo.IdTipoMovimiento - Concepto: @tipo.Concepto</p>
   }
   ```

## 📋 Consideraciones Adicionales

### Para Rifas

Si las rifas se almacenan de manera diferente (por ejemplo, un solo registro por semestre en lugar de por mes), puedes ajustar la lógica:

```csharp
// Si las rifas se almacenan con un campo específico para semestre
tieneRifaSemestre1 = Model.pagos.Any(p => 
    p.IdAlumno == alumno.IdAlumno && 
    p.IdTipoMovimiento == tipoRifa.IdTipoMovimiento &&
    p.Semestre == 1 && // Si tienes un campo Semestre
    p.Anyo == añoActual &&
    p.Activo == true);
```

### Para Mensualidad

Si necesitas considerar el año anterior cuando estamos en enero:

```csharp
int añoRequerido = añoActualMensualidad;
if (mesActual == 1)
{
    // Si estamos en enero, debe tener pagado diciembre del año anterior
    mesRequerido = 12;
    añoRequerido = añoActualMensualidad - 1;
}
```

## 🎨 Estilos Visuales

Los estados se muestran con badges de Bootstrap:
- **Solvente**: Badge verde (`bg-success`)
- **Insolvente**: Badge rojo (`bg-danger`)

Puedes cambiar los colores ajustando las clases:
```csharp
string claseRifa = (tieneRifaSemestre1 && tieneRifaSemestre2) ? "success" : "danger";
// Cambiar "success" por "info", "warning", etc.
```

## 🧪 Pruebas

1. **Probar con un alumno que tenga rifas pagadas:**
   - Debe mostrar "Solvente" en la columna de rifa

2. **Probar con un alumno que deba rifa:**
   - Debe mostrar "Insolvente" en la columna de rifa

3. **Probar en diciembre con un alumno que deba noviembre:**
   - Debe mostrar "Insolvente" en la columna de mensualidad

4. **Probar con un alumno al día en mensualidad:**
   - Debe mostrar "Solvente" en la columna de mensualidad



