using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using DatingApp.API.Data;
using Microsoft.EntityFrameworkCore;
using System.Web.Http.Cors;

namespace DatingApp.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    //[EnableCors(origins:"*", headers:"*", methods:"*")]        // Default policy.
    public class ValuesController : ControllerBase
    {
        //Injectar sqlite
        private readonly DataContext _context;
        public ValuesController(DataContext context)
        {
            this._context = context;
        }
        /*
        [HttpGet]
        public ActionResult<IEnumerable<string>> Get()
        {
            return new String[]{"value1", "value2"};
        }
        */
             // [EnableCors(origins:"*", headers:"*", methods:"*")]        // Default policy.
        [AcceptVerbs("Get", "POST")]
        public async Task<IActionResult> GetValues()
        {
             var values = await _context.Values.ToListAsync();
             return Ok(values);
        }
        [HttpGet("{id}")]
        public async Task<IActionResult> GetValue(int id)
        {
            var value = await _context.Values.FirstOrDefaultAsync(x=>x.Id == id);
            return Ok(value);
        }
    }
}
