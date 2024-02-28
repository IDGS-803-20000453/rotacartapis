namespace rutacart.Models;
public class PedidoConDetallesDto
{
    public int UsuarioID { get; set; }
    public DateTime FechaPedido { get; set; } = DateTime.UtcNow; // O la fecha que corresponda
    public string Estado { get; set; }
    public decimal Total { get; set; } // Esto podría calcularse en base a los detalles, si es necesario
    public string DireccionEnvio { get; set; }
    public decimal PesoTotal { get; set; } // Esto podría calcularse en base a los detalles, si es necesario
    public decimal VolumenTotal { get; set; } // Esto podría calcularse en base a los detalles, si es necesario
    public DateTime FechaEntrega { get; set; }

    public List<DetallePedidoDto> Detalles { get; set; }
}

public class DetallePedidoDto
{
    public int ProductoID { get; set; }
    public decimal Precio { get; set; } // Podría obtenerse del producto directamente, si es fijo
    public int Cantidad { get; set; }
}
