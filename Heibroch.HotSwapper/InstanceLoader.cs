using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.Loader;

namespace Heibroch.HotSwapper
{
    internal class InstanceLoader<T>
    {
        private List<Assembly> instance1Assemblies = new List<Assembly>();
        private List<Assembly> instance2Assemblies = new List<Assembly>();

        private AssemblyLoadContext assemblyLoadContext1;
        private AssemblyLoadContext assemblyLoadContext2;

        private void UnloadAssemblies(string instanceDirectory)
        {
            Console.WriteLine($"Disposing instance: \"{instanceDirectory}\"");

            if (instanceDirectory.IsInstance1Directory())
                UnloadAssembliesInstance(instance1Assemblies, ref assemblyLoadContext1);
            else
                UnloadAssembliesInstance(instance2Assemblies, ref assemblyLoadContext2);
        }
        
        private static void UnloadAssembliesInstance(List<Assembly> assemblies, ref AssemblyLoadContext assemblyLoadContext)
        {
            //Clear loaded assembly references
            assemblies.Clear();

            //Unload loaded assemblies
            assemblyLoadContext.Unload();
            assemblyLoadContext = null;
        }
        
        private void LoadAssemblies(string instanceDirectory)
        {
            if  (instanceDirectory.IsInstance1Directory())
                LoadAssembliesInstance(instanceDirectory, instance1Assemblies, ref assemblyLoadContext1);
            else
                LoadAssembliesInstance(instanceDirectory, instance2Assemblies, ref assemblyLoadContext2);
        }

        private static void LoadAssembliesInstance(string instanceDirectory, List<Assembly> instanceAssemblies, ref AssemblyLoadContext assemblyLoadContext)
        {
            instanceAssemblies.Clear();
            assemblyLoadContext = new AssemblyLoadContext("instance1", true);

            var assemblyFiles = Directory.GetFiles(instanceDirectory, "*.dll");

            foreach (var assemblyFilePath in assemblyFiles)
            {
                var assembly = assemblyLoadContext.LoadFromStream(new MemoryStream(File.ReadAllBytes(assemblyFilePath))); //Had to read bytes instead of LoadFile or else file will be in use for next time we try to replace it :(
                instanceAssemblies.Add(assembly);
            }
        }

        private T ResolveTarget(List<Assembly> instanceAssemblies)
        {
            //Load types
            foreach (var assembly in instanceAssemblies)
            {
                if (assembly.FullName.Contains("Microsoft")) continue;
                if (assembly.FullName.StartsWith("System")) continue;

                var types = assembly.GetExportedTypes();
                foreach (var type in types)
                {
                    if (!type.GetInterfaces().Any(x => x == typeof(T))) continue;
                    var constructors = type.GetConstructors();
                    if (constructors.Length <= 0) continue;
                    var constructor = constructors[0];
                    return  (T)constructor.Invoke(new object[0]);
                }
            }

            throw new FileLoadException($"Could not locate type \"{typeof(T)}\" within assembly");
        }

        public T LoadObject(string instanceDirectory)
        {
            LoadAssemblies(instanceDirectory);
            return ResolveTarget(instanceDirectory.IsInstance1Directory() ? instance1Assemblies : instance2Assemblies);
        }

        public T LoadObject(string fromInstanceDirectory, string toInstanceDirectory)
        {
            UnloadAssemblies(fromInstanceDirectory);
            return LoadObject(toInstanceDirectory);
        }
    }
}
