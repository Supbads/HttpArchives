using Microsoft.AspNetCore.Identity;

namespace HttpArchivesService.Data.Entities
{
    public class HttpArchiveRecord
    {
        public int Id { get; set; }

        public string FileName { get; set; }
        public string UserId { get; set; }
        public int? DirId { get; set; } //nullable can be root

        //public FormFile HarFile { get; set; }

        public IdentityUser User { get; set; }
        public Directory Directory { get; set; }
    }
}
