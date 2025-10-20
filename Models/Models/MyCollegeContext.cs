using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;

namespace Api_Colegio.Models;

public partial class MyCollegeContext : DbContext
{
    public MyCollegeContext()
    {
    }

    public MyCollegeContext(DbContextOptions<MyCollegeContext> options)
        : base(options)
    {
    }

    public virtual DbSet<TblAlumno> TblAlumnos { get; set; }

    public virtual DbSet<TblCatAsignacionProfesorMaterium> TblCatAsignacionProfesorMateria { get; set; }

    public virtual DbSet<TblCatAsignatura> TblCatAsignaturas { get; set; }

    public virtual DbSet<TblCatCuenta> TblCatCuentas { get; set; }

    public virtual DbSet<TblCatDetalleVenta> TblCatDetalleVentas { get; set; }

    public virtual DbSet<TblCatGrupo> TblCatGrupos { get; set; }

    public virtual DbSet<TblCatModalidad> TblCatModalidads { get; set; }

    public virtual DbSet<TblCatPeridoEvaluacion> TblCatPeridoEvaluacions { get; set; }

    public virtual DbSet<TblCatProducto> TblCatProductos { get; set; }

    public virtual DbSet<TblCatProveedore> TblCatProveedores { get; set; }

    public virtual DbSet<TblCatRecinto> TblCatRecintos { get; set; }

    public virtual DbSet<TblCatSexo> TblCatSexos { get; set; }

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

    public virtual DbSet<TblPagos> TblPagos { get; set; }    

    public virtual DbSet<TblPagoColegiatura> TblPagoColegiaturas { get; set; }

    public virtual DbSet<TblPlanCuenta> TblPlanCuentas { get; set; }

    public virtual DbSet<TblProfesore> TblProfesores { get; set; }

    public virtual DbSet<TblRecibosCaja> TblRecibosCajas { get; set; }

    public virtual DbSet<TblReciboEgreso> TblReciboEgreso { get; set; }
    

    public virtual DbSet<TblRol> TblRols { get; set; }

    public virtual DbSet<TblTransaccionesContable> TblTransaccionesContables { get; set; }

    public virtual DbSet<TblUsuario> TblUsuarios { get; set; }

    public virtual DbSet<TblUsuarioRol> TblUsuarioRols { get; set; }

    public virtual DbSet<TblVenta> TblVentas { get; set; }

    public virtual DbSet<TblCatTipoRecibo> TblTipoRecibos { get; set; }

    public virtual DbSet<TblCatTipoMovimiento> TblTipoMovimientos { get; set; }
    public virtual DbSet<TblCatDiscapacidad> TblDiscapacidad { get; set; }

    public virtual DbSet<TblCatMovimientoInventario> TblMovInventario { get; set; }

    public virtual DbSet<TblCatMesesPendientes> TblMesesPendientes { get; set; }
    public virtual DbSet<TblCatDetallePagos> TblDetallePagos { get; set; }
    public virtual DbSet<TblCatMeses> TblMeses { get; set; }
    public virtual DbSet<TblCatTiposDescuento> TblTipoDescuento { get; set; }

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
            entity.Property(e => e.Cedula).HasMaxLength(20);
            entity.Property(e => e.CodigoAlumno)
                .HasMaxLength(20)
                .IsUnicode(false);
            entity.Property(e => e.CodigoMINED)
                .HasMaxLength(50)
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
            entity.HasOne(d => d.IdGradoNavigation)
        .WithMany(p => p.TblAlumno)
        .HasForeignKey(d => d.IdGrado)
        .OnDelete(DeleteBehavior.ClientSetNull) // opcional
        .HasConstraintName("FK__Tbl_Alumn__IdGra__7167D3BD");

            entity.HasOne(d => d.IdGrupoNavigation)
       .WithMany(p => p._tblAlumno)
       .HasForeignKey(d => d.IdGrupo)
       .OnDelete(DeleteBehavior.ClientSetNull) // opcional
       .HasConstraintName("[FK__Tbl_Alumn__IdGru__7720AD13]");

            entity.HasOne(d => d.IdModalidadNavigation)
        .WithMany(p => p._tblAlumno)
        .HasForeignKey(d => d.IdModalidad)
        .OnDelete(DeleteBehavior.ClientSetNull) // opcional
        .HasConstraintName("FK__Tbl_Alumn__IdMod__73501C2F");

