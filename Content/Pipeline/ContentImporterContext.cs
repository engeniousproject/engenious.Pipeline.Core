using System;

namespace engenious.Content.Pipeline
{
    public class ContentImporterContext : ContentContext
    {
        public ContentImporterContext(Guid buildId, CreatedContentCode createdContentCode, string contentDirectory)
            : base(buildId, createdContentCode, contentDirectory)
        {
        }

        public override void Dispose()
        {
        }
    }
}

