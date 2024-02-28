using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using rutacart.Data;
using rutacart.Models;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace rutacart.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class DetallesPedidoController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public DetallesPedidoController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: api/DetallePedido/Pedido/5
        [HttpGet("Pedido/{pedidoId}")]
        public async Task<ActionResult<IEnumerable<DetallePedido>>> GetDetallesPorPedido(int pedidoId)
        {
            var detalles = await _context.DetallesPedido
                .Where(dp => dp.PedidoID == pedidoId)
                .Include(dp => dp.Producto) // Opcional, si quieres devolver datos del producto en la misma consulta
                .ToListAsync();

            if (detalles == null || detalles.Count == 0)
            {
                return NotFound();
            }

            return detalles;
        }

        // POST: api/DetallePedido
        [HttpPost]
        public async Task<ActionResult<DetallePedido>> PostDetallePedido(DetallePedido detallePedido)
        {
            // Validación adicional, como verificar la existencia del Pedido y Producto, podría ser necesaria aquí
            _context.DetallesPedido.Add(detallePedido);
            await _context.SaveChangesAsync();

            // Considerar calcular y actualizar el subtotal en el Pedido aquí, si es necesario

            return CreatedAtAction("GetDetallesPorPedido", new { pedidoId = detallePedido.PedidoID }, detallePedido);
        }
    }
}
