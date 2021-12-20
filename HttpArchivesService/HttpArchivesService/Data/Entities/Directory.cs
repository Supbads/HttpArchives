using Microsoft.AspNetCore.Identity;
using System.Collections.Generic;

namespace HttpArchivesService.Data.Entities
{
    public class Directory
    {
        public int Id { get; set; }
        public int? ParentDirId { get; set; }
        public string UserId { get; set; }
        public string DirectoryName { get; set; }

        public IdentityUser User { get; set; } //ref
        public Directory ParentDirectory { get; set; }
        public ICollection<Directory> NestedDirectories { get; set; }
        public ICollection<HttpArchiveRecord> ArchiveRecords { get; set; }
    }
}
