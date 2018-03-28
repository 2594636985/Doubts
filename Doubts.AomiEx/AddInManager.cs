using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;

namespace Doubts.AomiEx
{
    public class AddInManager
    {
        private static AddInEngine addInEngine;

        private static bool initialized = false;

        internal static AddInEngine AddInEngine { set; get; }

        public static void Initialize(string addInDir = "AddIns")
        {
            Initialize(Path.Combine(AppDomain.CurrentDomain.BaseDirectory, addInDir), AddInEngineMode.Folder);
        }

        public static void Initialize(string addInDir, AddInEngineMode addInEngineMode = AddInEngineMode.Folder)
        {
            if (!initialized)
            {
                switch (addInEngineMode)
                {
                    case AddInEngineMode.Memory:
                        addInEngine = new MemoryAddInEngine();
                        break;
                    default:
                        addInEngine = new FolderAddInEngine();
                        break;
                }

                if (!Directory.Exists(addInDir))
                    Directory.CreateDirectory(addInDir);


                if (addInEngineMode == AddInEngineMode.Folder)
                    addInEngine.AddAddInsFromDirectory(addInDir);

                addInEngine.Initialize();

                initialized = true;
            }
        }


        public static bool IsInitialized
        {
            get { return initialized; }
        }


        public static List<T> GetInstance<T>(string path, object parameter = null, bool throwOnNotFound = true)
        {
            return addInEngine.BuildItems<T>(path, parameter, throwOnNotFound);
        }

        public static object[] GetInstance(string path, object parameter = null, bool throwOnNotFound = true)
        {
            return addInEngine.BuildItems<object>(path, parameter, throwOnNotFound).ToArray();
        }

        public static T GetSingleInstance<T>(string path, object parameter = null, bool throwOnNotFound = true)
        {
            return GetInstance<T>(path, parameter, throwOnNotFound).FirstOrDefault();
        }
    }
}
