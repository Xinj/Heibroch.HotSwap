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
        
        private void UnloadAssemblies(List<Assembly> assemblies)
        {
            assemblies.Clear();

            var assembliesToUnload = assemblies.ToList();
            assembliesToUnload.Reverse(); //We want to unload in opposite order of how they loaded... Because it sounds good (haven't tested this)

            GC.Collect();

            for (int i = 0; i < assembliesToUnload.Count; i++)
            {
                var assembly = assembliesToUnload[i];
                assembliesToUnload[i] = null;
                AssemblyLoadContext.GetLoadContext(assembly).Unload();                
            }
        }

        private void LoadAssemblies(string instanceDirectory)
        {
            var loadedInstanceAssemblies = instanceDirectory.IsInstance1Directory() ? instance1Assemblies : instance2Assemblies;
            loadedInstanceAssemblies.Clear();

            var assemblyFiles = Directory.GetFiles(instanceDirectory, "*.dll");

            foreach (var assemblyFilePath in assemblyFiles)
            {
                var assembly = Assembly.Load(File.ReadAllBytes(assemblyFilePath)); //Had to read bytes instead of LoadFile or else file will be in use for next time we try to replace it :(
                loadedInstanceAssemblies.Add(assembly);
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
            UnloadAssemblies(fromInstanceDirectory.IsInstance1Directory() ? instance1Assemblies : instance2Assemblies);
            return LoadObject(toInstanceDirectory);
        }
    }
}
