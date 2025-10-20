using System;
using System.Collections.Generic;

namespace Api_Colegio.Models;

public partial class TblUsuarioRol
{
    public int IdUsuario { get; set; }

    public int IdRol { get; set; }

    public virtual TblRol IdRolNavigation { get; set; } = null!;

    public virtual TblUsuario IdUsuarioNavigation { get; set; } = null!;
}
