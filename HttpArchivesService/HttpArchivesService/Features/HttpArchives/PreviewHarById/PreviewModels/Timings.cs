namespace HttpArchivesService.Features.HttpArchives.PreviewHarById.PreviewModels
{
    public class Timings
    {
        public double Blocked { get; set; }
        public double Dns { get; set; }
        public double SSL { get; set; }
        public double Connect { get; set; }
        public double Send { get; set; }
        public double Wait { get; set; }
        public double Receive { get; set; }
        public double BlockedQueueing { get; set; }
    }
}