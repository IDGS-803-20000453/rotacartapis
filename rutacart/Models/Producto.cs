namespace rutacart.Models;

public class Producto
{
    public int ProductoID { get; set; }
    public string Nombre { get; set; }
    public string Descripcion { get; set; }
    public decimal Precio { get; set; }
    public decimal Peso { get; set; }
    public decimal Volumen { get; set; }
    public int Stock { get; set; }
    public int CategoriaID { get; set; }
    public string ImagenURL { get; set; }

    public Categoria Categoria { get; set; }
    public ICollection<ItemCarrito> ItemsCarrito { get; set; }
}
