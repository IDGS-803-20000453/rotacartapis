using Microsoft.EntityFrameworkCore;
using rutacart.Models;

namespace rutacart.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        public DbSet<Usuarios> Usuarios { get; set; }
        public DbSet<Producto> Productos { get; set; }
        public DbSet<Categoria> Categorias { get; set; }
        public DbSet<Carrito> Carritos { get; set; }
        public DbSet<ItemCarrito> ItemCarrito { get; set; }
        public DbSet<DetallePedido> DetallesPedido { get; set; }
        public DbSet<Envio> Envios { get; set; }
        public DbSet<Pedido> Pedidos { get; set; }
        public DbSet<Proveedor> Proveedores { get; set; }


        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Configuración de la relación entre Usuarios y Pedidos
            modelBuilder.Entity<Pedido>()
                .HasOne(p => p.Usuarios) // Asegúrate de que aquí usas el nombre de la propiedad de navegación en plural, si así lo has decidido mantener.
                .WithMany(u => u.Pedidos) // Asegúrate de que la propiedad de navegación en Usuarios sea una colección y se llame 'Pedidos'.
                .HasForeignKey(p => p.UsuarioID); // El nombre de la propiedad de clave foránea en Pedido que enlaza a Usuarios.

            // Configuraciones adicionales para otras relaciones...
        }
    }
}
