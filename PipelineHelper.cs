using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using engenious.Content.Models;
using engenious.Content.Pipeline;

namespace engenious.Content
{
    /// <summary>
    ///     Helper class for pipeline importers and processors.
    /// </summary>
    public static class PipelineHelper
    {
        private static readonly Dictionary<string, Type> ResolvableTypes = new();
        private static readonly List<Type> Importers = new();
        private static readonly Dictionary<string, Type> Processors = new();


        private static readonly List<KeyValuePair<Type, string>> ProcessorsByType = new();

        private static readonly HashSet<Assembly> Assemblies = new();

        private static bool _assembliesCollected, _processorsCollected, _importersCollected, _resolveableTypesCollected;

        private static void InitDefaultAssemblies()
        {
            if (_assembliesCollected)
                return;
            _assembliesCollected = true;
            Assemblies.Clear();
            AddAssembly(Assembly.GetExecutingAssembly());
            var entryAssembly = Assembly.GetEntryAssembly();
            if (entryAssembly != null)
                AddAssembly(entryAssembly);
            AddAssembly(typeof(IContentImporter).Assembly);
        }

        /// <summary>
        ///     Default initialization for assemblies, importers and processors.
        /// </summary>
        public static void DefaultInit()
        {
            InitDefaultAssemblies();
            InitializeCache();
        }

        /// <summary>
        ///     Tries to resolve a pipeline type using a full type name.
        /// </summary>
        /// <param name="fullName">The full type name to try to resolve.</param>
        /// <returns>The resolved pipeline type;<c>null</c> if no type could be resolved.</returns>
        public static Type? ResolveType(string fullName)
        {
            return ResolvableTypes.TryGetValue(fullName, out var res) ? res : null;
        }

        /// <summary>
        ///     Gets the processor type name matching for a content item name.
        /// </summary>
        /// <param name="name">The name of the item to get the processor for.</param>
        /// <param name="importerName">The used importer to process the data from.</param>
        /// <returns>The type name of the processor if a match was found; otherwise <c>null</c>.</returns>
        public static string? GetProcessor(string name, string? importerName)
        {
            var tp = GetImporterType(Path.GetExtension(name), importerName);
            if (tp == null) return string.Empty;
            foreach (var attr in tp.GetCustomAttributes(true).Select(x => x as ContentImporterAttribute))
            {
                if (attr == null)
                    continue;
                return attr.DefaultProcessor;
            }

            return string.Empty;
        }

        private static void AddAssembly(Assembly assembly)
        {
            if (assembly.FullName.StartsWith("System.") || Assemblies.Contains(assembly))
                return;

            Assemblies.Add(assembly);

            foreach (var r in assembly.GetReferencedAssemblies())
                try
                {
                    AddAssembly(Assembly.Load(r));
                }
                catch
                {
                    // ignored
                }
        }

        /// <summary>
        ///     Pre build process for a content project.
        /// </summary>
        /// <param name="currentProject">The project to do the prebuild process for.</param>
        public static void PreBuilt(ContentProject? currentProject)
        {
            if (currentProject == null)
                return;
            InitDefaultAssemblies();
            foreach (var reference in currentProject.References)
                try
                {
                    if (File.Exists(reference))
                        AddAssembly(Assembly.LoadFile(reference));
                }
                catch
                {
                    // ignored
                }

            InitializeCache();
        }

        private static void ListImporters()
        {
            if (_importersCollected)
                return;
            _importersCollected = true;
            Importers.Clear();
            foreach (var imp in EnumerateImporters())
                Importers.Add(imp);
        }

        /// <summary>
        ///     Gets a list of all processors that con process a specific type.
        /// </summary>
        /// <param name="tp">The type to get matching processors for.</param>
        /// <returns>The processors that can process the type.</returns>
        public static List<string> GetProcessors(Type tp)
        {
            DefaultInit();
            var fitting = new List<string>();
            foreach (var pair in ProcessorsByType)
                if (pair.Key.IsAssignableFrom(tp))
                    fitting.Add(pair.Value);
            return fitting;
        }

