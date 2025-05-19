using ClosedXML.Excel;
using ProjectTask = Server.Models.ProjectTask;

namespace Server.Services
{
    public class AnalyticsService
    {
        public MemoryStream GenerateExcel(List<ProjectTask> tasks)
        {
            using var workbook = new XLWorkbook();
            var worksheet = workbook.Worksheets.Add("Tasks");

            // Заголовки
            worksheet.Cell(1, 1).Value = "ID";
            worksheet.Cell(1, 2).Value = "Title";
            worksheet.Cell(1, 3).Value = "Assignee";
            worksheet.Cell(1, 4).Value = "Status";
            worksheet.Cell(1, 5).Value = "Start Date";
            worksheet.Cell(1, 6).Value = "End Date";
            worksheet.Cell(1, 7).Value = "Progress";
            worksheet.Cell(1, 8).Value = "Parent ID";
            worksheet.Cell(1, 9).Value = "Children";
            worksheet.Cell(1, 10).Value = "Dependencies";

            // Данные
            for (int i = 0; i < tasks.Count; i++)
            {
                var task = tasks[i];
                worksheet.Cell(i + 2, 1).Value = task.Id.ToString();
                worksheet.Cell(i + 2, 2).Value = task.Title;
                worksheet.Cell(i + 2, 3).Value = task.AssignedUsers.Count==0? "Unassigned": string.Join(';', task.AssignedUsers.Select(item=>item.Title).ToArray());
                worksheet.Cell(i + 2, 4).Value = task.Status ?? "Not Started";
                worksheet.Cell(i + 2, 5).Value = task.StartDate.ToString("yyyy-MM-dd");
                worksheet.Cell(i + 2, 6).Value = task.EndDate.ToString("yyyy-MM-dd");
                worksheet.Cell(i + 2, 7).Value = task.Progress;
                worksheet.Cell(i + 2, 8).Value = task.ParentTaskId.ToString() ?? "";
                worksheet.Cell(i + 2, 9).Value = string.Join(", ", task.Subtasks.Select(c => c.Id));
                worksheet.Cell(i + 2, 10).Value = string.Join(", ", task.IncomingDependencies
                    .Select(d => $"{d.FromTaskId}→{d.ToTaskId} ({d.Type})"));
            }

            var stream = new MemoryStream();
            workbook.SaveAs(stream);
            stream.Position = 0;
            return stream;
        }
    }
}
