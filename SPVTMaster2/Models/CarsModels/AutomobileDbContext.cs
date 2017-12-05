using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SPVTMaster2.Models.CarsModels
{
    public class AutomobileDbContext : DbContext
    {
        public AutomobileDbContext(DbContextOptions<AutomobileDbContext> options) : base(options)
        {
           
        }
        public DbSet<Cars> Cars { get; set; }

    }
}
