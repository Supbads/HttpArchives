using System;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;

namespace HttpArchivesService.Data.Entities
{
    public class HttpArchiveRecord
    {
        //[Key] // todo: test
        //[DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        public string FileName { get; set; }
        public string UserId { get; set; } // user it belongs to not nullable
        public int? DirId { get; set; } //nullable can be root
        //public FormFile HarFile { get; set; }

        public IdentityUser User { get; set; } 
        public Directory Directory { get; set; }
    }
}
