using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Domain_Layer.Models;
using Microsoft.AspNetCore.Authorization;
using MediatR;
using Application_Layer.DTO_s;
using Application_Layer.Commands.CategoryCommands.AddServiceToCategory;
using Application_Layer.Commands.CategoryCommands.RemoveServiceFromCategory;
using Application_Layer.Commands.CategoryCommands.DeleteCategory;
using Application_Layer.Queries.CategoryQueries;

namespace API_Layer.Controllers
{
    [Route("api/categories")]
    [ApiController]
    public class CategoryController : ControllerBase
    {
        private readonly IMediator _mediator;

        public CategoryController(IMediator mediator)
        {
            _mediator = mediator;
        }

        // GET: api/categories
        [HttpGet("GetAllCategories")]
        [AllowAnonymous]
        public async Task<IActionResult> GetAllCategories()
        {
            var query = new GetAllCategoriesQuery();
            var categories = await _mediator.Send(query);
            return Ok(categories);
        }

        // POST: api/categories/create
        [HttpPost("create")]
        [Authorize(Roles = "Admin,Employee")]
        public async Task<IActionResult> CreateCategory([FromBody] CategoryCreateDTO categoryCreateDto)
        {
            var command = new CreateCategoryCommand(categoryCreateDto);
            var result = await _mediator.Send(command);
            return CreatedAtAction(nameof(GetAllCategories), new { id = result.Id }, result);
        }

        // PUT: api/categories/update/{id}
        [HttpPut("update/{id}")]
        [Authorize(Roles = "Admin,Employee")]
        public async Task<IActionResult> UpdateCategory(Guid id, [FromBody] CategoryModel category)
        {
            if (id != category.Id) return BadRequest();
            var command = new UpdateCategoryCommand(category);
            var result = await _mediator.Send(command);
            if (!result) return NotFound();
            return NoContent();
        }

        // DELETE: api/categories/delete/{id}
        [HttpDelete("delete/{id}")]
        [Authorize(Roles = "Admin,Employee")]
        public async Task<IActionResult> DeleteCategory(Guid id)
        {
            var command = new DeleteCategoryCommand(id);
            var result = await _mediator.Send(command);
            
            if (!result.Success)
                return NotFound(result.Message);
            
            return NoContent();
        }

        // POST: api/categories/{categoryId}/add-service
        [HttpPost("{categoryId}/add-service")]
        [Authorize(Roles = "Admin,Employee")]
        public async Task<IActionResult> AddServiceToCategory(Guid categoryId,  Guid serviceId)
        {
            var command = new AddServiceToCategoryCommand(categoryId, serviceId);
            var result = await _mediator.Send(command);
            
            if (!result.Success)
                return BadRequest(result.Message);
            
            return Ok(result.Message);
        }

        // DELETE: api/categories/{categoryId}/remove-service/{serviceId}
        [HttpDelete("{categoryId}/remove-service/{serviceId}")]
        [Authorize(Roles = "Admin,Employee")]
        public async Task<IActionResult> RemoveServiceFromCategory(Guid categoryId, Guid serviceId)
        {
            var command = new RemoveServiceFromCategoryCommand(categoryId, serviceId);
            var result = await _mediator.Send(command);
            if (!result.Success) return NotFound();
            return NoContent();
        }
    }
} 