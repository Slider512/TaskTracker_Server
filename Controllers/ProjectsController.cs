using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Server.Data;
using Server.DTOs;
using Server.Models;
using Server.Services;
using System.Data.Entity;
using System.Security.Claims;

namespace Server.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    public class ProjectsController : ControllerBase
    {
        private readonly UserManager<User> _userManager;
        private readonly AppDbContext _context;

        public ProjectsController(AppDbContext context, UserManager<User> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        // GET: api/projects
        [HttpGet]
        public async Task<ActionResult<IEnumerable<ProjectDto>>> GetProjects()
        {
            var userId = base.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (userId == null)
            {
                return Unauthorized(new { message = "User not authenticated" });
            }

            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
            {
                return NotFound(new { message = "User not found" });
            }

            var query = _context.Projects
                .Where(item=>item.CompanyId== user.CompanyId)
                .AsQueryable();

            var projects = await query
                .Select(t => new ProjectDto
                {
                    Id = t.Id,
                    Name = t.Name,
                    CompanyId = t.CompanyId,
                })
                .ToListAsync();

            return Ok(projects);
        }

        [HttpGet("{projectId}/tasks")]
        public async Task<ActionResult<IEnumerable<TaskDto>>> GetProjectTasks(string projectId)
        {
            var userId = base.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (userId == null)
            {
                return Unauthorized(new { message = "User not authenticated" });
            }

            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
            {
                return NotFound(new { message = "User not found" });
            }

            var project = _context.Projects
                .Where(item => item.CompanyId == user.CompanyId && item.Id.ToString() == projectId)
                .AsQueryable().FirstOrDefault();

            if (project == null)
            {
                return NotFound(new { message = "Project not found" });
            }

            var query = _context.ProjectTasks
                .Where(item => item.CompanyId == user.CompanyId && item.ProjectId == project.Id)
                .AsQueryable();

            var tasks = await query
                .Select(t => new TaskDto
                {
                    Id = t.Id,
                    Title = t.Title,
                    AssignedUsers = t.AssignedUsers.Select(item=>item.Id).ToList(),
                    Status = t.Status,
                    StartDate = t.StartDate,
                    EndDate = t.EndDate,
                    Progress = t.Progress,
                    CreatedAt = t.CreatedAt,
                    UpdatedAt = t.UpdatedAt,
                    ParentId = t.ParentTaskId,
                    Subtasks = t.Subtasks.Select(c => c.Id).ToList(),
                    /* Dependencies = t.IncomingDependencies
                         .Select(d => new DependencyDto
                         {
                             Id = d.Id,
                             FromTaskId = d.FromTaskId,
                             ToTaskId = d.ToTaskId,
                             Type = d.Type
                         })
                         .Concat(t.OutgoingDependencies
                             .Select(d => new DependencyDto
                             {
                                 Id = d.Id,
                                 FromTaskId = d.FromTaskId,
                                 ToTaskId = d.ToTaskId,
                                 Type = d.Type
                             }))
                         .ToList()*/
                })
                .ToListAsync();

            return Ok(tasks);
        }

        [HttpPost]
        public async Task<ActionResult<Project>> CreateProject([FromBody] Project project)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            project.Id = Guid.NewGuid();
            project.CreatedAt = DateTime.UtcNow;

            _context.Projects.Add(project);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetProjects), new { id = project.Id }, project);
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteProject(string id)
        {
            var project = await _context.Projects.FindAsync(id);
            if (project == null)
            {
                return NotFound();
            }

            _context.Projects.Remove(project);
            await _context.SaveChangesAsync();

            return NoContent();
        }
    }
}
