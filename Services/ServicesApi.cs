using System.Diagnostics;
using System.Net.Security;
//using System.Security.Cryptography.X509Certificates;
using System.Text;
using Newtonsoft.Json;
using WebColegio.Models;

namespace WebColegio.Services
{
    public class ServicesApi:IServicesApi
    {
       // private static string url = "https://localhost:7008/";
        
        private readonly string _apiUrl;
        public ServicesApi(IConfiguration config)
        {
            _apiUrl = config["ApiUrl"];
        }
        
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
        #endregion
        #region Metodos Post
        public async Task<bool> PostAlumnosAsync(TblAlumno alumnos)
        {
            bool respuesta = false;

            // Asegurar datos mínimos requeridos
            alumnos.Activo = true;
            alumnos.UsuarioRegistro = 1;
            alumnos.FechaRegistro = DateTime.Today;

            try
            {
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
        //public async Task<List<Grados>> GetIdAlumnoIdAsync()
        //{
        //    List<Grados> Gradoslist = new List<Grados>();
        //    using (var httpclient = new HttpClient())
        //    {

        //        var response = await httpclient.GetAsync(url + "api/Grados");

        //        if (response.IsSuccessStatusCode)
        //        {
        //            var content = await response.Content.ReadAsStringAsync();
        //            var list_Grados = JsonConvert.DeserializeObject<List<Grados>>(content);

        //            Gradoslist = list_Grados;
        //        }
        //        return Gradoslist;
        //    }


        //}

        #endregion


    }
}
