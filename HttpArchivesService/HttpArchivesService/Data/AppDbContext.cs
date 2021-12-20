using HttpArchivesService.Data.Entities;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace HttpArchivesService.Data
{
    public class AppDbContext : IdentityDbContext //DbContext
    {
        public DbSet<HttpArchiveRecord> HttpArchiveRecords { get; set; }
        public DbSet<Directory> Directories { get; set; }
        public object Where { get; internal set; }

        public AppDbContext(DbContextOptions<AppDbContext> dbContextOptions) : base(dbContextOptions)
        {
            
        }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            builder.Entity<HttpArchiveRecord>(b => {
                b.ToTable("HttpArchiveRecords");
                
                b.HasKey(x => x.Id);
                b.Property(x => x.Id).ValueGeneratedOnAdd();
                b.Property(x => x.UserId).IsRequired();
                b.Property(x => x.FileName).IsRequired();
                b.HasOne(x => x.User).WithMany().HasForeignKey(x => x.UserId);
                b.Property(x => x.DirId).IsRequired(false); //todo test root
                b.HasOne(x => x.Directory).WithMany(d => d.ArchiveRecords); //.IsRequired(false); todo test
            });

            builder.Entity<Directory>(b => {
                b.ToTable("Directories");

                b.HasKey(x => x.Id);
                b.Property(x => x.Id).ValueGeneratedOnAdd();
                b.Property(x => x.DirectoryName).IsRequired();
                b.Property(x => x.ParentDirId).IsRequired(false);
                b.HasOne(x => x.ParentDirectory).WithMany(x => x.NestedDirectories).HasForeignKey(x => x.ParentDirId);
                b.Property(x => x.UserId).IsRequired(true);
                b.HasOne(x => x.User).WithMany().HasForeignKey(x => x.UserId);
            });
        }
            
    }
}
