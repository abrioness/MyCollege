using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using WebColegio.Models.ViewModel;
using WebColegio.Services;

namespace WebColegio.Controllers
{
    public class ProductosController : Controller
    {
        private readonly IServicesApi _Iservices;
        public ProductosController (IServicesApi servicesApi)
        {
            _Iservices = servicesApi;
        }

        // GET: ProductosController
        public async Task<ActionResult> Index()
        {
            var _productos = await _Iservices.GetProductosAsync();
            var _movInvebtario = await _Iservices.GetMovInventarioAsync();
            var _categoriaProducto = await _Iservices.GetCategoriaProductoAsync();

            var viewModel = new ColeccionCatalogos
            {
                producto = _productos,
                categoriasProducto=_categoriaProducto,
                movinventario = _movInvebtario
            };


            return View(viewModel);
        }

        // GET: ProductosController/Details/5
        public ActionResult Details(int id)
        {
            return View();
        }

        // GET: ProductosController/Create
        public async Task<ActionResult> Create()
        {
            var productos = await _Iservices.GetProductosAsync();

            int? maxNumero = productos
             .Where(r => !string.IsNullOrEmpty(r.CodigoBarra))
             .Select(r =>
             {
                 // Extraer los últimos 3 dígitos (por ejemplo "CORLAP-001" → 1)
                 var partes = r.CodigoBarra.Split('-');
                 if (partes.Length > 1 && int.TryParse(partes.Last(), out int num))
                     return num;
                 return 0;
             })
             .DefaultIfEmpty(0)
             .Max();

            var siguienteCodigo = (maxNumero.HasValue ? maxNumero.Value + 1 : 1).ToString("D5");
            var viewmodel = new productoViewModel
            {
                SiguienteCodigo=siguienteCodigo,

                listCategoriaProducto = (await _Iservices.GetCategoriaProductoAsync())
                                   .Select(r => new SelectListItem
                                   {
                                       Value = r.IdCateProducto.ToString(),
                                       Text = r.NombreCategoria,
                                       //Selected = r.IdPregunta == respuestas.IdPregunta
                                   }).ToList(),
                listMovimientoInventario = (await _Iservices.GetMovInventarioAsync())
                                   .Select(r => new SelectListItem
                                   {
                                       Value = r.IdMovInventario.ToString(),
                                       Text = r.MovimientoInventario,
                                       //Selected = r.IdPregunta == respuestas.IdPregunta
                                   }).ToList()
            };
            return View(viewmodel);
        }

        // POST: ProductosController/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Create(productoViewModel producto)
        {
            bool response = false;
            bool ExisteProducto = false;
            try
            {
                ExisteProducto = await _Iservices.ValidarProductos(producto.tblproducto.CodigoBarra,producto.tblproducto.IdCateProducto);
                if (ExisteProducto)
                {
                    TempData["Mensaje"] = "El Producto ya Posee un Registro.";
                    TempData["Tipo"] = "warning";
                    return RedirectToAction("Create", "Productos");
                }

                if (producto != null)
                {
                    //producto.tblproducto.CodigoBarra = await GenerarCodigoAutomaticoAsync(producto.tblproducto.NombreProducto);
                    response = await _Iservices.PostProductosAsync(producto.tblproducto);
                    if (response)
                    {
                        TempData["Mensaje"] = "Se Guardo Correctamente el Producto.";
                        TempData["Tipo"] = "info";

                        return  RedirectToAction("Create","Productos");
                    }

                }
                return NoContent();
            }
            catch
            {
                return View();
            }
        }
        //public class CodigoProductoHelper
        //{
        //    public static string GenerarCodigo(string nombreProducto, int correlativo)
        //    {
        //        if (string.IsNullOrWhiteSpace(nombreProducto))
        //            return "";

        //        // Quitar acentos y pasar a mayúsculas
        //        nombreProducto = QuitarAcentos(nombreProducto).ToUpper();

        //        // Dividir en palabras
        //        var palabras = nombreProducto.Split(' ', StringSplitOptions.RemoveEmptyEntries);

        //        // Tomar las tres primeras letras de las dos primeras palabras
        //        string parte1 = palabras.Length > 0 ? new string(palabras[0].Take(3).ToArray()) : "";
        //        string parte2 = palabras.Length > 1 ? new string(palabras[1].Take(3).ToArray()) : "";

        //        // Combinar ambas partes
        //        string baseCodigo = $"{parte1}{parte2}";

        //        // Agregar correlativo con 3 dígitos (001, 002, 003...)
        //        string codigoFinal = $"{baseCodigo}-{correlativo:D3}";

        //        return codigoFinal;
        //    }

        //    private static string QuitarAcentos(string texto)
        //    {
        //        var normalized = texto.Normalize(System.Text.NormalizationForm.FormD);
        //        var sb = new StringBuilder();
        //        foreach (var c in normalized)
        //        {
        //            if (System.Globalization.CharUnicodeInfo.GetUnicodeCategory(c)
        //                != System.Globalization.UnicodeCategory.NonSpacingMark)
        //            {
        //                sb.Append(c);
        //            }
        //        }
        //        return sb.ToString().Normalize(System.Text.NormalizationForm.FormC);
        //    }
        //}
        //public async Task<string> GenerarCodigoAutomaticoAsync(string nombreProducto)
        //{
        //    // Obtener el último número usado
        //    var lisProducto = await _Iservices.GetProductosAsync();
                
        //        lisProducto.OrderByDescending(p => p.IdProducto)
        //        .Select(p => p.CodigoBarra).FirstOrDefault();
        //    var ultimoCodigo=lisProducto.ToString();
        //    int correlativo = 1;

        //    if (!string.IsNullOrEmpty(ultimoCodigo))
        //    {
        //        // Extraer la parte numérica final
        //        var partes = ultimoCodigo.Split('-');
        //        if (partes.Length == 2 && int.TryParse(partes[1], out int ultimoNum))
        //        {
        //            correlativo = ultimoNum + 1;
        //        }
        //    }

        //    // Generar nuevo código
        //    return CodigoProductoHelper.GenerarCodigo(nombreProducto, correlativo);
        //}
        // GET: ProductosController/Edit/5
        public ActionResult Edit(int id)
        {
            return View();
        }

        // POST: ProductosController/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(int id, IFormCollection collection)
        {
            try
            {
                return RedirectToAction(nameof(Index));
            }
            catch
            {
                return View();
            }
        }

        // GET: ProductosController/Delete/5
        public ActionResult Delete(int id)
        {
            return View();
        }

        // POST: ProductosController/Delete/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Delete(int id, IFormCollection collection)
        {
            try
            {
                return RedirectToAction(nameof(Index));
            }
            catch
            {
                return View();
            }
        }
    }
}
