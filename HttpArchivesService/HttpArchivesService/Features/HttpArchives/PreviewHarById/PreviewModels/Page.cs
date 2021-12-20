using System;

namespace HttpArchivesService.Features.HttpArchives.PreviewHarById.PreviewModels
{
    public class Page
    {
        public DateTimeOffset StartedDateTime { get; set; }

        public string Id { get; set; }

        public string Title { get; set; }

        public PageTimings PageTimings { get; set; }
    }
}
