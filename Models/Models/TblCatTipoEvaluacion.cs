using System;
using System.Collections.Generic;

namespace Api_Colegio.Models;

public partial class TblCatTipoEvaluacion
{
    public int IdTipoEvaluacion { get; set; }

    public string NombreTipEvaluacion { get; set; } = null!;

    public decimal Porcentaje { get; set; }

    public bool Activo { get; set; }

    public int UsuarioRegistro { get; set; }

    public DateTime FechaRegistro { get; set; }

    public int? UsuarioActualiza { get; set; }

    public DateTime? FechaActualiza { get; set; }

    public virtual ICollection<TblNota> TblNota { get; set; } = new List<TblNota>();
}
