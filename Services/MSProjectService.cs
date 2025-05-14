using Aspose.Tasks;

namespace Server.Services
{
    public class MSProjectService
    {
        public List<Server.Models.Task> ParseMppFile(string filePath)
        {
            var project = new Project(filePath);
            var tasks = new List<Server.Models.Task>();

            foreach (var task in project.RootTask.Children)
            {
                tasks.Add(new Server.Models.Task
                {
                    Id = new Guid(task.Guid),
                    Title = task.Name,
                    StartDate = task.Start,
                    EndDate = task.Finish,
                    Progress = task.PercentComplete,
                    // = task.Cost,
                    EffortHours = (int)(task.Work.Convert(TimeUnitType.Hour).ToDouble())
                });
            }

            return tasks;
        }
    }
}
