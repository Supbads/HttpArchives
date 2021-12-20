using Microsoft.AspNetCore.Http;
using System;

namespace HttpArchivesService.Features.HttpArchives.UploadHarFiles
{
    public class HARUploadDto
    {
        public string LocalId { get; set; } // id on fe (unpersisted)
        public string Name { get; set; }
        public int? DirId { get; set; }
        public IFormFile File { get; set; }
    }
}