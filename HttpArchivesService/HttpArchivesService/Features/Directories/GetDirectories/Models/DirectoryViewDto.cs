using System;
using System.Collections.Generic;

namespace HttpArchivesService.Features.Directories.GetDirectories
{
    public class DirectoryViewDto
    {
        public int Id { get; set; }
        public int? ParentDirId { get; set; }
        public string Name { get; set; }

        public List<DirectoryViewDto> InnerDirectories { get; set; }
        public List<HarFileDto> HarFilesPreview { get; set; }
    }
}