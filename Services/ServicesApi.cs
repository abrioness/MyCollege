using System.Diagnostics;
using System.Net.Http;
using System.Net.Security;
//using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Newtonsoft.Json;
using WebColegio.Models;
using WebColegio.Models.ViewModel;

namespace WebColegio.Services
{
    public class ServicesApi:IServicesApi
    {
        private readonly string url;
        private readonly IHttpClientFactory _httpClientFactory;

        public string? LastApiError { get; private set; }

        public ServicesApi(IConfiguration config, IHttpClientFactory httpClientFactory)
        {
            var baseUrl = config["ApiSettings:BaseUrl"];
            url = string.IsNullOrWhiteSpace(baseUrl) ? "" : baseUrl.TrimEnd('/') + "/";
            _httpClientFactory = httpClientFactory;
        }

        private HttpClient CreateApiClient() => _httpClientFactory.CreateClient("ColegioApi");

        private void RegistrarErrorApi(string operacion, HttpResponseMessage response, string? cuerpo = null)
        {
            cuerpo ??= response.Content.ReadAsStringAsync().GetAwaiter().GetResult();
            LastApiError = $"{operacion}: HTTP {(int)response.StatusCode} {response.ReasonPhrase}. {cuerpo}".Trim();
            Debug.WriteLine(LastApiError);
        }

        private void LimpiarErrorApi() => LastApiError = null;
        //Metodo para Listar usuarios
        #region Metodos Get

        public async Task<List<TblAlumno>> GetAlumnosAsync()
        {
            List<TblAlumno> Alumnoslist = new List<TblAlumno>();
            //var handler = new HttpClientHandler();
            //handler.ServerCertificateCustomValidationCallback =
            //    (request, cert, chain, errors) => true;
            using (var httpclient = CreateApiClient())
            {
               
                var response = await httpclient.GetAsync(url + "api/Alumnos");

                if (response.IsSuccessStatusCode)
                {
                    var content = await response.Content.ReadAsStringAsync();
                    var list_alumnos = JsonConvert.DeserializeObject<List<TblAlumno>>(content);

                    Alumnoslist = list_alumnos;
                }
                return Alumnoslist;
            }

        }

        //Get hacia la Api para listar usuarios
        public async Task<List<TblUsuarios>> GetUsuariosAsync()
        {
            List<TblUsuarios> ListUsuarios = new List<TblUsuarios>();
            //var handler = new HttpClientHandler();
            //handler.ServerCertificateCustomValidationCallback =
            //    (request, cert, chain, errors) => true;
            using (var httpclient = CreateApiClient())
            {

                var response = await httpclient.GetAsync(url + "api/Usuarios");

                if (response.IsSuccessStatusCode)
                {
                    var content = await response.Content.ReadAsStringAsync();
                    var list_usuario = JsonConvert.DeserializeObject<List<TblUsuarios>>(content);

                    ListUsuarios = list_usuario;
                }
                return ListUsuarios;
            }


        }
        public async Task<List<Sexos>> GetSexosAsync()
        {
            List<Sexos> Sexoslist = new List<Sexos>();
            using (var httpclient = CreateApiClient())
            {

                var response = await httpclient.GetAsync(url + "api/CatSexo");

                if (response.IsSuccessStatusCode)
                {
                    var content = await response.Content.ReadAsStringAsync();
                    var list_sexos = JsonConvert.DeserializeObject<List<Sexos>>(content);

                    Sexoslist = list_sexos;
                }
                return Sexoslist;
            }


        }
        public async Task<List<Recintos>> GetRecintosAsync()
        {
            List<Recintos> Recintoslist = new List<Recintos>();
            using (var httpclient = CreateApiClient())
            {

                var response = await httpclient.GetAsync(url + "api/CatRecintos");

                if (response.IsSuccessStatusCode)
                {
                    var content = await response.Content.ReadAsStringAsync();
                    var list_recintos = JsonConvert.DeserializeObject<List<Recintos>>(content);

                    Recintoslist = list_recintos;
                }
                return Recintoslist;
            }


        }
        public async Task<List<Modalidades>> GetModalidadesAsync()
        {
            List<Modalidades> Modalidadeslist = new List<Modalidades>();
            using (var httpclient = CreateApiClient())
            {

                var response = await httpclient.GetAsync(url + "api/CatModalidad");

                if (response.IsSuccessStatusCode)
                {
                    var content = await response.Content.ReadAsStringAsync();
                    var list_modalidades = JsonConvert.DeserializeObject<List<Modalidades>>(content);

                    Modalidadeslist = list_modalidades;
                }
                return Modalidadeslist;
            }


        }
        public async Task<List<Grupos>> GetGruposAsync()
        {
            List<Grupos> Gruposlist = new List<Grupos>();
            using (var httpclient = CreateApiClient())
            {

                var response = await httpclient.GetAsync(url + "api/CatGrupos");

                if (response.IsSuccessStatusCode)
                {
                    var content = await response.Content.ReadAsStringAsync();
                    var list_Grupos = JsonConvert.DeserializeObject<List<Grupos>>(content);

                    Gruposlist = list_Grupos;
                }
                return Gruposlist;
            }


        }

        public async Task<List<Turnos>> GetTurnosAsync()
        {
            List<Turnos> Turnoslist = new List<Turnos>();
            using (var httpclient = CreateApiClient())
            {

                var response = await httpclient.GetAsync(url + "api/CatTurnos");

                if (response.IsSuccessStatusCode)
                {
                    var content = await response.Content.ReadAsStringAsync();
                    var list_Turnos = JsonConvert.DeserializeObject<List<Turnos>>(content);

                    Turnoslist = list_Turnos;
                }
                return Turnoslist;
            }


        }
        public async Task<List<Grados>> GetGradosAsync()
        {
            List<Grados> Gradoslist = new List<Grados>();
            using (var httpclient = CreateApiClient())
            {

                var response = await httpclient.GetAsync(url + "api/Grados");

                if (response.IsSuccessStatusCode)
                {
                    var content = await response.Content.ReadAsStringAsync();
                    var list_Grados = JsonConvert.DeserializeObject<List<Grados>>(content);

                    Gradoslist = list_Grados;
                }
                return Gradoslist;
            }


        }
        public async Task<List<TipoEvaluacion>> GetTipEvaluacionAsync()
        {
            List<TipoEvaluacion> tipoEvaluacion = new List<TipoEvaluacion>();
            using (var httpclient = CreateApiClient())
            {

                var response = await httpclient.GetAsync(url + "api/CatTipoEvaluacion");

                if (response.IsSuccessStatusCode)
                {
                    var content = await response.Content.ReadAsStringAsync();
                    var evaluacion = JsonConvert.DeserializeObject<List<TipoEvaluacion>>(content);

                    tipoEvaluacion = evaluacion;
                }
                return tipoEvaluacion;
            }


        }

        public async   Task<List<PeriodoEvaluacion>> GetPeriodoEvaluacionAsync()
        {
            List<PeriodoEvaluacion> periodoEvaluacion = new List<PeriodoEvaluacion>();
            using (var httpclient = CreateApiClient())
            {

                var response = await httpclient.GetAsync(url + "api/CatPeridoEvaluacion");

                if (response.IsSuccessStatusCode)
                {
                    var content = await response.Content.ReadAsStringAsync();
                    var evaluacionPeriodo = JsonConvert.DeserializeObject<List<PeriodoEvaluacion>>(content);

                    periodoEvaluacion = evaluacionPeriodo;
                }
                return periodoEvaluacion;
            }


        }

        public async Task<List<Asignaturas>> GetAsignaturaAsync()
        {
            List<Asignaturas> asignatura = new List<Asignaturas>();
            using (var httpclient = CreateApiClient())
            {

                var response = await httpclient.GetAsync(url + "api/CatAsignaturas");

                if (response.IsSuccessStatusCode)
                {
                    var content = await response.Content.ReadAsStringAsync();
                    var resultado = JsonConvert.DeserializeObject<List<Asignaturas>>(content);

                    asignatura = resultado;
                }
                return asignatura;
            }


        }
        public async Task<List<TblNotas>> GetNotasAsync()
        {
            var notas = new List<TblNotas>();
            using (var httpclient = CreateApiClient())
            {
                if (string.IsNullOrEmpty(url))
                    return notas;
                var response = await httpclient.GetAsync(url + "api/Notas");
                if (response.IsSuccessStatusCode)
                {
                    var content = await response.Content.ReadAsStringAsync();
                    var resultado = JsonConvert.DeserializeObject<List<TblNotas>>(content);
                    notas = resultado ?? new List<TblNotas>();
                }
                else
                {
                    Debug.WriteLine($"GetNotasAsync error: {(int)response.StatusCode} {await response.Content.ReadAsStringAsync()}");
                }
                return notas;
            }


        }
        public async Task<List<FacturaColegiatura>> GetFacturacionAsync()
        {
            List<FacturaColegiatura> facturas = new List<FacturaColegiatura>();
            using (var httpclient = CreateApiClient())
            {

                var response = await httpclient.GetAsync(url + "api/FacturaColegiatura");

                if (response.IsSuccessStatusCode)
                {
                    var content = await response.Content.ReadAsStringAsync();
                    var resultado = JsonConvert.DeserializeObject<List<FacturaColegiatura>>(content);

                    facturas = resultado;
                }
                return facturas;
            }


        }
        public async Task<List<TblEstadoPago>> GetEstadoPagoAsync()
        {
            List<TblEstadoPago> estadoPago = new List<TblEstadoPago>();
            using (var httpclient = CreateApiClient())
            {
                var response = await httpclient.GetAsync(url + "api/EstadoPagos");

                if (response.IsSuccessStatusCode)
                {
                    var content = await response.Content.ReadAsStringAsync();
                    var resultado = JsonConvert.DeserializeObject<List<TblEstadoPago>>(content);

                    estadoPago = resultado;
                }
                return estadoPago;
            }
        }

