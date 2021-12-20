using System;

namespace HttpArchivesService.Features.HttpArchives.GetHarsByCurrentUser
{
    public class HarFileDto
    {
        public int Id { get; set; }
        public int? DirectoryId { get; set; }
        public string Name { get; set; }
    }
}
