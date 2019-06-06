using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using fDotNetCoreContainerHelper;
using Microsoft.AspNetCore.Mvc;

namespace Donation.RestApi.Entrance.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class InfoController : ControllerBase
    {
        // GET api/values
        [HttpGet]
        public JsonResult Get()
        {
            var s = RuntimeHelper.GetAppSettings("AllowedHosts");
            return new JsonResult(RuntimeHelper.GetContextInformationDictionary());
        }

        //// GET api/values/5
        //[HttpGet("{id}")]
        //public ActionResult<string> Get(int id)
        //{
        //    return "value";
        //}

        //// POST api/values
        //[HttpPost]
        //public void Post([FromBody] string value)
        //{
        //}

        //// PUT api/values/5
        //[HttpPut("{id}")]
        //public void Put(int id, [FromBody] string value)
        //{
        //}

        //// DELETE api/values/5
        //[HttpDelete("{id}")]
        //public void Delete(int id)
        //{
        //}
    }
}