            entity.HasOne(d => d.IdSexoNavigation)
       .WithMany(p => p._tblAlumno)
       .HasForeignKey(d => d.IdSexo)
       .OnDelete(DeleteBehavior.ClientSetNull) // opcional
       .HasConstraintName("FK__Tbl_Alumn__IdSex__753864A1");

            entity.HasOne(d => d.IdTurnoNavigation)
       .WithMany(p => p._tblAlumno)
       .HasForeignKey(d => d.IdTurno)
       .OnDelete(DeleteBehavior.ClientSetNull) // opcional
       .HasConstraintName("FK__Tbl_Alumn__IdTur__725BF7F6");

            entity.HasOne(d => d.IdRecintoNavigation)
      .WithMany(p => p._tblAlumno)
      .HasForeignKey(d => d.IdRecinto)
      .OnDelete(DeleteBehavior.ClientSetNull) // opcional
      .HasConstraintName("FK__Tbl_Alumn__IdRec__762C88DA");

            entity.HasOne(d => d.IdDiscapacidadNavigation)
     .WithMany(p => p.TblAlumnos)
     .HasForeignKey(d => d.IdDiscapacidad)
     .OnDelete(DeleteBehavior.ClientSetNull) // opcional
     .HasConstraintName("FK__Tbl_Alumn__IdDis__23BE4960");

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
            entity.HasKey(e => e.IdAsignatura).HasName("PK_IdAsignatura");

            entity.ToTable("Tbl_CatAsignaturas");

            entity.Property(e => e.FechaActualiza).HasColumnType("datetime");
            entity.Property(e => e.FechaRegistro).HasColumnType("datetime");
            entity.Property(e => e.NombreAsignatura).HasMaxLength(100);
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


        modelBuilder.Entity<TblCatDetallePagos>(entity =>
        {
            entity.HasKey(e => e.IdDetPago);

            entity.ToTable("Tbl_CatDetallePagos");

            entity.Property(e => e.MontoOriginal).HasColumnType("decimal(10, 2)");
            entity.Property(e => e.MontoPagado).HasColumnType("decimal(10, 2)");
            

            entity.HasOne(d => d.IdMesesNavigation).WithMany(p => p.TblCatDetallePagos)
                .HasForeignKey(d => d.IdMes)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__Tbl_CatDe__IdMes__46136164");

            entity.HasOne(d => d.IdPagosNavigation).WithMany(p => p.TblCatDetallePagos)
                .HasForeignKey(d => d.IdPago)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__DetallePa__IdPag__3C89F72A");
            entity.HasOne(d => d.IdTipoDescuentoNavigation).WithMany(p => p.TblCatDetallePagos)
                .HasForeignKey(d => d.IdTipoDescuento)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__DetallePa__TipoD__3D7E1B63");
            entity.HasOne(d => d.IdEstadoPagoNavigation).WithMany(p => p.TblCatDetallePagos)
                .HasForeignKey(d => d.IdEstadoPago)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("[FK__DetallePa__IdEst__3E723F9C]");
        });


        modelBuilder.Entity<TblCatMesesPendientes>(entity =>
        {
            entity.HasKey(e => e.IdMesPendiente);

            entity.ToTable("Tbl_CatMesesPendientes");

            entity.Property(e => e.Monto).HasColumnType("decimal(10, 2)");            


            entity.HasOne(d => d.IdEstadoPagoNavigation).WithMany(p => p.TblCatMesesPendientes)
                .HasForeignKey(d => d.IdEstadoPago)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("[FK__MesesPend__IdEst__4336F4B9]");

            entity.HasOne(d => d.IdMesesNavigation).WithMany(p => p.TblCatMesesPendientes)
                .HasForeignKey(d => d.IdMes)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__Tbl_CatMe__IdMes__4707859D");
            entity.HasOne(d => d.IdAlumnoNavigation).WithMany(p => p.TblMesesPendientes)
                .HasForeignKey(d => d.IdAlumno)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__MesesPend__IdAlu__4242D080");
            
        });


        modelBuilder.Entity<TblCatTiposDescuento>(entity =>
        {
            entity.HasKey(e => e.IdTipoDescuento);

            entity.ToTable("Tbl_CatTiposDecuento");

            entity.Property(e => e.NombreDescuento).HasMaxLength(50);
            entity.Property(e => e.FechaActualizo).HasColumnType("datetime");
            entity.Property(e => e.FechaRegistro).HasColumnType("datetime");

           

        });

