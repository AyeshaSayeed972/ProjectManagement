using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ProjectManagement.DTOs.Common;
using ProjectManagement.DTOs.Release;
using ProjectManagement.Services.Interfaces;

namespace ProjectManagement.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class ReleasesController : ControllerBase
{
    private readonly IReleaseService _releaseService;

    public ReleasesController(IReleaseService releaseService)
    {
        _releaseService = releaseService;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll([FromQuery] PaginationQuery pagination)
    {
        var result = await _releaseService.GetAllAsync(pagination.PageNumber, pagination.PageSize);
        return Ok(result);
    }

    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetById(int id)
    {
        var release = await _releaseService.GetByIdAsync(id);
        return release is null ? NotFound() : Ok(release);
    }

    [HttpPost]
    [Authorize(Roles = "PM")]
    public async Task<IActionResult> Create([FromBody] CreateReleaseDto dto)
    {
        var created = await _releaseService.CreateAsync(dto);
        return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
    }

    [HttpPut("{id:int}")]
    [Authorize(Roles = "PM")]
    public async Task<IActionResult> Update(int id, [FromBody] UpdateReleaseDto dto)
    {
        var updated = await _releaseService.UpdateAsync(id, dto);
        return Ok(updated);
    }

    [HttpDelete("{id:int}")]
    [Authorize(Roles = "PM")]
    public async Task<IActionResult> Delete(int id)
    {
        await _releaseService.DeleteAsync(id);
        return NoContent();
    }
}
