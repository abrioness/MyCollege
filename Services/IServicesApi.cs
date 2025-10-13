using Microsoft.AspNetCore.Mvc;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using WebColegio.Models;
using WebColegio.Models.ViewModel;

namespace WebColegio.Services
{
    public interface IServicesApi
    {
        #region Metodos Get
        Task<List<TblAlumno>> GetAlumnosAsync();
        Task<List<Recintos>> GetRecintosAsync();
        Task<List<Sexos>> GetSexosAsync();
        Task<List<Modalidades>> GetModalidadesAsync();
        Task<List<Grupos>> GetGruposAsync();
        Task<List<Turnos>> GetTurnosAsync();
        Task<List<Grados>> GetGradosAsync();
        Task<List<TipoEvaluacion>> GetTipEvaluacionAsync();
        Task<List<PeriodoEvaluacion>> GetPeriodoEvaluacionAsync();
        Task<List<Asignaturas>> GetAsignaturaAsync();
        Task<List<TblNotas>> GetNotasAsync();
        Task<List<FacturaColegiatura>> GetFacturacionAsync();
        Task<List<TipoColegiatura>> GetTipoColegiatuuraAsync();
        Task<List<TblEstadoPago>> GetEstadoPagoAsync();
        Task<TblAlumno> V_alumnoNotas(int idnota);
        Task<List<TblUsuarios>> GetUsuariosAsync();
        Task<List<TblPago>> GetPagosAsync();
        Task<List<TblReciboCaja>> GetRecibosCajaAsync();

        Task<TblUsuarios> GetLogin(string usuario);

        Task<List<Productos>> GetProductosAsync();
        Task<List<CategoriaProducto>> GetCategoriaProductoAsync();
        Task<List<TblInventario>> GetInventarioAsync();

        Task<List<CatTipoMovimiento>> GetTipoMovimientoAsync();
        Task<List<CatTipoRecibo>> GetTipoReciboAsync();
        Task<List<CatMetodoPago>> GetMetodoPagoAsync();
        Task<List<CatDiscapacidad>> GetDiscapacidadAsync();
        Task<List<CatMovInventario>> GetMovInventarioAsync();
        #endregion
        #region Metodos Post
        Task<bool> PostAlumnosAsync(TblAlumno alumnos);
        Task<bool> PostNotasAsync(TblNotas notas);
        Task<bool> PostPagosAsync(TblPago pagos);
        Task<bool> PostFacturacionAsync(FacturaColegiatura factura);
        
        //Task<bool> PostPagosAsync(PagosViewModel pagos);
        Task<bool> PostReciboCajaAsync(TblReciboCaja reciboCaja);
        Task<bool> PostInventarioAsync(TblInventario inventario);
        Task<bool> PostProductosAsync(Productos producto);

        #endregion
        #region Metodos Put
        Task<bool> UpdateAlumnos(TblAlumno alumno);
        Task<bool> UpdateNotas(TblNotas nota);
        #endregion
        #region Metodos Get por Id
        Task<TblAlumno> GetAlumnoIdAsync(int id);
        Task<TblNotas> GetNotasById(int id);
        Task<TblReciboCaja> GetReciboCajaById(int id);
        Task<List<TblNotas>> GetNotasAlumnoById(int idAlumno);
        Task<ArqueoCajaViewModel> GetArqueoById(int id);
        Task<TblPago> GetPagoById(int id);

        #endregion
        #region Metodos de Busqueda
        Task<List<TblAlumno>> searchAlumnosAsync();
        #endregion
        #region Medotos de Validacion
        Task<bool>ValidarNotas(int idAsignatura,int idPeriodoEva,int idAlumno);
        Task<bool> ValidarFacturas(int idTipoColegiatura, int idEstadoPago, int idAlumno, string mesFacturado, string anyoFacturado);
        Task<bool> ValidarProductos(string codigo, int categoria);
        Task<bool> ValidarAlumnoDuplicado(string codigo, string nombre, string apellido);
        #endregion

        #region Metodos para Genear Códigos
        Task<string> GenerarCodigoAlumno();
        #endregion

    }
}
