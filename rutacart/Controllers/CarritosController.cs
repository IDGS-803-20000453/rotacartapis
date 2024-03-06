using Microsoft.AspNetCore.Mvc;
using System.Data;
using Microsoft.Extensions.Configuration;
using System.Data.SqlClient;

namespace rutacart.Controllers
{
    [ApiController] // Asegura que el controlador sea tratado como API Controller
    [Route("api/[controller]")] // Define una ruta base para el controlador
    public class CarritosController : ControllerBase
    {
        private readonly IConfiguration _configuration;
        private readonly string connectionString;

        public CarritosController(IConfiguration configuration)
        {
            _configuration = configuration;
            connectionString = _configuration.GetConnectionString("DefaultConnection");
        }

        


        [HttpPost("EliminarProducto")]
        public IActionResult EliminarProductoDelCarrito(int carritoId, int productoId)
        {
            using (var conn = new SqlConnection(connectionString))
            {
                conn.Open();
                using (var cmd = new SqlCommand("EliminarProductoDelCarrito", conn))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.Add(new SqlParameter("@CarritoID", carritoId));
                    cmd.Parameters.Add(new SqlParameter("@ProductoID", productoId));

                    cmd.ExecuteNonQuery();
                }
            }
            return Ok();
        }

        [HttpPost("VaciarCarrito")]
        public IActionResult VaciarCarritoPorUsuario(int usuarioId)
        {
            using (var conn = new SqlConnection(connectionString))
            {
                conn.Open();
                using (var cmd = new SqlCommand("VaciarCarrito", conn))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    // Asumiendo que cambias el nombre del parámetro a @UsuarioID
                    cmd.Parameters.Add(new SqlParameter("@UsuarioID", usuarioId));

                    cmd.ExecuteNonQuery();
                }
            }
            return Ok();
        }

        [HttpPost("AgregarOActualizarProducto/{usuarioId}/{productoId}/{cantidad}")]
        public IActionResult AgregarOActualizarProductoEnCarrito(int usuarioId, int productoId, int cantidad)
        {
            using (var conn = new SqlConnection(connectionString))
            {
                conn.Open();
                using (var cmd = new SqlCommand("AgregarOActualizarProductoEnCarrito", conn))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.Add(new SqlParameter("@UsuarioID", usuarioId)); // Cambiado a UsuarioID
                    cmd.Parameters.Add(new SqlParameter("@ProductoID", productoId));
                    cmd.Parameters.Add(new SqlParameter("@Cantidad", cantidad));

                    cmd.ExecuteNonQuery();
                }
            }
            return Ok();
        }


        // Finalizar pedido
        [HttpPost("FinalizarPedido")] // Define la ruta específica para este action
        public IActionResult FinalizarPedido(int carritoId)
        {
            using (var conn = new SqlConnection(connectionString))
            {
                conn.Open();
                using (var cmd = new SqlCommand("FinalizarPedido", conn))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.Add(new SqlParameter("@CarritoID", carritoId));

                    cmd.ExecuteNonQuery();
                }
            }
            return Ok(); // Retorna un resultado más apropiado para una API
        }




        [HttpPost("ReducirProducto")]
        public IActionResult ReducirProductoDelCarritoPorUsuario(int usuarioId, int productoId)
        {
            using (var conn = new SqlConnection(connectionString))
            {
                conn.Open();
                using (var cmd = new SqlCommand("ReducirProductoDelCarritoPorUsuario", conn))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.Add(new SqlParameter("@UsuarioID", usuarioId));
                    cmd.Parameters.Add(new SqlParameter("@ProductoID", productoId));

                    cmd.ExecuteNonQuery();
                }
            }
            return Ok();
        }

        public class DetalleCarrito
        {
            public int ItemCarritoID { get; set; }
            public int ProductoID { get; set; }
            public string Nombre { get; set; }
            public string ImagenURL { get; set; }
            public int Cantidad { get; set; }
            public decimal Precio { get; set; }
            public decimal Peso { get; set; }
            public decimal Volumen { get; set; }
            public int CarritoID { get; set; }
            public int UsuarioID { get; set; }
        }

        [HttpGet("DetalleCarritoPorUsuario/{usuarioId}")]
        public IActionResult ObtenerDetalleCarritoPorUsuario(int usuarioId)
        {
            List<DetalleCarrito> detallesCarrito = new List<DetalleCarrito>();
            using (var conn = new SqlConnection(connectionString))
            {
                conn.Open();
                var query = @"SELECT ItemCarritoID,ProductoID, Nombre, ImagenURL, Cantidad, Precio, Peso, Volumen, CarritoID, UsuarioID 
                      FROM VistaDetalleCarrito 
                      WHERE UsuarioID = @UsuarioID";
                using (var cmd = new SqlCommand(query, conn))
                {
                    cmd.Parameters.AddWithValue("@UsuarioID", usuarioId);
                    using (var reader = cmd.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            detallesCarrito.Add(new DetalleCarrito
                            {
                                ItemCarritoID = reader.GetInt32(reader.GetOrdinal("ItemCarritoID")),
                                ProductoID = reader.GetInt32(reader.GetOrdinal("ProductoID")),
                                Nombre = reader.GetString(reader.GetOrdinal("Nombre")),
                                ImagenURL = reader.GetString(reader.GetOrdinal("ImagenURL")),
                                Cantidad = reader.GetInt32(reader.GetOrdinal("Cantidad")),
                                Precio = reader.GetDecimal(reader.GetOrdinal("Precio")),
                                Peso = reader.GetDecimal(reader.GetOrdinal("Peso")),
                                Volumen = reader.GetDecimal(reader.GetOrdinal("Volumen")),
                                CarritoID = reader.GetInt32(reader.GetOrdinal("CarritoID")),
                                UsuarioID = reader.GetInt32(reader.GetOrdinal("UsuarioID"))
                            });
                        }
                    }
                }
            }
            return Ok(detallesCarrito);
        }



    }
}
    

    
