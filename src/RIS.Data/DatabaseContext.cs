#region

using System.Configuration;
using System.Data.Entity;
using System.Data.Entity.Core.EntityClient;
using System.Data.Entity.Validation;
using System.Data.SqlServerCe;
using System.Linq;
using System.Threading.Tasks;
using RIS.Model;

#endregion

namespace RIS.Data
{
    public class DatabaseContext : DbContext
    {
        //Enable-Migrations -ProjectName RIS.Data -StartUpProjectName RIS -Verbose   
        //Add-Migration CreateDatabase -IgnoreChanges -ProjectName RIS.Data -StartUpProjectName RIS -Verbose   
        //Add-Migration V7_0_0 -ProjectName RIS.Data -StartUpProjectName RIS -Verbose       
        //Add-Migration V7_0_1 -ProjectName RIS.Data -StartUpProjectName RIS -Verbose       
        //Add-Migration V7_2_0 -ProjectName RIS.Data -StartUpProjectName RIS -Verbose     

        public DatabaseContext() : base("ConnectionString")
        {
#if DEBUG
            //this.Database.Log = Console.Write;
#endif

            Configuration.LazyLoadingEnabled = false;
            Configuration.ProxyCreationEnabled = false;
        }

        #region Private Properties

        private static string ConnectionString
        {
            get
            {
                var _connectionString = ConfigurationManager.ConnectionStrings["ConnectionString"].ConnectionString;
                var _entityStringBuilder = new EntityConnectionStringBuilder
                {
                    Provider = "System.Data.SqlServerCe.4.0",
                    Metadata = @"res://*/",
                    ProviderConnectionString = new SqlCeConnectionStringBuilder(_connectionString).ToString()
                };
                return new SqlCeConnectionStringBuilder(_connectionString).ToString();
            }
        }

        #endregion //Private Properties

        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            //AMS
            modelBuilder.Entity<Ams>().HasMany(x => x.Users).WithMany(x => x.Amss).Map(x =>
            {
                x.ToTable("AmsUsers");
                x.MapLeftKey("AmsId");
                x.MapRightKey("UserId");
            });
            modelBuilder.Entity<Ams>().HasMany(x => x.Pagers).WithMany(x => x.Amss).Map(x =>
            {
                x.ToTable("AmsPagers");
                x.MapLeftKey("AmsId");
                x.MapRightKey("PagerId");
            });

            //AlarmappGroups
            modelBuilder.Entity<AlarmappGroup>().HasMany(x => x.Pagers).WithMany(x => x.AlarmappGroups).Map(x =>
            {
                x.ToTable("AlarmappGroupPagers");
                x.MapLeftKey("AlarmappGroupId");
                x.MapRightKey("PagerId");
            });
            modelBuilder.Entity<AlarmappGroup>().HasMany(x => x.Vehicles).WithMany(x => x.AlarmappGroups).Map(x =>
            {
                x.ToTable("AlarmappGroupVehicles");
                x.MapLeftKey("AlarmappGroupId");
                x.MapRightKey("VehicleId");
            });

            base.OnModelCreating(modelBuilder);
        }

        #region DbSets

        public DbSet<AlarmappGroup> AlarmappGroups { get; set; }
        public DbSet<AlarmappDepartment> AlarmappDepartments { get; set; }
        public DbSet<Aao> Aaos { get; set; }
        public DbSet<AaoCondition> AaoConditions { get; set; }
        public DbSet<AaoVehicle> AaoVehicles { get; set; }
        public DbSet<Fileprint> Fileprints { get; set; }
        public DbSet<FileprintCondition> FileprintConditions { get; set; }
        public DbSet<Filter> Filters { get; set; }
        public DbSet<FilterField> FilterFields { get; set; }
        public DbSet<Printer> Printers { get; set; }
        public DbSet<User> Users { get; set; }
        public DbSet<Vehicle> Vehicles { get; set; }
        public DbSet<Pager> Pagers { get; set; }
        public DbSet<Ams> Amss { get; set; }

        #endregion //DbSets  

        #region Public Functions

        public override int SaveChanges()
        {
            try
            {
                return base.SaveChanges();
            }
            catch (DbEntityValidationException ex)
            {
                var errorMessages = ex.EntityValidationErrors.SelectMany(x => x.ValidationErrors)
                    .Select(x => x.ErrorMessage);
                var exceptionMessage = "Validation-Errors: " + string.Join(" | ", errorMessages);

                throw new DbEntityValidationException(exceptionMessage, ex.EntityValidationErrors);
            }
        }

        public override Task<int> SaveChangesAsync()
        {
            try
            {
                return base.SaveChangesAsync();
            }
            catch (DbEntityValidationException ex)
            {
                var errorMessages = ex.EntityValidationErrors.SelectMany(x => x.ValidationErrors)
                    .Select(x => x.ErrorMessage);
                var exceptionMessage = "Validation-Errors: " + string.Join(" | ", errorMessages);

                throw new DbEntityValidationException(exceptionMessage, ex.EntityValidationErrors);
            }
        }

        public void CheckConnection()
        {
            Database.Initialize(true);
            Database.Connection.Open();
            Database.Connection.Close();
        }

        #endregion //Public Functions
    }
}