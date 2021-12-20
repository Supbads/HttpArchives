using System.Collections.Generic;

namespace HttpArchivesService.Features.Directories.GetDirectories
{
    public class RootDirectory
    {
        public List<DirectoryViewDto> InnerDirectories { get; set; }
        public List<HarFileDto> HarFiles { get; set; }
    }
}
