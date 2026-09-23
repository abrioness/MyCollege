using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using WebColegio.Helpers;

namespace WebColegio.Filters
{
    /// <summary>Quita de TempData los avisos técnicos antes de mostrarlos al usuario.</summary>
    public sealed class SanitizarMensajesUsuarioFilter : IActionFilter
    {
        public void OnActionExecuting(ActionExecutingContext context)
        {
        }

        public void OnActionExecuted(ActionExecutedContext context)
        {
            if (MensajeUsuarioHelper.MostrarDetalleTecnico)
                return;
            if (context.Controller is not Controller controller)
                return;

            SanitizarTempData(controller.TempData, "Mensaje");
            SanitizarTempData(controller.TempData, "Message");
            SanitizarViewData(controller.ViewData, "Mensaje");
            SanitizarViewData(controller.ViewData, "Message");
        }

        private static void SanitizarTempData(Microsoft.AspNetCore.Mvc.ViewFeatures.ITempDataDictionary tempData, string clave)
        {
            if (!tempData.TryGetValue(clave, out var valor) || valor is not string texto)
                return;
            var limpio = MensajeUsuarioHelper.ParaUsuario(texto);
            if (!string.Equals(texto, limpio, StringComparison.Ordinal))
                tempData[clave] = limpio;
        }

        private static void SanitizarViewData(Microsoft.AspNetCore.Mvc.ViewFeatures.ViewDataDictionary viewData, string clave)
        {
            if (viewData[clave] is not string texto)
                return;
            var limpio = MensajeUsuarioHelper.ParaUsuario(texto);
            if (!string.Equals(texto, limpio, StringComparison.Ordinal))
                viewData[clave] = limpio;
        }
    }
}
