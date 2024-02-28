using System.Collections.Generic;

namespace rutacart.Models
{
    public class Categoria
    {
        public Categoria()
        {
            // Inicializa la colección para asegurar que no sea null
            Productos = new HashSet<Producto>();
        }

        public int CategoriaID { get; set; }
        public string Nombre { get; set; }
        public string Descripcion { get; set; }

        // La colección de productos ahora es opcional gracias a la inicialización en el constructor
        public virtual ICollection<Producto> Productos { get; set; }
    }
}
