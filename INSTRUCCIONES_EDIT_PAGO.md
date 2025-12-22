# Instrucciones: Implementación del Método Edit para Pagos

## ✅ Cambios Realizados

### 1. **PagosController.cs** - Métodos Edit implementados
   - ✅ Método GET `Edit(int id)`: Carga el pago y prepara el ViewModel
   - ✅ Método POST `Edit(int id, PagosViewModel)`: Actualiza el pago

### 2. **IServicesApi.cs** - Interfaz actualizada
   - ✅ Agregado método `Task<bool> UpdatePago(TblPago pago);`

### 3. **Views/Pagos/Edit.cshtml** - Vista creada
   - ✅ Formulario completo con todos los campos
   - ✅ Validación de campos requeridos
   - ✅ SelectLists precargados con valores seleccionados

## ⚠️ Acción Requerida: Implementar UpdatePago en el Servicio

Debes implementar el método `UpdatePago` en tu clase que implementa `IServicesApi` (probablemente `ServicesApi.cs`).

### Ejemplo de Implementación:

```csharp
public async Task<bool> UpdatePago(TblPago pago)
{
    try
    {
        // Construir la URL del endpoint
        string url = $"{_baseUrl}/api/Pagos/{pago.IdPago}";
        
        // Serializar el objeto
        var json = JsonConvert.SerializeObject(pago);
        var content = new StringContent(json, Encoding.UTF8, "application/json");
        
        // Realizar la petición PUT
        var response = await _httpClient.PutAsync(url, content);
        
        return response.IsSuccessStatusCode;
    }
    catch (Exception ex)
    {
        // Log del error
        Console.WriteLine($"Error al actualizar pago: {ex.Message}");
        return false;
    }
}
```

O si usas Entity Framework directamente:

```csharp
public async Task<bool> UpdatePago(TblPago pago)
{
    try
    {
        _context.TblPagos.Update(pago);
        await _context.SaveChangesAsync();
        return true;
    }
    catch (Exception ex)
    {
        Console.WriteLine($"Error al actualizar pago: {ex.Message}");
        return false;
    }
}
```

## 🔍 Verificación

1. **Busca tu archivo de implementación del servicio:**
   - Probablemente está en `Services/ServicesApi.cs` o similar

2. **Agrega el método UpdatePago siguiendo el mismo patrón que:**
   - `UpdateAlumnos`
   - `UpdateNotas`
   - `UpdateUsuario`

3. **Asegúrate de que el endpoint de la API esté disponible:**
   - Si usas una API externa, verifica que el endpoint PUT esté implementado
   - Si usas Entity Framework, el método Update debería funcionar directamente

## 📋 Campos que se Actualizan

El método Edit actualiza los siguientes campos:
- ✅ IdAlumno
- ✅ Monto
- ✅ NumeroRecibo
- ✅ Anyo
- ✅ IdMes
- ✅ FechaEmision
- ✅ Mora
- ✅ Descripcion
- ✅ IdMetodoPago
- ✅ IdTipoMovimiento
- ✅ IdTipoRecibo
- ✅ IdRecinto
- ✅ IdPeriodo
- ✅ IdGrado
- ✅ IdModalidad
- ✅ Activo
- ✅ UsuarioActualizo (se establece automáticamente)
- ✅ FechaActualizo (se establece automáticamente)

## 🔒 Campos que NO se Actualizan (Preservados)

- ✅ UsuarioRegistro (se mantiene el original)
- ✅ FechaRegistro (se mantiene la original)
- ✅ Serie (se mantiene "A")

## 🧪 Pruebas

1. **Navegar a la vista Edit:**
   - Desde Index: `asp-action="Edit" asp-route-id="@pago.IdPago"`
   - Desde Details: Ya hay un botón "Ver Detalles" que puedes modificar para agregar "Editar"

2. **Probar la actualización:**
   - Modifica algunos campos
   - Guarda los cambios
   - Verifica que se actualicen correctamente

3. **Verificar validaciones:**
   - Intenta guardar sin campos requeridos
   - Verifica que muestre mensajes de error

## 🔗 Agregar Botón de Editar en Otras Vistas

Puedes agregar un botón de editar en `Index.cshtml` o `Details.cshtml`:

```html
<a asp-action="Edit" asp-route-id="@pago.IdPago" class="btn btn-warning btn-sm">
    <i class="bi bi-pencil"></i> Editar
</a>
```

