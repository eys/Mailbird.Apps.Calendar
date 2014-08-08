using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;

namespace Mailbird.Apps.Calendar.Engine.Responses
{
    public static class AppDomainAssemblyTypeScanner
    {
        static AppDomainAssemblyTypeScanner()
        {
            LoadAssembliesWithDataProviderReferences();
        }

        /// <summary>
        /// Core assembly
        /// </summary>
        private static readonly Assembly DataProviderAssembly;

        /// <summary>
        /// App domain type cache
        /// </summary>
        private static IEnumerable<Type> _types;

        /// <summary>
        /// App domain assemblies cache
        /// </summary>
        private static IEnumerable<Assembly> _assemblies;

        /// <summary>
        /// Indicates whether the all Assemblies, that references a Nancy assembly, have already been loaded
        /// </summary>
        private static bool _dataProviderReferencingAssembliesLoaded;

        private static IEnumerable<Func<Assembly, bool>> _assembliesToScan;

        private static IEnumerable<KeyValuePair<Type, Assembly>> _typeAssemblies;

        /// <summary>
        /// The default assemblies for scanning.
        /// Includes the nancy assembly and anything referencing a nancy assembly
        /// </summary>
        public static Func<Assembly, bool>[] DefaultAssembliesToScan = new Func<Assembly, bool>[]
        {
            x => x == DataProviderAssembly,
            x => x.GetName().Name.StartsWith("Calendar",StringComparison.OrdinalIgnoreCase) || 
                 x.GetName().Name.StartsWith("Mailbird",StringComparison.OrdinalIgnoreCase) ||
                 x.GetReferencedAssemblies().Any(r => r.Name.StartsWith("Calendar", StringComparison.OrdinalIgnoreCase)) ||
                 x.GetReferencedAssemblies().Any(r => r.Name.StartsWith("Mailbird", StringComparison.OrdinalIgnoreCase))
        };

        /// <summary>
        /// Gets or sets a set of rules for which assemblies are scanned
        /// Defaults to just assemblies that have references to nancy, and nancy
        /// itself.
        /// Each item in the enumerable is a delegate that takes the assembly and 
        /// returns true if it is to be included. Returning false doesn't mean it won't
        /// be included as a true from another delegate will take precedence.
        /// </summary>
        public static IEnumerable<Func<Assembly, bool>> AssembliesToScan
        {
            private get
            {
                return _assembliesToScan ?? (_assembliesToScan = DefaultAssembliesToScan);
            }
            set
            {
                _assembliesToScan = value;
                UpdateTypes();
            }
        }

        /// <summary>
        /// Gets app domain types.
        /// </summary>
        public static IEnumerable<Type> Types
        {
            get { return _types; }
        }

        /// <summary>
        /// Gets app domain types.
        /// </summary>
        public static IEnumerable<Assembly> Assemblies
        {
            get { return _assemblies; }
        }

        /// <summary>
        /// Add predicates for determining which assemblies to scan for DataProvider types
        /// </summary>
        /// <param name="predicates">One or more predicates</param>
        public static void AddAssembliesToScan(params Func<Assembly, bool>[] predicates)
        {
            AssembliesToScan = AssembliesToScan.Union(predicates);
        }
        
        /// <summary>
        /// Load assemblies from a given directory matching a given wildcard.
        /// Assemblies will only be loaded if they aren't already in the appdomain.
        /// </summary>
        /// <param name="containingDirectory">Directory containing the assemblies</param>
        /// <param name="wildcardFilename">Wildcard to match the assemblies to load</param>
        public static void LoadAssemblies(string containingDirectory, string wildcardFilename)
        {
            UpdateAssemblies();

            var existingAssemblyPaths = _assemblies.Select(a => a.Location).ToArray();

            var unloadedAssemblies =
                Directory.GetFiles(containingDirectory, wildcardFilename).Where(
                    f => !existingAssemblyPaths.Contains(f, StringComparer.InvariantCultureIgnoreCase)).ToArray();


            foreach (var unloadedAssembly in unloadedAssemblies)
            {
                Assembly.Load(AssemblyName.GetAssemblyName(unloadedAssembly));
            }

            UpdateTypes();
        }

        /// <summary>
        /// Refreshes the type cache if additional assemblies have been loaded.
        /// Note: This is called automatically if assemblies are loaded using LoadAssemblies.
        /// </summary>
        public static void UpdateTypes()
        {
            UpdateAssemblies();
            var list = new List<Type>();
            var list2 = new List<KeyValuePair<Type,Assembly>>() ;
            foreach (var assembly in _assemblies)
            {
                list.AddRange(assembly.SafeGetExportedTypes().Where(type => !type.IsAbstract));
                list2.AddRange(assembly.SafeGetExportedTypes().Where(type => !type.IsAbstract).Select(type => new KeyValuePair<Type, Assembly>(type, assembly)));
            }
            _types = list.ToArray();
            _typeAssemblies = list2.ToArray();
        }

        /// <summary>
        /// Updates the assembly cache from the appdomain
        /// </summary>
        private static void UpdateAssemblies()
        {
            _assemblies = AppDomain.CurrentDomain.GetAssemblies()
                .Where(a => AssembliesToScan.Any(asm => asm(a)) && !a.IsDynamic && !a.ReflectionOnly)
                .Select(a => a)
                .ToArray();
            _assemblies = _assemblies.DistinctBy(x=>x.GetName().Name);
        }

