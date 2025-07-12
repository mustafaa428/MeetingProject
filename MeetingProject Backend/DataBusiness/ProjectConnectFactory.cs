using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace DataBusiness
{
    public class ProjectConnectFactory : IDesignTimeDbContextFactory<ProjectConnect>
    {
        public ProjectConnect CreateDbContext(string[] args)
        {
            var optionsBuilder = new DbContextOptionsBuilder<ProjectConnect>();
            // Connection stringinizi buraya ekleyin
            optionsBuilder.UseSqlServer("Server=(localdb)\\MSSQLLocalDB;Database=MeetingProject;Trusted_Connection=True;");

            return new ProjectConnect(optionsBuilder.Options);
        }
    }
}
