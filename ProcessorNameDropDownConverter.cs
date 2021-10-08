using System.ComponentModel;
using System.IO;
using engenious.Content.Models;

namespace engenious.Content
{
    internal class ProcessorNameDropDownConverter : StringConverter
    {
        public override bool GetStandardValuesSupported(ITypeDescriptorContext context) {
            return (context?.Instance is ContentFile);
        }

        public override bool GetStandardValuesExclusive(ITypeDescriptorContext context) {     
            return true;
        }

        public override StandardValuesCollection GetStandardValues(ITypeDescriptorContext context) {
            if (!(context?.Instance is ContentFile))
                return null;
            var file = (ContentFile)context.Instance;

            var ext = Path.GetExtension(file.Name);
            var baseType = PipelineHelper.GetImporterOutputType(ext,file.ImporterName);
            
            return new StandardValuesCollection(PipelineHelper.GetProcessors(baseType));
            
        }
    }
}