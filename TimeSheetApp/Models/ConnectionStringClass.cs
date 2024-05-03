using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using timesheetapps.Models;

namespace timesheetapps.Models
{
    public class ConnectionStringClass : DbContext
    {
        public ConnectionStringClass(DbContextOptions<ConnectionStringClass> options) : base(options)
        {

        }
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            //HasNokey
            modelBuilder.Entity<LoginClass>().HasNoKey();
        }

        public DbSet<UserClass> Users { get; set; }
        public DbSet<RolesClass> Roles { get; set; }
        public DbSet<AccountClass> Account { get; set; }
        public DbSet<TimeSheetClass> TimeSheet { get; set; }
        public DbSet<LoginClass> LoginUser { get; set; }
        public DbSet<DisplayTimeSheetClass> DisplayTimeSheet { get; set; }
        public DbSet<DisplayUsersClass> DisplayUsers { get; set; }

        internal Task SignOutAsync()
        {
            throw new NotImplementedException();
        }
    }
}
