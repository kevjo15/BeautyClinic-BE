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
using Application_Layer.DTOs;
using Microsoft.AspNetCore.Authorization;
using Application_Layer.Queries.ServiceQueries;
using Microsoft.AspNetCore.Http;
using Application_Layer.Commands.ServiceCommands.UploadServiceImage;
using Application_Layer.Queries.ServiceQueries.GetAllServicesWithSas;
using System.Threading;
using System.IO;

namespace API_Layer.Controllers
{
    [Route("api/services")]
    [ApiController]
    public class ServiceController : BaseApiController
    {
        private readonly IMediator _mediator;

        public ServiceController(IMediator mediator)
        {
            _mediator = mediator;
        }

        [HttpGet]
        [AllowAnonymous]
        public async Task<IActionResult> GetAllServices()
        {
            var services = await _mediator.Send(new GetAllServicesQuery());
            return Ok(services);
        }

        [HttpGet("with-sas")]
        [AllowAnonymous]
        public async Task<ActionResult<IReadOnlyList<ServiceDTO>>> GetAllWithSas(CancellationToken ct)
        {
            return Ok(await _mediator.Send(new GetAllServicesWithSasQuery(), ct));
        }

        // POST: api/services/create
        [HttpPost]
        [Authorize(Roles = "Admin,Employee")]
        public async Task<IActionResult> CreateService([FromBody] ServiceDTO serviceDto)
        {
            var command = new CreateServiceCommand(serviceDto);
            var result = await _mediator.Send(command);
            return HandleResult(result);
        }

        // PUT: api/services/update/{id}
        [HttpPut("{id}")]
        [Authorize(Roles = "Admin,Employee")]
        public async Task<IActionResult> UpdateService(Guid id, [FromBody] ServiceDTO serviceDto)
        {
            var command = new UpdateServiceCommand(id, serviceDto);
            var result = await _mediator.Send(command);
            return HandleResult(result);
        }

        // DELETE: api/services/delete/{id}
        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin,Employee")]
        public async Task<IActionResult> DeleteService(Guid id)
        {
            var command = new DeleteServiceCommand(id);
            var result = await _mediator.Send(command);
            return HandleResult(result, () => Ok("Service deleted successfully."));
        }

        [HttpGet("search")]
        [AllowAnonymous]
        public async Task<IActionResult> GetServiceByName([FromQuery] string name)
        {
            var query = new GetServiceByNameQuery(name);
            var services = await _mediator.Send(query);
            return Ok(services);
        }

        [HttpGet("category/{categoryId}")]
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

            await using var stream = file.OpenReadStream();
            using var memoryStream = new MemoryStream();
            await stream.CopyToAsync(memoryStream, ct);

            var upload = new FileUploadRequest(
                file.FileName,
                file.ContentType ?? "application/octet-stream",
                memoryStream.ToArray());

            var res = await _mediator.Send(new UploadServiceImageCommand(serviceId, upload), ct);
            return Ok(res);
        }
    }
}
