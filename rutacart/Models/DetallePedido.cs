namespace rutacart.Models;

public class DetallePedido
{
    public int DetallePedidoID { get; set; }
    public int PedidoID { get; set; }
    public int ProductoID { get; set; }
    public decimal Precio { get; set; }
    public int Cantidad { get; set; }
    // Subtotal se manejará en el código, no necesita ser una propiedad almacenada

    public Pedido Pedido { get; set; }
    public Producto Producto { get; set; }
}
