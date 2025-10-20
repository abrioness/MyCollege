using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;

namespace Api_Colegio.Models;

public partial class MyCollegeContext1 : DbContext
{
    public MyCollegeContext1()
    {
    }

    public MyCollegeContext1(DbContextOptions<MyCollegeContext1> options)
        : base(options)
    {
    }

    public virtual DbSet<TblAlumno> TblAlumnos { get; set; }

    public virtual DbSet<TblCatAsignacionProfesorMaterium> TblCatAsignacionProfesorMateria { get; set; }

    public virtual DbSet<TblCatAsignatura> TblCatAsignaturas { get; set; }

    public virtual DbSet<TblCatCuenta> TblCatCuentas { get; set; }

    public virtual DbSet<TblCatDetalleVenta> TblCatDetalleVentas { get; set; }
    public virtual DbSet<TblCatSexo> TblCatSexos { get; set; }

    public virtual DbSet<TblCatGrupo> TblCatGrupos { get; set; }

    public virtual DbSet<TblCatModalidad> TblCatModalidads { get; set; }

    public virtual DbSet<TblCatPeridoEvaluacion> TblCatPeridoEvaluacions { get; set; }

    public virtual DbSet<TblCatProducto> TblCatProductos { get; set; }

    public virtual DbSet<TblCatProveedore> TblCatProveedores { get; set; }

    public virtual DbSet<TblCatRecinto> TblCatRecintos { get; set; }

    public virtual DbSet<TblCatTipoColegiatura> TblCatTipoColegiaturas { get; set; }

    public virtual DbSet<TblCatTipoEvaluacion> TblCatTipoEvaluacions { get; set; }

    public virtual DbSet<TblCatTurno> TblCatTurnos { get; set; }

    public virtual DbSet<TblCategoriaProducto> TblCategoriaProductos { get; set; }

    public virtual DbSet<TblCierreCaja> TblCierreCajas { get; set; }

    public virtual DbSet<TblColegiatura> TblColegiaturas { get; set; }

    public virtual DbSet<TblDetalleTranContable> TblDetalleTranContables { get; set; }

    public virtual DbSet<TblEstadoPago> TblEstadoPagos { get; set; }

    public virtual DbSet<TblFacturaColegiatura> TblFacturaColegiaturas { get; set; }

    public virtual DbSet<TblGrado> TblGrados { get; set; }

    public virtual DbSet<TblInventario> TblInventarios { get; set; }

    public virtual DbSet<TblMateria> TblMaterias { get; set; }

    public virtual DbSet<TblMetodoPago> TblMetodoPagos { get; set; }

    public virtual DbSet<TblNota> TblNotas { get; set; }

    public virtual DbSet<TblPago> TblPagos { get; set; }

    public virtual DbSet<TblPagoColegiatura> TblPagoColegiaturas { get; set; }

    public virtual DbSet<TblPlanCuenta> TblPlanCuentas { get; set; }

    public virtual DbSet<TblProfesore> TblProfesores { get; set; }

    public virtual DbSet<TblRol> TblRols { get; set; }

    public virtual DbSet<TblTransaccionesContable> TblTransaccionesContables { get; set; }

    public virtual DbSet<TblUsuario> TblUsuarios { get; set; }

    public virtual DbSet<TblUsuarioRol> TblUsuarioRols { get; set; }

