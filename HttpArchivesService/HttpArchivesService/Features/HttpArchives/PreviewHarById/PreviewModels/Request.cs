using System.Collections.Generic;

namespace HttpArchivesService.Features.HttpArchives.PreviewHarById.PreviewModels
{
    public class Request
    {
        public string Method { get; set; }
        
        public string Url { get; set; }

        public List<NameValuePair> Headers { get; set; }
        
        public List<NameValuePair> QueryString { get; set; }
    }
}
