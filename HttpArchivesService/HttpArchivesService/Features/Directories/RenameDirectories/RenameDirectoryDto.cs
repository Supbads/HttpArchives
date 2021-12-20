using System;

namespace HttpArchivesService.Features.Directories.RenameDirectories
{
    public class RenameDirectoryDto
    {
        public int DirectoryId { get; set; }
        public string NewName { get; set; }
    }
}
