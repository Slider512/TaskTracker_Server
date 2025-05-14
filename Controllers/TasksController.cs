using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Server.Data;
using Server.DTOs;
using Server.Models;
using Server.Services;
using Task = Server.Models.Task;

namespace Server.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class TasksController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly NotificationService _notificationService;
        private readonly AnalyticsService _analyticsService;

        public TasksController(AppDbContext context, NotificationService notificationService, AnalyticsService analyticsService)
        {
            _context = context;
            _notificationService = notificationService;
            _analyticsService = analyticsService;
        }

        // GET: api/tasks
        [HttpGet]
        public async Task<ActionResult<IEnumerable<TaskDto>>> GetTasks(
            [FromQuery] string? status,
            [FromQuery] string? assignee,
            [FromQuery] string? sortBy,
            [FromQuery] string? sortOrder,
            [FromQuery] string? parentId)
        {
            var query = _context.Tasks
                .Include(t => t.IncomingDependencies)
                .Include(t => t.OutgoingDependencies)
                .Include(t => t.Subtasks)
                .AsQueryable();

            // Фильтрация
            if (!string.IsNullOrEmpty(status))
                query = query.Where(t => t.Status == status);
            if (!string.IsNullOrEmpty(assignee))
                query = query.Where(t => t.Assignee == assignee);
            if (!string.IsNullOrEmpty(parentId))
                query = query.Where(t => t.ParentTaskId.ToString() == parentId);

            // Сортировка
            if (!string.IsNullOrEmpty(sortBy))
            {
                bool isDescending = sortOrder?.ToLower() == "desc";
                query = sortBy.ToLower() switch
                {
                    "title" => isDescending ? query.OrderByDescending(t => t.Title) : query.OrderBy(t => t.Title),
                    "startdate" => isDescending ? query.OrderByDescending(t => t.StartDate) : query.OrderBy(t => t.StartDate),
                    "enddate" => isDescending ? query.OrderByDescending(t => t.EndDate) : query.OrderBy(t => t.EndDate),
                    "progress" => isDescending ? query.OrderByDescending(t => t.Progress) : query.OrderBy(t => t.Progress),
                    _ => query
                };
            }

            var tasks = await query
                .Select(t => new TaskDto
                {
                    Id = t.Id,
                    Title = t.Title,
                    Assignee = t.Assignee,
                    Status = t.Status,
                    StartDate = t.StartDate,
                    EndDate = t.EndDate,
                    Progress = t.Progress,
                    CreatedAt = t.CreatedAt,
                    UpdatedAt = t.UpdatedAt,
                    ParentId = t.ParentTaskId,
                    Subtasks = t.Subtasks.Select(c => c.Id).ToList(),
                    Dependencies = t.IncomingDependencies
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
                        .ToList()
                })
                .ToListAsync();

            return Ok(tasks);
        }

        // GET: api/tasks/{id}
        [HttpGet("{id}")]
        public async Task<ActionResult<TaskDto>> GetTask(string id)
        {
            var task = await _context.Tasks
                .Include(t => t.IncomingDependencies)
                .Include(t => t.OutgoingDependencies)
                .Include(t => t.Subtasks)
                .FirstOrDefaultAsync(t => t.Id.ToString() == id);

            if (task == null)
                return NotFound();

            return Ok(new TaskDto
            {
                Id = task.Id,
                Title = task.Title,
                Assignee = task.Assignee,
                Status = task.Status,
                StartDate = task.StartDate,
                EndDate = task.EndDate,
                Progress = task.Progress,
                CreatedAt = task.CreatedAt,
                UpdatedAt = task.UpdatedAt,
                ParentId = task.ParentTaskId,
                Subtasks = task.Subtasks.Select(c => c.Id).ToList(),
                Dependencies = task.IncomingDependencies
                    .Select(d => new DependencyDto
                    {
                        Id = d.Id,
                        FromTaskId = d.FromTaskId,
                        ToTaskId = d.ToTaskId,
                        Type = d.Type
                    })
                    .Concat(task.OutgoingDependencies
                        .Select(d => new DependencyDto
                        {
                            Id = d.Id,
                            FromTaskId = d.FromTaskId,
                            ToTaskId = d.ToTaskId,
                            Type = d.Type
                        }))
                    .ToList()
            });
        }

        // POST: api/tasks
        [HttpPost]
        public async Task<ActionResult<TaskDto>> CreateTask(TaskDto taskDto)
        {
            // Валидация ParentId
            if (taskDto.ParentId != null && !await _context.Tasks.AnyAsync(t => t.Id == taskDto.ParentId))
                return BadRequest("Invalid ParentId.");

            var task = new Task
            {
                Title = taskDto.Title,
                Assignee = taskDto.Assignee,
                Status = taskDto.Status ?? "Not Started",
                StartDate = taskDto.StartDate,
                EndDate = taskDto.EndDate,
                Progress = taskDto.Progress,
                CreatedAt = DateTime.UtcNow,
                ParentTaskId = taskDto.ParentId
            };

            _context.Tasks.Add(task);
            await _context.SaveChangesAsync();

            // Создание зависимостей
            foreach (var depDto in taskDto.Dependencies)
            {
                if (!await _context.Tasks.AnyAsync(t => t.Id == depDto.FromTaskId) ||
                    !await _context.Tasks.AnyAsync(t => t.Id == depDto.ToTaskId))
                    continue; // Пропускаем невалидные зависимости

                var dependency = new Dependency
                {
                    FromTaskId = depDto.FromTaskId,
                    ToTaskId = depDto.ToTaskId,
                    Type = depDto.Type
                };
                _context.Dependencies.Add(dependency);
            }

            await _context.SaveChangesAsync();

            // Уведомление через SignalR
            await _notificationService.NotifyTaskCreated(task.Id.ToString(), task.Title);

            return CreatedAtAction(nameof(GetTask), new { id = task.Id }, new TaskDto
            {
                Id = task.Id,
                Title = task.Title,
                Assignee = task.Assignee,
                Status = task.Status,
                StartDate = task.StartDate,
                EndDate = task.EndDate,
                Progress = task.Progress,
                CreatedAt = task.CreatedAt,
                ParentId = task.ParentTaskId,
                Subtasks = new List<Guid>(),
                Dependencies = taskDto.Dependencies
            });
        }

        // PUT: api/tasks/{id}
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateTask(string id, TaskDto taskDto)
        {
            var task = await _context.Tasks
                .Include(t => t.IncomingDependencies)
                .Include(t => t.OutgoingDependencies)
                .FirstOrDefaultAsync(t => t.Id.ToString() == id);
            if (task == null)
                return NotFound();

            // Валидация ParentId
            if (taskDto.ParentId != null && !await _context.Tasks.AnyAsync(t => t.Id == taskDto.ParentId))
                return BadRequest("Invalid ParentId.");

            // Обновление полей задачи
            task.Title = taskDto.Title;
            task.Assignee = taskDto.Assignee;
            task.Status = taskDto.Status;
            task.StartDate = taskDto.StartDate;
            task.EndDate = taskDto.EndDate;
            task.Progress = taskDto.Progress;
            task.UpdatedAt = DateTime.UtcNow;
            task.ParentTaskId = taskDto.ParentId;

            // Обновление зависимостей
            var existingDepIds = task.IncomingDependencies
                .Concat(task.OutgoingDependencies)
                .Select(d => d.Id)
                .ToList();
            var newDepIds = taskDto.Dependencies.Select(d => d.Id).ToList();

            // Удаление старых зависимостей
            var depsToRemove = existingDepIds.Except(newDepIds).ToList();
            _context.Dependencies.RemoveRange(
                _context.Dependencies.Where(d => depsToRemove.Contains(d.Id)));

            // Добавление новых зависимостей
            foreach (var depDto in taskDto.Dependencies)
            {
                if (!await _context.Tasks.AnyAsync(t => t.Id == depDto.FromTaskId) ||
                    !await _context.Tasks.AnyAsync(t => t.Id == depDto.ToTaskId))
                    continue;

                if (!existingDepIds.Contains(depDto.Id))
                {
                    var dependency = new Dependency
                    {
                        Id = depDto.Id == Guid.Empty ? Guid.NewGuid() :depDto.Id,
                        FromTaskId = depDto.FromTaskId,
                        ToTaskId = depDto.ToTaskId,
                        Type = depDto.Type
                    };
                    _context.Dependencies.Add(dependency);
                }
            }

            await _context.SaveChangesAsync();

            // Уведомление через SignalR
            await _notificationService.NotifyTaskUpdated(task.Id.ToString(), task.Title);

            return NoContent();
        }

        // DELETE: api/tasks/{id}
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteTask(string id)
        {
            var task = await _context.Tasks
                .Include(t => t.IncomingDependencies)
                .Include(t => t.OutgoingDependencies)
                .Include(t => t.Subtasks)
                .FirstOrDefaultAsync(t => t.Id.ToString() == id);
            if (task == null)
                return NotFound();

            // Проверка зависимостей
            if (task.IncomingDependencies.Any() || task.OutgoingDependencies.Any())
                return BadRequest("Cannot delete task with existing dependencies.");

            _context.Tasks.Remove(task);
            await _context.SaveChangesAsync();

            // Уведомление через SignalR
            await _notificationService.NotifyTaskDeleted(id);

            return NoContent();
        }

        // POST: api/tasks/dependencies
        [HttpPost("dependencies")]
        public async Task<ActionResult<DependencyDto>> CreateDependency(DependencyDto depDto)
        {
            if (!await _context.Tasks.AnyAsync(t => t.Id == depDto.FromTaskId) ||
                !await _context.Tasks.AnyAsync(t => t.Id == depDto.ToTaskId))
                return BadRequest("Invalid FromTaskId or ToTaskId.");

            if (!new[] { "FS", "FF", "SS", "SF" }.Contains(depDto.Type))
                return BadRequest("Invalid dependency type. Must be FS, FF, SS, or SF.");

            var dependency = new Dependency
            {
                FromTaskId = depDto.FromTaskId,
                ToTaskId = depDto.ToTaskId,
                Type = depDto.Type
            };

            _context.Dependencies.Add(dependency);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetTask), new { id = depDto.ToTaskId }, depDto);
        }

        // DELETE: api/tasks/dependencies/{id}
        [HttpDelete("dependencies/{id}")]
        public async Task<IActionResult> DeleteDependency(string id)
        {
            var dependency = await _context.Dependencies.FindAsync(id);
            if (dependency == null)
                return NotFound();

            _context.Dependencies.Remove(dependency);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        // GET: api/tasks/export/excel
        [HttpGet("export/excel")]
        public async Task<IActionResult> ExportToExcel()
        {
            var tasks = await _context.Tasks.ToListAsync();
            var stream = _analyticsService.GenerateExcel(tasks);
            return File(stream, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", "Tasks.xlsx");
        }
    }
}