        /// <summary>
        /// Loads any assembly that references a DataProvider assembly.
        /// </summary>
        public static void LoadAssembliesWithDataProviderReferences()
        {
            if (_dataProviderReferencingAssembliesLoaded) return;
            UpdateAssemblies();
            UpdateTypes();
            _dataProviderReferencingAssembliesLoaded = true;
        }

        /// <summary>
        /// Gets all types implementing a particular interface/base class
        /// </summary>
        /// <param name="type">Type to search for</param>
        /// <returns>An <see cref="IEnumerable{T}"/> of types.</returns>
        public static IEnumerable<Type> TypesOf(Type type)
        {
            return type.IsInterface ? 
                Types.Where(x => x.GetInterface(type.Name) != null) : 
                Types.Where(type.IsAssignableFrom);
        }

        public static void UpdateAssemblies(string folders)
        {
            var existingAssemblyPaths = _assemblies.Select(a => a.Location).ToArray();
            foreach (var directory in GetAssemblyDirectories(folders))
            {
                var unloadedAssemblies = Directory.GetFiles(directory, "*.dll")
                    .Where(f => !existingAssemblyPaths.Contains(f, StringComparer.InvariantCultureIgnoreCase)).ToArray();

                foreach (var unloadedAssembly in unloadedAssemblies)
                {
                    if (AppDomain.CurrentDomain.GetAssemblies().Any(x => x.GetName().Name == Path.GetFileNameWithoutExtension(unloadedAssembly))) continue;
                    Assembly inspectedAssembly = null;
                    try
                    {
                        inspectedAssembly = Assembly.ReflectionOnlyLoadFrom(unloadedAssembly);
                    }
                    catch (BadImageFormatException)
                    {
                        //the assembly maybe it's not managed code
                    }

                    if (inspectedAssembly == null) continue;

                    try
                    {
                        Assembly.Load(inspectedAssembly.GetName());
                    }
                    catch { }
                }
            }
            UpdateTypes();
        }
        
        /// <summary>
        /// Gets all types implementing a particular interface/base class
        /// </summary>
        /// <typeparam name="TType">Type to search for</typeparam>
        /// <returns>An <see cref="IEnumerable{T}"/> of types.</returns>
        public static IEnumerable<Type> TypesOf<TType>()
        {
            return TypesOf(typeof(TType));
        }

        public static IEnumerable<KeyValuePair<Type, Assembly>> TypesOfWithAssembly(Type type, string folders)
        {
            UpdateAssemblies(folders);
            return type.IsInterface
                ? _typeAssemblies.Where(x => x.Key.GetInterface(type.Name) != null)
                : _typeAssemblies.Where(x=>type.IsAssignableFrom(x.Key));
        }

        /// <summary>
        /// Gets all types implementing a particular interface/base class
        /// </summary>
        /// <param name="type">Type to search for</param>
        /// <param name="folders">Folders to scan in</param>
        /// <returns>An <see cref="IEnumerable{T}"/> of types.</returns>
        public static IEnumerable<Type> TypesOf(Type type, string folders)
        {
            UpdateAssemblies(folders);
            return type.IsInterface ?
                Types.Where(x => x.GetInterface(type.Name) != null) :
                Types.Where(type.IsAssignableFrom);
        }

        /// <summary>
        /// Returns the directories containing dll files. It uses the default convention as stated by microsoft.
        /// </summary>
        private static IEnumerable<string> GetAssemblyDirectories(string folders)
        {
            var privateBinPathDirectories = folders.Split(';');

            foreach (var privateBinPathDirectory in privateBinPathDirectories.Where(x => !string.IsNullOrWhiteSpace(x)))
            {
                if (Regex.IsMatch(privateBinPathDirectory, @"^[.\\]"))
                {
                    if (Directory.Exists(
                            Path.GetFullPath(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, privateBinPathDirectory))))
                    {
                        yield return Path.GetFullPath(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, privateBinPathDirectory));
                    }
                }
                else 
                {
                    if (Directory.Exists(privateBinPathDirectory))
                    {
                        yield return privateBinPathDirectory;
                    }
                }
            }

            if (AppDomain.CurrentDomain.SetupInformation.PrivateBinPathProbe == null)
            {
                yield return AppDomain.CurrentDomain.SetupInformation.ApplicationBase;
            }
        }
    }

    public static class AppDomainAssemblyTypeScannerExtensions
    {
        public static IEnumerable<Type> NotOfType<TType>(this IEnumerable<Type> types)
        {
            return types.Where(t => !typeof(TType).IsAssignableFrom(t));
        }
    }

    /// <summary>
    /// Assembly extension methods
    /// </summary>
    public static class AssemblyExtensions
    {
        /// <summary>
        /// Gets exported types from an assembly and catches common errors
        /// that occur when running under test runners.
        /// </summary>
        /// <param name="assembly">Assembly to retreive from</param>
        /// <returns>An array of types</returns>
        public static Type[] SafeGetExportedTypes(this Assembly assembly)
        {
            Type[] types;
            try
            {
                types = assembly.GetExportedTypes();
            }
            catch (FileNotFoundException)
            {
                types = new Type[] { };
            }
            catch (NotSupportedException)
            {
                types = new Type[] { };
            }
            return types;
        }

        public static IEnumerable<TSource> DistinctBy<TSource, TKey>(this IEnumerable<TSource> source, Func<TSource, TKey> keySelector)
        {
            var seenKeys = new HashSet<TKey>();
            return source.Where(element => seenKeys.Add(keySelector(element)));
        }
    }
}
