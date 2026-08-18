using Microsoft.AspNetCore.Mvc;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using WebColegio.Models;
using WebColegio.Models.ViewModel;

namespace WebColegio.Services
{
    public interface IServicesApi
    {
        /// <summary>Último error HTTP al llamar la API (p. ej. 401 sin JWT válido).</summary>
        string? LastApiError { get; }

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
        Task<TblAlumno> V_alumnoNotas(string cedulaTutor);
        Task<List<TblUsuarios>> GetUsuariosAsync();
        Task<List<TblPago>> GetPagosAsync();
        Task<List<TblPagoCaja>> GetPagoCajaAsync();
        Task<List<TblReciboCaja>> GetRecibosCajaAsync();
        Task<List<TblEgreso>> GetEgresoAsync();
        Task<List<TblArqueoDiario>> GetArqueoDiarioAsync();

        Task<List<Productos>> GetProductosAsync();
        Task<Productos?> GetProductoByIdAsync(int id);
        /// <summary>Obtiene el producto por código y categoría (para ingreso a producto existente).</summary>
        Task<Productos?> GetProductoByCodigoYCategoriaAsync(string codigo, int idCategoria);
        Task<List<CategoriaProducto>> GetCategoriaProductoAsync();
        //Task<List<TblInventario>> GetInventarioAsync();
        Task<List<CatTipoMovimiento>> GetTipoMovimientoAsync();
        Task<List<CatTipoRecibo>> GetTipoReciboAsync();
        Task<List<CatMetodoPago>> GetMetodoPagoAsync();
        Task<List<TblCatMeses>> GetMesesAsync();
        Task<List<CatPeriodo>> GetPeriodoAsync();
        Task<List<CatDiscapacidad>> GetDiscapacidadAsync();
        Task<List<CatMovInventario>> GetMovInventarioAsync();
        Task<List<MovimientoInventario>> GetMovimientoInventarioAsync();

        Task<List<TblCostoMensualidad>> GetCostosMensualidadAsync();
        Task<List<TblCostoMatricula>> GetCostosMatriculaAsync();
        Task<List<TblRol>> GetRolAsync();

        #endregion
        #region Metodos Post
        /// <summary>Guarda alumno en la API. Incluye mensaje de error si falla e IdAlumno si la API lo devuelve en el cuerpo.</summary>
        Task<(bool Exito, string? DetalleError, int? IdAlumnoCreado)> PostAlumnosAsync(TblAlumno alumnos);
        /// <summary>Guarda nota en la API. Detalle contiene mensaje de error si Exito es false.</summary>
        Task<(bool Exito, string? Detalle)> PostNotasAsync(TblNotas notas);
        Task<(bool Exito, int IdPagoCreado, string? DetalleError)> PostPagosAsync(TblPago pagos);
        Task<bool> PostFacturacionAsync(FacturaColegiatura factura);
        
        Task<bool> PostPagosCajaAsync(TblPagoCaja pagosCaja);
        Task<bool> PostEgresoAsync(TblEgreso egresos);
        Task<bool> PostReciboCajaAsync(TblReciboCaja reciboCaja);
        Task<bool> PostMovimientoInventarioAsync(MovimientoInventario movimiento);
        Task<List<MovimientoInventario>> GetMovimientosInventarioAsync(int? idProducto, DateTime? desde, DateTime? hasta);
        Task<bool> PostProductosAsync(Productos producto);
        Task<(bool Exito, string? DetalleError)> UpdateProductoAsync(Productos producto);
        /// <summary>Mismo comportamiento que <see cref="UpdateProductoAsync"/> (nombre plural por convención de negocio).</summary>
        Task<(bool Exito, string? DetalleError)> UpdateProductosAsync(Productos producto);
        /// <returns>Éxito y, si falla, texto de error devuelto por la API (o mensaje genérico).</returns>
        Task<(bool Ok, string? ErrorMessage)> PostArqueoDiarioAsync(TblArqueoDiario arqueo);
        Task<bool> PostUsuarios(TblUsuarios usuario);

        #endregion
        #region Metodos Put
        Task<bool> UpdateAlumnos(TblAlumno alumno);
        Task<bool> UpdateNotas(TblNotas nota);
        Task<bool> UpdateUsuario(TblUsuarios usuario);
        Task<bool> UpdatePago(TblPago pago);
        Task<bool> UpdateEgreso(TblEgreso egreso);
        Task<bool> UpdatePagoCaja(TblPagoCaja pagoCaja);
        #endregion
        #region Metodos Get por Id
        Task<TblAlumno> GetAlumnoIdAsync(int id);
        Task<TblNotas> GetNotasById(int id);
        Task<TblReciboCaja> GetReciboCajaById(int id);
        Task<List<TblNotas>> GetNotasAlumnoById(int idAlumno);
        Task<ArqueoDiarioViewModel> GetArqueoById(int id);
        Task<TblPago> GetPagoById(int id);
        Task<TblPagoCaja> GetPagoCajaById(int id);
        Task<List<TblNotas>> GetNotasPorUsuario(string usuario);
        Task<TblUsuarios?> GetLogin(string usuario);
        Task<TblRol> GetRol(int idrol);
        Task<TblEgreso> GetEgresoCajaById(int id);
        Task<TblUsuarios> GetUsuarioIdAsync(int idUser);
        #endregion
        #region Metodos de Busqueda
        Task<List<TblAlumno>> searchAlumnosAsync();
        #endregion
        #region Medotos de Validacion
        Task<bool>ValidarNotas(int idAsignatura,int idPeriodoEva,int idAlumno);
        Task<bool> ValidarFacturas(int idTipoColegiatura, int idEstadoPago, int idAlumno, string mesFacturado, string anyoFacturado);
        Task<bool> ValidarProductos(string codigo, int categoria);
        Task<bool> ValidarAlumnoDuplicado(string codigoMINED, string? codigoAlumno = null, int? excluirIdAlumno = null);
        Task<bool> validarUsuarios(string cedula);
        //Task<int> ValidarMesesPendientes(List<TblPago> pagos, int idmes);
        #endregion

        #region Metodos para Genear Códigos
        Task<string> GenerarCodigoAlumno();
        #endregion

    }
}
