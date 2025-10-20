using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace Api_Colegio.Models;

public partial class TblEstadoPago
{
    public int IdEstadoPago { get; set; }

    public string EstadoPago { get; set; } = null!;

    public string? Descripcion { get; set; }

    public bool Activo { get; set; }

    public DateOnly FecharRegistra { get; set; }

    public int IdUsuarioRegistra { get; set; }

    public DateOnly? FecharActualiza { get; set; }

    public int? IdUsuarioActualiza { get; set; }

    [JsonIgnore]
    public virtual ICollection<TblCatDetallePagos> TblCatDetallePagos { get; set; }
    [JsonIgnore]
    public virtual ICollection<TblCatMesesPendientes> TblCatMesesPendientes { get; set; }
}
