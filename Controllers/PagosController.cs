using System;
using System.Diagnostics.Metrics;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json.Linq;
using WebColegio.Models;
using WebColegio.Models.ViewModel;
using WebColegio.Services;
using static System.Net.Mime.MediaTypeNames;

namespace WebColegio.Controllers
{
    public class PagosController : Controller
    {
        private readonly IServicesApi _Iservices;
        public const int MoraPorMes = 10;
        public PagosController(IServicesApi services)
        {
            _Iservices = services;
        }
        // GET: PagosController
        
        public async Task<ActionResult> Index(DateTime? fechainicio, DateTime? fechafin)
        {

            var _pagos = await _Iservices.GetPagosAsync();
            var _alumnos = await _Iservices.GetAlumnosAsync();
            var _tipoMovimiento = await _Iservices.GetTipoMovimientoAsync();
            var _tipoRecibo = await _Iservices.GetTipoReciboAsync();
            var _metodoPago = await _Iservices.GetMetodoPagoAsync();
            var _meses = await _Iservices.GetMesesAsync();
            var _recinto = await _Iservices.GetRecintosAsync();
            //var _modalidad = await _Iservices.GetModalidadesAsync();
            //var _grados = await _Iservices.GetGradosAsync();
            
            // Convertir a IQueryable para aplicar filtros de manera eficiente
            IQueryable<TblPago> query = _pagos.AsQueryable();

            // Aplicar filtros de manera acumulativa sin ejecutar la consulta
            if (fechainicio.HasValue)
            {
                // Normalizar la fecha de inicio al inicio del día (00:00:00)
                var fechaInicioNormalizada = fechainicio.Value.Date;
                query = query.Where(a => a.FechaRegistro >= fechaInicioNormalizada);
            }
            if (fechafin.HasValue)
            {
                // Normalizar la fecha de fin al final del día (23:59:59)
                var fechaFinNormalizada = fechafin.Value.Date.AddDays(1).AddTicks(-1);
                query = query.Where(a => a.FechaRegistro <= fechaFinNormalizada);
            }

            // Ejecutar la consulta SOLO al final, después de aplicar todos los filtros
            var pagosFiltrados = query.ToList();
   
            var VieModelPagos = new ColeccionCatalogos
            {
                pagos = pagosFiltrados,
                alumno = _alumnos,
                tipoMovimiento = _tipoMovimiento,
                tipoRecibo = _tipoRecibo,
                metodoPago = _metodoPago,
                meses = _meses,
                recintos = _recinto
                //modalidades = _modalidad,
                //grados = _grados    

            };
            if (VieModelPagos == null)
            {
                TempData["Message"] = "No hay Pagos registrados";
                return View("NotFound"); // Redirige a una vista de error o no encontrado
            }
            else
            {
                TempData["Message"] = "Pagos encontrados";
                return View(VieModelPagos);
            }
        }
        [Authorize]
        public async Task<ActionResult> EstadoCuenta()
        {

            var _pagos = await _Iservices.GetPagosAsync();
            var _alumnos = await _Iservices.GetAlumnosAsync();
            var _tipoMovimiento = await _Iservices.GetTipoMovimientoAsync();
            var _tipoRecibo = await _Iservices.GetTipoReciboAsync();
            var _metodoPago = await _Iservices.GetMetodoPagoAsync();
            var _meses = await _Iservices.GetMesesAsync();
            var _notas = await _Iservices.GetNotasAsync();
            var _periodo = await _Iservices.GetPeriodoAsync();
            var _grados = await _Iservices.GetGradosAsync();
            var _recinto = await _Iservices.GetRecintosAsync();



            var VieModelEstadoCuenta = new ColeccionCatalogos
            {
                pagos = _pagos,
                alumno = _alumnos,
                tipoMovimiento = _tipoMovimiento,
                tipoRecibo = _tipoRecibo,
                metodoPago = _metodoPago,
                meses = _meses,
                periodo = _periodo,
                grados = _grados,
                recintos=_recinto

            };
            if (VieModelEstadoCuenta == null)
            {
                TempData["Message"] = "No hay estado de cuenta registradas";
                return View("NotFound"); // Redirige a una vista de error o no encontrado
            }
            else
            {
                TempData["Message"] = "Estado de Cuenta encontradas";
                return View(VieModelEstadoCuenta);
            }
        }
        //Formato Matricula


        // GET: PagosController/Details/5
        [Authorize]
        public async Task<ActionResult> Details(int id, bool imprimir = false)
        {

            var listpagos = await _Iservices.GetPagoById(id);
            var _alumnos = await _Iservices.GetAlumnosAsync();
            var _tipoMovimiento = await _Iservices.GetTipoMovimientoAsync();
            var _grados = await _Iservices.GetGradosAsync();
            var _turnos = await _Iservices.GetTurnosAsync();
            var _metodoPago = await _Iservices.GetMetodoPagoAsync();
            var viewModel = new PagosViewModel
            {
                Pago = listpagos,
                alumnos = _alumnos,
                metodoPago = _metodoPago,
                tipoMovimiento = _tipoMovimiento,
                cantidadEnLetras = NumeroALetras(listpagos.Monto),
                grados = _grados,
                turnos=_turnos,
                recintos = (await _Iservices.GetRecintosAsync())
                .Select(r => new SelectListItem
                {
                    Value = r.IdRecinto.ToString(),
                    Text = r.Recinto,
                    //Selected = r.IdPregunta == respuestas.IdPregunta
                }).ToList(),

            };

            if (listpagos == null)
            {
                return NotFound();
            }

            // Pasar el parámetro de impresión a la vista
            ViewBag.Imprimir = imprimir;

            return View(viewModel);
        }
        //numeros en letra
        private string NumeroALetras(decimal numero)
        {
            return Humanizer.NumberToWordsExtension.ToWords((long)numero, new System.Globalization.CultureInfo("es"))
                .ToUpper() + " CÓRDOBAS";
        }

