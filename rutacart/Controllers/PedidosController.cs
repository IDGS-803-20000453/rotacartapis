using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using rutacart.Data;
using rutacart.Models;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using System.Data.SqlClient;
using System.Data.Common;

namespace rutacart.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PedidosController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public PedidosController(ApplicationDbContext context)
        {
            _context = context;
        }




        // GET: api/Pedidos
        [HttpGet]
        public async Task<ActionResult<IEnumerable<PedidosDto>>> GetPedidos(int? usuarioId = null)
        {
            try
            {
                var query = _context.Pedidos.AsQueryable();

                if (usuarioId.HasValue)
                {
                    var usuario = await _context.Usuarios.FindAsync(usuarioId.Value);
                    if (usuario == null)
                    {
                        return NotFound(new { message = $"No se encontró el usuario con ID {usuarioId.Value}." });
                    }

                    query = query.Where(p => p.UsuarioID == usuarioId.Value);
                }

                var pedidosList = await query
                    .Select(p => new PedidosDto
                    {
                        PedidoID = p.PedidoID,
                        UsuarioID = p.UsuarioID , // Asegúrate de manejar los nulos aquí según tu lógica de negocio.
                        FechaPedido = p.FechaPedido,
                        Estado = p.Estado,
                        Total = p.Total,
                        DireccionEnvio = p.DireccionEnvio,
                        PesoTotal = p.PesoTotal,
                        VolumenTotal = p.VolumenTotal,
                        FechaEntrega = p.FechaEntrega
                    })
                    .ToListAsync();

                return pedidosList;
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred while processing your request.", details = ex.Message });
            }
        }

        public class PedidosDto
        {
            public int PedidoID { get; set; }
            public int? UsuarioID { get; set; }
            public DateTime? FechaPedido { get; set; }
            public string? Estado { get; set; }
            public decimal? Total { get; set; }
            public string? DireccionEnvio { get; set; }
            public decimal? PesoTotal { get; set; }
            public decimal? VolumenTotal { get; set; }
            public DateTime? FechaEntrega { get; set; }
        }

        

        // POST: api/Pedidos
        [HttpPost]
        public async Task<ActionResult<Pedido>> PostPedido([FromBody] PedidoConDetallesDto pedidoDto)
        {
            var pedido = new Pedido
            {
                UsuarioID = pedidoDto.UsuarioID,
                FechaPedido = pedidoDto.FechaPedido,
                Estado = pedidoDto.Estado,
                Total = pedidoDto.Total,
                DireccionEnvio = pedidoDto.DireccionEnvio,
                PesoTotal = pedidoDto.PesoTotal,
                VolumenTotal = pedidoDto.VolumenTotal,
                FechaEntrega = pedidoDto.FechaEntrega,
            };

            _context.Pedidos.Add(pedido);
            await _context.SaveChangesAsync(); // Guarda el pedido para generar el PedidoID necesario para los detalles

            foreach (var detalleDto in pedidoDto.Detalles)
            {
                var detallePedido = new DetallePedido
                {
                    PedidoID = pedido.PedidoID, // ID generado
                    ProductoID = detalleDto.ProductoID,
                    Precio = detalleDto.Precio,
                    Cantidad = detalleDto.Cantidad,
                };

                _context.DetallesPedido.Add(detallePedido);
            }

            await _context.SaveChangesAsync(); // Guarda los detalles del pedido

            return CreatedAtAction("GetPedido", new { id = pedido.PedidoID }, pedido);
        }


        // PUT: api/Pedidos/5
        [HttpPut("{id}")]
        public async Task<IActionResult> PutPedido(int id, Pedido pedido)
        {
            if (id != pedido.PedidoID)
            {
                return BadRequest();
            }

            _context.Entry(pedido).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!PedidoExists(id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }

            return NoContent();
        }

        // DELETE: api/Pedidos/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeletePedido(int id)
        {
            var pedido = await _context.Pedidos.FindAsync(id);
            if (pedido == null)
            {
                return NotFound();
            }

            _context.Pedidos.Remove(pedido);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        // POST: api/Pedidos/Crear
        [HttpPost("Crear")]
        public IActionResult CrearPedidoYEnvio([FromBody] CrearPedidoYEnvioDto pedidoDto)
        {
            string connectionString = _context.Database.GetDbConnection().ConnectionString;

            using (var conn = new SqlConnection(connectionString))
            {
                conn.Open();
                using (var cmd = new SqlCommand("CrearPedidoYEnvio", conn))
                {
                    cmd.CommandType = CommandType.StoredProcedure;

                    // Definir los parámetros de entrada
                    cmd.Parameters.Add(new SqlParameter("@UsuarioID", pedidoDto.UsuarioID));
                    cmd.Parameters.Add(new SqlParameter("@DireccionEnvio", pedidoDto.DireccionEnvio));
                    cmd.Parameters.Add(new SqlParameter("@PesoTotal", pedidoDto.PesoTotal));
                    cmd.Parameters.Add(new SqlParameter("@VolumenTotal", pedidoDto.VolumenTotal));

                    // Ejecutar el procedimiento almacenado
                    cmd.ExecuteNonQuery();

                    // Retornar el resultado. Ajusta según necesites retornar algún valor específico
                    return Ok();
                }
            }
        }

        // PATCH: api/Pedidos/ActualizarEstado/5
        [HttpPatch("ActualizarEstado/{id}")]
        public async Task<IActionResult> ActualizarEstadoPedido(int id, [FromBody] ActualizarEstadoDto estadoDto)
        {
            var pedido = await _context.Pedidos.FindAsync(id);
            if (pedido == null)
            {
                return NotFound();
            }

            // Asegúrate de que solo se actualice el estado si el pedido está en estado "Pendiente"
            if (pedido.Estado == "Pendiente")
            {
                pedido.Estado = estadoDto.Estado; // Actualiza el estado a "Enviado" o cualquier otro estado proporcionado
                _context.Pedidos.Update(pedido);
                await _context.SaveChangesAsync();

                return NoContent();
            }
            else
            {
                return BadRequest(new { message = "El pedido no está en estado 'Pendiente' y no puede ser actualizado." });
            }
        }

        public class ActualizarEstadoDto
        {
            public string Estado { get; set; }
        }






        // PUT: api/Pedidos/Finalizar/5
        [HttpPut("Finalizar/{id}")]
        public async Task<IActionResult> FinalizarPedido(int id)
        {
            var pedido = await _context.Pedidos.FindAsync(id);
            if (pedido == null)
            {
                return NotFound();
            }

            // Aquí podrías agregar lógica para verificar el stock, calcular el total, etc.
            // Por ahora, solo vamos a actualizar el estado del pedido.

            pedido.Estado = "Finalizado";
            _context.Pedidos.Update(pedido);
            await _context.SaveChangesAsync();

            return NoContent(); // O podrías devolver un status más específico si es necesario.
        }

        // GET: api/Pedidos/PedidoEnvio/Todos
        [HttpGet("PedidoEnvio/Todos")]
        public async Task<ActionResult<IEnumerable<PedidoEnvioDto>>> GetTodosPedidosEnvios()
        {
            return await ObtenerPedidosEnvios(null);
        }
        // GET: api/Pedidos/PedidoEnvio/PorUsuario/{usuarioId}
        [HttpGet("PedidoEnvio/PorUsuario/{usuarioId}")]
        public async Task<ActionResult<IEnumerable<PedidoEnvioDto>>> GetPedidosEnviosPorUsuario(int usuarioId)
        {
            return await ObtenerPedidosEnvios(usuarioId);
        }
        private async Task<ActionResult<IEnumerable<PedidoEnvioDto>>> ObtenerPedidosEnvios(int? usuarioId)
        {
            var pedidoEnvioList = new List<PedidoEnvioDto>();
            var connection = _context.Database.GetDbConnection();

            try
            {
                await connection.OpenAsync();
                using (var command = connection.CreateCommand())
                {
                    string sqlQuery = usuarioId.HasValue
                        ? "SELECT * FROM VistaClientePedidosEnvios WHERE UsuarioID = @UsuarioID"
                        : "SELECT * FROM VistaClientePedidosEnvios";
                    command.CommandText = sqlQuery;

                    if (usuarioId.HasValue)
                    {
                        var parameter = command.CreateParameter();
                        parameter.ParameterName = "@UsuarioID";
                        parameter.Value = usuarioId.Value;
                        command.Parameters.Add(parameter);
                    }

                    using (var result = await command.ExecuteReaderAsync())
                    {
                        while (await result.ReadAsync())
                        {
                            pedidoEnvioList.Add(MapearPedidoEnvioDto(result));
                        }
                    }
                }
            }
            finally
            {
                await connection.CloseAsync();
            }

            if (pedidoEnvioList.Count == 0)
            {
                return NotFound(new { message = "No se encontraron pedidos." });
            }

            return pedidoEnvioList;
        }


        private PedidoEnvioDto MapearPedidoEnvioDto(DbDataReader result)
        {
            return new PedidoEnvioDto
            {
                PedidoID = result.GetInt32(result.GetOrdinal("PedidoID")),
                UsuarioID = result.IsDBNull(result.GetOrdinal("UsuarioID")) ? null : result.GetInt32(result.GetOrdinal("UsuarioID")),
                FechaPedido = result.IsDBNull(result.GetOrdinal("FechaPedido")) ? null : result.GetDateTime(result.GetOrdinal("FechaPedido")),
                EstadoPedido = result.IsDBNull(result.GetOrdinal("EstadoPedido")) ? null : result.GetString(result.GetOrdinal("EstadoPedido")),
                Total = result.IsDBNull(result.GetOrdinal("Total")) ? null : result.GetDecimal(result.GetOrdinal("Total")),
                DireccionEnvio = result.IsDBNull(result.GetOrdinal("DireccionEnvio")) ? null : result.GetString(result.GetOrdinal("DireccionEnvio")),
                EnvioID = result.IsDBNull(result.GetOrdinal("EnvioID")) ? null : result.GetInt32(result.GetOrdinal("EnvioID")),
                EstadoEnvio = result.IsDBNull(result.GetOrdinal("EstadoEnvio")) ? null : result.GetString(result.GetOrdinal("EstadoEnvio")),
                FechaEntregaEstimada = result.IsDBNull(result.GetOrdinal("FechaEntregaEstimada")) ? null : result.GetDateTime(result.GetOrdinal("FechaEntregaEstimada")),
                Nombre = result.IsDBNull(result.GetOrdinal("Nombre")) ? null : result.GetString(result.GetOrdinal("Nombre")),
                ImagenURL = result.IsDBNull(result.GetOrdinal("ImagenURL")) ? null : result.GetString(result.GetOrdinal("ImagenURL")),
                Cantidad = result.IsDBNull(result.GetOrdinal("Cantidad")) ? null : result.GetInt32(result.GetOrdinal("Cantidad")),
                Precio = result.IsDBNull(result.GetOrdinal("Precio")) ? null : result.GetDecimal(result.GetOrdinal("Precio"))
            };
        }


        public class PedidoEnvioDto
        {
            public int? PedidoID { get; set; }
            public int? UsuarioID { get; set; }
            public DateTime? FechaPedido { get; set; }
            public string? EstadoPedido { get; set; }
            public decimal? Total { get; set; }
            public string? DireccionEnvio { get; set; }
            public int? EnvioID { get; set; }
            public string? EstadoEnvio { get; set; }
            public DateTime? FechaEntregaEstimada { get; set; }
            public string? Nombre { get; set; }
            public string? ImagenURL { get; set; }
            public int? Cantidad { get; set; }
            public decimal? Precio { get; set; }
        }







        private bool PedidoExists(int id)
        {
            return _context.Pedidos.Any(e => e.PedidoID == id);
        }
    }

    public class CrearPedidoYEnvioDto
    {
        public int UsuarioID { get; set; }
        public string DireccionEnvio { get; set; }
        public decimal PesoTotal { get; set; }
        public decimal VolumenTotal { get; set; }
    }
    


}