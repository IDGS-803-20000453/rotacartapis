using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using rutacart.Data;
using rutacart.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace rutacart.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ProveedoresController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public ProveedoresController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: api/Proveedores
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Proveedor>>> GetProveedores()
        {
            var proveedores = await _context.Proveedores
                .Select(p => new
                {
                    p.ProveedorID,
                    p.Nombre,
                    p.Direccion,
                    p.Telefono,
                    p.Email
                })
                .ToListAsync();
            return Ok(proveedores);
        }

        // GET: api/Proveedores/5
        [HttpGet("{id}")]
        public async Task<ActionResult<Proveedor>> GetProveedor(int id)
        {
            var proveedor = await _context.Proveedores
                .Where(p => p.ProveedorID == id)
                .Select(p => new
                {
                    p.ProveedorID,
                    p.Nombre,
                    p.Direccion,
                    p.Telefono,
                    p.Email
                })
                .FirstOrDefaultAsync(); // Cambiado a FirstOrDefaultAsync

            if (proveedor == null)
            {
                return NotFound();
            }

            return Ok(proveedor);
        }

        // POST: api/Proveedores
        [HttpPost]
        public async Task<ActionResult<Proveedor>> PostProveedor([FromBody] ProveedorDto proveedorDto)
        {
            var proveedor = new Proveedor
            {
                Nombre = proveedorDto.Nombre,
                Direccion = proveedorDto.Direccion,
                Telefono = proveedorDto.Telefono,
                Email = proveedorDto.Email
                // Envios se inicializa como null o una colección vacía según tu lógica de negocio
            };

            _context.Proveedores.Add(proveedor);
            await _context.SaveChangesAsync();

            return CreatedAtAction("GetProveedor", new { id = proveedor.ProveedorID }, proveedor);
        }

        // PUT: api/Proveedores/5
        [HttpPut("{id}")]
        public async Task<IActionResult> PutProveedor(int id, Proveedor proveedor)
        {
            if (id != proveedor.ProveedorID)
            {
                return BadRequest();
            }

            _context.Entry(proveedor).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!ProveedorExists(id))
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

        // DELETE: api/Proveedores/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteProveedor(int id)
        {
            var proveedor = await _context.Proveedores.FindAsync(id);
            if (proveedor == null)
            {
                return NotFound();
            }

            _context.Proveedores.Remove(proveedor);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private bool ProveedorExists(int id)
        {
            return _context.Proveedores.Any(e => e.ProveedorID == id);
        }
    }
}
