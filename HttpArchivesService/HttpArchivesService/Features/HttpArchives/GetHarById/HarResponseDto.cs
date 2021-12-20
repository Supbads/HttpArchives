using System;

namespace HttpArchivesService.Features.HttpArchives.GetHarById
{
    public class HarResponseDto
    {
        public int Id { get; set; }
        public string FileName { get; set; }
        public int? DirId { get; set; }

        //public FormFile HarFile { get; set; }
    }
}