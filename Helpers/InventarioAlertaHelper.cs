using WebColegio.Models;

namespace WebColegio.Helpers
{
    public static class InventarioAlertaHelper
    {
        public const int StockMinimoPorDefecto = 10;

        public static int StockMinimoEfectivo(int stockMinimo)
            => stockMinimo > 0 ? stockMinimo : StockMinimoPorDefecto;

        public static int NormalizarStockMinimo(int stockMinimo)
            => StockMinimoEfectivo(stockMinimo);

        public static bool EstaBajoMinimo(int stockActual, int stockMinimo)
            => stockActual <= StockMinimoEfectivo(stockMinimo);

        public static bool EstaBajoMinimo(Productos? producto)
            => producto != null && EstaBajoMinimo(producto.StockActual, producto.StockMinimo);

        public static string TextoAlerta(Productos producto)
        {
            int minimo = StockMinimoEfectivo(producto.StockMinimo);
            return $"{producto.NombreProducto} (stock {producto.StockActual}, mínimo {minimo})";
        }
    }
}