        /// <summary>
        ///     Gets a list of all importers that can import a specific content extension.
        /// </summary>
        /// <param name="extension">The extension to get matching importers for.</param>
        /// <returns>The importers that can import the extension.</returns>
        public static List<string> GetImporters(string extension)
        {
            DefaultInit();
            var fitting = new List<string>();
            foreach (var type in Importers)
            {
                var attribute =
                    (ContentImporterAttribute)type.GetCustomAttributes(typeof(ContentImporterAttribute), true).First();
                if (attribute.DisplayName != null && attribute.FileExtensions.Contains(extension.ToUpperInvariant()))
                    fitting.Add(attribute.DisplayName);
            }

            return fitting;
        }

        private static void InitializeCache()
        {
            ListResolveableTypes();
            ListImporters();
            ListProcessors();
        }

        private static void ListResolveableTypes()
        {
            if (_resolveableTypesCollected)
                return;
            _resolveableTypesCollected = true;
            ResolvableTypes.Clear();
            foreach (var assembly in Assemblies)
            {
                try
                {
                    foreach (var type in assembly.GetTypes())
                    {
                        if (type.FullName == null)
                            continue;
                        ResolvableTypes[type.FullName] = type;
                    }
                }
                catch (ReflectionTypeLoadException)
                {
                }
            }
        }

        private static void ListProcessors()
        {
            if (_processorsCollected)
                return;
            _processorsCollected = true;
            Processors.Clear();
            ProcessorsByType.Clear();
            foreach (var assembly in Assemblies)
                try
                {
                    foreach (var type in assembly.GetTypes())
                    {
                        try
                        {
                            if (typeof(IContentProcessor).IsAssignableFrom(type) && !(type.IsAbstract || type.IsInterface))
                            {
                                if (!Processors.ContainsKey(type.Name))
                                    Processors.Add(type.Name, type);

                                var baseType = GetProcessorInputType(type);
                                ProcessorsByType.Add(new KeyValuePair<Type, string>(baseType, type.Name));
                            }
                        }
                        catch (FileNotFoundException)
                        {
                        }
                    }
                }
                catch (ReflectionTypeLoadException)
                {
                }
        }

        /// <summary>
        ///     Gets the output type of a matching importer found with <see cref="GetImporterType"/>.
        /// </summary>
        /// <param name="extension">The file extension to find the importer output type for.</param>
        /// <param name="importerName">The importer name to get the output type for;<c>null</c> to find the best match. </param>
        /// <returns>The output type the importer outputs.</returns>
        public static Type GetImporterOutputType(string extension, string? importerName)
        {
            var tp = GetImporterType(extension, importerName);
            var field = tp?.GetField("_exportType",
                BindingFlags.Static | BindingFlags.NonPublic | BindingFlags.FlattenHierarchy);
            if (field == null)
                return typeof(object);
            var lambda = Expression.Lambda<Func<Type>>(Expression.Field(null, field));
            var func = lambda.Compile();
            return func();
        }

        /// <summary>
        ///     Gets the input type a processor accepts to process.
        /// </summary>
        /// <param name="tp">The type of the processor to get the input type of.</param>
        /// <returns>The input type the processor accepts.</returns>
        public static Type GetProcessorInputType(Type tp)
        {
            var field = tp.GetField("_importType",
                BindingFlags.Static | BindingFlags.NonPublic | BindingFlags.FlattenHierarchy);
            if (field == null)
                return typeof(object);
            var lambda = Expression.Lambda<Func<Type>>(Expression.Field(null, field));
            var func = lambda.Compile();
            return func();
        }

        /// <summary>
        ///     Gets the type of the best matching importer type for an extension and importer name.
        /// </summary>
        /// <param name="extension">The file extension to find the importer type for.</param>
        /// <param name="importerName">The importer name to get the type for;<c>null</c> to find the best match. </param>
        /// <returns>The matched importer type.</returns>
        public static Type? GetImporterType(string extension, string? importerName)
        {
            DefaultInit();
            foreach (var type in Importers)
            {
                var attribute =
                    (ContentImporterAttribute)type.GetCustomAttributes(typeof(ContentImporterAttribute), true).First();
                if (attribute.FileExtensions.Contains(extension.ToUpperInvariant()) &&
                    (importerName == null || attribute.DisplayName == importerName))
                    return type;
            }

            return null;
        }

