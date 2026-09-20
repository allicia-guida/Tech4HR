using Microsoft.EntityFrameworkCore;

namespace Tech4Hr.API.Data
{
    public class Tech4HrDbContext : DbContext
    {
        public Tech4HrDbContext(DbContextOptions<Tech4HrDbContext> options)
            : base(options)
        {
        }

        // DbSets serão adicionados conforme os modelos forem criados
    }
}
