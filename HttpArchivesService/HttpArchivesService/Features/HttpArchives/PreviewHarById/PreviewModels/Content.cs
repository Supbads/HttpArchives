namespace HttpArchivesService.Features.HttpArchives.PreviewHarById.PreviewModels
{
    public class Content
    {
        public int Size { get; set; }
        public string MimeType { get; set; }

        //nukes the performance if mime type is text/html
        //public string Text { get; set; }
    }
}
