using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Amazon.DynamoDBv2.DataModel;
using HelloDynamoDb.Models;
using Microsoft.AspNetCore.Mvc;

namespace HelloDynamoDb.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class EmployeeController : ControllerBase
    {
        public IDynamoDBContext Context { get; }

        public EmployeeController(IDynamoDBContext context)
        {
            Context = context;
        }

        // GET: api/Employee
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Employee>>> Get()
        {
            var result = await Context
                .ScanAsync<Employee>(Enumerable.Empty<ScanCondition>())
                .GetRemainingAsync();
            return Ok(result);
        }

        // GET: api/Employee/john
        [HttpGet("{id}", Name = nameof(Get))]
        public async Task<ActionResult<Employee>> Get(string id)
        {
            var result = await Context.LoadAsync<Employee>(id);
            if (result == null) return NotFound();
            return Ok(result);
        }

        // POST: api/Employee
        [HttpPost]
        public async Task<IActionResult> Post([FromBody] Employee value)
        {
            await Context.SaveAsync(value);
            return CreatedAtAction(nameof(Get), new { id = value.LoginAlias });
        }

        // PUT: api/Employee/john
        [HttpPut("{id}")]
        public async Task<ActionResult<Employee>> Put(string id, [FromBody] Employee value)
        {
            if (string.Compare(id, value.LoginAlias, true) != 0) return BadRequest();
            await Context.SaveAsync(value);
            var result = await Context.LoadAsync<Employee>(id, new DynamoDBContextConfig{ ConsistentRead = true });
            return Ok(result);
        }

        // DELETE: api/ApiWithActions/john
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(string id)
        {
            var result = await Context.LoadAsync<Employee>(id);
            if (result == null) return NotFound();
            await Context.DeleteAsync<Employee>(result);
            return NoContent();
        }
    }
}
