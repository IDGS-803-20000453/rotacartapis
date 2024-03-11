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

                if (!string.IsNullOrEmpty(estadoDto.Estado))
                {
                    envio.Estado = estadoDto.Estado;
                    _context.Envios.Update(envio);
                    await _context.SaveChangesAsync();
                    return NoContent();
                }
                else
                {
                    return BadRequest(new { message = "El estado no puede ser nulo o vacío." });
                }
            }
            catch (Exception ex)
            {
                // Log the exception details
                return StatusCode(500, new { message = "An error occurred while processing your request.", details = ex.Message });
            }
        }


        // Definición de ActualizarEstadoEnvioDto dentro del controlador
        public class ActualizarEstadoEnvioDto
        {
            public string Estado { get; set; }
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
