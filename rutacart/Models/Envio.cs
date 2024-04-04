namespace rutacart.Models;

public class Envio
{
    public int EnvioID { get; set; }
    public int PedidoID { get; set; }
    public int? ProveedorID { get; set; }
    public decimal? CostoEnvio { get; set; } // Hacerlo anulable si puede ser nulo en la DB
    public DateTime? FechaEnvio { get; set; } // Hacerlo anulable si puede ser nulo en la DB
    public DateTime? FechaEntregaEstimada { get; set; } // Hacerlo anulable si puede ser nulo en la DB
    public string? Estado { get; set; }

    public Pedido Pedido { get; set; }
    public Proveedor Proveedor { get; set; }
}
