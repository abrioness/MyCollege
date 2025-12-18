using System;
using System.Diagnostics.Metrics;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
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
            var query = _pagos;
            if (fechainicio.HasValue)
            {
                query = _pagos.Where(a => a.FechaRegistro >= fechainicio.Value).ToList();
            }
            if (fechafin.HasValue)
            {
                query = _pagos.Where(a => a.FechaRegistro <= fechafin.Value).ToList();
            }

            var VieModelPagos = new ColeccionCatalogos
            {
                pagos=query,
                alumno = _alumnos,
                tipoMovimiento = _tipoMovimiento,
                tipoRecibo = _tipoRecibo,
                metodoPago = _metodoPago,
                meses=_meses,
                recintos=_recinto
                //modalidades = _modalidad,
                //grados = _grados    

            };
            if (VieModelPagos == null)
            {
                TempData["Message"] = "No hay notas registradas";
                return View("NotFound"); // Redirige a una vista de error o no encontrado
            }
            else
            {
                TempData["Message"] = "Pagos encontrados";
                return View(VieModelPagos);
            }
        }
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


            var VieModelEstadoCuenta = new ColeccionCatalogos
            {
                pagos = _pagos,
                alumno = _alumnos,
                tipoMovimiento = _tipoMovimiento,
                tipoRecibo = _tipoRecibo,
                metodoPago = _metodoPago,
                meses = _meses,
                periodo = _periodo,
                grados = _grados    

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
        public async Task<ActionResult> Details(int id)
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
                turnos=_turnos
                
            };

            if (listpagos == null)
            {
                return NotFound();
            }

            return View(viewModel);
        }
        //numeros en letra
        private string NumeroALetras(decimal numero)
        {
            return Humanizer.NumberToWordsExtension.ToWords((long)numero, new System.Globalization.CultureInfo("es"))
                .ToUpper() + " CÓRDOBAS";
        }

        // GET: PagosController/Create
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
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<ActionResult> Create(PagosViewModel pagos, string MesesSeleccionados)//List<int> MesesSeleccionados)
        {
            bool response = false;
            bool validarDuplicado = false;
           

            var buscarIdGuardado = await _Iservices.GetPagosAsync();
           
            try
            {
                int idUsuario = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));
                int periodo= await _Iservices.GetPeriodoAsync().ContinueWith(p=>p.Result.FirstOrDefault(a=>a.Activo && a.Actual)?.IdPeriodo) ?? 0;
                pagos.Pago.IdPeriodo = periodo;


                if (pagos != null)
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
                            return RedirectToAction("Details", "Pagos", new { id = idPag + 1 });
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
                                    p.Anyo == pagos.Pago.Anyo)
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

                            
                            validarDuplicado = buscarIdGuardado.Any(r => r.NumeroRecibo == pagos.Pago.NumeroRecibo && r.IdMes ==idMes && r.IdPeriodo==periodo && r.Serie == "A" && r.Activo == true);
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
                                mesesPagadosAcumulados.Add(idMes);
                            if (response)
                            {
                                var idPag = buscarIdGuardado.Max(a => a.IdPago);


                                TempData["Mensaje"] = "Pago registrado correctamente.";
                                TempData["Tipo"] = "success";
                                return RedirectToAction("Details", "Pagos", new { id = idPag + 1 });
                            }
                            else
                            {
                                //var idPag = buscarIdGuardado.Max(a => a.IdPago);


                                TempData["Mensaje"] = "No se logro procesar el pago.";
                                TempData["Tipo"] = "warning";
                                return RedirectToAction("Create");
                            }
                        }
                        // 4️⃣ Mensaje final
                       
                        
                        

                    }
                    else
                    {
                        if (pagos.Pago.IdTipoMovimiento == 2)
                        {
                            int periodoMatricula = 0;
                            if (periodo == pagos.Pago.IdPeriodo)
                            {
                                periodoMatricula = pagos.Pago.IdPeriodo + 1;
                            }
                            else
                            {
                                periodoMatricula = pagos.Pago.IdPeriodo;
                            }
                                decimal restarMensualidad = await ObtenerMensualidadDecimal(pagos.Pago.IdRecinto, pagos.Pago.IdGrado, periodoMatricula);
                                decimal obtenerMat = await ObtenerMatriculaDecimal(pagos.Pago.IdRecinto, pagos.Pago.IdModalidad, periodoMatricula);

                            var totalMatricual = Convert.ToDecimal(pagos.Pago.Monto);
                            if (totalMatricual == obtenerMat)
                            {
                                decimal valormatricula = totalMatricual - restarMensualidad;
                                pagos.Pago.Monto = valormatricula;
                                var validarExistePagoMatricula =  _Iservices.GetPagosAsync().Result.Where(a => a.IdAlumno == pagos.Pago.IdAlumno && a.IdTipoMovimiento==pagos.Pago.IdTipoMovimiento && a.IdPeriodo == periodoMatricula).Count()>0;
                                if(validarExistePagoMatricula)
                                {
                                    TempData["Mensaje"] = "El Pago de la Matricula ya se hizo efectiva.";
                                    TempData["Tipo"] = "waring";
                                    return RedirectToAction("Create", "Pagos");
                                }
                                pagos.Pago.IdPeriodo = periodoMatricula;
                                await _Iservices.PostPagosAsync(pagos.Pago);
                                
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

                                    // otros campos...
                                };
                                response = await _Iservices.PostPagosAsync(pagoprimermes);
                                if (response)
                                {
                                    var idPag = buscarIdGuardado.Max(a => a.IdPago);
                                    TempData["Mensaje"] = "Pago registrado correctamente.";
                                    TempData["Tipo"] = "success";
                                    return RedirectToAction("Details", "Pagos", new { id =idPag +1 });
                                }
                                else
                                {
                                    TempData["Mensaje"] = "No se proceso el Pago.";
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
                                return RedirectToAction("Details", "Pagos", new { id = idPag + 1 });
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
                return NoContent();
            }
            catch (Exception ex)
            {
                // En caso de error, redirigir a Create para que se cargue el modelo correctamente
                TempData["Mensaje"] = "Ocurrió un error al procesar el pago. Por favor, intente nuevamente.";
                TempData["Tipo"] = "warning";
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
        public ActionResult Edit(int id)
        {
            return View();
        }

        // POST: PagosController/Edit/5
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

        // GET: PagosController/Delete/5
        public ActionResult Delete(int id)
        {
            return View();
        }

        // POST: PagosController/Delete/5
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