        // GET: PagosController/Create
        [Authorize]
        public async Task<ActionResult> Create()
        {
            var recibos = await _Iservices.GetPagosAsync();
            var maxNumero = recibos
               .Where(r => r.Serie == "A")
               .Max(r => (int?)r.NumeroRecibo);

            var siguienteNumero = maxNumero.HasValue ? maxNumero.Value + 1 : 10001;
            var viewmodel = new PagosViewModel
            {

                SiguienteNumero = siguienteNumero,
                tipoMovimientoSelectListItem = (await _Iservices.GetTipoMovimientoAsync())
                                   .Select(r => new SelectListItem
                                   {
                                       Value = r.IdTipoMovimiento.ToString(),
                                       Text = r.Concepto,
                                       //Selected = r.IdPregunta == respuestas.IdPregunta
                                   }).ToList(),
                tipoRecibosSelectListItem = (await _Iservices.GetTipoReciboAsync())
                                   .Select(r => new SelectListItem
                                   {
                                       Value = r.IdTipoRecibo.ToString(),
                                       Text = r.TipoRecibo,
                                       //Selected = r.IdPregunta == respuestas.IdPregunta
                                   }).ToList(),
                metodoPagoSelectListItem = (await _Iservices.GetMetodoPagoAsync())
                                   .Select(r => new SelectListItem
                                   {
                                       Value = r.IdMetodoPago.ToString(),
                                       Text = r.MetodoPago,
                                       //Selected = r.IdPregunta == respuestas.IdPregunta
                                   }).ToList(),
                    meses = (await _Iservices.GetMesesAsync()),
                
                periodo = (await _Iservices.GetPeriodoAsync())
                .Select(r=> new SelectListItem
                {

                    Value = r.IdPeriodo.ToString(),
                    Text = r.Periodo.ToString(),
                }
                ).ToList(),
                gradosSelectListItem = (await _Iservices.GetGradosAsync())
                .Select(r => new SelectListItem
                {
                    Value = r.IdGrado.ToString(),
                    Text = r.NombreGrado,
                    //Selected = r.IdPregunta == respuestas.IdPregunta
                }).ToList(),
                recintos = (await _Iservices.GetRecintosAsync())
                .Select(r => new SelectListItem
                {

                    Value = r.IdRecinto.ToString(),
                    Text = r.Recinto.ToString(),
                }
                ).ToList(),
                modalidadSelectListItem = (await _Iservices.GetModalidadesAsync())
                .Select(r => new SelectListItem
                {

                    Value = r.IdModalidad.ToString(),
                    Text = r.Modalidad.ToString(),
                }
                ).ToList()


            };

            return View(viewmodel);
        }

        // POST: PagosController/Create
        [Authorize]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Create(PagosViewModel pagos, string MesesSeleccionados)//List<int> MesesSeleccionados)
        {
            bool response = false;
            bool validarDuplicado = false;
           
            // Validar que pagos y pagos.Pago no sean null primero
            if (pagos == null || pagos.Pago == null)
            {
                TempData["Mensaje"] = "Error: Los datos del pago no fueron recibidos correctamente.";
                TempData["Tipo"] = "warning";
                return RedirectToAction("Create");
            }

            // Validar campos críticos manualmente antes de validar el ModelState
            var erroresManuales = new List<string>();
            
            if (pagos.Pago.IdAlumno == 0)
            {
                erroresManuales.Add("Debe seleccionar un alumno");
            }
            
            if (pagos.Pago.IdTipoMovimiento == 0)
            {
                erroresManuales.Add("Debe seleccionar un tipo de movimiento");
            }
            
            if (pagos.Pago.IdTipoRecibo == 0)
            {
                erroresManuales.Add("Debe seleccionar un tipo de recibo");
            }
            
            if (pagos.Pago.IdMetodoPago == 0)
            {
                erroresManuales.Add("Debe seleccionar un método de pago");
            }
            
            if (pagos.Pago.IdRecinto == 0)
            {
                erroresManuales.Add("Debe seleccionar un centro de estudio");
            }
            
            if (pagos.Pago.Monto <= 0)
            {
                erroresManuales.Add("El monto debe ser mayor a cero");
            }

            if (erroresManuales.Any())
            {
                TempData["Mensaje"] = "Error de validación: " + string.Join(", ", erroresManuales);
                TempData["Tipo"] = "warning";
                return RedirectToAction("Create");
            }

            // Limpiar errores del ModelState para campos opcionales que pueden causar problemas
            ModelState.Remove("Pago.Anyo");
            ModelState.Remove("Pago.IdMes");
            ModelState.Remove("Pago.IdModalidad"); // Puede ser opcional según el tipo de movimiento
            ModelState.Remove("Pago.IdGrado"); // Puede ser opcional según el tipo de movimiento
            ModelState.Remove("Pago.Serie"); // Se establece automáticamente en el código
            ModelState.Remove("cantidadEnLetras"); // Campo calculado, no requerido
            ModelState.Remove("MesesSeleccionados"); // Parámetro opcional del método
            
            // Validar el modelo solo para campos críticos
            if (!ModelState.IsValid)
            {
                var erroresValidacion = ModelState
                    .Where(x => x.Value.Errors.Count > 0)
                    .SelectMany(x => x.Value.Errors.Select(e => $"{x.Key}: {e.ErrorMessage}"))
                    .ToList();
                
                // Solo mostrar errores si son de campos críticos
                var erroresCriticos = erroresValidacion
                    .Where(e => !e.Contains("Anyo") && 
                               !e.Contains("IdMes") && 
                               !e.Contains("IdModalidad") && 
                               !e.Contains("IdGrado"))
                    .ToList();
                
                if (erroresCriticos.Any())
                {
                    TempData["Mensaje"] = "Error de validación: " + string.Join("; ", erroresCriticos);
                    TempData["Tipo"] = "warning";
                    return RedirectToAction("Create");
                }
            }

            var buscarIdGuardado = await _Iservices.GetPagosAsync();
           
            try
            {
                int idUsuario = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));
                
