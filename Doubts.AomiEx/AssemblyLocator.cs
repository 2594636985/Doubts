using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;

namespace Doubts.AomiEx
{
    static class AssemblyLocator
    {
        static Dictionary<string, Assembly> assemblies = new Dictionary<string, Assembly>();
        static bool initialized;

        public static void Init()
        {
            lock (assemblies)
            {
                if (initialized)
                    return;
                initialized = true;
                AppDomain.CurrentDomain.AssemblyLoad += CurrentDomain_AssemblyLoad;
                AppDomain.CurrentDomain.AssemblyResolve += CurrentDomain_AssemblyResolve;
            }
        }

        static Assembly CurrentDomain_AssemblyResolve(object sender, ResolveEventArgs args)
        {
            lock (assemblies)
            {
                Assembly assembly = null;
                assemblies.TryGetValue(args.Name, out assembly);
                return assembly;
            }
        }

        static void CurrentDomain_AssemblyLoad(object sender, AssemblyLoadEventArgs args)
        {
            Assembly assembly = args.LoadedAssembly;
            lock (assemblies)
            {
                assemblies[assembly.FullName] = assembly;
            }
        }
    }
}
