using Microsoft.AspNetCore.Mvc;

using Microsoft.EntityFrameworkCore;
using rutacart.Data;
using rutacart.Models;
using System.Threading.Tasks;

namespace rutacart.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ProductosController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public ProductosController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: api/Productos
        [HttpGet]
        public async Task<IActionResult> GetProductos()
        {
            // Incluye la categoría al obtener los productos y selecciona los campos que necesitas
            var productos = await _context.Productos
                .Include(p => p.Categoria)
                .Select(p => new
                {
                    p.ProductoID,
                    p.Nombre,
                    p.Descripcion,
                    p.Precio,
                    p.Peso,
                    p.Volumen,
                    p.Stock,
                    p.CategoriaID,
                    CategoriaNombre = p.Categoria.Nombre, // Incluye el nombre de la categoría
                    p.ImagenURL
                })
                .ToListAsync();

            return Ok(productos);
        }


        // GET: api/Productos/5
        [HttpGet("{id}")]
        public async Task<IActionResult> GetProducto(int id)
        {
            var producto = await _context.Productos.FindAsync(id);

            if (producto == null)
            {
                return NotFound();
            }

            return Ok(producto);
        }

        public class ProductoCreacionDto
        {
            public string Nombre { get; set; }
            public string Descripcion { get; set; }
            public decimal Precio { get; set; }
            public decimal Peso { get; set; }
            public decimal Volumen { get; set; }
            public int Stock { get; set; }
            public int CategoriaID { get; set; }
            public string ImagenURL { get; set; }
        }

        public class ProductoEdicionDto
        {
            public string Nombre { get; set; }
            public string Descripcion { get; set; }
            public decimal Precio { get; set; }
            public decimal Peso { get; set; }
            public decimal Volumen { get; set; }
            public int Stock { get; set; }
            public int CategoriaID { get; set; }
            public string ImagenURL { get; set; }
        }



        // POST: api/Productos
        [HttpPost]
        public async Task<IActionResult> CreateProducto([FromBody] ProductoCreacionDto productoDto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var producto = new Producto
            {
                Nombre = productoDto.Nombre,
                Descripcion = productoDto.Descripcion,
                Precio = productoDto.Precio,
                Peso = productoDto.Peso,
                Volumen = productoDto.Volumen,
                Stock = productoDto.Stock,
                CategoriaID = productoDto.CategoriaID,
                ImagenURL = productoDto.ImagenURL
                // No asignas directamente Categoria ni ItemsCarrito aquí
            };

            _context.Productos.Add(producto);
            await _context.SaveChangesAsync();

            return CreatedAtAction("GetProducto", new { id = producto.ProductoID }, producto);
        }


        // PUT: api/Productos/5
        // PUT: api/Productos/5
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateProducto(int id, [FromBody] ProductoEdicionDto productoDto)
        {
            var producto = await _context.Productos.FindAsync(id);
            if (producto == null)
            {
                return NotFound("Producto no encontrado.");
            }

            producto.Nombre = productoDto.Nombre;
            producto.Descripcion = productoDto.Descripcion;
            producto.Precio = productoDto.Precio;
            producto.Peso = productoDto.Peso;
            producto.Volumen = productoDto.Volumen;
            producto.Stock = productoDto.Stock;
            producto.CategoriaID = productoDto.CategoriaID;

            // Actualiza ImagenURL solo si se proporciona una nueva
            if (productoDto.ImagenURL != null)
            {
                producto.ImagenURL = productoDto.ImagenURL;
            }

            _context.Entry(producto).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
                return NoContent();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!ProductoExists(id))
                {
                    return NotFound("Producto no encontrado para actualizar.");
                }
                else
                {
                    throw;
                }
            }
        }




        // DELETE: api/Productos/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteProducto(int id)
        {
            var producto = await _context.Productos.FindAsync(id);
            if (producto == null)
            {
                return NotFound();
            }

            _context.Productos.Remove(producto);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private bool ProductoExists(int id)
        {
            return _context.Productos.Any(e => e.ProductoID == id);
        }
    }
}
