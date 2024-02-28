namespace rutacart.Models;

public class Envio
{
    public int EnvioID { get; set; }
    public int PedidoID { get; set; }
    public int ProveedorID { get; set; }
    public decimal CostoEnvio { get; set; }
    public DateTime FechaEnvio { get; set; }
    public DateTime FechaEntregaEstimada { get; set; }
    public string Estado { get; set; }

    public Pedido Pedido { get; set; }
    public Proveedor Proveedor { get; set; }
}
