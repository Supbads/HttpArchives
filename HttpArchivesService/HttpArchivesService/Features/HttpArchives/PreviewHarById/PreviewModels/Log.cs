using System.Collections.Generic;

namespace HttpArchivesService.Features.HttpArchives.PreviewHarById.PreviewModels
{
    public class Log
    {
        public string Version { get; set; }

        public Creator Creator { get; set; }

        public List<Page> Pages { get; set; }

        public List<Entry> Entries { get; set; }
    }
}
