using System;
using System.Threading;

namespace Heibroch.HotSwapper
{
    public class HotSwappedObject<T> : IDisposable
    {
        private readonly string instanceRoot;
        private bool isInstanceChanging;
        private readonly InstanceMonitor instanceMonitor;
        private readonly InstanceLoader<T> instanceLoader;
        private ManualResetEvent operationResetEvent;
        private AutoResetEvent instanceChangeResetEvent;
        
        public HotSwappedObject(string instanceRoot, int activeInstance)
        {
            this.instanceRoot = instanceRoot;

            //operationQueue = new ConcurrentQueue<Action<T>>();
            operationResetEvent = new ManualResetEvent(false);
            instanceChangeResetEvent = new AutoResetEvent(false);
            
            //Start monitoring the instances
            instanceMonitor = new InstanceMonitor(instanceRoot, activeInstance, OnInstanceChanged);

            //Load in the first instance
            instanceLoader = new InstanceLoader<T>();
            TargetObject = instanceLoader.LoadObject(instanceRoot.ToInstanceDirectory(activeInstance));
        }

        private void OnInstanceChanged(int instance)
        {
            //Wait for instance suspension
            Console.WriteLine($"Change in instance {instance} detected. Waiting for instance suspension...");
            isInstanceChanging = true;
            instanceChangeResetEvent.WaitOne(5000); //If no operations were called, we shouldn't wait forever.
            Console.WriteLine($"Instance suspension complete!");
                       
            //Temporary solution for taking file replacement into account
            Console.WriteLine($"Waiting for file copy operation to complete...");
            Thread.Sleep(3000);
            Console.WriteLine($"Waiting period complete!");

            //Remove resources
            Console.WriteLine($"Cleaning up suspended instance target object...");
            TargetObject = default;
            GC.Collect();
            Console.WriteLine($"Resource cleanup complete!");

            //Reload new object
            Console.WriteLine($"Loading new instance object...");
            TargetObject = instanceLoader.LoadObject(instanceRoot.ToInstanceDirectory(instance == 1 ? 2 : 1), instanceRoot.ToInstanceDirectory(instance));
            Console.WriteLine($"Load of new instance target object complete!");

            //Resume operation            
            Console.WriteLine($"Resuming operation...");
            isInstanceChanging = false;            
            operationResetEvent.Set();
        }

        public void PerformOperation(Action<T> action)
        {
            if (isInstanceChanging == true || TargetObject == null)
            {
                Console.WriteLine("OPERATION INFO! Operation requested performed, but halted due to pending instance change...");
                instanceChangeResetEvent.Set();
                operationResetEvent.Reset();
                operationResetEvent.WaitOne();
                Console.WriteLine("OPERATION INFO! Resumed with performing operation that was previously halted!");
            }
                       
            action(TargetObject);
        }

        public void Dispose()
        {
            TargetObject = default;
            instanceChangeResetEvent.Set();
            operationResetEvent.Set();
        }

        private T TargetObject { get; set; }
    }
}
