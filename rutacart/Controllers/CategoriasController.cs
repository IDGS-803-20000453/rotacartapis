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
    public class CategoriasController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public CategoriasController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: api/Categorias
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Categoria>>> GetCategorias()
        {
            // Selecciona solo los campos que deseas devolver en tu API, excluyendo los productos.
            var categorias = await _context.Categorias
                .Select(c => new
                {
                    c.CategoriaID,
                    c.Nombre,
                    c.Descripcion
                })
                .ToListAsync();

            return Ok(categorias);
        }

        // GET: api/Categorias/5
        [HttpGet("{id}")]
        public async Task<ActionResult<IEnumerable<Categoria>>> GetCategoria(int id)
        {
            // Selecciona solo los campos que deseas devolver en tu API, excluyendo los productos.
            var categoria = await _context.Categorias
                .Where(c => c.CategoriaID == id)
                .Select(c => new
                {
                    c.CategoriaID,
                    c.Nombre,
                    c.Descripcion
                })
                .ToListAsync();

            if (categoria == null)
            {
                return NotFound();
            }

            return Ok(categoria);
        }   

        // POST: api/Categorias
        [HttpPost]
        public async Task<ActionResult<Categoria>> PostCategoria(Categoria categoria)
        {
            _context.Categorias.Add(categoria);
            await _context.SaveChangesAsync();

            return CreatedAtAction("GetCategoria", new { id = categoria.CategoriaID }, categoria);
        }

        public class CategoriaEdicionDto
        {
            public string Nombre { get; set; }
            public string Descripcion { get; set; }
        }


        // PUT: api/Categorias/5
        [HttpPut("{id}")]
        public async Task<IActionResult> PutCategoria(int id, [FromBody] CategoriaEdicionDto categoriaDto)
        {
            var categoria = await _context.Categorias.FindAsync(id);
            if (categoria == null)
            {
                return NotFound();
            }

            // Actualiza los campos de la categoría existente con los valores del DTO
            categoria.Nombre = categoriaDto.Nombre;
            categoria.Descripcion = categoriaDto.Descripcion;

            _context.Entry(categoria).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!CategoriaExists(id))
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


        // DELETE: api/Categorias/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteCategoria(int id)
        {
            var categoria = await _context.Categorias.FindAsync(id);
            if (categoria == null)
            {
                return NotFound();
            }

            _context.Categorias.Remove(categoria);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private bool CategoriaExists(int id)
        {
            return _context.Categorias.Any(e => e.CategoriaID == id);
        }
    }
}

