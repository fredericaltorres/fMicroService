﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Donation.Model;
using Donation.Queue.Lib;
using Donation.Service;
using fDotNetCoreContainerHelper;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace Donation.RestApi.Entrance.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class DonationController : ControllerBase
    {
        private readonly IDonationQueueEndqueue queue;

        public DonationController(IDonationQueueEndqueue queue)
        {
            this.queue = queue;
        }

        [HttpGet("{id}")]
        public DonationDTO GetDonationDTO(long id)
        {
            return new DonationDTO();
        }

        // https://docs.microsoft.com/en-us/aspnet/core/tutorials/first-web-api?view=aspnetcore-2.2&tabs=visual-studio

        [HttpPost]
        [ProducesResponseType(typeof(DonationDTO), StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme )]
        public async Task<IActionResult> PostDonationDTO(DonationDTO donationDTO)
        {
            donationDTO.__EntranceMachineID = RuntimeHelper.GetMachineName();
            var donationsService = new DonationsValidationService(donationDTO);
            var errors = donationsService.ValidateData();
            if(errors.NoError)
            {                
                await this.queue.EnqueueAsync(donationDTO);
                return CreatedAtAction(nameof(GetDonationDTO), new { id = donationDTO.Guid.ToString() }, donationDTO);
            }
            else
            {
                Console.WriteLine(errors.ToString());
                return BadRequest(errors.ToString());
            }
        }
    }
}