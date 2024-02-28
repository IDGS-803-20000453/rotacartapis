namespace rutacart.Models;

public class Proveedor
{
    public int ProveedorID { get; set; }
    public string Nombre { get; set; }
    public string Direccion { get; set; }
    public string Telefono { get; set; }
    public string Email { get; set; }

    // Esta es una relación opcional. No es necesario tener envíos para crear un proveedor.
    public virtual ICollection<Envio> Envios { get; set; }
}
