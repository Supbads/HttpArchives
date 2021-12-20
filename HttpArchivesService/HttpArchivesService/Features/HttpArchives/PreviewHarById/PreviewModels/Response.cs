namespace HttpArchivesService.Features.HttpArchives.PreviewHarById.PreviewModels
{
    public class Response
    {
        public int Status { get; set; }
        public string StatusText { get; set; }
        public Content Content { get; set; }
        public string RedirectURL { get; set; }
        public int HeadersSize { get; set; }
        public int BodySize { get; set; }
        public string _Error { get; set; }
    }
}
