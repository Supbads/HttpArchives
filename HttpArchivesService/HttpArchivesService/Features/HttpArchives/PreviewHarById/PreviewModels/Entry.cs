using System;

namespace HttpArchivesService.Features.HttpArchives.PreviewHarById.PreviewModels
{
    public class Entry
    {
        public string PageRef { get; set; }

        public Initiator _initiator { get; set; }

        public string _priority { get; set; }
        public string Priority { get; set; }

        public DateTimeOffset StartedDateTime { get; set; }

        public double Time { get; set; }

        public Request Request { get; set; }

        public Response Response { get; set; }

        public Timings Timings { get; set; }

        public string ServerIPAddress { get; set; }

        public string Connection { get; set; }
    }
}
