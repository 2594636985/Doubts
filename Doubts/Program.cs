using Doubts.AomiEx;
using Doubts.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;

namespace Doubts
{
    public class Program
    {
        ///// <summary>
        ///// 应用于唯一的实列应用
        ///// </summary>
        private static Mutex mutex = new Mutex(true, "2FDA6FAD-D715-4A78-A2AE-C6F45BCC2436");

        [STAThread]
        static void Main(string[] args)
        {
            if (mutex.WaitOne(TimeSpan.Zero, true))
            {
                AddInManager.Initialize();

                Renderer renderer = AddInManager.GetSingleInstance<Renderer>("/Doubts/Renderer");

                if (renderer != null)
                    renderer.Main(args);

                mutex.ReleaseMutex();
            }
        }
    }
}