        modelBuilder.Entity<TblCatMeses>(entity =>
        {
            entity.HasKey(e => e.IdMes);

            entity.ToTable("Tbl_CatMeses");

           
            entity.Property(e => e.FechaActualizo).HasColumnType("datetime");
            entity.Property(e => e.FechaRegistro).HasColumnType("datetime");
            entity.Property(e => e.Mes).HasMaxLength(20);


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



        modelBuilder.Entity<TblCatDiscapacidad>(entity =>
        {
            entity.HasKey(e => e.Id_Discapacidad);

            entity.ToTable("Tbl_CatDiscapacidad");

            entity.Property(e => e.Discapacidad).HasMaxLength(100);
         
            entity.Property(e => e.FechaActualizo).HasColumnType("datetime");
            entity.Property(e => e.FechaRegistro).HasColumnType("datetime");         

            
        });
        modelBuilder.Entity<TblCatMovimientoInventario>(entity =>
        {
            entity.HasKey(e => e.IdMovInventario);

            entity.ToTable("Tbl_CatMovInventario");

            entity.Property(e => e.MovimientoInventario).HasMaxLength(50);

            entity.Property(e => e.FechaActualizo).HasColumnType("datetime");
            entity.Property(e => e.FechaRegistro).HasColumnType("datetime");


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

        modelBuilder.Entity<TblCatSexo>(entity =>
        {
            entity.HasKey(e => e.IdSexo);

            entity.ToTable("Tbl_CatSexo");

            entity.Property(e => e.FechaActualiza).HasColumnType("datetime");
            entity.Property(e => e.FechaRegistro).HasColumnType("datetime");
            entity.Property(e => e.Sexo).HasMaxLength(50);
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
            

            entity.HasOne(d => d.IdProductoNavigation).WithMany(p => p.TblInventarios)
                .HasForeignKey(d => d.IdProducto)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_Tbl_Inventario_Tbl_CatProductos");

            entity.HasOne(d => d.IdMovInventarioNavigation).WithMany(p => p.TblInventario)
               .HasForeignKey(d => d.TipoMovimiento)
               .OnDelete(DeleteBehavior.ClientSetNull)
               .HasConstraintName("[FK__Tbl_Inven__TipoM__37C5420D]");
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
        modelBuilder.Entity<TblCatTipoRecibo>(entity =>
        {
            entity.HasKey(e => e.IdTipoRecibo);

            entity.ToTable("Tbl_CatTipoRecibo");

            entity.Property(e => e.Descripcion).HasColumnType("text");
            entity.Property(e => e.FechaActualizo).HasColumnType("datetime");
            entity.Property(e => e.FechaRegistro).HasColumnType("datetime");
            entity.Property(e => e.TipoRecibo).HasMaxLength(100);
        });

        modelBuilder.Entity<TblCatTipoMovimiento>(entity =>
        {
            entity.HasKey(e => e.IdTipoMovimiento);

            entity.ToTable("Tbl_CatTipoMovimiento");

            entity.Property(e => e.Descripcion).HasColumnType("text");
            entity.Property(e => e.FechaActualizo).HasColumnType("datetime");
            entity.Property(e => e.FechaRegistro).HasColumnType("datetime");
            entity.Property(e => e.Concepto).HasMaxLength(100);
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

            entity.Property(e => e.CuartoCorte).HasColumnType("decimal(5, 2)");
            entity.Property(e => e.Descripcion).HasColumnType("text");
            entity.Property(e => e.FechaActualiza).HasColumnType("datetime");
            entity.Property(e => e.FechaRegistro).HasColumnType("datetime");
            entity.Property(e => e.NotaFinal).HasColumnType("decimal(5, 2)");
            entity.Property(e => e.PrimerCorte).HasColumnType("decimal(5, 2)");
            entity.Property(e => e.SegundoCorte).HasColumnType("decimal(5, 2)");
            entity.Property(e => e.TercerCorte).HasColumnType("decimal(5, 2)");

            entity.HasOne(d => d.IdAsignaturaNavigation).WithMany(p => p.TblNotas)
                .HasForeignKey(d => d.IdAsignatura)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__Tbl_Notas__IdAsi__7E02B4CC");

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
              .HasConstraintName("[FK__Tbl_Notas__IdAlu__0FEC5ADD]");

            entity.HasOne(d => d.IdModalidadNavigation).WithMany(p => p._tblNotas)
              .HasForeignKey(d => d.IdModalidad)
              .OnDelete(DeleteBehavior.ClientSetNull)
              .HasConstraintName("FK__Tbl_Notas__IdMod__10E07F16");

            entity.HasOne(d => d.IdGradoNavigation).WithMany(p => p._tblNotas)
             .HasForeignKey(d => d.IdGrado)
             .OnDelete(DeleteBehavior.ClientSetNull)
             .HasConstraintName("FK__Tbl_Notas__IdGra__0EF836A4");
        });

        modelBuilder.Entity<TblPagos>(entity =>
        {
            entity.HasKey(e => e.IdPago);

            entity.ToTable("TblPagos");

            //entity.Property(e => e.IdPago).ValueGeneratedNever();
            entity.Property(e => e.FechaActualizo).HasColumnType("datetime");
            entity.Property(e => e.FechaRegistro).HasColumnType("datetime");
            entity.Property(e => e.IdAlumno).ValueGeneratedOnAdd();
            entity.Property(e => e.IdMetodoPago).ValueGeneratedOnAdd();
            entity.Property(e => e.Monto).HasColumnType("decimal(10, 2)");
            

            entity.HasOne(d => d.IdAlumnoNavigation).WithMany(p => p.TblPagos)
                .HasForeignKey(d => d.IdAlumno)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__TblPagos__IdAlum__51EF2864");

            entity.HasOne(d => d.IdMetodoPagoNavigation).WithMany(p => p.TblPagos)
                .HasForeignKey(d => d.IdMetodoPago)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__TblPagos__IdMeto__6ABAD62E");


            entity.HasOne(d => d.IdTipomovimientoNavigation).WithMany(p => p.tblPagos)
               .HasForeignKey(d => d.IdTipoMovimiento)
               .OnDelete(DeleteBehavior.ClientSetNull)
               .HasConstraintName("FK__TblPagos__IdTipo__595B4002");
            entity.HasOne(d => d.IdTipoReciboNavigation).WithMany(p => p.tblPagos)
               .HasForeignKey(d => d.IdTipoRecibo)
               .OnDelete(DeleteBehavior.ClientSetNull)
               .HasConstraintName("[FK__TblPagos__IdTipo__0539C240]");
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

        modelBuilder.Entity<TblRecibosCaja>(entity =>
        {
            entity.HasKey(e => e.IdRecibo).HasName("PK__Tbl_Reci__2FEC473166654DBF");

            entity.ToTable("Tbl_RecibosCaja");

            entity.Property(e => e.FechaActualizo).HasColumnType("datetime");
            entity.Property(e => e.FechaRegistro).HasColumnType("datetime");
            entity.Property(e => e.RecibeDe)
                .HasMaxLength(100)
                .IsUnicode(false);
            entity.Property(e => e.Serie)
                .HasMaxLength(5)
                .HasDefaultValue("A");

            //entity.HasOne(d => d.IdPagoNavigation).WithMany(p => p.TblRecibosCajas)
            //    .HasForeignKey(d => d.IdPago)
            //    .OnDelete(DeleteBehavior.ClientSetNull)
            //    .HasConstraintName("FK__Tbl_Recib__IdPag__41B8C09B");
            entity.HasOne(d => d.IdGradoNavigation).WithMany(p => p._tblCaja)
                .HasForeignKey(d => d.IdGrado)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK__Tbl_Recib__IdGra__56B3DD81");
        });

        modelBuilder.Entity<TblReciboEgreso>(entity =>
        {
            entity.HasKey(e => e.IdReciboEgreso);//.HasName("PK__Tbl_Reci__2FEC473166654DBF");

            entity.ToTable("Tbl_ReciboEgreso");

            entity.Property(e => e.FechaActualizo).HasColumnType("datetime");
            entity.Property(e => e.FechaRegistro).HasColumnType("datetime");
            entity.Property(e => e.Pagar)
                .HasMaxLength(100)
                .IsUnicode(false);
            entity.Property(e => e.Serie)
                .HasMaxLength(5)
                .HasDefaultValue("A");

            //entity.HasOne(d => d.IdPagoNavigation).WithMany(p => p.TblRecibosCajas)
            //    .HasForeignKey(d => d.IdPago)
            //    .OnDelete(DeleteBehavior.ClientSetNull)
            //    .HasConstraintName("FK__Tbl_Recib__IdPag__41B8C09B");
            //entity.HasOne(d => d.IdGradoNavigation).WithMany(p => p._tblCaja)
            //    .HasForeignKey(d => d.IdGrado)
            //    .OnDelete(DeleteBehavior.ClientSetNull)
            //    .HasConstraintName("FK__Tbl_Recib__IdGra__56B3DD81");
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
