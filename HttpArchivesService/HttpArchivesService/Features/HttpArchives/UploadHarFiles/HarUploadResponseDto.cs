using System;
using System.Collections.Generic;

namespace HttpArchivesService.Features.HttpArchives.UploadHarFiles
{
    public class HarUploadResponseDto
    {
        public Dictionary<string, int> HarFilesNameToIdMapping { get; set; }
    }
}