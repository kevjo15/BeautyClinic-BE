using Application_Layer.Commands.ServiceCommands.CreateService;
using Application_Layer.Commands.ServiceCommands.UpdateService;
using Application_Layer.Commands.ServiceCommands.DeleteService;
using Domain_Layer.Models;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Application_Layer.Queries.ServiceQueries.GetAllServices;
using Application_Layer.DTO_s;
using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Application_Layer.Queries.ServiceQueries;
using Application_Layer.Services;
using Microsoft.AspNetCore.Http;
using Application_Layer.Features.Services.Commands.UploadServiceImage;
using Application_Layer.Features.Services.Queries.GetAllServicesWithSas;
using System.Threading;

namespace API_Layer.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ServiceController : ControllerBase
    {
        private readonly IMediator _mediator;

        public ServiceController(IMediator mediator)
        {
            _mediator = mediator;
        }

        [HttpGet("GetAllServices")]
        [AllowAnonymous]
        public async Task<IActionResult> GetAllServices()
        {
            var services = await _mediator.Send(new GetAllServicesQuery());
            return Ok(services);
        }

        [HttpGet("GetAllServicesWithSas")]
        [AllowAnonymous]
        public async Task<ActionResult<IReadOnlyList<ServiceDTO>>> GetAllWithSas(CancellationToken ct)
        {
            return Ok(await _mediator.Send(new GetAllServicesWithSasQuery(), ct));
        }

        // POST: api/services/create
        [HttpPost("create")]
        [Authorize(Roles = "Admin,Employee")]
        public async Task<IActionResult> CreateService([FromBody] ServiceDTO serviceDto)
        {
            var command = new CreateServiceCommand(serviceDto);
            var result = await _mediator.Send(command);
            if (!result.Success) return BadRequest(result.Message);
            return Ok(new { result.Message, result.CreatedService });
        }

        // PUT: api/services/update/{id}
        [HttpPut("update/{id}")]
        [Authorize(Roles = "Admin,Employee")]
        public async Task<IActionResult> UpdateService(Guid id, [FromBody] ServiceDTO serviceDto)
        {
            var command = new UpdateServiceCommand(id, serviceDto);
            var result = await _mediator.Send(command);
            if (!result.Success) return BadRequest(result.Message);
            return Ok(new { result.Message, result.UpdatedService });
        }

        // DELETE: api/services/delete/{id}
        [HttpDelete("delete/{id}")]
        [Authorize(Roles = "Admin,Employee")]
        public async Task<IActionResult> DeleteService(Guid id)
        {
            var command = new DeleteServiceCommand(id);
            var result = await _mediator.Send(command);
            if (!result) return BadRequest("Failed to delete service.");
            return Ok("Service deleted successfully.");
        }

        [HttpGet("search")]
        [AllowAnonymous]
        public async Task<IActionResult> GetServiceByName([FromQuery] string name)
        {
            var query = new GetServiceByNameQuery(name);
            var services = await _mediator.Send(query);
            return Ok(services);
        }

        [HttpGet("by-category/{categoryId}")]
        [AllowAnonymous]
        public async Task<IActionResult> GetServicesByCategory(Guid categoryId)
        {
            var query = new GetServicesByCategoryQuery(categoryId);
            var services = await _mediator.Send(query);
            return Ok(services);
        }

        [Authorize]
        [HttpPost("{serviceId}/image")]
        public async Task<ActionResult<UploadServiceImageResult>> UploadImage([FromRoute] Guid serviceId, IFormFile file, CancellationToken ct)
        {
            if (file is null) return BadRequest("file is required");
            var res = await _mediator.Send(new UploadServiceImageCommand(serviceId, file), ct);
            return Ok(res);
        }
    }
}