        public async Task<List<TipoColegiatura>> GetTipoColegiatuuraAsync()
        {
            List<TipoColegiatura> tipoColegiatura = new List<TipoColegiatura>();
            using (var httpclient = CreateApiClient())
            {
                var response = await httpclient.GetAsync(url + "api/CatTipoColegiaturas");

                if (response.IsSuccessStatusCode)
                {
                    var content = await response.Content.ReadAsStringAsync();
                    var resultado = JsonConvert.DeserializeObject<List<TipoColegiatura>>(content);

                    tipoColegiatura = resultado;
                }
                return tipoColegiatura;
            }
        }

        public async Task<List<TblPago>> GetPagosAsync()
        {
            LimpiarErrorApi();
            if (string.IsNullOrWhiteSpace(url))
            {
                LastApiError = "ApiSettings:BaseUrl no está configurada en appsettings.";
                return new List<TblPago>();
            }

            using (var httpclient = CreateApiClient())
            {
                try
                {
                    var response = await httpclient.GetAsync(url + "api/Pagos");
                    if (response.IsSuccessStatusCode)
                    {
                        var content = await response.Content.ReadAsStringAsync();
                        try
                        {
                            var resultado = JsonConvert.DeserializeObject<List<TblPago>>(content);
                            return resultado ?? new List<TblPago>();
                        }
                        catch (JsonException ex)
                        {
                            LastApiError = "DeserializeError";
                            Debug.WriteLine($"Error al deserializar pagos: {ex.Message}");
                            return new List<TblPago>();
                        }
                    }

                    var errorBody = await response.Content.ReadAsStringAsync();
                    RegistrarErrorApi("GET api/Pagos", response, errorBody);
                    if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
                        LastApiError = "Unauthorized";
                    return new List<TblPago>();
                }
                catch (HttpRequestException ex)
                {
                    LastApiError = $"ConnectionError: {ex.Message}";
                    return new List<TblPago>();
                }
                catch (TaskCanceledException ex)
                {
                    LastApiError = $"Timeout: {ex.Message}";
                    return new List<TblPago>();
                }
            }
        }
        public async Task<List<TblPagoCaja>> GetPagoCajaAsync()
        {
            List<TblPagoCaja> pagosCaja = new List<TblPagoCaja>();
            using (var httpclient = CreateApiClient())
            {
                var response = await httpclient.GetAsync(url + "api/TblPagoCajas");
                if (response.IsSuccessStatusCode)
                {
                    var content = await response.Content.ReadAsStringAsync();
                    var resultado = JsonConvert.DeserializeObject<List<TblPagoCaja>>(content);
                    pagosCaja = resultado;
                }
                return pagosCaja;
            }
        }
        public async Task<List<TblEgreso>> GetEgresoAsync()
        {
            List<TblEgreso> egreso = new List<TblEgreso>();
            using (var httpclient = CreateApiClient())
            {
                var response = await httpclient.GetAsync(url + "api/TblEgresos");
                if (response.IsSuccessStatusCode)
                {
                    var content = await response.Content.ReadAsStringAsync();
                    var resultado = JsonConvert.DeserializeObject<List<TblEgreso>>(content);
                    egreso = resultado;
                }
                return egreso;
            }
        }
        //Get de Arqueo Diario
        public async Task<List<TblArqueoDiario>> GetArqueoDiarioAsync()
        {
            List<TblArqueoDiario> arqueoDiario = new List<TblArqueoDiario>();
            using (var httpclient = CreateApiClient())
            {
                var response = await httpclient.GetAsync(url + "api/TblArqueoDiarios");
                if (response.IsSuccessStatusCode)
                {
                    var content = await response.Content.ReadAsStringAsync();
                    var resultado = JsonConvert.DeserializeObject<List<TblArqueoDiario>>(content);
                    arqueoDiario = resultado;
                }
                return arqueoDiario;
            }
        }

        public async Task<List<TblReciboCaja>> GetRecibosCajaAsync()
        {
            List<TblReciboCaja> reciboCajas = new List<TblReciboCaja>();
            using (var httpclient = CreateApiClient())
            {
                var response = await httpclient.GetAsync(url + "api/RecibosCajas");
                if (response.IsSuccessStatusCode)
                {
                    var content = await response.Content.ReadAsStringAsync();
                    var resultado = JsonConvert.DeserializeObject<List<TblReciboCaja>>(content);
                    reciboCajas = resultado;
                }
                return reciboCajas;
            }
        }

        public async Task<List<Productos>> GetProductosAsync()
        {
            List<Productos> productos = new List<Productos>();
            using (var httpclient = CreateApiClient())
            {
                var response = await httpclient.GetAsync(url + "api/TblProductos");
                if (response.IsSuccessStatusCode)
                {
                    var content = await response.Content.ReadAsStringAsync();
                    var resultado = JsonConvert.DeserializeObject<List<Productos>>(content);
                    productos = resultado;
                }
                return productos;
            }

        }

        public async Task<Productos?> GetProductoByIdAsync(int id)
        {
            using (var httpclient = CreateApiClient())
            {
                var response = await httpclient.GetAsync(url + $"api/TblProductos/{id}");
                if (response.IsSuccessStatusCode)
                {
                    var content = await response.Content.ReadAsStringAsync();
                    return JsonConvert.DeserializeObject<Productos>(content);
                }
            }
            return null;
        }

        public async Task<Productos?> GetProductoByCodigoYCategoriaAsync(string codigo, int idCategoria)
        {
            var productos = await GetProductosAsync();
            return productos?.FirstOrDefault(p =>
                string.Equals(p.CodigoBarra, codigo?.Trim(), StringComparison.OrdinalIgnoreCase) &&
                p.IdCateProducto == idCategoria);
        }

        public async Task<List<CatDiscapacidad>> GetDiscapacidadAsync()
        {
            List<CatDiscapacidad> discapacidad = new List<CatDiscapacidad>();
            using (var httpclient = CreateApiClient())
            {
                var response = await httpclient.GetAsync(url + "api/CatDiscapacidad");
                if (response.IsSuccessStatusCode)
                {
                    var content = await response.Content.ReadAsStringAsync();
                    var resultado = JsonConvert.DeserializeObject<List<CatDiscapacidad>>(content);
                    discapacidad = resultado;
                }
                return discapacidad;
            }

        }
        public async Task<List<CatMovInventario>> GetMovInventarioAsync()
        {
            List<CatMovInventario> movInventario = new List<CatMovInventario>();
            using (var httpclient = CreateApiClient())
            {
                var response = await httpclient.GetAsync(url + "api/CatMovimientoInventario");
                if (response.IsSuccessStatusCode)
                {
                    var content = await response.Content.ReadAsStringAsync();
                    var resultado = JsonConvert.DeserializeObject<List<CatMovInventario>>(content);
                    movInventario = resultado;
                }
                return movInventario;
            }

        }
    public async Task<List<MovimientoInventario>> GetMovimientoInventarioAsync()
        {
            List<MovimientoInventario> movimientoInventario = new List<MovimientoInventario>();
            using (var httpclient = CreateApiClient())
            {
                var response = await httpclient.GetAsync(url + "api/MovimientoInventario");
                if (response.IsSuccessStatusCode)
                {
                    var content = await response.Content.ReadAsStringAsync();
                    var resultado = JsonConvert.DeserializeObject<List<MovimientoInventario>>(content);
                    movimientoInventario = resultado;
                }
                return movimientoInventario;
            }

        }
        public async Task<List<CategoriaProducto>> GetCategoriaProductoAsync()
        {
            List<CategoriaProducto> categoriaProducto = new List<CategoriaProducto>();
            using (var httpclient = CreateApiClient())
            {
                var response = await httpclient.GetAsync(url + "api/CategoriaProducto");
                if (response.IsSuccessStatusCode)
                {
                    var content = await response.Content.ReadAsStringAsync();
                    var resultado = JsonConvert.DeserializeObject<List<CategoriaProducto>>(content);
                    categoriaProducto = resultado;
                }
                return categoriaProducto;
            }

        }

