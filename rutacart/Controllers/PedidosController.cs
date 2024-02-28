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
    public class PedidosController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public PedidosController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: api/Pedidos
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Pedido>>> GetPedidos()
        {
            return await _context.Pedidos
                                 .Include(p => p.Usuarios)
                                 .Include(p => p.DetallesPedido)
                                 .ToListAsync();
        }

        // GET: api/Pedidos/5
        [HttpGet("{id}")]
        public async Task<ActionResult<Pedido>> GetPedido(int id)
        {
            var pedido = await _context.Pedidos
                                       .Include(p => p.Usuarios)
                                       .Include(p => p.DetallesPedido)
                                       .FirstOrDefaultAsync(p => p.PedidoID == id);

            if (pedido == null)
            {
                return NotFound();
            }

            return pedido;
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

        private bool PedidoExists(int id)
        {
            return _context.Pedidos.Any(e => e.PedidoID == id);
        }
    }
}
