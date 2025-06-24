using API.Context;
using API.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class PhoneModelController : ControllerBase
    {
        private readonly AppDbContext _context;

        public PhoneModelController(AppDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<PhoneModel>>> GetAll()
        {
            return await _context.PhoneModels.ToListAsync();
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<PhoneModel>> GetById(int id)
        {
            var model = await _context.PhoneModels.FindAsync(id);
            if (model == null) return NotFound();
            return model;
        }

        [HttpPost]
        [Authorize(Roles = "admin")]
        public async Task<ActionResult<PhoneModel>> Create(PhoneModel model)
        {
            _context.PhoneModels.Add(model);
            await _context.SaveChangesAsync();
            return CreatedAtAction(nameof(GetById), new { id = model.Id }, model);
        }

        [HttpPost("update/{id}")]
        [Authorize(Roles = "admin")]
        public async Task<IActionResult> Update(int id, PhoneModel updated)
        {
            if (id != updated.Id) return BadRequest();

            _context.Entry(updated).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!_context.PhoneModels.Any(e => e.Id == id))
                    return NotFound();
                else throw;
            }

            return NoContent();
        }

        [HttpPost("delete/{id}")]
        [Authorize(Roles = "admin")]
        public async Task<IActionResult> Delete(int id)
        {
            var model = await _context.PhoneModels.FindAsync(id);
            if (model == null) return NotFound();

            _context.PhoneModels.Remove(model);
            await _context.SaveChangesAsync();
            return NoContent();
        }
    }

}
