using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Donation.Model;
using fDotNetCoreContainerHelper;
using Microsoft.AspNetCore.Mvc;

namespace Donation.RestApi.Entrance.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class DonationController : ControllerBase
    {

        [HttpGet("{id}")]
        public DonationDTO GetDonationDTO(long id)
        {
            return new DonationDTO();
        }



        // https://docs.microsoft.com/en-us/aspnet/core/tutorials/first-web-api?view=aspnetcore-2.2&tabs=visual-studio

        [HttpPost]
        public CreatedAtActionResult PostDonationDTO(DonationDTO donationDTO)
        {
            return CreatedAtAction(nameof(GetDonationDTO), new { id = donationDTO.Guid.ToString() }, donationDTO);
        }
        //public async Task<ActionResult<DonationDTO>> PostDonationDTO(DonationDTO donationDTO)
        //{
        //    return CreatedAtAction(nameof(DonationDTO), new { id = donationDTO.Guid.ToString() }, donationDTO);
        //}
    }
}