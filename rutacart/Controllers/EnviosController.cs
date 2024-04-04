using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using rutacart.Data;
using System.Threading.Tasks;

namespace rutacart.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class EnviosController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public EnviosController(ApplicationDbContext context)
        {
            _context = context;
        }

        [HttpPatch("ActualizarEstado/{id}")]
        public async Task<IActionResult> ActualizarEstadoEnvio(int id, [FromBody] ActualizarEstadoEnvioDto estadoDto)
        {
            try
            {
                var envio = await _context.Envios.FindAsync(id);
                if (envio == null)
                {
                    return NotFound();
                }

                envio.Estado = estadoDto.Estado;
                if (estadoDto.CostoEnvio.HasValue) // Asegúrate de que el costo de envío se proporciona antes de actualizar
                {
                    envio.CostoEnvio = estadoDto.CostoEnvio.Value;
                }

                // Actualiza el proveedorID del envío
                envio.ProveedorID = estadoDto.ProveedorID;

                _context.Envios.Update(envio);
                await _context.SaveChangesAsync();

                if (envio.Estado == "Pendiente")
                {
                    // Devuelve un código especial, por ejemplo, 202 (Accepted) con un mensaje personalizado
                    return StatusCode(202, new { message = "El estado del envío ha sido actualizado a 'Pendiente'." });
                }

                return NoContent(); // Si no es "Pendiente", simplemente confirma que la actualización fue exitosa
            }
            catch (Exception ex)
            {
                // Log the exception details
                return StatusCode(500, new { message = "An error occurred while processing your request.", details = ex.Message });
            }
        }


        public class ActualizarEstadoEnvioDto
        {
            public string Estado { get; set; }
            public decimal? CostoEnvio { get; set; } // El costo de envío como un campo opcional
            public int ProveedorID { get; set; } // Agrega el ID del proveedor
        }


        public class EnvioDto
        {
            public int EnvioID { get; set; }
            public int PedidoID { get; set; }
            public int? ProveedorID { get; set; }
            public decimal? CostoEnvio { get; set; }
            public DateTime? FechaEnvio { get; set; }
            public DateTime? FechaEntregaEstimada { get; set; }
            public string Estado { get; set; }
        }
        // GET: api/Envios
        [HttpGet]
        public async Task<ActionResult<IEnumerable<EnvioDto>>> GetEnvios(int? pedidoId = null)
        {
            var query = _context.Envios.AsQueryable();

            if (pedidoId.HasValue)
            {
                query = query.Where(e => e.PedidoID == pedidoId);
            }

            var enviosList = await query
                .Select(envio => new EnvioDto
                {
                    EnvioID = envio.EnvioID,
                    PedidoID = envio.PedidoID,
                    ProveedorID = envio.ProveedorID,
                    CostoEnvio = envio.CostoEnvio,
                    FechaEnvio = envio.FechaEnvio,
                    FechaEntregaEstimada = envio.FechaEntregaEstimada,
                    Estado = envio.Estado
                })
                .ToListAsync();

            return enviosList;
        }



    }
}
