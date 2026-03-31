using WebColegio.Models;

namespace WebColegio;

public class NotasLibretaCeldaModel
{
    public TblNotas Nota { get; set; } = null!;
    public bool SoloCualitativo { get; set; }
}