    public virtual DbSet<TblVenta> TblVentas { get; set; }
    public virtual DbSet<TblRecibosCaja> TblRecibosCaja { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        => optionsBuilder.UseSqlServer("Name=ConnectionStrings:Conexion");

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<TblAlumno>(entity =>
        {
            entity.HasKey(e => e.IdAlumno).HasName("PK_Alumnos");

            entity.ToTable("Tbl_Alumnos");

            entity.Property(e => e.Apellido)
                .HasMaxLength(100)
                .IsUnicode(false);
            entity.Property(e => e.Correo)
                .HasMaxLength(100)
                .IsUnicode(false);
            entity.Property(e => e.Direccion)
                .HasMaxLength(200)
                .IsUnicode(false);
            entity.Property(e => e.FechaActualiza).HasColumnType("datetime");
            entity.Property(e => e.FechaRegistro).HasColumnType("datetime");
            entity.Property(e => e.Nombre)
                .HasMaxLength(100)
                .IsUnicode(false);
            entity.Property(e => e.NombreMadre)
                .HasMaxLength(100)
                .IsUnicode(false);
            entity.Property(e => e.NombrePadre)
                .HasMaxLength(100)
                .IsUnicode(false);
            entity.Property(e => e.NombreTutor)
                .HasMaxLength(100)
                .IsUnicode(false);
            entity.Property(e => e.Telefono)
                .HasMaxLength(20)
                .IsUnicode(false);

            entity.HasOne(d => d.IdGradoNavigation).WithMany(p => p.TblAlumnos)
                .HasForeignKey(d => d.IdGrado)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Tbl_Alumnos_Tbl_Grados");

            entity.HasOne(d => d.IdSexoNavigation).WithMany(p => p.TblAlumnos)
                .HasForeignKey(d => d.IdSexo)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__Tbl_Alumn__IdSex__43D61337");

            entity.HasOne(d => d.IdGrupoNavigation).WithMany(p => p.TblAlumnos)
                .HasForeignKey(d => d.IdGrupo)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__Tbl_Alumn__IdGru__10566F31");

            entity.HasOne(d => d.IdModalidadNavigation).WithMany(p => p.TblAlumnos)
                .HasForeignKey(d => d.IdModalidad)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__Tbl_Alumn__IdMod__114A936A");

            entity.HasOne(d => d.IdRecintoNavigation).WithMany(p => p.TblAlumnos)
                .HasForeignKey(d => d.IdRecinto)
                .HasConstraintName("FK__Tbl_Alumn__IdRec__123EB7A3");

            entity.HasOne(d => d.IdTurnoNavigation).WithMany(p => p.TblAlumnos)
                .HasForeignKey(d => d.IdTurno)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__Tbl_Alumn__IdTur__0F624AF8");
        });

        modelBuilder.Entity<TblCatAsignacionProfesorMaterium>(entity =>
        {
            entity.HasKey(e => e.IdAsignacion);

            entity.ToTable("Tbl_CatAsignacionProfesorMateria");

            entity.Property(e => e.FechaActualiza).HasColumnType("datetime");
            entity.Property(e => e.FechaRegistro).HasColumnType("datetime");

            entity.HasOne(d => d.IdGradoNavigation).WithMany(p => p.TblCatAsignacionProfesorMateria)
                .HasForeignKey(d => d.IdGrado)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Tbl_CatAsignacionProfesorMateria_Tbl_Grados");

            entity.HasOne(d => d.IdMateriaNavigation).WithMany(p => p.TblCatAsignacionProfesorMateria)
                .HasForeignKey(d => d.IdMateria)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Tbl_CatAsignacionProfesorMateria_Tbl_Materias");

            entity.HasOne(d => d.IdProfesorNavigation).WithMany(p => p.TblCatAsignacionProfesorMateria)
                .HasForeignKey(d => d.IdProfesor)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Tbl_CatAsignacionProfesorMateria_Tbl_Profesores");
        });

        modelBuilder.Entity<TblCatAsignatura>(entity =>
        {
            entity.HasKey(e => e.IdAsignatura);

            entity.ToTable("Tbl_CatAsignaturas");

            //entity.Property(e => e.IdAsignatura).HasColumnName("Tbl_CatAsignatura");
            entity.Property(e => e.FechaActualiza).HasColumnType("datetime");
            entity.Property(e => e.FechaRegistro).HasColumnType("datetime");
            entity.Property(e => e.NombreAsignatura).HasMaxLength(100);
        });

        modelBuilder.Entity<TblCatSexo>(entity =>
        {
            entity.HasKey(e => e.IdSexo);

            entity.ToTable("Tbl_CatSexo");

            //entity.Property(e => e.IdSexo).HasColumnName("Sexo");
            entity.Property(e => e.FechaActualiza).HasColumnType("datetime");
            entity.Property(e => e.FechaRegistro).HasColumnType("datetime");
            entity.Property(e => e.Sexo).HasMaxLength(50);
        });

        modelBuilder.Entity<TblCatCuenta>(entity =>
        {
            entity.HasKey(e => e.IdCuenta);

            entity.ToTable("Tbl_CatCuentas");

            entity.Property(e => e.FechaActualiza).HasColumnType("datetime");
            entity.Property(e => e.FechaRegistro).HasColumnType("datetime");
            entity.Property(e => e.NombreCuenta).HasMaxLength(100);
        });

        modelBuilder.Entity<TblCatDetalleVenta>(entity =>
        {
            entity.HasKey(e => e.IdDetaVentas);

            entity.ToTable("Tbl_CatDetalleVentas");

            entity.Property(e => e.DescuentoLinea).HasColumnType("decimal(10, 2)");
            entity.Property(e => e.FechaActualiza).HasColumnType("datetime");
            entity.Property(e => e.FechaRegistro).HasColumnType("datetime");
            entity.Property(e => e.ImpuestoLinea).HasColumnType("decimal(10, 2)");
            entity.Property(e => e.PrecioUniVenta).HasColumnType("decimal(10, 2)");
            entity.Property(e => e.SubtoralLinea).HasColumnType("decimal(10, 2)");
            entity.Property(e => e.TotalLinea).HasColumnType("decimal(10, 2)");

            entity.HasOne(d => d.IdProductoNavigation).WithMany(p => p.TblCatDetalleVenta)
                .HasForeignKey(d => d.IdProducto)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Tbl_CatDetalleVentas_Tbl_CatProductos");

            entity.HasOne(d => d.IdVentaNavigation).WithMany(p => p.TblCatDetalleVenta)
                .HasForeignKey(d => d.IdVenta)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__Tbl_CatDe__IdVen__1332DBDC");
        });

        modelBuilder.Entity<TblCatGrupo>(entity =>
        {
            entity.HasKey(e => e.IdGrupo);

            entity.ToTable("Tbl_CatGrupo");

            entity.Property(e => e.FechaActualiza).HasColumnType("datetime");
            entity.Property(e => e.FechaRegistro).HasColumnType("datetime");
            entity.Property(e => e.NombreGrupo).HasMaxLength(100);
        });

        modelBuilder.Entity<TblCatModalidad>(entity =>
        {
            entity.HasKey(e => e.IdModalidad);

            entity.ToTable("Tbl_CatModalidad");

            entity.Property(e => e.FechaActualiza).HasColumnType("datetime");
            entity.Property(e => e.FechaRegistro).HasColumnType("datetime");
            entity.Property(e => e.Modalidad).HasMaxLength(100);
        });

        modelBuilder.Entity<TblCatPeridoEvaluacion>(entity =>
        {
            entity.HasKey(e => e.IdPeriodo);

            entity.ToTable("Tbl_CatPeridoEvaluacion");

            entity.Property(e => e.FechaActualiza).HasColumnType("datetime");
            entity.Property(e => e.FechaRegistro).HasColumnType("datetime");
            entity.Property(e => e.NombrePeriodo).HasMaxLength(50);
        });

        modelBuilder.Entity<TblCatProducto>(entity =>
        {
            entity.HasKey(e => e.IdProducto);

            entity.ToTable("Tbl_CatProductos");

            entity.Property(e => e.CodigoBarra).HasMaxLength(50);
            entity.Property(e => e.CostoUnitario).HasColumnType("decimal(10, 2)");
            entity.Property(e => e.Descripcion).HasColumnType("text");
            entity.Property(e => e.FechaActualiza).HasColumnType("datetime");
            entity.Property(e => e.FechaRegistro).HasColumnType("datetime");
            entity.Property(e => e.NombreProducto).HasMaxLength(200);
            entity.Property(e => e.PrecioVenta).HasColumnType("decimal(10, 2)");

            entity.HasOne(d => d.IdCateProductoNavigation).WithMany(p => p.TblCatProductos)
                .HasForeignKey(d => d.IdCateProducto)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__Tbl_CatPr__IdCat__14270015");

            entity.HasOne(d => d.IdProveedorNavigation).WithMany(p => p.TblCatProductos)
                .HasForeignKey(d => d.IdProveedor)
                .HasConstraintName("FK_Tbl_CatProductos_Tbl_CatProveedores");
        });

        modelBuilder.Entity<TblCatProveedore>(entity =>
        {
            entity.HasKey(e => e.IdProveedor);

            entity.ToTable("Tbl_CatProveedores");

            entity.Property(e => e.ContactoPrincipal).HasMaxLength(100);
            entity.Property(e => e.Direccion).HasMaxLength(255);
            entity.Property(e => e.FechaActualiza).HasColumnType("datetime");
            entity.Property(e => e.FechaRegistro).HasColumnType("datetime");
            entity.Property(e => e.NombreProveedor).HasMaxLength(100);
            entity.Property(e => e.Telefono).HasMaxLength(20);
        });

        modelBuilder.Entity<TblCatRecinto>(entity =>
        {
            entity.HasKey(e => e.IdRecinto);

            entity.ToTable("Tbl_CatRecinto");

            entity.Property(e => e.FechaActualiza).HasColumnType("datetime");
            entity.Property(e => e.FechaRegistro).HasColumnType("datetime");
            entity.Property(e => e.Recinto).HasMaxLength(100);
        });

        modelBuilder.Entity<TblCatTipoColegiatura>(entity =>
        {
            entity.HasKey(e => e.IdTipoColegiatura);

            entity.ToTable("Tbl_CatTipoColegiatura");

            entity.Property(e => e.Descripcion).HasColumnType("text");
            entity.Property(e => e.FechaActualiza).HasColumnType("datetime");
            entity.Property(e => e.FechaRegistro).HasColumnType("datetime");
            entity.Property(e => e.MontoBase).HasColumnType("decimal(10, 2)");
            entity.Property(e => e.NombreConcepto).HasMaxLength(100);
        });

        modelBuilder.Entity<TblCatTipoEvaluacion>(entity =>
        {
            entity.HasKey(e => e.IdTipoEvaluacion);

            entity.ToTable("Tbl_CatTipoEvaluacion");

            entity.Property(e => e.FechaActualiza).HasColumnType("datetime");
            entity.Property(e => e.FechaRegistro).HasColumnType("datetime");
            entity.Property(e => e.NombreTipEvaluacion).HasMaxLength(50);
            entity.Property(e => e.Porcentaje).HasColumnType("decimal(5, 2)");
        });

        modelBuilder.Entity<TblCatTurno>(entity =>
        {
            entity.HasKey(e => e.IdTurno);

            entity.ToTable("Tbl_CatTurno");

            entity.Property(e => e.FechaActualiza).HasColumnType("datetime");
            entity.Property(e => e.FechaRegistro).HasColumnType("datetime");
            entity.Property(e => e.NombreTurno).HasMaxLength(100);
        });

        modelBuilder.Entity<TblCategoriaProducto>(entity =>
        {
            entity.HasKey(e => e.IdCateProducto);

            entity.ToTable("Tbl_CategoriaProducto");

            entity.Property(e => e.Descripcion).HasColumnType("text");
            entity.Property(e => e.FechaActualiza).HasColumnType("datetime");
            entity.Property(e => e.FechaRegistro).HasColumnType("datetime");
            entity.Property(e => e.NombreCategoria).HasMaxLength(100);
        });

        modelBuilder.Entity<TblCierreCaja>(entity =>
        {
            entity.HasKey(e => e.IdCierreCaja);

            entity.ToTable("Tbl_CierreCaja");

            entity.Property(e => e.Descripcion).HasColumnType("text");
            entity.Property(e => e.DiferenciaEfectivo).HasColumnType("decimal(10, 2)");
            entity.Property(e => e.EstadoCierre).HasMaxLength(50);
            entity.Property(e => e.FechaActualiza).HasColumnType("datetime");
            entity.Property(e => e.FechaApertura).HasColumnType("datetime");
            entity.Property(e => e.FechaCierre).HasColumnType("datetime");
            entity.Property(e => e.FechaRegistro).HasColumnType("datetime");
            entity.Property(e => e.MontoFinCaja).HasColumnType("decimal(10, 2)");
            entity.Property(e => e.MontoIniCaja).HasColumnType("decimal(10, 2)");
            entity.Property(e => e.TotalVentasEfectivo).HasColumnType("decimal(10, 2)");
            entity.Property(e => e.TotalVentasOtros).HasColumnType("decimal(10, 2)");
            entity.Property(e => e.TotalVentasTarjetas).HasColumnType("decimal(10, 2)");
        });

        modelBuilder.Entity<TblColegiatura>(entity =>
        {
            entity.HasKey(e => e.IdColegiatura);

            entity.ToTable("Tbl_Colegiatura");

            entity.Property(e => e.Descripcion).HasColumnType("text");
            entity.Property(e => e.MontoMensual).HasColumnType("decimal(10, 2)");
        });

        modelBuilder.Entity<TblDetalleTranContable>(entity =>
        {
            entity.HasKey(e => e.IdDetTranContable);

            entity.ToTable("Tbl_DetalleTranContable");

            entity.Property(e => e.Descripcion).HasColumnType("text");
            entity.Property(e => e.FechaActualiza).HasColumnType("datetime");
            entity.Property(e => e.FechaRegistro).HasColumnType("datetime");
            entity.Property(e => e.Monto).HasColumnType("decimal(10, 2)");
            entity.Property(e => e.TipoMoviento).HasMaxLength(20);
        });

        modelBuilder.Entity<TblEstadoPago>(entity =>
        {
            entity.HasKey(e => e.IdEstadoPago);

            entity.ToTable("Tbl_EstadoPago");

            entity.Property(e => e.Descripcion).HasColumnType("text");
            entity.Property(e => e.EstadoPago)
                .HasMaxLength(100)
                .IsUnicode(false);
        });

        modelBuilder.Entity<TblFacturaColegiatura>(entity =>
        {
            entity.HasKey(e => e.IdFactura).HasName("PK_Tbl_Factura");

            entity.ToTable("Tbl_FacturaColegiatura");

            entity.Property(e => e.Descripcion).HasColumnType("text");
            entity.Property(e => e.FechaActualiza).HasColumnType("datetime");
            entity.Property(e => e.FechaRegistro).HasColumnType("datetime");
            entity.Property(e => e.MesFacturado).HasMaxLength(20);
            entity.Property(e => e.MontoTotal).HasColumnType("decimal(10, 2)");

            entity.HasOne(d => d.IdAlumnoNavigation).WithMany(p => p.TblFacturaColegiaturas)
                .HasForeignKey(d => d.IdAlumno)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__Tbl_Factu__IdAlu__160F4887");

            entity.HasOne(d => d.IdTipoColegiaturaNavigation).WithMany(p => p.TblFacturaColegiaturas)
                .HasForeignKey(d => d.IdTipoColegiatura)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Tbl_FacturaColegiatura_Tbl_CatTipoColegiatura");
        });

        modelBuilder.Entity<TblGrado>(entity =>
        {
            entity.HasKey(e => e.IdGrado);

            entity.ToTable("Tbl_Grados");

            entity.Property(e => e.FechaActualiza).HasColumnType("datetime");
            entity.Property(e => e.FechaRegistro).HasColumnType("datetime");
            entity.Property(e => e.NivelEducativo).HasMaxLength(50);
            entity.Property(e => e.NombreGrado).HasMaxLength(50);
        });

        modelBuilder.Entity<TblInventario>(entity =>
        {
            entity.HasKey(e => e.IdInventario);

            entity.ToTable("Tbl_Inventario");

            entity.Property(e => e.Descripcion).HasColumnType("text");
            entity.Property(e => e.FechaActualiza).HasColumnType("datetime");
            entity.Property(e => e.FechaMovimiento).HasColumnType("datetime");
            entity.Property(e => e.FechaRegistro).HasColumnType("datetime");
            entity.Property(e => e.ReferenciaDocumento).HasMaxLength(100);
            entity.Property(e => e.TipoMovimiento).HasMaxLength(50);

            entity.HasOne(d => d.IdProductoNavigation).WithMany(p => p.TblInventarios)
                .HasForeignKey(d => d.IdProducto)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Tbl_Inventario_Tbl_CatProductos");
        });

        modelBuilder.Entity<TblMateria>(entity =>
        {
            entity.HasKey(e => e.IdMateria);

            entity.ToTable("Tbl_Materias");

            entity.Property(e => e.Descripcion).HasColumnType("text");
            entity.Property(e => e.FechaActualiza).HasColumnType("datetime");
            entity.Property(e => e.FechaRegistro).HasColumnType("datetime");
            entity.Property(e => e.NombreMateria).HasMaxLength(100);
        });

        modelBuilder.Entity<TblMetodoPago>(entity =>
        {
            entity.HasKey(e => e.IdMetodoPago);

            entity.ToTable("Tbl_MetodoPago");

            entity.Property(e => e.Descripcion).HasColumnType("text");
            entity.Property(e => e.MetodoPago)
                .HasMaxLength(50)
                .IsUnicode(false);
        });

        modelBuilder.Entity<TblNota>(entity =>
        {
            entity.HasKey(e => e.IdNota);

            entity.ToTable("Tbl_Notas");

            entity.Property(e => e.PrimerCorte).HasColumnType("decimal(5, 2)");
            entity.Property(e => e.SegundoCorte).HasColumnType("decimal(5, 2)");
            entity.Property(e => e.TercerCorte).HasColumnType("decimal(5, 2)");
            entity.Property(e => e.CuartoCorte).HasColumnType("decimal(5, 2)");
            entity.Property(e => e.NotaFinal).HasColumnType("decimal(5, 2)");
            entity.Property(e => e.Descripcion).HasColumnType("text");
            entity.Property(e => e.FechaActualiza).HasColumnType("datetime");
            entity.Property(e => e.FechaRegistro).HasColumnType("datetime");

            //entity.HasOne(d => d.IdAsignacionNavigation).WithMany(p => p.TblNota)
            //    .HasForeignKey(d => d.IdAsignacion)
            //    .OnDelete(DeleteBehavior.ClientSetNull)
            //    .HasConstraintName("FK_Tbl_Notas_Tbl_CatAsignacionProfesorMateria");

            entity.HasOne(d => d.IdPeriodoNavigation).WithMany(p => p.TblNota)
                .HasForeignKey(d => d.IdPeriodo)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Tbl_Notas_Tbl_CatPeridoEvaluacion");

            entity.HasOne(d => d.IdTipoEvaluacionNavigation).WithMany(p => p.TblNota)
                .HasForeignKey(d => d.IdTipoEvaluacion)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Tbl_Notas_Tbl_CatTipoEvaluacion");

            entity.HasOne(d => d.IdAlumnoNavigation).WithMany(p => p.TblNotas)
                .HasForeignKey(d => d.IdAlumno)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Tbl_Notas_Tbl_Alumnos");

            entity.HasOne(d => d.IdAsignaturaNavigation).WithMany(p => p.TblNotas)
                .HasForeignKey(d => d.IdAsignatura)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Tbl_Notas_Tbl_CatAsignaturas");
        });

        modelBuilder.Entity<TblPago>(entity =>
        {
            entity.HasKey(e => e.IdPago);

               entity.ToTable("Tbl_Pagos");

            entity.Property(e => e.MontoPagado).HasColumnType("decimal(10, 2)");
            entity.Property(e => e.Observaciones).HasColumnType("text");
            entity.Property(e => e.Referencia)
                .HasMaxLength(100)
                .IsUnicode(false);

            entity.HasOne(d => d.IdAlumnoNavigation).WithMany()
                .HasForeignKey(d => d.IdAlumno)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Tbl_Pagos_Tbl_Alumno");
            entity.HasOne(d => d.IdMetodoPagoNavigation).WithMany()
                .HasForeignKey(d => d.IdMetodoPago)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Tbl_Pagos_Tbl_MetodoPago");
        });

        modelBuilder.Entity<TblRecibosCaja>(entity =>
        {
            entity
                .HasNoKey()
                .ToTable("Tbl_RecibosCaja");

            entity.Property(e => e.RecibiDe).HasColumnType("decimal(10, 2)");
            entity.Property(e => e.Serie).HasColumnType("text");
            entity.Property(e => e.NumeroRecibo)
                .HasMaxLength(100)
                .IsUnicode(false);

            entity.HasOne(d => d.IdPagoNavigation).WithMany()
                .HasForeignKey(d => d.IdPago)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Tbl_RecibosCaja_Tbl_Pagos");
        });

        modelBuilder.Entity<TblPagoColegiatura>(entity =>
        {
            entity.HasKey(e => e.IdPago);

            entity.ToTable("Tbl_PagoColegiatura");

            entity.Property(e => e.Descripcion).HasColumnType("text");
            entity.Property(e => e.FechaActualiza).HasColumnType("datetime");
            entity.Property(e => e.FechaPago).HasColumnType("datetime");
            entity.Property(e => e.FechaRegistro).HasColumnType("datetime");
            entity.Property(e => e.MontoPagado).HasColumnType("decimal(10, 2)");
            entity.Property(e => e.ReferenciaPago).HasMaxLength(100);

            entity.HasOne(d => d.IdFacturaNavigation).WithMany(p => p.TblPagoColegiaturas)
                .HasForeignKey(d => d.IdFactura)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Tbl_PagoColegiatura_Tbl_FacturaColegiatura");

            entity.HasOne(d => d.IdMetodoPagoNavigation).WithMany(p => p.TblPagoColegiaturas)
                .HasForeignKey(d => d.IdMetodoPago)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Tbl_PagoColegiatura_Tbl_MetodoPago");
        });

        modelBuilder.Entity<TblPlanCuenta>(entity =>
        {
            entity.HasKey(e => e.IdCuenta);

            entity.ToTable("Tbl_PlanCuentas");

            entity.Property(e => e.CodigoCuenta).HasMaxLength(50);
            entity.Property(e => e.FechaActualiza).HasColumnType("datetime");
            entity.Property(e => e.FechaRegistro).HasColumnType("datetime");
            entity.Property(e => e.NombreCuenta).HasMaxLength(150);
            entity.Property(e => e.SubTipoCuenta).HasMaxLength(50);
            entity.Property(e => e.TipoCuenta).HasMaxLength(50);
        });

        modelBuilder.Entity<TblProfesore>(entity =>
        {
            entity.HasKey(e => e.IdProfesor);

            entity.ToTable("Tbl_Profesores");

            entity.Property(e => e.Apellido).HasMaxLength(100);
            entity.Property(e => e.Correo).HasMaxLength(100);
            entity.Property(e => e.FechaActualiza).HasColumnType("datetime");
            entity.Property(e => e.FechaRegistro).HasColumnType("datetime");
            entity.Property(e => e.Nombre).HasMaxLength(100);
            entity.Property(e => e.Telefono).HasMaxLength(50);
        });

        modelBuilder.Entity<TblRol>(entity =>
        {
            entity.HasKey(e => e.IdRol);

            entity.ToTable("Tbl_Rol");

            entity.Property(e => e.Descripcion).HasColumnType("text");
            entity.Property(e => e.FechaActualiza).HasColumnType("datetime");
            entity.Property(e => e.FechaRegistro).HasColumnType("datetime");
            entity.Property(e => e.NombreRol).HasMaxLength(100);
        });

        modelBuilder.Entity<TblTransaccionesContable>(entity =>
        {
            entity.HasKey(e => e.IdTransaccion);

            entity.ToTable("Tbl_TransaccionesContables");

            entity.Property(e => e.Descripcion).HasColumnType("text");
            entity.Property(e => e.FechaActualiza).HasColumnType("datetime");
            entity.Property(e => e.FechaRegistro).HasColumnType("datetime");
            entity.Property(e => e.TipoDocOrigen).HasMaxLength(50);
        });

        modelBuilder.Entity<TblUsuario>(entity =>
        {
            entity.HasKey(e => e.IdUsuario);

            entity.ToTable("Tbl_Usuario");

            entity.Property(e => e.Correo).HasMaxLength(100);
            entity.Property(e => e.FechaActualiza).HasColumnType("datetime");
            entity.Property(e => e.FecharRegistro).HasColumnType("datetime");
            entity.Property(e => e.NombreCompleto).HasMaxLength(100);
            entity.Property(e => e.NombreUsuario).HasMaxLength(50);
            entity.Property(e => e.Password).HasMaxLength(255);
        });

        modelBuilder.Entity<TblUsuarioRol>(entity =>
        {
            entity
                .HasNoKey()
                .ToTable("Tbl_Usuario_Rol");

            entity.HasIndex(e => e.IdUsuario, "IX_Tbl_Usuario_Rol");

            entity.HasOne(d => d.IdRolNavigation).WithMany()
                .HasForeignKey(d => d.IdRol)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Tbl_Usuario_Rol_Tbl_Rol1");

            entity.HasOne(d => d.IdUsuarioNavigation).WithMany()
                .HasForeignKey(d => d.IdUsuario)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Tbl_Usuario_Rol_Tbl_Usuario1");
        });

        modelBuilder.Entity<TblVenta>(entity =>
        {
            entity.HasKey(e => e.IdVenta);

            entity.ToTable("Tbl_Ventas");

            entity.Property(e => e.IdVenta).ValueGeneratedNever();
            entity.Property(e => e.Descripcion).HasColumnType("text");
            entity.Property(e => e.EstadoVenta).HasMaxLength(50);
            entity.Property(e => e.FechaActualiza).HasColumnType("datetime");
            entity.Property(e => e.FechaRegistro).HasColumnType("datetime");
            entity.Property(e => e.FechaVenta).HasColumnType("datetime");
            entity.Property(e => e.TotalBruto).HasColumnType("decimal(10, 2)");
            entity.Property(e => e.TotalDescuento).HasColumnType("decimal(10, 2)");
            entity.Property(e => e.TotalImpuesto).HasColumnType("decimal(10, 2)");
            entity.Property(e => e.TotalNeto).HasColumnType("decimal(10, 2)");

            entity.HasOne(d => d.IdAlumnoNavigation).WithMany(p => p.TblVenta)
                .HasForeignKey(d => d.IdAlumno)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Tbl_Ventas_Tbl_Alumnos");
        });

        OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
