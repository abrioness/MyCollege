using System.Diagnostics;
using System.Net.Security;
//using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;
using WebColegio.Models;

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
