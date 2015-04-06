using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Text;

namespace ORMs.EntityFramework
{
    public class DataContext : DbContext
    {
        public DataContext()
            : base("Connection")
        {
            this.Configuration.LazyLoadingEnabled = false;
        }

        public DbSet<Entidade> Lancamentos { get; set; }

        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            Database.SetInitializer<DataContext>(null);

            modelBuilder.Entity<Entidade>().ToTable(Entidade.TABLE_NAME);
        }
    }
}
