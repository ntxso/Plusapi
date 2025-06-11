using API.Context;
using API.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ColorController : ControllerBase
    {
        private readonly AppDbContext _context;

        public ColorController(AppDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<Color>>> GetAll()
        {
            return await _context.Colors.ToListAsync();
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<Color>> GetById(int id)
        {
            var color = await _context.Colors.FindAsync(id);
            if (color == null) return NotFound();
            return color;
        }

        [HttpPost]
        public async Task<ActionResult<Color>> Create(Color color)
        {
            _context.Colors.Add(color);
            await _context.SaveChangesAsync();
            return CreatedAtAction(nameof(GetById), new { id = color.Id }, color);
        }

        [HttpPost("update/{id}")]
        public async Task<IActionResult> Update(int id, Color updated)
        {
            if (id != updated.Id) return BadRequest();

            _context.Entry(updated).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!_context.Colors.Any(e => e.Id == id))
                    return NotFound();
                else throw;
            }

            return NoContent();
        }

        [HttpPost("delete/{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var color = await _context.Colors.FindAsync(id);
            if (color == null) return NotFound();

            _context.Colors.Remove(color);
            await _context.SaveChangesAsync();
            return NoContent();
        }
    }

}
