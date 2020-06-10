using System;
using System.IO;
using System.Threading;

namespace Heibroch.HotSwapper
{
    public class InstanceMonitor : IDisposable
    {
        private readonly FileSystemWatcher fileSystemWatcher1;
        private readonly FileSystemWatcher fileSystemWatcher2;
        private bool isDisposing;
        private Action<int> instanceChangedCallback;
        private int activeInstance;

        public InstanceMonitor(string instanceRoot, int activeInstance, Action<int> instanceChangedCallback)
        {
            this.activeInstance = activeInstance;
            this.instanceChangedCallback = instanceChangedCallback;

            var instance1Path = Path.Combine(instanceRoot, "Instance1");
            fileSystemWatcher1 = CreateSystemFileWatcher(instance1Path, OnInstance1Changed);

            var instance2Path = Path.Combine(instanceRoot, "Instance2");
            fileSystemWatcher2 = CreateSystemFileWatcher(instance2Path, OnInstance2Changed);            
        }

        private void OnInstance1Changed(object arg1, FileSystemEventArgs arg2) => ChangeInstance(1);

        private void OnInstance2Changed(object arg1, FileSystemEventArgs arg2) => ChangeInstance(2);

        private void ChangeInstance(int instance)
        {
            Thread.MemoryBarrier();
            if (instance == activeInstance) return;
            activeInstance = instance;
            Thread.MemoryBarrier();

            instanceChangedCallback(instance);
        }

        private FileSystemWatcher CreateSystemFileWatcher(string directory, FileSystemEventHandler fileChanged)
        {
            if (!Directory.Exists(directory))
                Console.WriteLine($"ERROR: Directory \"{directory}\" does not exist");
            
            Console.WriteLine($"Subscribing to instance changes on {directory}");
            
            var watcher = new FileSystemWatcher();           
            watcher.Path = directory;
            watcher.NotifyFilter = NotifyFilters.LastAccess | NotifyFilters.LastWrite | NotifyFilters.FileName | NotifyFilters.DirectoryName;
            watcher.Filter = "*.dll";
            watcher.Changed += fileChanged;
            watcher.EnableRaisingEvents = true;
            return watcher;            
        }
        
        public void Dispose()
        {
            if (isDisposing) return;
            isDisposing = true;

            fileSystemWatcher1.Changed -= OnInstance1Changed;
            fileSystemWatcher2.Changed -= OnInstance2Changed;
        }
    }
}