        public async Task<List<CatTipoMovimiento>> GetTipoMovimientoAsync()
        {
            List<CatTipoMovimiento> tipoMovimientos = new List<CatTipoMovimiento>();
            using (var httpclient = CreateApiClient())
            {
                var response = await httpclient.GetAsync(url + "api/TipoMovimientos");
                if (response.IsSuccessStatusCode)
                {
                    var content = await response.Content.ReadAsStringAsync();
                    var resultado = JsonConvert.DeserializeObject<List<CatTipoMovimiento>>(content);
                    tipoMovimientos = resultado;
                }
                return tipoMovimientos;
            }

        }
        public async Task<List<CatTipoRecibo>> GetTipoReciboAsync()
        {
            List<CatTipoRecibo> tipoRecibo = new List<CatTipoRecibo>();
            using (var httpclient = CreateApiClient())
            {
                var response = await httpclient.GetAsync(url + "api/TipoRecibos");
                if (response.IsSuccessStatusCode)
                {
                    var content = await response.Content.ReadAsStringAsync();
                    var resultado = JsonConvert.DeserializeObject<List<CatTipoRecibo>>(content);
                    tipoRecibo = resultado;
                }
                return tipoRecibo;
            }

        }
        public async Task<List<CatMetodoPago>> GetMetodoPagoAsync()
        {
            List<CatMetodoPago> metodoPago = new List<CatMetodoPago>();
            using (var httpclient = CreateApiClient())
            {
                var response = await httpclient.GetAsync(url + "api/TblMetodoPago");
                if (response.IsSuccessStatusCode)
                {
                    var content = await response.Content.ReadAsStringAsync();
                    var resultado = JsonConvert.DeserializeObject<List<CatMetodoPago>>(content);
                    metodoPago = resultado;
                }
                return metodoPago;
            }

        }
        public async Task<List<TblCostoMensualidad>> GetCostosMensualidadAsync()
        {
            List<TblCostoMensualidad> costoMensualidad = new List<TblCostoMensualidad>();
            using (var httpclient = CreateApiClient())
            {
                var response = await httpclient.GetAsync(url + "api/TblCostoMensualidad");
                if (response.IsSuccessStatusCode)
                {
                    var content = await response.Content.ReadAsStringAsync();
                    var resultado = JsonConvert.DeserializeObject<List<TblCostoMensualidad>>(content);
                    costoMensualidad = resultado;
                }
                return costoMensualidad;
            }

        }

        public async Task<List<TblCostoMatricula>> GetCostosMatriculaAsync()
        {
            List<TblCostoMatricula> costoMatricula = new List<TblCostoMatricula>();
            try
            {
                using (var httpclient = CreateApiClient())
                {
                    var response = await httpclient.GetAsync(url + "api/TblCostoMatriculas");
                    if (response.IsSuccessStatusCode)
                    {
                        var content = await response.Content.ReadAsStringAsync();

                        // Verificar que el contenido no esté vacío
                        if (!string.IsNullOrWhiteSpace(content))
                        {
                            var resultado = JsonConvert.DeserializeObject<List<TblCostoMatricula>>(content);

                            // Validar que el resultado no sea null antes de asignarlo
                            if (resultado != null)
                            {
                                costoMatricula = resultado;
                            }
                            else
                            {
                                Debug.WriteLine("Error: La deserialización retornó null para TblCostoMatricula");
                            }
                        }
                        else
                        {
                            Debug.WriteLine("Error: El contenido de la respuesta está vacío");
                        }
                    }
                    else
                    {
                        var errorContent = await response.Content.ReadAsStringAsync();
                        Debug.WriteLine($"Error en la respuesta: {response.StatusCode} - {errorContent}");
                    }
                }
            }
            catch (JsonException ex)
            {
                Debug.WriteLine($"Error de deserialización JSON: {ex.Message}");
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Error en GetCostosMatriculaAsync: {ex.Message}");
            }

            return costoMatricula;
        }

        

        public async Task<List<TblCatMeses>> GetMesesAsync()
        {
            List<TblCatMeses> meses = new List<TblCatMeses>();
            using (var httpclient = CreateApiClient())
            {
                var response = await httpclient.GetAsync(url + "api/CatMeses");
                if (response.IsSuccessStatusCode)
                {
                    var content = await response.Content.ReadAsStringAsync();
                    var resultado = JsonConvert.DeserializeObject<List<TblCatMeses>>(content);
                    meses = resultado;
                }
                return meses;
            }

        }

        public async Task<List<CatPeriodo>> GetPeriodoAsync()
        {
            List<CatPeriodo> periodo = new List<CatPeriodo>();
            using (var httpclient = CreateApiClient())
            {
                var response = await httpclient.GetAsync(url + "api/CatPeriodo");
                if (response.IsSuccessStatusCode)
                {
                    var content = await response.Content.ReadAsStringAsync();
                    var resultado = JsonConvert.DeserializeObject<List<CatPeriodo>>(content);
                    periodo = resultado;
                }
                return periodo;
            }

        }
        //public  async Task<List<TblInventario>> GetInventarioAsync()
        //{
        //    List<TblInventario> inventario = new List<TblInventario>();
        //    using (var httpclient = CreateApiClient())
        //    {
        //        var response = await httpclient.GetAsync(url + "api/Inventario");
        //        if (response.IsSuccessStatusCode)
        //        {
        //            var content = await response.Content.ReadAsStringAsync();
        //            var resultado = JsonConvert.DeserializeObject<List<TblInventario>>(content);
        //            inventario = resultado;
        //        }
        //        return inventario;
        //    }

        //}

        public async Task<TblRol> GetRol(int idrol)
        {

            using (var httpClient = CreateApiClient())
            {
                var rol = new TblRol();

                var response = await httpClient.GetAsync(url + $"api/TblRol/"+ idrol);
                if (response.IsSuccessStatusCode)
                {
                    var data = await response.Content.ReadAsStringAsync();
                    var r = JsonConvert.DeserializeObject<TblRol>(data);
                    rol = r;
                }
                return rol;
            }


        }
        public async Task<TblAlumno> V_alumnoNotas(string cedulaTutor)
        {
            var ValumnoNotas = new TblAlumno();
            using (var httpclient = CreateApiClient())
            {

                var response = await httpclient.GetAsync(url + "api/Alumnos/alumnoNota?cedulaTutor="+cedulaTutor);

                if (response.IsSuccessStatusCode)
                {
                    var content = await response.Content.ReadAsStringAsync();
                    var resultado = JsonConvert.DeserializeObject <TblAlumno>(content);

                    ValumnoNotas = resultado;
                }
                return ValumnoNotas;
            }
        }
        //Get de Login
        public async Task<TblUsuarios?> GetLogin(string usuario)
        {
            LimpiarErrorApi();
            if (string.IsNullOrWhiteSpace(usuario))
                return null;

            if (string.IsNullOrWhiteSpace(url))
            {
                LastApiError = "ApiSettings:BaseUrl no configurada.";
                return null;
            }

            using (var httpClient = CreateApiClient())
            {
                try
                {
                    var loginEncoded = Uri.EscapeDataString(usuario.Trim());
                    var requestUrl = url + $"api/Usuarios/obtenerUsuario?login={loginEncoded}";
                    var response = await httpClient.GetAsync(requestUrl);
                    if (!response.IsSuccessStatusCode)
                    {
                        var errorBody = await response.Content.ReadAsStringAsync();
                        RegistrarErrorApi("GET api/Usuarios/obtenerUsuario", response, errorBody);
                        return null;
                    }

                    var data = await response.Content.ReadAsStringAsync();
                    if (string.IsNullOrWhiteSpace(data) || data.Trim() == "null")
                    {
                        LastApiError = "GET api/Usuarios/obtenerUsuario: respuesta vacía.";
                        return null;
                    }

                    try
                    {
                        var respuesta = JsonConvert.DeserializeObject<TblUsuarios>(data);
                        if (respuesta == null || respuesta.IdUsuario <= 0)
                        {
                            LastApiError = "GET api/Usuarios/obtenerUsuario: usuario no encontrado en JSON.";
                            return null;
                        }
                        return respuesta;
                    }
                    catch (JsonException ex)
                    {
                        LastApiError = $"DeserializeError: {ex.Message}";
                        return null;
                    }
                }
                catch (HttpRequestException ex)
                {
                    LastApiError = $"ConnectionError: {ex.Message} (URL: {url})";
                    return null;
                }
                catch (TaskCanceledException ex)
                {
                    LastApiError = $"Timeout: {ex.Message} (URL: {url})";
                    return null;
                }
            }
        }
        public async Task<List<TblRol>> GetRolAsync()
        {

            using (var httpClient = CreateApiClient())
            {
                var rol = new List<TblRol>();
                var response = await httpClient.GetAsync(url + $"api/TblRol/ObtenerRol");
                if (response.IsSuccessStatusCode)
                {
                    var data = await response.Content.ReadAsStringAsync();
                    var respuesta = JsonConvert.DeserializeObject<List<TblRol>>(data);
                    rol = respuesta;
                }
                return rol;
            }
        }


        //Validación 
        public async Task<bool> validarUsuarios(string login, string cedula)//, int idtematica)
        {

            using (var httpClient = CreateApiClient())
            {
                var response = await httpClient.GetAsync(url + $"api/Usuarios/validarUsuario?login={login}&cedula={cedula}");
                if (response.IsSuccessStatusCode)
                {
                    return true;
                }
                return false;
            }
        }

