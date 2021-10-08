﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using engenious.Content.Pipeline;
using engenious.Content.Models;

namespace engenious.Content
{
    public static class PipelineHelper
    {
        private static IList<Type> _importers;
        private static readonly Dictionary<string, Type> Processors = new();


        private static readonly List<KeyValuePair<Type, string>> ProcessorsByType = new();

        private static readonly HashSet<Assembly> Assemblies = new();
        public static void DefaultInit()
        {
            Assemblies.Clear();
            AddAssembly(Assembly.GetExecutingAssembly());
            AddAssembly(Assembly.GetEntryAssembly());
            AddAssembly(typeof(IContentImporter).Assembly);
            ListImporters();
            ListProcessors();
        }
        public static string GetProcessor(string name, string importerName)
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
            {
                try
                {
                    AddAssembly(Assembly.Load(r));
                }
                catch
                {
                    // ignored
                }
            }
        }
        
        public static void PreBuilt(ContentProject currentProject)
        {
            if (currentProject == null)
                return;
            Assemblies.Clear();
            AddAssembly(Assembly.GetExecutingAssembly());
            AddAssembly(Assembly.GetEntryAssembly());
            AddAssembly(typeof(IContentImporter).Assembly);
            if (currentProject.References == null)
                currentProject.References = new List<string>();
            foreach (var reference in currentProject.References)
            {
                try
                {
                    if (File.Exists(reference))
                        AddAssembly(Assembly.LoadFile(reference));
                }
                catch
                {
                    // ignored
                }
            }
            ListImporters();
            ListProcessors();
        }

        private static void ListImporters()
        {
            _importers = EnumerateImporters().ToList();
        }
        public static List<string> GetProcessors(Type tp)
        {
            if (ProcessorsByType == null)
                DefaultInit();
            var fitting = new List<string>();
            foreach (var pair in ProcessorsByType)
            {
                if (pair.Key.IsAssignableFrom(tp))
                {
                    fitting.Add(pair.Value);
                }
            }
            return fitting;
        }

        public static List<string> GetImporters(string extension)
        {
            if (_importers == null)
                DefaultInit();
            var fitting = new List<string>();
            // ReSharper disable once PossibleNullReferenceException
            foreach (var type in _importers)
            {
                var attribute =
                    (ContentImporterAttribute)type.GetCustomAttributes(typeof(ContentImporterAttribute), true).First();
                if (attribute.FileExtensions != null && attribute.FileExtensions.Contains(extension.ToUpperInvariant()))
                    fitting.Add(attribute.DisplayName);
            }
            return fitting;
        }
        
        private static void ListProcessors()
        {
            Processors.Clear();
            ProcessorsByType.Clear();
            foreach (var assembly in Assemblies)
            {
                foreach (var type in assembly.GetTypes())
                {

                    if (typeof(IContentProcessor).IsAssignableFrom(type) && !(type.IsAbstract || type.IsInterface))
                    {
                        if (!Processors.ContainsKey(type.Name))
                            Processors.Add(type.Name, type);

                        var baseType = GetProcessorInputType(type);
                        ProcessorsByType.Add(new KeyValuePair<Type, string>(baseType, type.Name));
                    }
                }
            }
        }
        public static Type GetImporterOutputType(string extension, string importerName)
        {
            var tp = GetImporterType(extension, importerName);
            var field = tp?.GetField("_exportType", BindingFlags.Static | BindingFlags.NonPublic | BindingFlags.FlattenHierarchy);
            if (field == null)
                return typeof(object);
            var lambda = Expression.Lambda<Func<Type>>(Expression.Field(null, field));
            var func = lambda.Compile();
            return func();
        }
        public static Type GetProcessorInputType(Type tp)
        {
            var field = tp?.GetField("_importType", BindingFlags.Static | BindingFlags.NonPublic | BindingFlags.FlattenHierarchy);
            if (field == null)
                return typeof(object);
            var lambda = Expression.Lambda<Func<Type>>(Expression.Field(null, field));
            var func = lambda.Compile();
            return func();
        }
        public static Type GetImporterType(string extension, string importerName)
        {
            if (_importers == null)
                DefaultInit();
            // ReSharper disable once PossibleNullReferenceException
            foreach (var type in _importers)
            {
                var attribute = (ContentImporterAttribute)type.GetCustomAttributes(typeof(ContentImporterAttribute), true).First();
                if (attribute.FileExtensions.Contains(extension.ToUpperInvariant()) && (importerName == null || attribute.DisplayName == importerName))
                    return type;
            }
            return null;
        }

        public static IContentImporter CreateImporter(string extension, ref string importerName)
        {
            if (_importers == null)
                DefaultInit();
            // ReSharper disable once PossibleNullReferenceException
            foreach (var type in _importers)
            {
                var attribute = (ContentImporterAttribute)type.GetCustomAttributes(typeof(ContentImporterAttribute), true).First();
                if (attribute.FileExtensions != null && attribute.FileExtensions.Contains(extension.ToUpperInvariant()) &&
                    (string.IsNullOrEmpty(importerName) || attribute.DisplayName == importerName))
                {
                    importerName = attribute.DisplayName;
                    return (IContentImporter)Activator.CreateInstance(type);
                }
            }
            importerName = null;
            return null;
        }
        public static IContentImporter CreateImporter(string extension, string importerName = null)
        {
            return CreateImporter(extension, ref importerName);
        }

        public static IContentProcessor CreateProcessor(Type importerType, string processorName)
        {
            if (Processors == null)
                DefaultInit();
            Type type;
            // ReSharper disable once PossibleNullReferenceException
            if (!string.IsNullOrEmpty(processorName) && Processors.TryGetValue(processorName, out type))
                return (IContentProcessor)Activator.CreateInstance(type);
            // ReSharper disable once PossibleNullReferenceException
            var attribute = (ContentImporterAttribute)importerType.GetCustomAttributes(typeof(ContentImporterAttribute), true).First();
            if (Processors.TryGetValue(attribute.DefaultProcessor, out type))
                return (IContentProcessor)Activator.CreateInstance(type);

            return null;
        }


        private static TValue GetAttributeValue<TAttribute, TValue>(
            this Type type,
            Func<TAttribute, TValue> valueSelector)
            where TAttribute : Attribute
        {
            var att = type.GetCustomAttributes(
                          typeof(TAttribute), true
                      ).FirstOrDefault() as TAttribute;
            return att != null ? valueSelector(att) : default(TValue);
        }

        private static IEnumerable<Type> EnumerateImporters()
        {
            foreach (var assembly in Assemblies)
            {
                foreach (var type in assembly.GetTypes())
                {
                    if (!typeof(IContentImporter).IsAssignableFrom(type) || type.IsValueType || type.IsInterface || type.IsAbstract || type.ContainsGenericParameters || type.GetConstructor(
                            BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic,
                            null, Type.EmptyTypes, null) == null)
                        continue;

                    var importerAttribute = (ContentImporterAttribute)Attribute.GetCustomAttributes(type, typeof(ContentImporterAttribute)).FirstOrDefault();
                    if (importerAttribute != null)
                    {
                        yield return type;
                    }
                }
            }
        }
    }
}