        /// <summary>
        ///     Creates a content importer from a given extension and importer name.
        /// </summary>
        /// <param name="extension">The file extension to create the importer for.</param>
        /// <param name="importerName">
        ///     The importer name to create;<c>null</c> to find the best match.
        ///     This value gets changed to the matched importer name.
        /// </param>
        /// <returns>The created content importer.</returns>
        public static IContentImporter? CreateImporter(string extension, ref string? importerName)
        {
            DefaultInit();
            foreach (var type in Importers)
            {
                var attribute =
                    (ContentImporterAttribute)type.GetCustomAttributes(typeof(ContentImporterAttribute), true).First();
                if (attribute.FileExtensions.Contains(extension.ToUpperInvariant()) &&
                    (string.IsNullOrEmpty(importerName) || attribute.DisplayName == importerName))
                {
                    importerName = attribute.DisplayName;
                    return (IContentImporter)Activator.CreateInstance(type);
                }
            }

            importerName = null;
            return null;
        }

        /// <summary>
        ///     Creates a content importer from a given extension and importer name.
        /// </summary>
        /// <param name="extension">The file extension to create the importer for.</param>
        /// <param name="importerName">The importer name to create;<c>null</c> to find the best match</param>
        /// <returns>The created content importer.</returns>
        public static IContentImporter? CreateImporter(string extension, string? importerName = null)
        {
            return CreateImporter(extension, ref importerName);
        }

        /// <summary>
        ///     Creates a content processor from a given importerType and processor name.
        /// </summary>
        /// <param name="importerType">The importer type that imported the content item.</param>
        /// <param name="processorName">The processor name to create;<c>null</c> to find the best match</param>
        /// <returns>The created content processor.</returns>
        public static IContentProcessor? CreateProcessor(Type importerType, string? processorName)
        {
            DefaultInit();
            if (!string.IsNullOrEmpty(processorName) && Processors.TryGetValue(processorName!, out var type))
                return (IContentProcessor)Activator.CreateInstance(type);
            var attribute =
                (ContentImporterAttribute)importerType.GetCustomAttributes(typeof(ContentImporterAttribute), true)
                    .First();

            if (attribute.DefaultProcessor != null && Processors.TryGetValue(attribute.DefaultProcessor, out type))
                return (IContentProcessor)Activator.CreateInstance(type);

            return null;
        }


        private static TValue? GetAttributeValue<TAttribute, TValue>(
            this Type type,
            Func<TAttribute, TValue> valueSelector)
            where TAttribute : Attribute
        {
            var att = type.GetCustomAttributes(
                typeof(TAttribute), true
            ).FirstOrDefault() as TAttribute;
            return att != null ? valueSelector(att) : default;
        }

        private static IEnumerable<Type> EnumerateImporters()
        {
            foreach (var assembly in Assemblies)
            {
                Type[] types;
                try
                {
                    types = assembly.GetTypes();
                }
                catch (ReflectionTypeLoadException)
                {
                    continue;
                }
                foreach (var type in types)
                {
                    try
                    {
                        if (!typeof(IContentImporter).IsAssignableFrom(type) || type.IsValueType || type.IsInterface ||
                            type.IsAbstract || type.ContainsGenericParameters || type.GetConstructor(
                                BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic,
                                null, Type.EmptyTypes, null) == null)
                            continue;

                        var importerAttribute = (ContentImporterAttribute?)Attribute
                            .GetCustomAttributes(type, typeof(ContentImporterAttribute)).FirstOrDefault();
                        if (importerAttribute is null) continue;
                    }
                    catch (FileNotFoundException)
                    {
                        continue;
                    }
                    yield return type;
                }
            }
        }
    }
}