                // Solo usar el período actual como fallback si el usuario no seleccionó ninguno
                // Si el usuario seleccionó un período en la vista, usar ese
                if (pagos.Pago.IdPeriodo == 0)
                {
                    int periodoActual = await _Iservices.GetPeriodoAsync().ContinueWith(p=>p.Result.FirstOrDefault(a=>a.Activo && a.Actual)?.IdPeriodo) ?? 0;
                    pagos.Pago.IdPeriodo = periodoActual;
                }
                // Si ya tiene un valor (seleccionado por el usuario), mantenerlo


                if (pagos != null && pagos.Pago != null)
                {
                    pagos.Pago.UsuarioRegistro = idUsuario;
                    pagos.Pago.FechaRegistro = DateTime.Now;
                    pagos.Pago.Serie = "A";
                    pagos.Pago.Activo = true;
                    if (pagos.Pago.IdTipoMovimiento == 10)
                    {
                        int mes = Convert.ToInt32(MesesSeleccionados);
                        pagos.Pago.IdMes = mes;
                        if (string.IsNullOrEmpty(MesesSeleccionados))
                        {
                            TempData["Mensaje"] = "Debe Ingresar el Mes para hacer el pago de la Rifa";
                            TempData["Tipo"] = "warning";
                            return RedirectToAction("Create");
                        }
                        var existePagoRifa = buscarIdGuardado.Where(a => a.IdAlumno == pagos.Pago.IdAlumno && a.IdMes <=6  && a.IdMes >= 6 && a.IdTipoMovimiento == 10).ToList();
                        if (existePagoRifa.Count > 0)
                        {

                            TempData["Mensaje"] = "Ya existe el pago de la Rifa para este semestre";
                            TempData["Tipo"] = "warning";
                            return RedirectToAction("Create");
                        }
                        response = await _Iservices.PostPagosAsync(pagos.Pago);
                        if (response)
                        {
                            var idPag = buscarIdGuardado.Max(a => a.IdPago);


                            TempData["Mensaje"] = "Pago registrado correctamente.";
                            TempData["Tipo"] = "success";
                            return RedirectToAction("Details", "Pagos", new { id = idPag + 1, imprimir = true });
                        }
                        else
                        {
                            TempData["Mensaje"] = "No se proceso el Pago.";
                            TempData["Tipo"] = "warning";
                            return RedirectToAction("Create");
                        }
                    }

                    
                    if (!string.IsNullOrEmpty(MesesSeleccionados) && pagos.Pago.IdTipoMovimiento == 1)//MesesSeleccionados != null && MesesSeleccionados.Any())
                    {
                        var ids = MesesSeleccionados.Split(',').Select(int.Parse).ToList();
                        var listpagos = await _Iservices.GetPagosAsync();

                        bool aplicoMora = false;
                        bool duplicado = false;
                        
                        if (BecaCompleta(pagos.Pago.IdAlumno,pagos.Pago.IdPeriodo).Result && pagos.Pago.IdTipoMovimiento==1)
                        {
                            TempData["Mensaje"] = "El Estudiante Posee Beca Completa.";
                            TempData["Tipo"] = "warning";
                            return RedirectToAction("Create");
                        }
                        if (MediaBeca(pagos.Pago.IdAlumno, pagos.Pago.IdPeriodo).Result && pagos.Pago.Monto > 320 && pagos.Pago.IdTipoMovimiento==1)
                        {
                            TempData["Mensaje"] = "El Estudiante Cuenta con Media Beca.";
                            TempData["Tipo"] = "warning";
                            return RedirectToAction("Create");
                        }
                        
                        //total = ids * mensualidad;
                        //if((ids* mensualidad)=pagos.Pago.Monto)

                        //crear meses pagados previos
                        var mesesPagadosBD = listpagos
                        .Where(p => p.IdAlumno == pagos.Pago.IdAlumno &&
                                    p.IdTipoMovimiento == pagos.Pago.IdTipoMovimiento &&
                                    p.IdPeriodo == pagos.Pago.IdPeriodo)
                        .Select(p => p.IdMes)
                        .ToHashSet();
                        //crear meses pagados virtualmente
                        var mesesPagadosAcumulados = new HashSet<int?>(mesesPagadosBD);

                        var idsOrdenados = ids.OrderBy(m => m).ToList();
                        if (duplicado)
                        {
                            TempData["Mensaje"] = "Pago del mes seleccionado, ya fue registrado.";
                            TempData["Tipo"] = "warning";
                        }
                        if (aplicoMora)
                        {
                            TempData["Mensaje"] = "Pago registrado con mora de C$ 10 aplicada.";
                            TempData["Tipo"] = "info";
                        }
                        
                        foreach (var idMes in idsOrdenados)
                        {
                            // 1️⃣ Validar duplicado

                            
                            validarDuplicado = buscarIdGuardado.Any(r => r.NumeroRecibo == pagos.Pago.NumeroRecibo && r.IdMes ==idMes && r.IdPeriodo==pagos.Pago.IdPeriodo && r.Serie == "A" && r.Activo == true);
                            if (validarDuplicado)
                            {
                                TempData["Mensaje"] = "El número de Recibo ya Existe.";
                                TempData["Tipo"] = "warning";
                                return RedirectToAction("Create");
                            }

                            if (mesesPagadosAcumulados.Contains(idMes))
                            {
                                // Evita duplicado
                                //duplicado = true;

                                TempData["Mensaje"] = $"El mes {Mes(idMes).Result} ya fue pagado por este alumno.";
                                TempData["Tipo"] = "warning";
                                continue;
                            }
                            var todosLosMeses = Enumerable.Range(1, 12); // o hasta el mes actual
                             mesesPagadosAcumulados
                            .Distinct()
                            .Where(x => x >= 1 && x <= 12)
                            .ToList();
                            // 2. Verificar si hay meses anteriores sin pagar
                            var mesesPendientes = Enumerable.Range(1, idMes - 1)
                            .Except(mesesPagadosAcumulados.Cast<int>())
                            .Distinct()
                            .ToList();
                            //var mesesPendientes = Enumerable.Range(1, idMes - 1)
                            //    .Except(mesesPagadosAcumulados)
                            //    .ToList();

                            //int anioActual =(int) pagos.Pago.Anyo; //pagos.Pago.IdPeriodo;

                            
                            if (mesesPendientes.Any())
                            {
                                var primerMesFaltante = mesesPendientes.Min();

                                TempData["Mensaje"] = $"No puede pagar el mes de {Mes(idMes).Result}. Debe pagar primero el mes de {Mes(primerMesFaltante).Result}.";
                                    TempData["Tipo"] = "warning";
                                    return RedirectToAction("Create");
                                
                            }
                            var hoy = DateTime.Now;
                           // if(mesesPendientes.Count > 0)
                           // {
                           // int moraPorMes = 10;
                           // decimal moraTotal = mesesPendientes.Count * moraPorMes;
                           // pagos.Mora =(int) moraTotal;
                           // pagos.MontoTotal = mensualidad + moraTotal;
                           //}
                            
                                // ejemplo: crear un pago por cada mes
                                var nuevoPago = new TblPago
                                {
                                    IdAlumno = pagos.Pago.IdAlumno,
                                    
                                    NumeroRecibo=pagos.Pago.NumeroRecibo,
                                    Anyo=pagos.Pago.Anyo,
                                    IdMes = idMes,
                                    IdTipoRecibo = pagos.Pago.IdTipoRecibo,
                                    IdTipoMovimiento = pagos.Pago.IdTipoMovimiento,
                                    IdMetodoPago = pagos.Pago.IdMetodoPago,
                                    IdGrado=pagos.Pago.IdGrado,
                                    IdPeriodo=pagos.Pago.IdPeriodo,
                                    IdRecinto=pagos.Pago.IdRecinto,
                                    FechaEmision=pagos.Pago.FechaEmision,
                                    Mora = pagos.Pago.Mora,
                                    Monto = pagos.Pago.Monto,
                                    Descripcion=pagos.Pago.Descripcion,
                                    UsuarioRegistro=pagos.Pago.UsuarioRegistro,
                                    Activo=pagos.Pago.Activo,
                                    FechaRegistro=pagos.Pago.FechaRegistro,
                                    Serie=pagos.Pago.Serie
                                  
                                    // otros campos...
                                };

                                //await _Iservices.InsertarPagoAsync(nuevoPago);
                                response = await _Iservices.PostPagosAsync(nuevoPago);
                                
                                if (!response)
                                {
                                    TempData["Mensaje"] = $"No se logro procesar el pago del mes {Mes(idMes).Result}.";
                                    TempData["Tipo"] = "warning";
                                    return RedirectToAction("Create");
                                }
                                
                                mesesPagadosAcumulados.Add(idMes);
                        }
                        
                        // Si todos los meses se procesaron correctamente, obtener el ID del último pago
                        var pagosActualizados = await _Iservices.GetPagosAsync();
                        var idPag = pagosActualizados.Max(a => a.IdPago);
                        TempData["Mensaje"] = "Pago registrado correctamente.";
                        TempData["Tipo"] = "success";
                        return RedirectToAction("Details", "Pagos", new { id = idPag, imprimir = true });
                        // 4️⃣ Mensaje final
                       
                        
                        

                    }
                    else
                    {
                        if (pagos.Pago.IdTipoMovimiento == 2)
                        {
                            int periodoMatricula = pagos.Pago.IdPeriodo;
                            
                            // Validar que los campos requeridos estén presentes
                            if (pagos.Pago.IdGrado == 0)
                            {
                                TempData["Mensaje"] = "Debe seleccionar el Nivel (Grado) para procesar la matrícula completa.";
                                TempData["Tipo"] = "warning";
                                return RedirectToAction("Create", "Pagos");
                            }
                            
                            if (!pagos.Pago.IdModalidad.HasValue || pagos.Pago.IdModalidad.Value == 0)
                            {
                                TempData["Mensaje"] = "Debe seleccionar la Modalidad para procesar la matrícula completa.";
                                TempData["Tipo"] = "warning";
                                return RedirectToAction("Create", "Pagos");
                            }
                            
                            decimal restarMensualidad = await ObtenerMensualidadDecimal(pagos.Pago.IdRecinto, pagos.Pago.IdGrado, periodoMatricula);
                            decimal obtenerMat = await ObtenerMatriculaDecimal(pagos.Pago.IdRecinto, pagos.Pago.IdModalidad, periodoMatricula);
                            
                            // Validar que se obtuvieron los valores correctamente
                            if (obtenerMat == 0)
                            {
                                TempData["Mensaje"] = "No se encontró el costo de matrícula para los parámetros seleccionados.";
                                TempData["Tipo"] = "warning";
                                return RedirectToAction("Create", "Pagos");
                            }

                            var totalMatricual = Convert.ToDecimal(pagos.Pago.Monto);
                            
                            // Calcular cuánto se ha pagado ya de matrícula (tipo 2 o 4)
                            var pagosMatriculaPrevios = _Iservices.GetPagosAsync().Result
                                .Where(a => a.IdAlumno == pagos.Pago.IdAlumno && 
                                           (a.IdTipoMovimiento == 2 || a.IdTipoMovimiento == 4) && 
                                           a.IdPeriodo == periodoMatricula &&
                                           a.Activo == true)
                                .ToList();
                            
                            decimal totalPagadoMatricula = pagosMatriculaPrevios.Sum(p => p.Monto);
                            decimal faltaPorPagar = obtenerMat - totalPagadoMatricula;
                            
                            // Validar que el monto ingresado no exceda lo que falta por pagar
                            if (totalMatricual > faltaPorPagar)
                            {
                                TempData["Mensaje"] = $"El monto ingresado (C$ {totalMatricual:N2}) excede lo que falta por pagar (C$ {faltaPorPagar:N2}). Total de matrícula: C$ {obtenerMat:N2}, ya pagado: C$ {totalPagadoMatricula:N2}.";
                                TempData["Tipo"] = "warning";
                                return RedirectToAction("Create", "Pagos");
                            }
                            
                            // Si ya se pagó el total completo, no permitir más pagos
                            if (faltaPorPagar <= 0.01m)
                            {
                                TempData["Mensaje"] = "La matrícula ya está completamente pagada.";
                                TempData["Tipo"] = "warning";
                                return RedirectToAction("Create", "Pagos");
                            }
                            
                            // Comparar decimales con tolerancia (0.01) para evitar problemas de precisión
                            // Verificar si el monto ingresado completa el pago de matrícula
                            decimal diferencia = Math.Abs(totalMatricual - faltaPorPagar);
                            bool esMontoCompleto = diferencia < 0.01m;
                            
                            // Si el monto completa el pago pendiente de matrícula, procesar como matrícula completa
                            if (esMontoCompleto)
                            {
                                // El monto ingresado completa lo que falta, restar la mensualidad
                                decimal valormatricula = totalMatricual - restarMensualidad;
                                pagos.Pago.Monto = valormatricula;
                                pagos.Pago.IdPeriodo = periodoMatricula;
                                response = await _Iservices.PostPagosAsync(pagos.Pago);
                                
                                if (!response)
                                {
                                    TempData["Mensaje"] = "No se pudo registrar el pago de matrícula.";
                                    TempData["Tipo"] = "warning";
                                    return RedirectToAction("Create");
                                }
                                
                                var pagoprimermes = new TblPago
                                {
                                    IdAlumno = pagos.Pago.IdAlumno,
                                    NumeroRecibo = pagos.Pago.NumeroRecibo,
                                    Anyo = pagos.Pago.Anyo,
                                    IdMes = 1,
                                    IdTipoRecibo = pagos.Pago.IdTipoRecibo,
                                    IdTipoMovimiento = pagos.Pago.IdTipoMovimiento,
                                    IdMetodoPago = pagos.Pago.IdMetodoPago,
                                    IdGrado = pagos.Pago.IdGrado,
                                    IdPeriodo = periodoMatricula,
                                    IdRecinto = pagos.Pago.IdRecinto,
                                    FechaEmision = pagos.Pago.FechaEmision,
                                    Mora = 0,
                                    Monto = valormatricula,
                                    Descripcion = pagos.Pago.Descripcion,
                                    UsuarioRegistro = pagos.Pago.UsuarioRegistro,
                                    Activo = pagos.Pago.Activo,
                                    FechaRegistro = pagos.Pago.FechaRegistro,
                                    Serie = pagos.Pago.Serie
                                };
                                
                                response = await _Iservices.PostPagosAsync(pagoprimermes);
                                if (response)
                                {
                                    // Obtener el ID del último pago guardado (el de matrícula)
                                    var pagosActualizados = await _Iservices.GetPagosAsync();
                                    var idPag = pagosActualizados.Max(a => a.IdPago);
                                    TempData["Mensaje"] = "Pago registrado correctamente.";
                                    TempData["Tipo"] = "success";
                                    return RedirectToAction("Details", "Pagos", new { id = idPag, imprimir = true });
                                }
                                else
                                {
                                    TempData["Mensaje"] = "No se proceso el Pago del primer mes.";
                                    TempData["Tipo"] = "warning";
                                    return RedirectToAction("Create");
                                }
                            }
                            else
                            {
                                // Si el monto es diferente (pago parcial), procesar como abono de matrícula
                                // Restar siempre la mensualidad del monto, similar al if anterior
                                decimal valormatricula = totalMatricual - restarMensualidad;
                                pagos.Pago.Monto = valormatricula;
                                pagos.Pago.IdPeriodo = periodoMatricula;
                                response = await _Iservices.PostPagosAsync(pagos.Pago);
                                
                                if (!response)
                                {
                                    TempData["Mensaje"] = "No se pudo registrar el abono de matrícula.";
                                    TempData["Tipo"] = "warning";
                                    return RedirectToAction("Create");
                                }
                                
                                // Crear el pago del primer mes con el valor de la mensualidad
                                var pagoprimermes = new TblPago
                                {
                                    IdAlumno = pagos.Pago.IdAlumno,
                                    NumeroRecibo = pagos.Pago.NumeroRecibo,
                                    Anyo = pagos.Pago.Anyo,
                                    IdMes = 1,
                                    IdTipoRecibo = pagos.Pago.IdTipoRecibo,
                                    IdTipoMovimiento = 1,
                                    IdMetodoPago = pagos.Pago.IdMetodoPago,
                                    IdGrado = pagos.Pago.IdGrado,
                                    IdPeriodo = periodoMatricula,
                                    IdRecinto = pagos.Pago.IdRecinto,
                                    FechaEmision = pagos.Pago.FechaEmision,
                                    Mora = 0,
                                    Monto = restarMensualidad,
                                    Descripcion = pagos.Pago.Descripcion,
                                    UsuarioRegistro = pagos.Pago.UsuarioRegistro,
                                    Activo = pagos.Pago.Activo,
                                    FechaRegistro = pagos.Pago.FechaRegistro,
                                    Serie = pagos.Pago.Serie
                                };
                                
                                response = await _Iservices.PostPagosAsync(pagoprimermes);
                                if (response)
                                {
                                    // Obtener el ID del último pago guardado (el de matrícula)
                                    var pagosActualizados = await _Iservices.GetPagosAsync();
                                    var idPag = pagosActualizados.Max(a => a.IdPago);
                                    TempData["Mensaje"] = "Abono de matrícula registrado correctamente.";
                                    TempData["Tipo"] = "success";
                                    return RedirectToAction("Details", "Pagos", new { id = idPag, imprimir = true });
                                }
                                else
                                {
                                    TempData["Mensaje"] = "No se proceso el Pago del primer mes.";
                                    TempData["Tipo"] = "warning";
                                    return RedirectToAction("Create");
                                }
                            }
                           
                        }
                        else
                        {
                            //pagos.Pago.IdMes = 0;
                            response = await _Iservices.PostPagosAsync(pagos.Pago);
                            if (response)
                            {
                                var idPag = buscarIdGuardado.Max(a => a.IdPago);


                                TempData["Mensaje"] = "Pago registrado correctamente.";
                                TempData["Tipo"] = "success";
                                return RedirectToAction("Details", "Pagos", new { id = idPag + 1, imprimir = true });
                            }
                            else
                            {
                                TempData["Mensaje"] = "No se proceso el Pago.";
                                TempData["Tipo"] = "warning";
                                return RedirectToAction("Create");
                            }
                        }
                    }
                }
                else
                {
                    TempData["Mensaje"] = "Error: Los datos del pago no son válidos.";
                    TempData["Tipo"] = "warning";
                    return RedirectToAction("Create");
                }
            }
            catch (Exception ex)
            {
                // En caso de error, redirigir a Create para que se cargue el modelo correctamente
                TempData["Mensaje"] = $"Ocurrió un error al procesar el pago: {ex.Message}";
                TempData["Tipo"] = "warning";
                // Log del error completo para debugging
                System.Diagnostics.Debug.WriteLine($"Error en Create de Pagos: {ex}");
                return RedirectToAction("Create");
            }
        }

        //Calcular meses atrasados
        public int CalcularMesesAtrasados(DateTime fechaDeuda, DateTime fechaActual)
        {
            // Normalizar días (solo comparar por año y mes)
            fechaDeuda = new DateTime(fechaDeuda.Year, fechaDeuda.Month, 1);
            fechaActual = new DateTime(fechaActual.Year, fechaActual.Month, 1);

            int meses = (fechaActual.Year - fechaDeuda.Year) * 12 + (fechaActual.Month - fechaDeuda.Month);
            if (fechaActual.Day < fechaDeuda.Day)
            {
                meses--;
            }

            return meses < 0 ? 0 : meses;
        }

        //calcular mora
        //public int CalcularMoraTotal(DateTime fechaDeuda, DateTime fechaActual)
        //{

        //    return mesesAtrasados * MoraPorMes;
        //}
        [Authorize]
        [HttpGet]
        public IActionResult ObtenerMora(int idAlumno,int idTipoMovimiento,int periodo)
        {
            int mes = DateTime.Now.Month;
            var listpagos =  _Iservices.GetPagosAsync().Result;
            var mesesPagadosBD = listpagos
                       .Where(p => p.IdAlumno == idAlumno &&
                                   p.IdTipoMovimiento == idTipoMovimiento &&
                                   p.IdPeriodo == periodo)
                       .Select(p => p.IdMes)
                       .ToHashSet();
            //crear meses pagados virtualmente
            var mesesPagadosAcumulados = new HashSet<int?>(mesesPagadosBD);
            var mesesPendientes = Enumerable.Range(1, mes - 1)
                            .Except(mesesPagadosAcumulados.Cast<int>())
                            .Distinct()
                            .ToList();
            int moraTotal = 10 * mesesPendientes.Count();

            return Json(new { mora = moraTotal, mes });
        }
        //Obtener nombre del mes.
        public async Task<string> Mes(int idmes)
        {
            string mes = await _Iservices.GetMesesAsync().ContinueWith(a => a.Result.FirstOrDefault(m => m.IdMes == idmes)?.Mes ?? "No hay Mes");

            return mes;
        }
        
        public async Task<bool>BecaCompleta(int idalumno,int periodo)
        {
            bool becaCompleta = await _Iservices.GetAlumnosAsync().ContinueWith(a => a.Result.FirstOrDefault(b => b.IdAlumno==idalumno && b.BecaCompleta==true && b.IdPeriodo==periodo)?.BecaCompleta ?? false);
            
            if(becaCompleta)
            {
                return true;
            }
                return false;
           
        }
        public async Task<bool>MediaBeca(int idalumno,int periodo)
        {
            
            bool mediaBeca = await _Iservices.GetAlumnosAsync().ContinueWith(a => a.Result.FirstOrDefault(b => b.IdAlumno == idalumno && b.MediaBeca == true && b.IdPeriodo == periodo)?.MediaBeca ?? false);
            
            if (mediaBeca)
            {
                return true;
            }
            
                return false;
           
        }
        public bool ExistePagoDuplicado(PagosViewModel nuevoPago)
        {

            var query =  _Iservices.GetPagosAsync().ContinueWith(
                         q => q.Result.Where(p => p.IdAlumno == nuevoPago.Pago.IdAlumno &&
                        p.IdPeriodo == nuevoPago.Pago.IdPeriodo &&
                        p.IdTipoMovimiento == nuevoPago.Pago.IdTipoMovimiento));

            if(!query.Result.Any())
            {
                return false;
            }
            

            return true;
        }
        public async Task<decimal> ObtenerMensualidadDecimal(
    int? idRecinto, int idGrado, int idPeriodo)
        {
            return await _Iservices.GetCostosMensualidadAsync()
                .ContinueWith(t => t.Result
                    .Where(x => x.IdRecinto == idRecinto &&
                                x.IdGrado == idGrado &&
                                x.IdPeriodo == idPeriodo &&
                                x.Activo)
                    .Select(x => x.CostoMensualidad)
                    .FirstOrDefault());
        }
        public async Task<decimal> ObtenerMatriculaDecimal(
    int? idRecinto, int? idModalidad, int idPeriodo)
        {
            return await _Iservices.GetCostosMatriculaAsync()
                .ContinueWith(t => t.Result
                    .Where(x => x.IdRecinto == idRecinto &&
                                 x.IdModalidad == idModalidad &&
                                x.IdPeriodo == idPeriodo &&
                                x.Activo)
                    .Select(x => x.CostoMatricula)
                    .FirstOrDefault());
        }

        [HttpGet]
        [Authorize]
        public IActionResult ObtenerMensualidad(int? idRecinto, int idGrado, int idPeriodo)
        {
            var mensualidad = _Iservices.GetCostosMensualidadAsync().Result
                .Where(x => x.IdRecinto == idRecinto &&
                            x.IdGrado == idGrado &&
                            //x.IdModalidad == idModalidad &&
                            x.IdPeriodo == idPeriodo &&
                            x.Activo == true)
                .Select(x => new {
                    costo = x.CostoMensualidad
                })
                .FirstOrDefault();

            return Json(mensualidad);
        }
        [HttpGet]
        [Authorize]
        public IActionResult ObtenerMatricula(int? idRecinto, int? idModalidad, int idPeriodo)
        {
            var matricula= _Iservices.GetCostosMatriculaAsync().Result
                .Where(x => x.IdRecinto == idRecinto &&
                            x.IdModalidad == idModalidad &&
                            //x.IdModalidad == idModalidad &&
                            x.IdPeriodo == idPeriodo &&
                            x.Activo == true)
                .Select(x => new {
                    costo = x.CostoMatricula
                })
                .FirstOrDefault();

            return Json(matricula);
        }

        [HttpGet]
        [Authorize]
        public IActionResult ObtenerAbonosPrevios(int idAlumno, int idTipoMovimiento, int idPeriodo)
        {
            try
            {
                var listpagos = _Iservices.GetPagosAsync().Result;
                var tiposMovimiento = _Iservices.GetTipoMovimientoAsync().Result;
                
                // Buscar abonos previos según el tipo de movimiento
                decimal totalAbonos = 0;
                
                if (idTipoMovimiento == 2 || idTipoMovimiento == 4) // Matrícula Completa o Confirmación de Matrícula
                {
                    // Si es Confirmación de Matrícula (tipo 4), buscar abonos previos del mismo tipo
                    // Si es Matrícula Completa (tipo 2), buscar abonos de confirmación de matrícula (tipo 4)
                    int tipoAbonoBuscado = idTipoMovimiento == 4 ? 4 : 4; // Ambos casos buscan tipo 4
                    
                    // Buscar abonos de confirmación de matrícula
                    var abonosMatricula = listpagos
                        .Where(p => p.IdAlumno == idAlumno &&
                                    p.IdPeriodo == idPeriodo &&
                                    p.IdTipoMovimiento == tipoAbonoBuscado &&
                                    p.Activo == true)
                        .ToList();
                    
                    totalAbonos = abonosMatricula.Sum(p => p.Monto);
                    
                    // Si no encuentra por ID, intentar buscar por concepto
                    if (totalAbonos == 0)
                    {
                        var tipoConfirmacion = tiposMovimiento
                            .FirstOrDefault(tm => tm.Concepto.ToLower().Contains("confirmación") || 
                                                 tm.Concepto.ToLower().Contains("reserva") ||
                                                 (tm.Concepto.ToLower().Contains("abono") && tm.Concepto.ToLower().Contains("matrícula")));
                        
                        if (tipoConfirmacion != null && tipoConfirmacion.IdTipoMovimiento != idTipoMovimiento)
                        {
                            var abonosPorConcepto = listpagos
                                .Where(p => p.IdAlumno == idAlumno &&
                                            p.IdPeriodo == idPeriodo &&
                                            p.IdTipoMovimiento == tipoConfirmacion.IdTipoMovimiento &&
                                            p.Activo == true)
                                .ToList();
                            
                            totalAbonos = abonosPorConcepto.Sum(p => p.Monto);
                        }
                    }
                }
                else if (idTipoMovimiento == 1) // Mensualidad
                {
                    // Buscar abonos de mensualidad previos
                    // Buscar el tipo de movimiento que corresponde a "Abono de Mensualidad"
                    var tipoAbonoMensualidad = tiposMovimiento
                        .FirstOrDefault(tm => (tm.Concepto.ToLower().Contains("Abono Mensualidad") && 
                                               tm.Concepto.ToLower().Contains("Mensualidad")) ||
                                              tm.Concepto.ToLower().Contains("abono mensualidad"));
                    
                    if (tipoAbonoMensualidad != null)
                    {
                        // Buscar abonos de mensualidad
                        var abonosMensualidad = listpagos
                            .Where(p => p.IdAlumno == idAlumno &&
                                        p.IdPeriodo == idPeriodo &&
                                        p.IdTipoMovimiento == tipoAbonoMensualidad.IdTipoMovimiento &&
                                        p.Activo == true)
                            .ToList();
                        
                        totalAbonos = abonosMensualidad.Sum(p => p.Monto);
                    }
                    // Si no hay tipo específico de abono de mensualidad, no se restan abonos
                    // porque las mensualidades pagadas no son abonos, son pagos completos
                }

                return Json(new { 
                    totalAbonos = totalAbonos,
                    tieneAbonos = totalAbonos > 0
                });
            }
            catch (Exception ex)
            {
                return Json(new { 
                    totalAbonos = 0,
                    tieneAbonos = false,
                    error = ex.Message
                });
            }
        }
        //    public (bool tienePendientes, List<int> mesesPendientes, decimal moraAcumulada, bool puedePagar)
        //ValidarPagoConMora(int idAlumno, int idTipoMovimiento, int periodo, int mesDeseado)
        //    {
        //        var pagosRealizados = listpagos
        //            .Where(p => p.IdAlumno == idAlumno &&
        //                       p.IdTipoMovimiento == idTipoMovimiento &&
        //                       p.IdPeriodo == periodo &&
        //                       p.IdMes <= mesDeseado) // Solo meses hasta el deseado
        //            .Select(p => p.IdMes)
        //            .OrderBy(m => m)
        //            .ToList();

        //        // Encontrar todos los meses que deberían estar pagados (1 hasta mesDeseado-1)
        //        var todosMesesRequeridos = Enumerable.Range(1, mesDeseado - 1).ToList();

        //        // Meses pendientes son los que no están en pagosRealizados
        //        var mesesPendientes = todosMesesRequeridos.Except(pagosRealizados).ToList();

        //        // Validar si puede pagar el mes deseado
        //        bool puedePagar = !mesesPendientes.Any() ||
        //                         (mesesPendientes.Count > 0 && mesesPendientes.Max() == mesDeseado - 1);

        //        // Calcular mora
        //        decimal montoBase = 1000; // Obtener de tu configuración
        //        decimal mora = mesesPendientes.Count * (montoBase * 0.10m);

        //        return (mesesPendientes.Any(), mesesPendientes, mora, puedePagar);
        //    }

        // GET: PagosController/Edit/5
        [Authorize]
        public async Task<ActionResult> Edit(int id)
        {
            var pago = await _Iservices.GetPagoById(id);
            
            if (pago == null)
            {
                TempData["Mensaje"] = "Pago no encontrado.";
                TempData["Tipo"] = "warning";
                return RedirectToAction(nameof(Index));
            }

            var viewmodel = new PagosViewModel
            {
                Pago = pago,
                SiguienteNumero = pago.NumeroRecibo ?? 0,
                tipoMovimientoSelectListItem = (await _Iservices.GetTipoMovimientoAsync())
                                   .Select(r => new SelectListItem
                                   {
                                       Value = r.IdTipoMovimiento.ToString(),
                                       Text = r.Concepto,
                                       Selected = r.IdTipoMovimiento == pago.IdTipoMovimiento
                                   }).ToList(),
                tipoRecibosSelectListItem = (await _Iservices.GetTipoReciboAsync())
                                   .Select(r => new SelectListItem
                                   {
                                       Value = r.IdTipoRecibo.ToString(),
                                       Text = r.TipoRecibo,
                                       Selected = r.IdTipoRecibo == pago.IdTipoRecibo
                                   }).ToList(),
                metodoPagoSelectListItem = (await _Iservices.GetMetodoPagoAsync())
                                   .Select(r => new SelectListItem
                                   {
                                       Value = r.IdMetodoPago.ToString(),
                                       Text = r.MetodoPago,
                                       Selected = r.IdMetodoPago == pago.IdMetodoPago
                                   }).ToList(),
                meses = await _Iservices.GetMesesAsync(),
                periodo = (await _Iservices.GetPeriodoAsync())
                .Select(r => new SelectListItem
                {
                    Value = r.IdPeriodo.ToString(),
                    Text = r.Periodo.ToString(),
                    Selected = r.IdPeriodo == pago.IdPeriodo
                }).ToList(),
                gradosSelectListItem = (await _Iservices.GetGradosAsync())
                .Select(r => new SelectListItem
                {
                    Value = r.IdGrado.ToString(),
                    Text = r.NombreGrado,
                    Selected = r.IdGrado == pago.IdGrado
                }).ToList(),
                recintos = (await _Iservices.GetRecintosAsync())
                .Select(r => new SelectListItem
                {
                    Value = r.IdRecinto.ToString(),
                    Text = r.Recinto.ToString(),
                    Selected = r.IdRecinto == pago.IdRecinto
                }).ToList(),
                modalidadSelectListItem = (await _Iservices.GetModalidadesAsync())
                .Select(r => new SelectListItem
                {
                    Value = r.IdModalidad.ToString(),
                    Text = r.Modalidad.ToString(),
                    Selected = r.IdModalidad == pago.IdModalidad
                }).ToList(),
                alumnos = await _Iservices.GetAlumnosAsync()
            };

            return View(viewmodel);
        }

        // POST: PagosController/Edit/5
        [Authorize]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Edit(int id, PagosViewModel pagosViewModel)
        {
            try
            {
                if (id != pagosViewModel.Pago.IdPago)
                {
                    TempData["Mensaje"] = "El ID del pago no coincide.";
                    TempData["Tipo"] = "warning";
                    return RedirectToAction(nameof(Index));
                }

                // Obtener el pago original para preservar algunos campos
                var pagoOriginal = await _Iservices.GetPagoById(id);
                if (pagoOriginal == null)
                {
                    TempData["Mensaje"] = "Pago no encontrado.";
                    TempData["Tipo"] = "warning";
                    return RedirectToAction(nameof(Index));
                }

                // Actualizar campos
                int idUsuario = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));
                pagosViewModel.Pago.UsuarioActualizo = idUsuario;
                pagosViewModel.Pago.FechaActualizo = DateTime.Now;
                
                // Preservar campos que no deben cambiar
                pagosViewModel.Pago.UsuarioRegistro = pagoOriginal.UsuarioRegistro;
                pagosViewModel.Pago.FechaRegistro = pagoOriginal.FechaRegistro;
                pagosViewModel.Pago.Serie = pagoOriginal.Serie; // No cambiar la serie

                // Llamar al servicio de actualización
                bool response = await _Iservices.UpdatePago(pagosViewModel.Pago);

                if (response)
                {
                    TempData["Mensaje"] = "Pago actualizado correctamente.";
                    TempData["Tipo"] = "success";
                    return RedirectToAction(nameof(Details), new { id = id });
                }
                else
                {
                    TempData["Mensaje"] = "No se pudo actualizar el pago.";
                    TempData["Tipo"] = "warning";
                    return RedirectToAction(nameof(Edit), new { id = id });
                }
            }
            catch (Exception ex)
            {
                TempData["Mensaje"] = $"Ocurrió un error al actualizar el pago: {ex.Message}";
                TempData["Tipo"] = "danger";
                return RedirectToAction(nameof(Edit), new { id = id });
            }
        }

        // GET: PagosController/Delete/5
        public ActionResult Delete(int id)
        {
            return View();
        }

        // POST: PagosController/Delete/5
        [Authorize]
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
