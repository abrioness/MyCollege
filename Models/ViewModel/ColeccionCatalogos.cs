namespace WebColegio.Models.ViewModel
{
    public class ColeccionCatalogos
    {
       public List<TipoEvaluacion> tipoEvaluaciones { get; set; } = new List<TipoEvaluacion>();
        public List<TblMaterias> materias { get; set; } = new List<TblMaterias>();
        public List<TblNotas> notas { get; set; } = new List<TblNotas>();
        public List<FacturaColegiatura> facturacion { get; set; } = new List<FacturaColegiatura>();
        public List<TblAlumno> alumno { get; set; } = new List<TblAlumno>();
        public List<PeriodoEvaluacion> periodoEvaluacions { get; set; } = new List<PeriodoEvaluacion>();
        public List<Recintos> recintos { get; set; } = new List<Recintos>();
        public List<Sexos> sexos { get; set; } = new List<Sexos>();

        public List<Turnos> turnos { get; set; } = new List<Turnos>();
        public List<Grupos> grupos { get; set; } = new List<Grupos>();
        public List<Grados> grados { get; set; } = new List<Grados>();
        public List<Modalidades> modalidades { get; set; } = new List<Modalidades>();
        public List<Asignaturas> asignaturas { get; set; } = new List<Asignaturas>();
        public List<TipoColegiatura> tipoColegiaturas { get; set; } = new List<TipoColegiatura>();
        public List<TblEstadoPago> estadoPagos { get; set; } = new List<TblEstadoPago>();

        public List<TblPago> pagos { get; set; } = new List<TblPago>();
        public List<TblReciboCaja> reciboCajas { get; set; } = new List<TblReciboCaja>();
        public List<TblUsuarios> usuarios { get; set; } = new List<TblUsuarios>();
        public List<Productos> producto { get; set; } = new List<Productos>();

        public List<CatTipoMovimiento> tipoMovimiento { get; set; } = new List<CatTipoMovimiento>();
        public List<CatTipoRecibo> tipoRecibo { get; set; } = new List<CatTipoRecibo>();
        public List<CatMetodoPago> metodoPago { get; set; } = new List<CatMetodoPago>();

        public List<CategoriaProducto> categoriasProducto { get;  set; } = new List<CategoriaProducto>();
        public List<CatDiscapacidad> discapacidad { get; set; } = new List<CatDiscapacidad>();
        public List<CatMovInventario> movinventario { get; set; } = new List<CatMovInventario>();
        public List<TblInventario> inventario { get; set; } = new List<TblInventario>();
        public List<TblCatMeses> meses { get; set; } = new List<TblCatMeses>();


    }
}
