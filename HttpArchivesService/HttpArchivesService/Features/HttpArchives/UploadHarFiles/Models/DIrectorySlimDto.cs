using System;
using System.Collections.Generic;

namespace HttpArchivesService.Features.HttpArchives.UploadHarFiles
{
    public class DIrectorySlimDto
    {
        public int Id { get; set; }
        public string UserId { get; set; }
        public IEnumerable<HarFileSlimDto> HARs { get; set; }
    }
}