namespace rutacart.Models;

public class ItemCarrito
{
    public int ItemCarritoID { get; set; }
    public int CarritoID { get; set; }
    public int ProductoID { get; set; }
    public int Cantidad { get; set; }
    public decimal Precio { get; set; }

    public Carrito Carrito { get; set; }
    public Producto Producto { get; set; }
}
