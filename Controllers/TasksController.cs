using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Server.Data;

namespace Server.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class TasksController : ControllerBase
    {
        private readonly AppDbContext _context;

        public TasksController(AppDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<Server.Models.Task>>> GetTasks()
        {
            return await _context.Tasks.ToListAsync();
        }

        [HttpPost]
        public async Task<ActionResult<Server.Models.Task>> CreateTask(Server.Models.Task task)
        {
            _context.Tasks.Add(task);
            await _context.SaveChangesAsync();
            return CreatedAtAction(nameof(GetTasks), new { id = task.Id }, task);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateTask(Guid id, Server.Models.Task task)
        {
            if (id != task.Id) return BadRequest();

            _context.Entry(task).State = EntityState.Modified;
            await _context.SaveChangesAsync();
            return NoContent();
        }

        [HttpGet("filter")]
        public async Task<ActionResult<IEnumerable<Server.Models.Task>>> GetFilteredTasks(
    [FromQuery] string? assignee,
    [FromQuery] string? status,
    [FromQuery] string? sortBy,
    [FromQuery] bool desc = false)
        {
            var query = _context.Tasks.AsQueryable();

            // Фильтрация
            if (!string.IsNullOrEmpty(assignee))
                query = query.Where(t => t.Assignee == assignee);

            if (!string.IsNullOrEmpty(status))
                query = query.Where(t => t.Status == status);

            // Сортировка
            query = sortBy?.ToLower() switch
            {
                "title" => desc ? query.OrderByDescending(t => t.Title) : query.OrderBy(t => t.Title),
                "startdate" => desc ? query.OrderByDescending(t => t.StartDate) : query.OrderBy(t => t.StartDate),
                "enddate" => desc ? query.OrderByDescending(t => t.EndDate) : query.OrderBy(t => t.EndDate),
                "progress" => desc ? query.OrderByDescending(t => t.Progress) : query.OrderBy(t => t.Progress),
                _ => query
            };

            return await query.ToListAsync();
        }

        [HttpGet("grouped")]
        public async Task<ActionResult<Dictionary<string, List<Server.Models.Task>>>> GetGroupedTasks(
            [FromQuery] string groupBy = "status")
        {
            var tasks = await _context.Tasks.ToListAsync();

            return groupBy.ToLower() switch
            {
                "assignee" => tasks.GroupBy(t => t.Assignee ?? "Unassigned")
                                 .ToDictionary(g => g.Key, g => g.ToList()),
                "status" => tasks.GroupBy(t => t.Status ?? "Not Started")
                               .ToDictionary(g => g.Key, g => g.ToList()),
                _ => tasks.GroupBy(t => t.Status ?? "Not Started")
                        .ToDictionary(g => g.Key, g => g.ToList())
            };
        }

        [HttpGet("export/excel")]
        public async Task<IActionResult> ExportToExcel()
        {
            var tasks = await _context.Tasks.ToListAsync();

            using var package = new ExcelPackage();
            var worksheet = package.Workbook.Worksheets.Add("Tasks");

            // Заголовки
            worksheet.Cells[1, 1].Value = "Title";
            worksheet.Cells[1, 2].Value = "Assignee";
            // ... остальные заголовки

            // Данные
            for (var i = 0; i < tasks.Count; i++)
            {
                worksheet.Cells[i + 2, 1].Value = tasks[i].Title;
                worksheet.Cells[i + 2, 2].Value = tasks[i].Assignee;
                // ... остальные данные
            }

            var stream = new MemoryStream();
            package.SaveAs(stream);
            stream.Position = 0;

            return File(stream, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", "tasks.xlsx");
        }
    }
}
