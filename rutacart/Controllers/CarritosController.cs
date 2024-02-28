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

        // Agregar producto al carrito
        [HttpPost("AgregarProducto")] // Define la ruta específica para este action
        public IActionResult AgregarProductoAlCarrito(int carritoId, int productoId, int cantidad)
        {
            using (var conn = new SqlConnection(connectionString))
            {
                conn.Open();
                using (var cmd = new SqlCommand("AgregarProductoAlCarrito", conn))
                {
                    cmd.CommandType = CommandType.StoredProcedure;
                    cmd.Parameters.Add(new SqlParameter("@CarritoID", carritoId));
                    cmd.Parameters.Add(new SqlParameter("@ProductoID", productoId));
                    cmd.Parameters.Add(new SqlParameter("@Cantidad", cantidad));

                    cmd.ExecuteNonQuery();
                }
            }
            return Ok(); // Retorna un resultado más apropiado para una API
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
    }
}
