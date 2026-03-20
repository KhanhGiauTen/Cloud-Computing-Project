using CloudContactManager.Data;
using CloudContactManager.Models;
using CloudContactManager.Services.Interfaces;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CloudContactManager.Controllers
{
    /// <summary>
    /// Web API controller for Customer CRUD operations consumed by the external HTML UI.
    /// Route base: /api/Customers
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    public class CustomersController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly ITenantProvider _tenantProvider;

        public CustomersController(AppDbContext context, ITenantProvider tenantProvider)
        {
            _context = context;
            _tenantProvider = tenantProvider;
        }

        // GET: api/Customers
        // Used by Views/Customers/Index.html and Views/Communication/Index.html
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Customer>>> GetCustomers()
        {
            var customers = await _context.Customers
                .OrderBy(c => c.FullName)
                .ToListAsync();

            return Ok(customers);
        }

        // GET: api/Customers/5
        // Used by Edit.html and Delete.html
        [HttpGet("{id}")]
        public async Task<ActionResult<Customer>> GetCustomer(int id)
        {
            var customer = await _context.Customers.FindAsync(id);

            if (customer == null)
            {
                return NotFound(new { Message = "Không tìm thấy khách hàng." });
            }

            return Ok(customer);
        }

        // POST: api/Customers
        // Used by Create.html
        [HttpPost]
        public async Task<ActionResult<Customer>> CreateCustomer([FromBody] Customer customer)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(new { Message = "Dữ liệu khách hàng không hợp lệ.", Errors = ModelState });
            }

            customer.TenantId = _tenantProvider.GetTenantId();

            _context.Customers.Add(customer);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetCustomer), new { id = customer.Id }, customer);
        }

        // PUT: api/Customers/5
        // Used by Edit.html
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateCustomer(int id, [FromBody] Customer customer)
        {
            if (id != customer.Id)
            {
                return BadRequest(new { Message = "ID khách hàng không khớp." });
            }

            customer.TenantId = _tenantProvider.GetTenantId();

            if (!ModelState.IsValid)
            {
                return BadRequest(new { Message = "Dữ liệu khách hàng không hợp lệ.", Errors = ModelState });
            }

            _context.Entry(customer).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!CustomerExists(id))
                {
                    return NotFound(new { Message = "Không tìm thấy khách hàng." });
                }
                throw;
            }

            return NoContent();
        }

        // DELETE: api/Customers/5
        // Used by Delete.html
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteCustomer(int id)
        {
            var customer = await _context.Customers.FindAsync(id);
            if (customer == null)
            {
                return NotFound(new { Message = "Không tìm thấy khách hàng." });
            }

            _context.Customers.Remove(customer);
            await _context.SaveChangesAsync();

            return Ok(new { Message = "Đã xóa khách hàng thành công." });
        }

        private bool CustomerExists(int id)
        {
            return _context.Customers.Any(e => e.Id == id);
        }
    }
}
