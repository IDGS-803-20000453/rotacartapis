namespace rutacart.Models;

public class Pedido
{
    public int PedidoID { get; set; }
    public int UsuarioID { get; set; }
    public DateTime FechaPedido { get; set; }
    public string Estado { get; set; }
    public decimal Total { get; set; }
    public string DireccionEnvio { get; set; }
    public decimal PesoTotal { get; set; }
    public decimal VolumenTotal { get; set; }
    public DateTime FechaEntrega { get; set; }

    public Usuarios Usuarios { get; set; }
    public ICollection<DetallePedido> DetallesPedido { get; set; }
}
