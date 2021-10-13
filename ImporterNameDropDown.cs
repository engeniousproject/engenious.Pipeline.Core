using System.ComponentModel;
using System.IO;
using engenious.Content.Models;

namespace engenious.Content
{
    internal class ImporterNameDropDownConverter : StringConverter
    {
        public override bool GetStandardValuesSupported(ITypeDescriptorContext context)
        {
            return context.Instance is ContentFile;
        }

        public override bool GetStandardValuesExclusive(ITypeDescriptorContext context)
        {
            return true;
        }

        public override StandardValuesCollection? GetStandardValues(ITypeDescriptorContext context)
        {
            if (context.Instance is not ContentFile file)
                return null;

            var ext = Path.GetExtension(file.Name);

            return new StandardValuesCollection(PipelineHelper.GetImporters(ext));
        }
    }
}