using System;
using System.ComponentModel;
using System.IO;
using System.Xml.Linq;
using engenious.Content.Pipeline;

namespace engenious.Content.Models
{
    /// <summary>
    ///     Class for content files for the content pipeline.
    /// </summary>
    public class ContentFile : ContentItem
    {
        private string? _importerName;

        private string? _processorName;

        /// <summary>
        ///     Initializes a new instance of the <see cref="ContentFile"/> class.
        /// </summary>
        /// <param name="name">Name of the file</param>
        /// <param name="parent">Parent item</param>
        public ContentFile(string name, ContentItem parent) : base(name, parent)
        {
            Importer = PipelineHelper.CreateImporter(Path.GetExtension(name), ref _importerName);
            if (string.IsNullOrWhiteSpace(_processorName))
                _processorName = PipelineHelper.GetProcessor(Name, _importerName);
            if (!string.IsNullOrWhiteSpace(_processorName) && Importer != null)
                Processor = PipelineHelper.CreateProcessor(Importer.GetType(), _processorName);
        }

        /// <summary>
        ///     Gets the path of the file
        /// </summary>
        public override string FilePath => Parent == null ? Name : Path.Combine(Parent.FilePath, Name);

        /// <summary>
        ///     Gets the resolved <see cref="IContentImporter"/> of the file.
        /// </summary>
        [Browsable(false)]
        public IContentImporter? Importer { get; private set; }

        /// <summary>
        ///     Gets the resolved <see cref="IContentProcessor"/> of the file.
        /// </summary>
        [Browsable(false)]
        public IContentProcessor? Processor { get; private set; }

        /// <summary>
        ///     Gets or sets the name of the importer to use for this file.
        /// </summary>
        [RefreshProperties(RefreshProperties.All)]
        [TypeConverter(typeof(ImporterNameDropDownConverter))]
        public string? ImporterName
        {
            get => _importerName;
            set
            {
                if (_importerName == value)
                    return;
                var old = _importerName;
                _importerName = value;
                Importer = PipelineHelper.CreateImporter(Path.GetExtension(FilePath), ref _importerName);
                OnPropertyChanged(old, value);
            }
        }

        /// <summary>
        ///     Gets or sets the name of the processor to use for this file.
        /// </summary>
        [RefreshProperties(RefreshProperties.All)]
        [TypeConverter(typeof(ProcessorNameDropDownConverter))]
        public string? ProcessorName
        {
            get => _processorName;
            set
            {
                if (value == _processorName) return;
                var old = _processorName;
                _processorName = value;

                if (string.IsNullOrWhiteSpace(_processorName))
                    _processorName = PipelineHelper.GetProcessor(Name, _importerName);

                if (old != _processorName && !string.IsNullOrWhiteSpace(_processorName) && Importer != null)
                    Processor = PipelineHelper.CreateProcessor(Importer.GetType(), _processorName);

                OnPropertyChanged(old, value);
            }
        }


        /// <summary>
        ///     Gets or sets the <see cref="Processor"/> specific settings.
        /// </summary>
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Content)]
        public ProcessorSettings? Settings
        {
            get => Processor?.Settings;
            set
            {
                if (Processor == null)
                    return;

                Processor.Settings = value;
            }
        }

        /// <inheritdoc />
        public override ContentItem Deserialize(XElement element)
        {
            SupressChangedEvent = true;
            Name = element.Element("Name")?.Value ??
                   throw new ArgumentException($"{nameof(element)} has no \"Name\" tag.");

            if (!File.Exists(FilePath))
                Error = ContentErrorType.NotFound;

            _importerName = element.Element("Importer")?.Value;
            Importer = PipelineHelper.CreateImporter(Path.GetExtension(FilePath), ref _importerName);

            if (Importer == null)
                Error |= ContentErrorType.ImporterError;

            _processorName = element.Element("Processor")?.Value;
            if (string.IsNullOrWhiteSpace(_processorName))
                _processorName = PipelineHelper.GetProcessor(Name, _importerName);

            if (!string.IsNullOrWhiteSpace(_processorName) && Importer != null)
                Processor = PipelineHelper.CreateProcessor(Importer.GetType(), _processorName);

            if (Processor == null)
                Error |= ContentErrorType.ProcessorError;

            if (Settings != null && element.Element("Settings") != null) Settings.Read(element.Element("Settings"));

            SupressChangedEvent = false;
            return this;
        }

        /// <inheritdoc />
        public override XElement Serialize()
        {
            var element = new XElement("ContentFile");

            element.Add(new XElement("Name", Name));
            element.Add(new XElement("Processor", ProcessorName));
            element.Add(new XElement("Importer", ImporterName));

            if (Settings != null)
            {
                var settingsElement = new XElement("Settings");
                Settings.Write(settingsElement);
                element.Add(settingsElement);
            }


            return element;
        }
    }
}