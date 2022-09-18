using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Formatters;
using System.Runtime.Serialization.Formatters.Binary;
using engenious.Content.CodeGenerator;
using NonSucking.Framework.Serialization;

namespace engenious.Content.Pipeline
{
    /// <summary>
    ///     Created content code for a full build process.
    /// </summary>
    [Nooson]
    public partial class CreatedContentCode
    {
        // private readonly Dictionary<string, CustomAttribute> _buildCacheAttributes;
        // private readonly MethodDefinition _buildCacheCtor;

        [NoosonInclude]
        private Dictionary<string, CreatedTypeContainer> _typeContainers;

        /// <summary>
        ///     Initializes a new instance of the <see cref="CreatedContentCode"/> class.
        /// </summary>
        /// <param name="buildId">The build id associated with the current build.</param>
        public CreatedContentCode(Guid buildId)
        {
            BuildId = buildId;

            _typeContainers = new Dictionary<string, CreatedTypeContainer>();
            // Types = new Dictionary<string, TypeDefinition>();

            // MostRecentBuildFileBuildIdMapping = new Dictionary<string, Guid?>();
        }
        // private Dictionary<string, TypeDefinition> Types { get; }

        // public Dictionary<string, Guid?> MostRecentBuildFileBuildIdMapping { get; }

        /// <summary>
        ///     Gets or sets the build id the created content code was built for.
        /// </summary>
        public Guid BuildId { get; set; }


        /// <summary>
        ///     Gets all created file definitions for the generated code.
        /// </summary>
        [NoosonIgnore]
        public IEnumerable<FileDefinition> FileDefinitions => _typeContainers.Values.Select(x => x.FileDefinition);

        /// <summary>
        ///     Loads <see cref="CreatedContentCode"/> from a serialized file.
        /// </summary>
        /// <param name="path">The file to load.</param>
        /// <param name="buildId">The build id of the build process to load the code into.</param>
        /// <returns>The loaded <see cref="CreatedContentCode"/>.</returns>
        public static CreatedContentCode Load(string path, Guid buildId)
        {
            try
            {
                using var fs = new FileStream(path, FileMode.Open, FileAccess.Read);
                var br = new BinaryReader(fs);
                return CreatedContentCode.Deserialize(br);
            }
            catch
            {
                return new CreatedContentCode(buildId);
            }
        }

        /// <summary>
        ///     Saves the created content code in a serialized format.
        /// </summary>
        /// <param name="path">The path to save the content code to.</param>
        public void Save(string path)
        {
            using var fs = new FileStream(path, FileMode.Create, FileAccess.Write);
            var bw = new BinaryWriter(fs);
            Serialize(bw);
            // var formatter = new BinaryFormatter();
            // formatter.Serialize(fs, this);
        }

        private static void CreatePathRecursively(string path)
        {
            Directory.CreateDirectory(Path.GetDirectoryName(path) ??
                                      throw new ArgumentException(
                                          "Not a valid file to create the parent directory of"));
        }

        /// <summary>
        ///     Write the code to a specific path directory.
        /// </summary>
        /// <param name="path">The path to write the code to.</param>
        public void WriteCode(string path)
        {
            foreach (var p in _typeContainers)
            {
                var a = p.Key;
                var b = p.Value;
                var destFile = Path.Combine(path, b.FileDefinition.Name);
                CreatePathRecursively(destFile);

                using var stream = File.Create(destFile);
                using var streamWriter = new StreamWriter(stream);
                using var codeBuilder = new StreamWriterCodeBuilder(streamWriter);
                b.FileDefinition.WriteTo(codeBuilder);
            }
        }

        /// <summary>
        ///     Gets a value indicating whether a specific build file creates content code.
        /// </summary>
        /// <param name="buildFile">The build file to check.</param>
        /// <returns>Whether the build file creates content code</returns>
        public bool CreatesUserContent(string buildFile)
        {
            return _typeContainers.ContainsKey(buildFile);
        }

        /// <summary>
        ///     Gets the build id created code was built in for a specific build file.
        /// </summary>
        /// <param name="buildFile">The build file to get the build id for.</param>
        /// <returns>Build id if available for the build file; otherwise <c>null</c>.</returns>
        public Guid? GetTypeContainerBuildId(string buildFile)
        {
            return _typeContainers.TryGetValue(buildFile, out var createdTypeContainer)
                ? createdTypeContainer.BuildId
                : null;
        }

        /// <summary>
        ///     Creates or updates the <see cref="CreatedTypeContainer"/> associated with the given build file.
        /// </summary>
        /// <param name="buildFile">The build file to create or get the <see cref="CreatedTypeContainer"/> for.</param>
        /// <param name="guid">The build id to set the <see cref="CreatedTypeContainer"/> build id to.</param>
        /// <returns>The created or updated <see cref="CreatedTypeContainer"/>.</returns>
        public CreatedTypeContainer AddOrUpdateTypeContainer(string buildFile, Guid guid)
        {
            if (!_typeContainers.TryGetValue(buildFile, out var createdTypeContainer))
            {
                createdTypeContainer = new CreatedTypeContainer(guid, buildFile);
                _typeContainers.Add(buildFile, createdTypeContainer);
            }

            return createdTypeContainer;
        }

        /// <summary>
        ///     Cleans and removes all created code.
        /// </summary>
        public void Clean()
        {
            _typeContainers.Clear();
        }


        /// <summary>
        ///     Class containing the created types for a specific build file.
        /// </summary>
        [Nooson]
        public partial class CreatedTypeContainer
        {
            /// <summary>
            ///     Initializes a new instance of the <see cref="CreatedTypeContainer"/> class.
            /// </summary>
            /// <param name="buildId">The build id associated with the types created for this build file.</param>
            /// <param name="buildFile">The build file the types are created for.</param>
            public CreatedTypeContainer(Guid buildId, string buildFile)
            {
                BuildId = buildId;
                BuildFile = buildFile;

                FileDefinition = new FileDefinition(buildFile + ".cs");
            }

            /// <summary>
            ///     Gets or sets the build id associated with the types created for this build file.
            /// </summary>
            public Guid BuildId { get; set; }

            /// <summary>
            ///     Gets or sets the build file the types are created for.
            /// </summary>
            public string BuildFile { get; set; }

            /// <summary>
            ///     Gets the <see cref="engenious.Content.CodeGenerator.FileDefinition"/>
            ///     containing the types created for this build file.
            /// </summary>
            public FileDefinition FileDefinition { get; set; }
        }
    }
}