using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Formatting;
using System.Threading.Tasks;
// using Actio.Api.Repositories;
using Actio.Common.Commands;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RawRabbit;

namespace Actio.Api.Controllers
{
    [Route("[controller]")]
    // [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    public class ActivitiesController : Controller
    {
        private readonly IBusClient _busClient;

        public ActivitiesController(IBusClient busClient)
        {
            _busClient = busClient;
        }

        //[HttpGet("")]
        //public async Task<IActionResult> Get()
        //{
        //    var activities = await _repository
        //        .BrowseAsync(Guid.Parse(User.Identity.Name));

        //    return Json(activities.Select(x => new {x.Id, x.Name, x.Category, x.CreatedAt}));
        //}

        //[HttpGet("{id}")]
        //public async Task<IActionResult> Get(Guid id)
        //{
        //    var activity = await _repository.GetAsync(id);
        //    if (activity == null)
        //    {
        //        return NotFound();
        //    }
        //    if (activity.UserId != Guid.Parse(User.Identity.Name))
        //    {
        //        return Unauthorized();
        //    }

        //    return Json(activity);
        //}

        // GET api/values
        [HttpGet]
        public ActionResult<IEnumerable<string>> Get()
        {
            return new string[] { "value1111", "value222", $"{DateTime.Now}" };
        }




        [HttpPost("")]
        public async Task<IActionResult> Post([FromBody]CreateActivity command)
        {
            command.Id = Guid.NewGuid();
            // command.UserId = Guid.Parse(User.Identity.Name);
            command.CreatedAt = DateTime.UtcNow;

            Console.WriteLine($"Publishing command Cat:{command.Category} name:{command.Name}");
            await _busClient.PublishAsync(command);

            return Accepted($"activities/{command.Id}"); // return 202
        }
    }
}