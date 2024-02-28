namespace rutacart.Models;

public class Carrito
{
    public int CarritoID { get; set; }
    public int UsuarioID { get; set; }
    public DateTime FechaCreacion { get; set; }
    public DateTime UltimaModificacion { get; set; }

    public Usuarios Usuario { get; set; }
    public ICollection<ItemCarrito> ItemsCarrito { get; set; }
}
