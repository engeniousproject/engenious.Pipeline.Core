using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;
using engenious.Content.CodeGenerator;
using engenious.Content.Pipeline;
namespace engenious.Content.Pipeline
{
    [Serializable]
    public class CreatedContentCode
    {
        public static CreatedContentCode Load(string path, Guid buildId)
        {
            try
            {
                using var fs = new FileStream(path, FileMode.Open, FileAccess.Read);
                var formatter = new BinaryFormatter();
                var contentCode = (CreatedContentCode)formatter.Deserialize(fs);
                contentCode.BuildId = buildId;
                return contentCode;
            }
            catch
            {
                return new CreatedContentCode(buildId);
            }
        }

        public void Save(string path)
        {
            using var fs = new FileStream(path, FileMode.Create, FileAccess.Write);
            var formatter = new BinaryFormatter();
            formatter.Serialize(fs, this);
        }
        
        
        [Serializable]
        public class CreatedTypeContainer
        {
            public CreatedTypeContainer(Guid buildId, string buildFile)
            {
                BuildId = buildId;
                BuildFile = buildFile;

                FileDefinition = new FileDefinition(buildFile + ".cs");
            }
        
            public Guid BuildId { get; set; }
            
            public string BuildFile { get; set; }
            
            public FileDefinition FileDefinition { get; set; }
        }

        // private readonly Dictionary<string, CustomAttribute> _buildCacheAttributes;
        // private readonly MethodDefinition _buildCacheCtor;

        private Dictionary<string, CreatedTypeContainer> _typeContainers;
        // private Dictionary<string, TypeDefinition> Types { get; }
        
        // public Dictionary<string, Guid?> MostRecentBuildFileBuildIdMapping { get; }
        public Guid BuildId { get; set; }

        public CreatedContentCode(Guid buildId)
        {
            BuildId = buildId;

            _typeContainers = new();
            // Types = new Dictionary<string, TypeDefinition>();

            // MostRecentBuildFileBuildIdMapping = new Dictionary<string, Guid?>();
        }
        
        private static void CreatePathRecursively(string path)
        {
            Directory.CreateDirectory(Path.GetDirectoryName(path) ??
                                      throw new ArgumentException(
                                          "Not a valid file to create the parent directory of"));
        }


        public IEnumerable<FileDefinition> FileDefinitions => _typeContainers.Values.Select(x => x.FileDefinition);

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

        public bool CreatesUserContent(string buildFile)
        {
            return _typeContainers.ContainsKey(buildFile);
        }

        public Guid? GetTypeContainerBuildId(string buildFile)
        {
            return _typeContainers.TryGetValue(buildFile, out var createdTypeContainer)
                ? createdTypeContainer.BuildId
                : null;
        }
        public CreatedTypeContainer AddOrUpdateTypeContainer(string buildFile, Guid guid)
        {
            
            if (!_typeContainers.TryGetValue(buildFile, out var createdTypeContainer))
            {
                createdTypeContainer = new CreatedTypeContainer(guid, buildFile);
                _typeContainers.Add(buildFile, createdTypeContainer);
            }

            return createdTypeContainer;
        }

        public void Clean()
        {
            _typeContainers.Clear();
        }
    }
}