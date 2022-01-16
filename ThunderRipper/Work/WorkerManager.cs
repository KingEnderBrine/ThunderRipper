using System;
using System.Collections.Generic;
using System.Reflection;
using System.Runtime.Loader;
using System.Text;
using ThunderRipperShared.Utilities;

namespace ThunderRipper.Work
{
    public sealed partial class WorkerManager : IWorkerManager, IDisposable
    {
        private readonly Dictionary<UnityVersion, WeakReference<Worker>> workers = new Dictionary<UnityVersion, WeakReference<Worker>>();

        public IWorker GetWorker(UnityVersion version)
        {
            if (!workers.TryGetValue(version, out var workerRef) || !workerRef.TryGetTarget(out var worker) || worker.Disposed)
            {
                worker = new Worker(version);
                workers[version] = new WeakReference<Worker>(worker);
            }

            return worker;
        }

        public void Dispose()
        {
            foreach (var workerRef in workers.Values)
            {
                if (workerRef.TryGetTarget(out var worker))
                {
                    worker.Dispose();
                }
            }
        }
    }
}
