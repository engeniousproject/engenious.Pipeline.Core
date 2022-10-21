using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.IO;
using System.Xml.Linq;
using engenious.Content.Models.History;
using engenious.ContentTool.Observer;

namespace engenious.Content.Models
{
    /// <summary>
    ///     Top level project for the content pipeline.
    /// </summary>
    public class ContentProject : ContentFolder
    {
        private readonly History.History _internalHistory;

        private readonly bool _readOnly;

        private string _configuration;

        private string _outputDirectory;

        /// <summary>
        ///     Initializes a new instance of the <see cref="ContentProject"/> class.
        /// </summary>
        /// <param name="name">Name of the project</param>
        /// <param name="contentProjectPath">Path of the actual project file</param>
        /// <param name="folderPath">Path of the project directory</param>
        /// <param name="readOnly">Whether the project is readonly or not.</param>
        public ContentProject(string name, string contentProjectPath, string folderPath, bool readOnly = false)
            : base(name, null)
        {
            ContentProjectPath = contentProjectPath;
            FilePath = folderPath;

            _readOnly = readOnly;
            _internalHistory = new History.History();
            History = new HistoryUnion();
            History.Add(_internalHistory);

            _configuration = "Debug";
            _outputDirectory = "bin/{Configuration}";


            //ContentItemChanged += (a, b) => HasUnsavedChanges = true;
            PropertyValueChanged += OnPropertyChangedT;
            CollectionChanged += OnCollectionChangedT;
        }

        /// <summary>
        ///     Gets the path of the ContentFolder
        /// </summary>
        public override string FilePath { get; }

        /// <summary>
        ///     Gets the parent item - always null for projects
        /// </summary>
        public override ContentItem? Parent => null;

        /// <summary>
        ///     Gets the path to the actual project file
        /// </summary>
        public string ContentProjectPath { get; set; }

        /// <inheritdoc />
        public override string RelativePath => string.Empty;

        /// <summary>
        ///     Gets or sets the directory to save the output to.
        /// </summary>
        public string OutputDirectory
        {
            get => _outputDirectory;
            set
            {
                if (value == _outputDirectory) return;
                var old = _outputDirectory;
                _outputDirectory = value;
                OnPropertyChanged(old, value);
            }
        }

        /// <summary>
        ///     Gets the configured directory to save the output to.
        /// </summary>
        public string ConfiguredOutputDirectory =>
            string.Format(OutputDirectory.Replace("{Configuration}", "{0}"), Project.Configuration);

        /// <summary>
        ///     Gets or sets the configuration of the project.
        /// </summary>
        public string Configuration
        {
            get => _configuration;
            set
            {
                if (value == _configuration) return;
                var old = _configuration;
                _configuration = value;
                OnPropertyChanged(old, value);
            }
        }

        /// <summary>
        ///     Gets the references of the project.
        /// </summary>
        public List<string> References { get; } = new();

        /// <summary>
        ///     Gets a value indicating whether the project has unsaved changes
        /// </summary>
        [Browsable(false)]
        public bool HasUnsavedChanges { get; private set; }

        /// <summary>
        ///     Gets the history of changes for this project.
        /// </summary>
        [Browsable(false)] public HistoryUnion History { get; }

        private void OnCollectionChangedT(object sender, NotifyCollectionChangedEventArgs args)
        {
            //var col = sender as ContentItemCollection;
            //if (col == null)
            //    throw new NotSupportedException();
            if (sender is not ContentFolder folder)
                return;
            var item = HistoryCollectionChange<ContentItem>.CreateInstance(folder.Content, args);
            if (item == null)
                throw new NotSupportedException();
            _internalHistory.Push(item);

            //History.Push(new HistoryCollectionChange<ContentItem>(col,args.Action,(IList<ContentItem>)args.OldItems,(IList<ContentItem>)args.NewItems));

            HasUnsavedChanges = true;
        }

        private void OnPropertyChangedT(object o, PropertyValueChangedEventArgs args)
        {
            _internalHistory.Push(new HistoryPropertyChange(o, args.PropertyName, args.OldValue, args.NewValue));
            HasUnsavedChanges = true;
        }

        /// <inheritdoc />
        public override ContentItem Deserialize(XElement element)
        {
            SuppressChangedEvent = true;
            Name = element.Element("Name")?.Value ?? "Content";
            _configuration = element.Element("Configuration")?.Value ?? "Release";
            _outputDirectory = element.Element("OutputDir")?.Value ?? "bin/{Configuration}";

            var refElement = element.Element("References");

            if (refElement is { HasElements: true })
                foreach (var referenceElement in refElement.Elements("Reference"))
                    References.Add(referenceElement.Value);
            var xElement = element.Element("Contents");
            if (xElement == null)
                return this;

            foreach (var subElement in xElement.Elements())
                if (subElement.Name == "ContentFile")
                    Content.Add(new ContentFile(string.Empty, this).Deserialize(subElement));
                else if (subElement.Name == "ContentFolder")
                    Content.Add(new ContentFolder(string.Empty, this).Deserialize(subElement));
            SuppressChangedEvent = false;

            return this;
        }

        /// <inheritdoc />
        public override XElement Serialize()
        {
            var element = new XElement("Content");

            element.Add(new XElement("Name", Name));

            var refElement = new XElement("References");

            foreach (var reference in References)
                refElement.Add(new XElement("Reference", reference));

            element.Add(new XElement("Configuration", Configuration));
            element.Add(new XElement("OutputDir", OutputDirectory));

            var contentElement = new XElement("Contents");
            foreach (var item in Content)
                contentElement.Add(item.Serialize());
            element.Add(contentElement);

            return element;
        }

        /// <summary>
        ///     Loads a content project from a file.
        /// </summary>
        /// <param name="path">The path to load the content project from.</param>
        /// <param name="readOnly">Whether to open the project in read-only mode.</param>
        /// <returns>The loaded content project.</returns>
        public static ContentProject Load(string path, bool readOnly = false)
        {
            return Load(path, Path.GetDirectoryName(path), readOnly);
        }

        private static ContentProject Load(string path, string contentFolderPath, bool readOnly = false)
        {
            var element = XElement.Load(path);

            var project = new ContentProject(string.Empty, path, contentFolderPath, readOnly);

            project.Deserialize(element);

            return project;
        }

        /// <summary>
        ///     Tries to save the content project.
        /// </summary>
        /// <remarks>Read-Only projects do nothing when this method is called.</remarks>
        public void Save()
        {
            Save(ContentProjectPath);
        }

        /// <summary>
        ///     Tries to save the content project to a specific file.
        /// </summary>
        /// <param name="path">The file path to save the content project to.</param>
        /// <remarks>Read-Only projects do nothing when this method is called.</remarks>
        public void Save(string path)
        {
            if (_readOnly)
                return;
            var element = Serialize();
            element.Save(path);
            ContentProjectPath = path;
            HasUnsavedChanges = false;
        }
    }
}