using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.Loader;
using System.Text.RegularExpressions;
using ThunderRipperShared.Utilities;

namespace ThunderRipper.Work
{
    public sealed class Worker : IWorker
    {
        private static readonly Dictionary<ContextInfo, int> loadedContexts = new Dictionary<ContextInfo, int>();

        private readonly ContextInfo contextInfo;
        public bool Disposed { get; private set; }
        public UnityVersion Version { get; }

        internal Worker(UnityVersion version)
        {
            Version = version;
            contextInfo = RequestContext(version);
        }

        public void Dispose()
        {
            if (Disposed)
            {
                return;
            }
            Disposed = true;
            ReleaseContext(contextInfo);
        }

        public Type GetTypeForAssetType(long typeID)
        {
            if ((contextInfo.TypeIDToTypeFieldInfo.GetValue(null) as Dictionary<long, Type>)?.TryGetValue(typeID, out var type) ?? false)
            {
                return type;
            }
            return null;
        }

        private static ContextInfo RequestContext(UnityVersion version)
        {
            var info = loadedContexts.Keys.FirstOrDefault(el => el.MinVersion <= version && version <= el.MaxVersion);
            if (info != null)
            {
                loadedContexts[info]++;
                return info;
            }
            foreach (var workerPath in Directory.GetFiles(Constants.RelativeWorkersPath, $"{Constants.ThunderRipperWorker}*.dll"))
            {
                var match = Regex.Match(workerPath, $@"{Constants.ThunderRipperWorker}_(?<from>.*?)_(?<to>.*?)\.dll");
                if (match.Success &&
                    UnityVersion.TryParse(match.Groups["from"].Value, out var from) &&
                    from <= version &&
                    UnityVersion.TryParse(match.Groups["to"].Value, out var to) &&
                    version <= to)
                {
                    info = new ContextInfo
                    {
                        Context = new AssemblyLoadContext(Path.GetFileName(workerPath), true),
                        MinVersion = from,
                        MaxVersion = to,
                    };
                    info.WorkerAssembly = info.Context.LoadFromAssemblyPath(Path.GetFullPath(workerPath));
                    info.TypeIDToTypeFieldInfo = info.WorkerAssembly.GetType("ThunderRipperWorker.Constants").GetField("TypeIDToType", BindingFlags.Public | BindingFlags.Static);
                    loadedContexts[info] = 1;
                    return info;
                }
            }
            throw new ArgumentException("Did not found worker for specified version");
        }

        private static void ReleaseContext(ContextInfo contextInfo)
        {
            if (loadedContexts.TryGetValue(contextInfo, out var count))
            {
                if (--count != 0)
                {
                    loadedContexts[contextInfo] = count;
                    return;
                }
                loadedContexts.Remove(contextInfo);
            }
            contextInfo.Context.Unload();
        }

        private class ContextInfo
        {
            public AssemblyLoadContext Context { get; set; }
            public Assembly WorkerAssembly { get; set; }
            public FieldInfo TypeIDToTypeFieldInfo { get; set; }
            public UnityVersion MinVersion { get; set; }
            public UnityVersion MaxVersion { get; set; }
        }
    }
}