        #endregion
        #region Metodos Post
        public async Task<(bool Exito, string? DetalleError, int? IdAlumnoCreado)> PostAlumnosAsync(TblAlumno alumnos)
        {
            if (alumnos == null)
                return (false, "No se recibieron datos del alumno.", null);

            if (string.IsNullOrWhiteSpace(url))
                return (false, "Falta configurar la URL de la API (ApiSettings:BaseUrl).", null);

            try
            {
                using (var httpClient = CreateApiClient())
                {
                    string jsonAlumnos = JsonConvert.SerializeObject(alumnos);
                    var content = new StringContent(jsonAlumnos, Encoding.UTF8, "application/json");
                    var response = await httpClient.PostAsync(url + "api/Alumnos/Guardar", content);
                    var body = await response.Content.ReadAsStringAsync();

                    if (!response.IsSuccessStatusCode)
                    {
                        var detalle = $"La API respondió {(int)response.StatusCode} {response.ReasonPhrase}. {body}".Trim();
                        Debug.WriteLine("Error POST Alumnos/Guardar: " + detalle);
                        return (false, string.IsNullOrWhiteSpace(body) ? detalle : body.Trim(), null);
                    }

                    int? idCreado = TryParseIdAlumnoFromApiBody(body);
                    return (true, null, idCreado);
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine("Excepción en PostAlumnosAsync: " + ex);
                return (false, ex.Message, null);
            }
        }

        private static int? TryParseIdAlumnoFromApiBody(string body)
        {
            if (string.IsNullOrWhiteSpace(body))
                return null;
            try
            {
                var alumno = JsonConvert.DeserializeObject<TblAlumno>(body);
                if (alumno != null && alumno.IdAlumno > 0)
                    return alumno.IdAlumno;
            }
            catch
            {
                /* ignorar */
            }
            try
            {
                var dyn = JsonConvert.DeserializeObject<Newtonsoft.Json.Linq.JObject>(body);
                var token = dyn?["idAlumno"] ?? dyn?["IdAlumno"];
                if (token != null && int.TryParse(token.ToString(), out var id) && id > 0)
                    return id;
            }
            catch
            {
                /* ignorar */
            }
            try
            {
                if (int.TryParse(body.Trim(), out var soloId) && soloId > 0)
                    return soloId;
            }
            catch
            {
                /* ignorar */
            }
            return null;
        }

        public async Task<(bool Exito, string? Detalle)> PostNotasAsync(TblNotas notas)
        {
            try
            {
                using (var httpClient = CreateApiClient())
                {
                    if (string.IsNullOrEmpty(url))
                        return (false, "Falta configurar ApiSettings:BaseUrl en appsettings (URL de la API).");

                    var jsonNotas = JsonConvert.SerializeObject(notas);
                    var content = new StringContent(jsonNotas, Encoding.UTF8, "application/json");
                    var response = await httpClient.PostAsync(url + "api/Notas/Guardar", content);

                    if (response.IsSuccessStatusCode)
                        return (true, null);

                    var errorBody = await response.Content.ReadAsStringAsync();
                    var detalle = $"HTTP {(int)response.StatusCode} {response.ReasonPhrase}. {errorBody}".Trim();
                    Debug.WriteLine("Error POST Notas/Guardar: " + detalle);
                    return (false, string.IsNullOrWhiteSpace(errorBody) ? detalle : errorBody);
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine("Excepción en PostNotasAsync: " + ex);
                return (false, ex.Message);
            }
        }

        public async Task<(bool Exito, int IdPagoCreado, string? DetalleError)> PostPagosAsync(TblPago pagos)
        {
            try
            {
                using (var httpClient = CreateApiClient())
                {
                    string jsonPagos = JsonConvert.SerializeObject(pagos);
                    var content = new StringContent(jsonPagos, Encoding.UTF8, "application/json");
                    var response = await httpClient.PostAsync(url + "api/Pagos", content);
                    var body = await response.Content.ReadAsStringAsync();

                    if (response.IsSuccessStatusCode)
                    {
                        int idCreado = 0;
                        try
                        {
                            var creado = JsonConvert.DeserializeObject<TblPago>(body);
                            idCreado = creado?.IdPago ?? 0;
                        }
                        catch
                        {
                            // El pago se guardó; el cuerpo puede no deserializarse por diferencias de modelo.
                        }
                        return (true, idCreado, null);
                    }

                    Debug.WriteLine("Error en POST Pagos: " + body);
                    return (false, 0, string.IsNullOrWhiteSpace(body) ? response.ReasonPhrase : body);
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine("Excepción en PostPagosAsync: " + ex.Message);
                return (false, 0, ex.Message);
            }
        }
        //Pago de Caja
        public async Task<bool> PostPagosCajaAsync(TblPagoCaja pagosCaja)
        {

            bool respuesta = false;

            // Asegurar datos mínimos requeridos
            //pagos.Activo = true;
            //pagos.UsuarioRegistro = 1;
            //pagos.FechaRegistro = DateTime.Now;

            try
            {
                using (var httpClient = CreateApiClient())
                {
                    // Serializar el objeto alumno
                    string jsonPagosCaja = JsonConvert.SerializeObject(pagosCaja);
                    var content = new StringContent(jsonPagosCaja, Encoding.UTF8, "application/json");

                    // Enviar POST
                    var response = await httpClient.PostAsync(url + "api/TblPagoCajas", content);

                    if (response.IsSuccessStatusCode)
                    {
                        respuesta = true;
                    }
                    else
                    {
                        // Para debug: mostrar mensaje de error
                        var errorMsg = await response.Content.ReadAsStringAsync();
                        Debug.WriteLine("Error en POST: " + errorMsg);
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine("Excepción en PostPagosCajaAsync: " + ex.Message);
            }

            return respuesta;
        }


        public async Task<bool> PostEgresoAsync(TblEgreso egresos)
        {

            bool respuesta = false;

            // Asegurar datos mínimos requeridos
            //pagos.Activo = true;
            //pagos.UsuarioRegistro = 1;
            //pagos.FechaRegistro = DateTime.Now;

            try
            {
                using (var httpClient = CreateApiClient())
                {
                    // Serializar el objeto alumno
                    string jsonEgreso = JsonConvert.SerializeObject(egresos);
                    var content = new StringContent(jsonEgreso, Encoding.UTF8, "application/json");

                    // Enviar POST
                    var response = await httpClient.PostAsync(url + "api/TblEgresos", content);

                    if (response.IsSuccessStatusCode)
                    {
                        respuesta = true;
                    }
                    else
                    {
                        // Para debug: mostrar mensaje de error
                        var errorMsg = await response.Content.ReadAsStringAsync();
                        Debug.WriteLine("Error en POST: " + errorMsg);
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine("Excepción en PostEgresoAsync: " + ex.Message);
            }

            return respuesta;
        }

        //POST de FACTURACION
        public async Task<bool> PostFacturacionAsync(FacturaColegiatura factura)
        {
            bool respuesta = false;

            // Asegurar datos mínimos requer
            factura.Activo = true;
            factura.UsuarioRegistro = 1;
            factura.FechaRegistro = DateTime.Now;
            

            try
            {
                using (var httpClient = CreateApiClient())
                {
                    // Serializar el objeto alumno
                    string jsonNotas = JsonConvert.SerializeObject(factura);
                    var content = new StringContent(jsonNotas, Encoding.UTF8, "application/json");

                    // Enviar POST
                    var response = await httpClient.PostAsync(url + "api/FacturaColegiatura", content);

                    if (response.IsSuccessStatusCode)
                    {
                        respuesta = true;
                    }
                    else
                    {
                        // Para debug: mostrar mensaje de error
                        var errorMsg = await response.Content.ReadAsStringAsync();
                        Debug.WriteLine("Error en POST: " + errorMsg);
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine("Excepción en PostAlumnosAsync: " + ex.Message);
            }

            return respuesta;
        }
        //Post de Recibo de Caja
        public async Task<bool> PostReciboCajaAsync(TblReciboCaja recibo)
        {
            bool respuesta = false;
            // Asegurar datos mínimos requer
            //List<TblReciboCaja> reciboslist = await GetRecibosCajaAsync();
            //var ultimoNumero = reciboslist
            //.Where(r => r.Serie == "A")
            //.Max(r => (int?)r.NumeroRecibo) ?? 10000;

            //var nuevoNumero = ultimoNumero + 1;
            //if(nuevoNumero==null)
            //{
            //    nuevoNumero = 10000 + 1;
            //}
            //recibo.NumeroRecibo = nuevoNumero;
            recibo.Activo = true;
            recibo.UsuarioRegistro = 1;
            recibo.FechaRegistro = recibo.FechaRegistro;
            try
            {
                using (var httpClient = CreateApiClient())
                {
                    // Serializar el objeto alumno
                    string jsonNotas = JsonConvert.SerializeObject(recibo);
                    var content = new StringContent(jsonNotas, Encoding.UTF8, "application/json");

                    // Enviar POST
                    var response = await httpClient.PostAsync(url + "api/RecibosCajas   ", content);

                    if (response.IsSuccessStatusCode)
                    {
                        respuesta = true;
                    }
                    else
                    {
                        // Para debug: mostrar mensaje de error
                        var errorMsg = await response.Content.ReadAsStringAsync();
                        Debug.WriteLine("Error en POST: " + errorMsg);
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine("Excepción en PostAlumnosAsync: " + ex.Message);
            }

            return respuesta;
        }

        //public async Task<bool> PostInventarioAsync(TblInventario inventario)
        //{
        //    bool respuesta = false;

        //    // Asegurar datos mínimos requeridos
        //    inventario.Activo = true;
        //    inventario.UsuarioRegistro = 1;
        //    inventario.FechaRegistro = DateTime.Now;

        //    try
        //    {
        //        using (var httpClient = CreateApiClient())
        //        {
        //            // Serializar el objeto alumno
        //            string jsonInventario = JsonConvert.SerializeObject(inventario);
        //            var content = new StringContent(jsonInventario, Encoding.UTF8, "application/json");

        //            // Enviar POST
        //            var response = await httpClient.PostAsync(url + "api/Inventario", content);

        //            if (response.IsSuccessStatusCode)
        //            {
        //                respuesta = true;
        //            }
        //            else
        //            {
        //                // Para debug: mostrar mensaje de error
        //                var errorMsg = await response.Content.ReadAsStringAsync();
        //                Debug.WriteLine("Error en POST: " + errorMsg);
        //            }
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        Debug.WriteLine("Excepción en PostInventarioAsync: " + ex.Message);
        //    }

        //    return respuesta;
        //}

        public async Task<bool> PostProductosAsync(Productos product)
        {
            bool respuesta = false;

            // Asegurar datos mínimos requeridos
           

            try
            {
                using (var httpClient = CreateApiClient())
                {
                    // Serializar el objeto alumno
                    string jsonProduct = JsonConvert.SerializeObject(product);
                    var content = new StringContent(jsonProduct, Encoding.UTF8, "application/json");

                    // Enviar POST
                    var response = await httpClient.PostAsync(url + "api/TblProductos", content);

                    if (response.IsSuccessStatusCode)
                    {
                        respuesta = true;
                    }
                    else
                    {
                        // Para debug: mostrar mensaje de error
                        var errorMsg = await response.Content.ReadAsStringAsync();
                        Debug.WriteLine("Error en POST: " + errorMsg);
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine("Excepción en PostProductosAsync: " + ex.Message);
            }

            return respuesta;
        }

        /// <summary>Actualiza un producto vía API PUT, fusionando con el registro actual y conservando usuario/fecha de registro.</summary>
        public async Task<(bool Exito, string? DetalleError)> UpdateProductoAsync(Productos producto)
        {
            if (producto == null || producto.IdProducto <= 0)
                return (false, "Producto no válido.");

            if (string.IsNullOrWhiteSpace(url))
                return (false, "Falta configurar la URL de la API (ApiSettings:BaseUrl).");

            var existente = await GetProductoByIdAsync(producto.IdProducto);
            if (existente == null || existente.IdProducto <= 0)
                return (false, "No se encontró el producto en la API.");

            var usuarioRegistro = existente.UsuarioRegistro;
            var fechaRegistro = existente.FechaRegistro;

            existente.NombreProducto = producto.NombreProducto;
            existente.CodigoBarra = producto.CodigoBarra;
            existente.Descripcion = producto.Descripcion;
            existente.ExistenciaInicial = producto.ExistenciaInicial;
            existente.IdMovInventario = producto.IdMovInventario;
            existente.IdCateProducto = producto.IdCateProducto;
            existente.IdProveedor = producto.IdProveedor;
            existente.IdRecinto = producto.IdRecinto;
            existente.CostoUnitario = producto.CostoUnitario;
            existente.StockActual = producto.StockActual;
            existente.StockMinimo = producto.StockMinimo;
            existente.Activo = producto.Activo;
            existente.ImporteInventario = producto.StockActual * producto.CostoUnitario;

            existente.UsuarioActualiza = producto.UsuarioActualiza;
            existente.FechaActualiza = producto.FechaActualiza;

            existente.UsuarioRegistro = usuarioRegistro;
            existente.FechaRegistro = fechaRegistro;

            try
            {
                using (var httpClient = CreateApiClient())
                {
                    var json = JsonConvert.SerializeObject(existente);
                    var content = new StringContent(json, Encoding.UTF8, "application/json");
                    var response = await httpClient.PutAsync(url + $"api/TblProductos/{existente.IdProducto}", content);
                    if (response.IsSuccessStatusCode)
                        return (true, null);

                    var errorMsg = await response.Content.ReadAsStringAsync();
                    Debug.WriteLine("Error en PUT Producto: " + errorMsg);
                    var detalle = $"La API respondió {(int)response.StatusCode} {response.ReasonPhrase}. {errorMsg}".Trim();
                    return (false, string.IsNullOrWhiteSpace(errorMsg) ? detalle : errorMsg.Trim());
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine("Excepción en UpdateProductoAsync: " + ex.Message);
                return (false, ex.Message);
            }
        }

        /// <summary>Alias descriptivo; delega en <see cref="UpdateProductoAsync"/>.</summary>
        public Task<(bool Exito, string? DetalleError)> UpdateProductosAsync(Productos producto) => UpdateProductoAsync(producto);

        public async Task<bool> PostMovimientoInventarioAsync(MovimientoInventario movimiento)
        {
            try
            {
                using (var httpClient = CreateApiClient())
                {
                    var json = JsonConvert.SerializeObject(movimiento);
                    var content = new StringContent(json, Encoding.UTF8, "application/json");
                    var response = await httpClient.PostAsync(url + "api/Inventario", content);
                    return response.IsSuccessStatusCode;
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine("Excepción en PostMovimientoInventarioAsync: " + ex.Message);
                return false;
            }
        }

        public async Task<List<MovimientoInventario>> GetMovimientosInventarioAsync(int? idProducto, DateTime? desde, DateTime? hasta)
        {
            var list = new List<MovimientoInventario>();
            try
            {
                var query = new List<string>();
                if (idProducto.HasValue) query.Add($"idProducto={idProducto.Value}");
                if (desde.HasValue) query.Add($"desde={desde.Value:yyyy-MM-dd}");
                if (hasta.HasValue) query.Add($"hasta={hasta.Value:yyyy-MM-dd}");
                var qs = query.Count > 0 ? "?" + string.Join("&", query) : "";
                using (var httpclient = CreateApiClient())
                {
                    var response = await httpclient.GetAsync(url + "api/Inventario" + qs);
                    if (response.IsSuccessStatusCode)
                    {
                        var content = await response.Content.ReadAsStringAsync();
                        var resultado = JsonConvert.DeserializeObject<List<MovimientoInventario>>(content);
                        if (resultado != null) list = resultado;
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine("Excepción en GetMovimientosInventarioAsync: " + ex.Message);
            }
            return list;
        }

        public async Task<(bool Ok, string? ErrorMessage)> PostArqueoDiarioAsync(TblArqueoDiario arqueo)
        {
            try
            {
                using (var httpClient = CreateApiClient())
                {
                    string jsonArqueo = JsonConvert.SerializeObject(arqueo);
                    var content = new StringContent(jsonArqueo, Encoding.UTF8, "application/json");

                    var response = await httpClient.PostAsync(url + "api/TblArqueoDiarios", content);

                    if (response.IsSuccessStatusCode)
                        return (true, null);

                    var errorMsg = await response.Content.ReadAsStringAsync();
                    Debug.WriteLine("Error en POST TblArqueoDiarios: " + (int)response.StatusCode + " " + errorMsg);
                    var brief = string.IsNullOrWhiteSpace(errorMsg)
                        ? $"HTTP {(int)response.StatusCode}"
                        : (errorMsg.Length > 500 ? errorMsg.Substring(0, 500) + "…" : errorMsg);
                    return (false, brief);
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine("Excepción en PostArqueoAsync: " + ex.Message);
                return (false, ex.Message);
            }
        }
        //Post registro de usuarios
        public async Task<bool> PostUsuarios(TblUsuarios usuario)
        {
            if (usuario == null)
            {
                return false;
            }
            using (var httpClient = CreateApiClient())
            {
                var content = JsonContent.Create(usuario);
                var guardarUsuario = await httpClient.PostAsync(url + "api/Usuarios/Guardar", content);
                if (guardarUsuario.IsSuccessStatusCode)
                {
                    return true;
                }
                return false;
            }

        }
        #endregion

        #region Metodos Get por Id
        public async Task<TblAlumno> GetAlumnoIdAsync(int id)
        {
            var alumno = new TblAlumno();
            using (var httpclient = CreateApiClient())
            {

                var response = await httpclient.GetAsync(url + "api/Alumnos/"+id);

                if (response.IsSuccessStatusCode)
                {
                    var content = await response.Content.ReadAsStringAsync();
                    var resultado = JsonConvert.DeserializeObject<TblAlumno>(content);

                    alumno = resultado;
                }
                return alumno;
            }


        }
        public async Task<TblNotas> GetNotasById(int id)
        {
            var notas = new TblNotas();
            using (var httpclient = CreateApiClient())
            {

                var response = await httpclient.GetAsync(url + "api/Notas/" + id);

                if (response.IsSuccessStatusCode)
                {
                    var content = await response.Content.ReadAsStringAsync();
                    var resultado = JsonConvert.DeserializeObject<TblNotas>(content);

                    notas = resultado;
                }
                return notas;
            }


        }
        public async Task<List<TblNotas>> GetNotasPorUsuario(string tutor)
        {
            List<TblNotas> notasTutor = new List<TblNotas>();
            using (var httpclient = CreateApiClient())
            {

                var response = await httpclient.GetAsync(url + $"api/Notas/GetNotaTutor?cedula={tutor}"); //&idalumno={idalumno}");

                if (response.IsSuccessStatusCode)
                {
                    var content = await response.Content.ReadAsStringAsync();
                    var resultado = JsonConvert.DeserializeObject<List<TblNotas>>(content);

                    notasTutor = resultado;
                }
                return notasTutor;
            }


        }


        public async Task<List<TblNotas>> GetNotasAlumnoById(int idAlumno)
        {
            List<TblNotas> notas = new List<TblNotas>();
            using (var httpclient = CreateApiClient())
            {

                var response = await httpclient.GetAsync(url + "api/Notas/GetNotasAlumno/" + idAlumno);

                if (response.IsSuccessStatusCode)
                {
                    var content = await response.Content.ReadAsStringAsync();
                    var resultado = JsonConvert.DeserializeObject<List<TblNotas>>(content);

                    notas = resultado;
                }
                return notas;
            }


        }

        //Get de Arqueo de Caja
        public async Task<ArqueoDiarioViewModel> GetArqueoById(int id)
        {
            // Suponiendo que tu API tiene un endpoint como:
            // GET https://tuservidor/api/arqueo/{id}
            using (var httpclient = CreateApiClient())
            {

                var response = await httpclient.GetAsync(url+$"api/arqueo/"+id);

                if (!response.IsSuccessStatusCode)
                {
                    throw new Exception($"Error al obtener el arqueo: {response.StatusCode}");
                }

                var json = await response.Content.ReadAsStringAsync();

                // Usar Newtonsoft.Json o System.Text.Json para deserializar
                var arqueo = JsonConvert.DeserializeObject<ArqueoDiarioViewModel>(json);

                return arqueo!;
            }
        }
        public async Task<TblPago> GetPagoById(int id)
        {
            // Suponiendo que tu API tiene un endpoint como:
            // GET https://tuservidor/api/arqueo/{id}
            using (var httpclient = CreateApiClient())
            {

                var response = await httpclient.GetAsync(url+$"api/Pagos/"+id);

                if (!response.IsSuccessStatusCode)
                {
                    throw new Exception($"Error al obtener el arqueo: {response.StatusCode}");
                }

                var json = await response.Content.ReadAsStringAsync();

                // Usar Newtonsoft.Json o System.Text.Json para deserializar
                var pago = JsonConvert.DeserializeObject<TblPago>(json);

                return pago!;
            }
        }
        public async Task<TblPagoCaja> GetPagoCajaById(int id)
        {
            // Suponiendo que tu API tiene un endpoint como:
            // GET https://tuservidor/api/arqueo/{id}
           
            using (var httpclient = CreateApiClient())
            {

                var response = await httpclient.GetAsync(url + $"api/TblPagoCajas/" + id);

                if (!response.IsSuccessStatusCode)
                {
                    throw new Exception($"Error al obtener el recibo de caja: {response.StatusCode}");
                }

                var json = await response.Content.ReadAsStringAsync();

                // Usar Newtonsoft.Json o System.Text.Json para deserializar
                var pagoCaja = JsonConvert.DeserializeObject<TblPagoCaja>(json);

                return pagoCaja!;
            }
        }
        public async Task<TblEgreso> GetEgresoCajaById(int id)
        {
            // Suponiendo que tu API tiene un endpoint como:
            // GET https://tuservidor/api/arqueo/{id}

            using (var httpclient = CreateApiClient())
            {

                var response = await httpclient.GetAsync(url + $"api/TblEgresos/" + id);

                if (!response.IsSuccessStatusCode)
                {
                    throw new Exception($"Error al obtener el recibo de egreso: {response.StatusCode}");
                }

                var json = await response.Content.ReadAsStringAsync();

                // Usar Newtonsoft.Json o System.Text.Json para deserializar
                var egreso = JsonConvert.DeserializeObject<TblEgreso>(json);

                return egreso!;
            }
        }
        public async Task<TblReciboCaja> GetReciboCajaById(int id)
        {
            
            // Suponiendo que tu API tiene un endpoint como:
            // GET https://tuservidor/api/arqueo/{id}
            using (var httpclient = CreateApiClient())
            {

                var response = await httpclient.GetAsync(url + $"api/RecibosCajas/"+id);

                if (!response.IsSuccessStatusCode)
                {
                    throw new Exception($"Error al obtener el recibo: {response.StatusCode}");
                }

                var json = await response.Content.ReadAsStringAsync();

                // Usar Newtonsoft.Json o System.Text.Json para deserializar
                var recibocaja = JsonConvert.DeserializeObject<TblReciboCaja>(json);

                return recibocaja!;
            }
        }
        public async Task<TblUsuarios> GetUsuarioIdAsync(int idUser)
        {

            // Suponiendo que tu API tiene un endpoint como:
            // GET https://tuservidor/api/arqueo/{id}
            using (var httpclient = CreateApiClient())
            {

                var response = await httpclient.GetAsync(url + $"api/Usuarios/" + idUser);

                if (!response.IsSuccessStatusCode)
                {
                    throw new Exception($"Error al obtener el usuario: {response.StatusCode}");
                }

                var json = await response.Content.ReadAsStringAsync();

                // Usar Newtonsoft.Json o System.Text.Json para deserializar
                var idUsuario = JsonConvert.DeserializeObject<TblUsuarios>(json);

                return idUsuario!;
            }
        }



        #endregion
        #region Metodos de Busqueda
        public async Task<List<TblAlumno>> searchAlumnosAsync()
        {
            List<TblAlumno> resultado = new List<TblAlumno>();
            using (var httpclient = CreateApiClient())
            {

                var response = await httpclient.GetAsync(url + "api/Alumnos/buscar");

                if (response.IsSuccessStatusCode)
                {
                    var content = await response.Content.ReadAsStringAsync();
                    var alumno = JsonConvert.DeserializeObject<List<TblAlumno>>(content);

                    resultado = alumno;
                }
                return resultado;
            }

            
        }
        #endregion

        #region Metodos de Put o Editar
        public async Task<bool> UpdateNotas(TblNotas nota)
        {
            var existingNota = await GetNotasById(nota.IdNota);
            if (existingNota == null || existingNota.IdNota <= 0)
                return false;

            existingNota.IdTipoEvaluacion = nota.IdTipoEvaluacion;
            existingNota.IdPeriodo = nota.IdPeriodo;
            existingNota.IdAlumno = nota.IdAlumno;
            existingNota.IdAsignatura = nota.IdAsignatura;
            existingNota.IdModalidad = nota.IdModalidad;
            existingNota.IdGrado = nota.IdGrado;
            existingNota.IdColegio = nota.IdColegio;
            existingNota.Descripcion = nota.Descripcion;
            existingNota.Acumulado1 = nota.Acumulado1;
            existingNota.Examen1 = nota.Examen1;
            existingNota.PrimerCorteCualitativo = nota.PrimerCorteCualitativo;
            existingNota.PrimerCorteCuantitativo = nota.PrimerCorteCuantitativo;
            existingNota.Acumulado2 = nota.Acumulado2;
            existingNota.Examen2 = nota.Examen2;
            existingNota.SegundoCorteCualitativo = nota.SegundoCorteCualitativo;
            existingNota.SegundoCorteCuantitativo = nota.SegundoCorteCuantitativo;
            existingNota.Acumulado3 = nota.Acumulado3;
            existingNota.Examen3 = nota.Examen3;
            existingNota.TercerCorteCualitativo = nota.TercerCorteCualitativo;
            existingNota.TercerCorteCuantitativo = nota.TercerCorteCuantitativo;
            existingNota.Acumulado4 = nota.Acumulado4;
            existingNota.Examen4 = nota.Examen4;
            existingNota.CuartoCorteCualitativo = nota.CuartoCorteCualitativo;
            existingNota.CuartoCorteCuantitativo = nota.CuartoCorteCuantitativo;
            existingNota.NotaFinalCualitativo = nota.NotaFinalCualitativo;
            existingNota.NotaFinalCuantitativo = nota.NotaFinalCuantitativo;
            existingNota.Activo = true;
            existingNota.UsuarioActualiza = nota.UsuarioActualiza is int u && u > 0 ? u : 1;
            existingNota.FechaActualiza = DateTime.Now;
            
            using (var httpClient = CreateApiClient())
            {
                // Convertimos el objeto a JSON
                var json = JsonConvert.SerializeObject(existingNota);
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                // Realizamos la solicitud PUT
                var response = await httpClient.PutAsync(url + $"api/Notas/{existingNota.IdNota}", content);

                return response.IsSuccessStatusCode;
            }

        }
        //Update para el registro de alumnos
        public async Task<bool> UpdateAlumnos(TblAlumno alumno)
        {
            if (alumno == null || alumno.IdAlumno <= 0)
                return false;

            var existingAlumno = await GetAlumnoIdAsync(alumno.IdAlumno);
            if (existingAlumno == null || existingAlumno.IdAlumno <= 0)
                return false;

            var usuarioRegistro = existingAlumno.UsuarioRegistro;
            var fechaRegistro = existingAlumno.FechaRegistro;

            // Datos personales e identificación
            existingAlumno.Nombre = alumno.Nombre;
            existingAlumno.Apellido = alumno.Apellido;
            existingAlumno.PartidaNacimiento = alumno.PartidaNacimiento;
            existingAlumno.FechaNacimiento = alumno.FechaNacimiento;
            existingAlumno.Edad = alumno.Edad;
            existingAlumno.CodigoAlumno = alumno.CodigoAlumno;
            existingAlumno.CodigoMINED = alumno.CodigoMINED;
            existingAlumno.CodigoUnico = alumno.CodigoUnico;
            existingAlumno.Cedula = alumno.Cedula;
            existingAlumno.GrupoEtnico = alumno.GrupoEtnico;
            existingAlumno.BecaCompleta = alumno.BecaCompleta;
            existingAlumno.MediaBeca = alumno.MediaBeca;

            // Académico / institucional
            existingAlumno.IdGrado = alumno.IdGrado;
            existingAlumno.IdTurno = alumno.IdTurno;
            existingAlumno.IdModalidad = alumno.IdModalidad;
            existingAlumno.IdRecinto = alumno.IdRecinto;
            existingAlumno.IdSexo = alumno.IdSexo;
            existingAlumno.IdGrupo = alumno.IdGrupo;
            if (alumno.IdPeriodo.HasValue && alumno.IdPeriodo.Value > 0)
                existingAlumno.IdPeriodo = alumno.IdPeriodo;

            // Ubicación y contacto
            existingAlumno.Direccion = alumno.Direccion;
            existingAlumno.Barrio = alumno.Barrio;
            existingAlumno.Telefono = alumno.Telefono;
            existingAlumno.Correo = alumno.Correo;
            existingAlumno.Departamento = alumno.Departamento;
            existingAlumno.Municipio = alumno.Municipio;

            // Padres / tutor
            existingAlumno.NombreMadre = alumno.NombreMadre;
            existingAlumno.CedulaMadre = alumno.CedulaMadre;
            existingAlumno.TelefonoMadre = alumno.TelefonoMadre;
            existingAlumno.NombrePadre = alumno.NombrePadre;
            existingAlumno.CedulaPadre = alumno.CedulaPadre;
            existingAlumno.TelefonoPadre = alumno.TelefonoPadre;
            existingAlumno.NombreTutor = alumno.NombreTutor;
            existingAlumno.CedulaTutor = alumno.CedulaTutor;
            existingAlumno.ContactoTutor = alumno.ContactoTutor;

            existingAlumno.Observaciones = alumno.Observaciones;
            existingAlumno.Peso = alumno.Peso;
            existingAlumno.Talla = alumno.Talla;
            existingAlumno.TipoEstudiante = alumno.TipoEstudiante;
            existingAlumno.Repitente = alumno.Repitente;
            existingAlumno.RecibioEducacionPreescolar = alumno.RecibioEducacionPreescolar;
            existingAlumno.IdDiscapacidad = alumno.IdDiscapacidad;
            existingAlumno.Activo = alumno.Activo;

            existingAlumno.UsuarioActualiza = alumno.UsuarioActualiza;
            existingAlumno.FechaActualiza = alumno.FechaActualiza;

            existingAlumno.UsuarioRegistro = usuarioRegistro;
            existingAlumno.FechaRegistro = fechaRegistro;

            using (var httpClient = CreateApiClient())
            {
                // Convertimos el objeto a JSON
                var json = JsonConvert.SerializeObject(existingAlumno);
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                // Realizamos la solicitud PUT
                var response = await httpClient.PutAsync(url + $"api/Alumnos/{existingAlumno.IdAlumno}", content);

                return response.IsSuccessStatusCode;
            }

        }
        public async Task<bool> UpdateUsuario(TblUsuarios usuario)
        {
            if (usuario == null || usuario.IdUsuario <= 0)
                return false;

            TblUsuarios usuariosUpdate;
            try
            {
                usuariosUpdate = await GetUsuarioIdAsync(usuario.IdUsuario);
            }
            catch (Exception ex)
            {
                Debug.WriteLine("UpdateUsuario: error al obtener usuario " + ex.Message);
                return false;
            }

            if (usuariosUpdate == null || usuariosUpdate.IdUsuario <= 0)
                return false;

            usuariosUpdate.IdRol = usuario.IdRol;
            usuariosUpdate.NombreCompleto = usuario.NombreCompleto ?? usuariosUpdate.NombreCompleto;
            usuariosUpdate.NombreUsuario = usuario.NombreUsuario ?? usuariosUpdate.NombreUsuario;
            usuariosUpdate.Cedula = usuario.Cedula;
            usuariosUpdate.Correo = usuario.Correo;
            usuariosUpdate.IdRecinto = usuario.IdRecinto;
            usuariosUpdate.Activo = usuario.Activo;
            usuariosUpdate.Bloqueo = usuario.Bloqueo;

            // Solo cambiar contraseña si el POST envió hash nuevo (bytes no vacíos)
            if (usuario.Password != null && usuario.Password.Length > 0)
                usuariosUpdate.Password = usuario.Password;

            usuariosUpdate.UsuarioActualiza = usuario.UsuarioActualiza;
            usuariosUpdate.FechaActualiza = usuario.FechaActualiza ?? DateTime.Now;

            using (var httpClient = CreateApiClient())
            {
                var json = JsonConvert.SerializeObject(usuariosUpdate);
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                var response = await httpClient.PutAsync(url + $"api/Usuarios/Actualizar?id={usuariosUpdate.IdUsuario}", content);

                return response.IsSuccessStatusCode;
            }

        }
        
        // Update para el registro de pagos
        public async Task<bool> UpdatePago(TblPago pago)
        {
            bool respuesta = false;
            
            try
            {
                var existingPago = await GetPagoById(pago.IdPago);
                
                if (existingPago == null)
                {
                    Debug.WriteLine("Pago no encontrado para actualizar: " + pago.IdPago);
                    return false;
                }
                
                // Actualizamos campos editables
                existingPago.IdAlumno = pago.IdAlumno;
                existingPago.Monto = pago.Monto;
                existingPago.NumeroRecibo = pago.NumeroRecibo;
                existingPago.Anyo = pago.Anyo;
                existingPago.IdMes = pago.IdMes;
                existingPago.FechaEmision = pago.FechaEmision;
                existingPago.Mora = pago.Mora;
                existingPago.TotalPagar = pago.TotalPagar;
                existingPago.Descripcion = pago.Descripcion;
                existingPago.IdMetodoPago = pago.IdMetodoPago;
                existingPago.IdTipoMovimiento = pago.IdTipoMovimiento;
                existingPago.IdTipoRecibo = pago.IdTipoRecibo;
                existingPago.IdRecinto = pago.IdRecinto;
                existingPago.IdPeriodo = pago.IdPeriodo;
                existingPago.IdGrado = pago.IdGrado;
                existingPago.IdModalidad = pago.IdModalidad;
                existingPago.Activo = pago.Activo;
                existingPago.UsuarioActualizo = pago.UsuarioActualizo;
                existingPago.FechaActualizo = pago.FechaActualizo;
                
                // Preservar campos que no deben cambiar
                // existingPago.UsuarioRegistro se mantiene
                // existingPago.FechaRegistro se mantiene
                // existingPago.Serie se mantiene

                using (var httpClient = CreateApiClient())
                {
                    // Convertimos el objeto a JSON
                    var json = JsonConvert.SerializeObject(existingPago);
                    var content = new StringContent(json, Encoding.UTF8, "application/json");

                    // Realizamos la solicitud PUT
                    // Ajusta la URL según tu endpoint de API si es diferente
                    var response = await httpClient.PutAsync(url + $"api/Pagos/{existingPago.IdPago}", content);

                    if (response.IsSuccessStatusCode)
                    {
                        respuesta = true;
                    }
                    else
                    {
                        // Para debug: mostrar mensaje de error
                        var errorMsg = await response.Content.ReadAsStringAsync();
                        Debug.WriteLine("Error en PUT Pago: " + errorMsg);
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine("Excepción en UpdatePago: " + ex.Message);
            }

            return respuesta;
        }

        public async Task<bool> UpdateEgreso(TblEgreso egreso)
        {
            try
            {
                var existing = await GetEgresoCajaById(egreso.IdEgreso);
                if (existing == null)
                {
                    Debug.WriteLine("Egreso no encontrado para actualizar: " + egreso.IdEgreso);
                    return false;
                }

                existing.Activo = egreso.Activo;
                existing.UsuarioActualizo = egreso.UsuarioActualizo;
                existing.FechaActualizo = egreso.FechaActualizo;

                using var httpClient = CreateApiClient();
                var json = JsonConvert.SerializeObject(existing);
                var content = new StringContent(json, Encoding.UTF8, "application/json");
                var response = await httpClient.PutAsync(url + $"api/TblEgresos/{existing.IdEgreso}", content);

                if (!response.IsSuccessStatusCode)
                {
                    var errorMsg = await response.Content.ReadAsStringAsync();
                    Debug.WriteLine("Error en PUT Egreso: " + errorMsg);
                }

                return response.IsSuccessStatusCode;
            }
            catch (Exception ex)
            {
                Debug.WriteLine("Excepción en UpdateEgreso: " + ex.Message);
                return false;
            }
        }

        public async Task<bool> UpdatePagoCaja(TblPagoCaja pagoCaja)
        {
            try
            {
                var existing = await GetPagoCajaById(pagoCaja.IdPagoCaja);
                if (existing == null)
                {
                    Debug.WriteLine("Pago caja no encontrado para actualizar: " + pagoCaja.IdPagoCaja);
                    return false;
                }

                existing.Activo = pagoCaja.Activo;
                existing.UsuarioActualizo = pagoCaja.UsuarioActualizo;
                existing.FechaActualizo = pagoCaja.FechaActualizo;

                using var httpClient = CreateApiClient();
                var json = JsonConvert.SerializeObject(existing);
                var content = new StringContent(json, Encoding.UTF8, "application/json");
                var response = await httpClient.PutAsync(url + $"api/TblPagoCajas/{existing.IdPagoCaja}", content);

                if (!response.IsSuccessStatusCode)
                {
                    var errorMsg = await response.Content.ReadAsStringAsync();
                    Debug.WriteLine("Error en PUT PagoCaja: " + errorMsg);
                }

                return response.IsSuccessStatusCode;
            }
            catch (Exception ex)
            {
                Debug.WriteLine("Excepción en UpdatePagoCaja: " + ex.Message);
                return false;
            }
        }
        #endregion

        #region Metodos de Validaciones

        public async Task<bool> ValidarAlumnoDuplicado(
            string codigoMINED,
            string? codigoAlumno = null,
            int? excluirIdAlumno = null)
        {
            var mined = codigoMINED?.Trim();
            var codigoEst = codigoAlumno?.Trim();

            if (string.IsNullOrWhiteSpace(mined) && string.IsNullOrWhiteSpace(codigoEst))
                return false;

            if (!string.IsNullOrWhiteSpace(mined))
            {
                var desdeApi = await ConsultarDuplicadoAlumnoEnApiAsync(mined);
                if (desdeApi == true)
                    return true;
            }

            return await ExisteAlumnoDuplicadoEnCatalogoLocalAsync(mined, codigoEst, excluirIdAlumno);
        }

        private async Task<bool?> ConsultarDuplicadoAlumnoEnApiAsync(string codigoMINED)
        {
            try
            {
                var encoded = Uri.EscapeDataString(codigoMINED);
                using var httpclient = CreateApiClient();

                // Algunas APIs usan codigo para MINED; otras codigoMINED.
                var urls = new[]
                {
                    url + $"api/Alumnos/existeAlumno?codigoMINED={encoded}",
                    url + $"api/Alumnos/existeAlumno?codigo={encoded}"
                };

                foreach (var endpoint in urls)
                {
                    var response = await httpclient.GetAsync(endpoint);
                    if (!response.IsSuccessStatusCode)
                        continue;

                    var content = await response.Content.ReadAsStringAsync();
                    var parsed = ParseBoolRespuestaApi(content);
                    if (parsed.HasValue)
                        return parsed.Value;
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"ValidarAlumnoDuplicado API: {ex.Message}");
            }

            return null;
        }

        private static bool? ParseBoolRespuestaApi(string? content)
        {
            if (string.IsNullOrWhiteSpace(content))
                return false;

            var trimmed = content.Trim();
            if (bool.TryParse(trimmed, out var directo))
                return directo;

            try
            {
                if (trimmed.StartsWith('{'))
                {
                    var obj = Newtonsoft.Json.Linq.JObject.Parse(trimmed);
                    foreach (var key in new[] { "existe", "exists", "result", "value", "duplicado" })
                    {
                        var token = obj[key];
                        if (token != null && token.Type == Newtonsoft.Json.Linq.JTokenType.Boolean)
                            return (bool)token;
                    }
                }

                return JsonConvert.DeserializeObject<bool>(trimmed);
            }
            catch
            {
                return null;
            }
        }

        private async Task<bool> ExisteAlumnoDuplicadoEnCatalogoLocalAsync(
            string? codigoMINED,
            string? codigoAlumno,
            int? excluirIdAlumno)
        {
            var lista = await GetAlumnosAsync() ?? new List<TblAlumno>();

            bool Coincide(TblAlumno a, string? valor, Func<TblAlumno, string?> selector)
            {
                if (string.IsNullOrWhiteSpace(valor))
                    return false;
                var actual = selector(a)?.Trim();
                return !string.IsNullOrWhiteSpace(actual) &&
                       string.Equals(actual, valor, StringComparison.OrdinalIgnoreCase);
            }

            return lista.Any(a =>
                a.Activo != false &&
                (!excluirIdAlumno.HasValue || a.IdAlumno != excluirIdAlumno.Value) &&
                (Coincide(a, codigoMINED, x => x.CodigoMINED) ||
                 Coincide(a, codigoAlumno, x => x.CodigoAlumno)));
        }
        //Metodo de busqueda de duplicidad de nota por asignatura y periodo de evaluacion
        public async Task<bool> ValidarNotas(int idAsignatura, int idPeriodoEva, int idAlumno)
        {
            var existe = false;



            using (var httpclient = CreateApiClient())
            {

                var response = await httpclient.GetAsync(url + $"api/Notas/ValidarDupNotas?idAsignatura={idAsignatura}&idPeridodEval={idPeriodoEva}&idAlumno={idAlumno}");

                if (response.IsSuccessStatusCode)
                {
                    var content = await response.Content.ReadAsStringAsync();
                    var resultado = JsonConvert.DeserializeObject<bool>(content);

                    if (resultado)
                    {
                       return existe=true;
                    }
                }
                return existe ;
            }
            
        }
        public async Task<bool> ValidarFacturas(int idTipoColegiatura, int idEstadoPago, int idAlumno,string mesFacturado,string anyoFacturado)
        {
            var existe = false;
            using (var httpclient = CreateApiClient())
            {

                var response = await httpclient.GetAsync(url + $"api/FacturaColegiatura/ValidarDupFacturas?idTipoColegiatura={idTipoColegiatura}&idEstadoPago={idEstadoPago}&idAlumno={idAlumno}&mesFacturado={mesFacturado}&anyoFacturado={anyoFacturado}");

                if (response.IsSuccessStatusCode)
                {
                    var content = await response.Content.ReadAsStringAsync();
                    var resultado = JsonConvert.DeserializeObject<bool>(content);

                    if (resultado != false)
                    {
                        return existe = true;
                    }
                }
                return existe;
            }

        }

        public async Task<bool> validarUsuarios(string login)//, int idtematica)
        {

            using (var httpClient = CreateApiClient())
            {
                var response = await httpClient.GetAsync(url + $"api/Usuarios/validarUsuario?login={login}");
                if (response.IsSuccessStatusCode)
                {
                    return true;
                }
                return false;
            }
        }
        public async Task<bool> ValidarProductos(string codigo, int categoria)
        {
            var existe = false;

            using (var httpclient = CreateApiClient())
            {

                var response = await httpclient.GetAsync(url + $"api/TblProductos/existeProducto?codigo={codigo}&categoriaProd={categoria}");

                if (response.IsSuccessStatusCode)
                {
                    var content = await response.Content.ReadAsStringAsync();
                    var resultado = JsonConvert.DeserializeObject<bool>(content);

                   
                        return existe=resultado;
                    
                }
                return existe;
            }

        }
        //public async Task<int> ValidarMesesPendientes(List<TblPago> pagos,int idmes)
        //{
        //    if(pagos == null)
        //    {
        //        return 0;
        //    }
        //    //var mesesPendientes = await GetPagosAsync();
        //    var pendientes = pagos
        //    .Where(p => p.IdAlumno == pagos. &&
        //               p.IdTipoMovimiento == pagos.IdTipoMovimiento &&
        //               p.IdPeriodo == pagos.IdPeriodo &&
        //               p.IdMes < idmes)
        //    .Select(p => p.IdMes).Count();
        //    return pendientes;
        //}
        #endregion

        #region Generar Código Estudiante
        public async Task<string> GenerarCodigoAlumno()
        {
            // Obtiene el último alumno insertado
            var alumnosList = await GetAlumnosAsync();
            var ultimo =  alumnosList
                .OrderByDescending(a => a.IdAlumno)
                .FirstOrDefault();

            int numero = ultimo == null ? 1 : ultimo.IdAlumno + 1;

            // Formato: ALU2025-0001
            return $"EST{DateTime.Now.Year}-{numero:D4}";
        }

        #endregion
    }
}
