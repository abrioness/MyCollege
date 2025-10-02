using System.Diagnostics;
using System.Net.Http;
using System.Net.Security;
//using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using WebColegio.Models;
using WebColegio.Models.ViewModel;

namespace WebColegio.Services
{
    public class ServicesApi:IServicesApi
    {
        private static string url = "https://localhost:7008/";
        

        public ServicesApi() { }
        //Metodo para Listar usuarios
        #region Metodos Get

        public async Task<List<TblAlumno>> GetAlumnosAsync()
        {
            List<TblAlumno> Alumnoslist = new List<TblAlumno>();
            //var handler = new HttpClientHandler();
            //handler.ServerCertificateCustomValidationCallback =
            //    (request, cert, chain, errors) => true;
            using (var httpclient = new HttpClient())
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
            using (var httpclient = new HttpClient())
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
            using (var httpclient = new HttpClient())
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
            using (var httpclient = new HttpClient())
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
            using (var httpclient = new HttpClient())
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
            using (var httpclient = new HttpClient())
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
            using (var httpclient = new HttpClient())
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
            using (var httpclient = new HttpClient())
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
            using (var httpclient = new HttpClient())
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
            using (var httpclient = new HttpClient())
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
            using (var httpclient = new HttpClient())
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
            List<TblNotas> notas = new List<TblNotas>();
            using (var httpclient = new HttpClient())
            {

                var response = await httpclient.GetAsync(url + "api/Notas");

                if (response.IsSuccessStatusCode)
                {
                    var content = await response.Content.ReadAsStringAsync();
                    var resultado = JsonConvert.DeserializeObject<List<TblNotas>>(content);

                    notas = resultado;
                }
                return notas;
            }


        }
        public async Task<List<FacturaColegiatura>> GetFacturacionAsync()
        {
            List<FacturaColegiatura> facturas = new List<FacturaColegiatura>();
            using (var httpclient = new HttpClient())
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
            using (var httpclient = new HttpClient())
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
            using (var httpclient = new HttpClient())
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
            List<TblPago> pagos = new List<TblPago>();
            using (var httpclient = new HttpClient())
            {
                var response =await httpclient.GetAsync(url + "api/Pagos");
                if (response.IsSuccessStatusCode)
                {
                    var content = await response.Content.ReadAsStringAsync();
                    var resultado = JsonConvert.DeserializeObject<List<TblPago>>(content);
                    pagos = resultado;
                }
                return  pagos;
            }
        }
        public async Task<List<TblReciboCaja>> GetRecibosCajaAsync()
        {
            List<TblReciboCaja> reciboCajas = new List<TblReciboCaja>();
            using (var httpclient = new HttpClient())
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


        public async Task<TblAlumno> V_alumnoNotas(int idnota)
        {
            var ValumnoNotas = new TblAlumno();
            using (var httpclient = new HttpClient())
            {

                var response = await httpclient.GetAsync(url + "api/Alumnos/alumnoNota?idnota="+idnota);

                if (response.IsSuccessStatusCode)
                {
                    var content = await response.Content.ReadAsStringAsync();
                    var resultado = JsonConvert.DeserializeObject <TblAlumno>(content);

                    ValumnoNotas = resultado;
                }
                return ValumnoNotas;
            }
        }


        #endregion
        #region Metodos Post
        public async Task<bool> PostAlumnosAsync(TblAlumno alumnos)
        {
            bool respuesta = false;
            bool validarDuplicado = await ValidarAlumnoDuplicado(alumnos);
            if (validarDuplicado)
            {
                return respuesta;
            }

            else
            {


                // Asegurar datos mínimos requeridos
                alumnos.Activo = true;
                alumnos.UsuarioRegistro = 1;
                alumnos.FechaRegistro = DateTime.Now;
                
                alumnos.CodigoEstudiante =await GenerarCodigoAlumno();

                using (var httpClient = new HttpClient())
                {
                    // Serializar el objeto alumno
                    string jsonAlumnos = JsonConvert.SerializeObject(alumnos);
                    var content = new StringContent(jsonAlumnos, Encoding.UTF8, "application/json");

                    // Enviar POST
                    var response = await httpClient.PostAsync(url + "api/Alumnos/Guardar", content);

                    if (response.IsSuccessStatusCode)
                    {
                         respuesta = true;
                    }
                    return respuesta;
                }

            } 
            
           
        }

        public async  Task<bool> PostNotasAsync(TblNotas notas)
        {
            bool respuesta = false;

            // Asegurar datos mínimos requeridos
            notas.Activo = true;
            notas.UsuarioRegistro = 1;
            notas.FechaRegistro = DateTime.Now;

            try
            {
                using (var httpClient = new HttpClient())
                {
                    // Serializar el objeto alumno
                    string jsonNotas = JsonConvert.SerializeObject(notas);
                    var content = new StringContent(jsonNotas, Encoding.UTF8, "application/json");

                    // Enviar POST
                    var response = await httpClient.PostAsync(url + "api/Notas/Guardar", content);

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
                using (var httpClient = new HttpClient())
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
                using (var httpClient = new HttpClient())
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

        #endregion

        #region Metodos Get por Id
        public async Task<TblAlumno> GetAlumnoIdAsync(int id)
        {
            var alumno = new TblAlumno();
            using (var httpclient = new HttpClient())
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
            using (var httpclient = new HttpClient())
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

        public async Task<List<TblNotas>> GetNotasAlumnoById(int idAlumno)
        {
            List<TblNotas> notas = new List<TblNotas>();
            using (var httpclient = new HttpClient())
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
        public async Task<ArqueoCajaViewModel> GetArqueoById(int id)
        {
            // Suponiendo que tu API tiene un endpoint como:
            // GET https://tuservidor/api/arqueo/{id}
            using (var httpclient = new HttpClient())
            {

                var response = await httpclient.GetAsync($"api/arqueo/{id}");

                if (!response.IsSuccessStatusCode)
                {
                    throw new Exception($"Error al obtener el arqueo: {response.StatusCode}");
                }

                var json = await response.Content.ReadAsStringAsync();

                // Usar Newtonsoft.Json o System.Text.Json para deserializar
                var arqueo = JsonConvert.DeserializeObject<ArqueoCajaViewModel>(json);

                return arqueo!;
            }
        }

        public async Task<TblReciboCaja> GetReciboCajaById(int id)
        {
            
            // Suponiendo que tu API tiene un endpoint como:
            // GET https://tuservidor/api/arqueo/{id}
            using (var httpclient = new HttpClient())
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


        #endregion
        #region Metodos de Busqueda
        public async Task<List<TblAlumno>> searchAlumnosAsync()
        {
            List<TblAlumno> resultado = new List<TblAlumno>();
            using (var httpclient = new HttpClient())
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
            // Actualizamos campos
            existingNota.IdTipoEvaluacion = nota.IdTipoEvaluacion;
            existingNota.IdPeriodo = nota.IdPeriodo;
            existingNota.IdAsignatura = nota.IdAsignatura;
            existingNota.Descripcion = nota.Descripcion;
            existingNota.PrimerCorte = nota.PrimerCorte;
            existingNota.SegundoCorte = nota.SegundoCorte;
            existingNota.TercerCorte = nota.TercerCorte;
            existingNota.CuartoCorte = nota.CuartoCorte;
            existingNota.NotaFinal = nota.NotaFinal;
            existingNota.Activo = true;
            existingNota.UsuarioActualiza = 1;
            existingNota.FechaActualiza = DateTime.Now;
            // agrega más propiedades según tu modelo

            if (existingNota == null)
            {
                return false;
            }
            
            using (var httpClient = new HttpClient())
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
            var existingAlumno = await GetAlumnoIdAsync(alumno.IdAlumno);
            // Actualizamos campos
            existingAlumno.Nombre = alumno.Nombre;
            existingAlumno.Apellido = alumno.Apellido;
            existingAlumno.FechaNacimiento = alumno.FechaNacimiento;
            existingAlumno.Edad = alumno.Edad;
            existingAlumno.CodigoEstudiante = alumno.CodigoEstudiante;
            existingAlumno.Cedula = alumno.Cedula;
            existingAlumno.IdGrado = alumno.IdGrado;
            existingAlumno.IdTurno = alumno.IdTurno;
            existingAlumno.IdModalidad = alumno.IdModalidad;
            existingAlumno.IdRecinto = alumno.IdRecinto;
            existingAlumno.IdSexo = alumno.IdSexo;
            existingAlumno.IdGrupo = alumno.IdGrupo;
            existingAlumno.Direccion = alumno.Direccion;
            existingAlumno.Telefono = alumno.Telefono;
            existingAlumno.NombreMadre = alumno.NombreMadre;
            existingAlumno.NombrePadre = alumno.NombrePadre;
            existingAlumno.NombreTutor = alumno.NombreTutor;
            existingAlumno.Correo = alumno.Correo;
            existingAlumno.Activo = true;
            existingAlumno.UsuarioActualiza = 1;
            existingAlumno.FechaActualiza = DateTime.Now;

           

            if (existingAlumno == null)
            {
                return false;
            }

            using (var httpClient = new HttpClient())
            {
                // Convertimos el objeto a JSON
                var json = JsonConvert.SerializeObject(existingAlumno);
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                // Realizamos la solicitud PUT
                var response = await httpClient.PutAsync(url + $"api/Alumnos/{existingAlumno.IdAlumno}", content);

                return response.IsSuccessStatusCode;
            }

        }
        #endregion

        #region Metodos de Validaciones
        private async Task<bool> ValidarAlumnoDuplicado(TblAlumno alumno)
        {
            bool validar = false;
            var existeAlumno = await GetAlumnosAsync();
            bool existe =  existeAlumno.Any(a =>
           a.Nombre == alumno.Nombre &&
           a.Apellido == alumno.Apellido &&
           a.FechaNacimiento == alumno.FechaNacimiento);

            if (existe)
            {

                return validar = true;
            }
            return validar;
        }
        //Metodo de busqueda de duplicidad de nota por asignatura y periodo de evaluacion
        public async Task<bool> ValidarNotas(int idAsignatura, int idPeriodoEva, int idAlumno)
        {
            var existe = false;
            using (var httpclient = new HttpClient())
            {

                var response = await httpclient.GetAsync(url + $"api/Notas/ValidarDupNotas?idAsignatura={idAsignatura}&idPeridodEval={idPeriodoEva}&idAlumno={idAlumno}");

                if (response.IsSuccessStatusCode)
                {
                    var content = await response.Content.ReadAsStringAsync();
                    var resultado = JsonConvert.DeserializeObject<bool>(content);

                    if (resultado != false)
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
            using (var httpclient = new HttpClient())
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

        #endregion

        #region Generar Código Estudiante
        private async Task<string> GenerarCodigoAlumno()
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
