using Chromium;
using Chromium.Event;
using Chromium.WebBrowser;
using Chromium.WebBrowser.Event;
using Doubts.WebFramework.DoUI;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
using System.Windows.Forms;

namespace Doubts.WebFramework
{
    public class DoubtsUILauncher
    {
        public const string CURRENT_CEF_VERSION = "3.3202.1678";

        internal static string DoubtsLibCefDirPath = null;
        internal static string DoubtsLocalesDir = null;
        internal static string DoubtsBrowserSubprocessPath = "Doubts.ChromiumSubProcess.exe";
        internal static readonly string ApplicationDataDir = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);
        internal static readonly RuntimeArch PlatformArch = CfxRuntime.PlatformArch == CfxPlatformArch.x64 ? RuntimeArch.x64 : RuntimeArch.x86;

        internal static List<GCHandle> SchemeHandlerGCHandles = new List<GCHandle>();

        /// <summary>
        /// 初始化浏览器
        /// </summary>
        /// <param name="localRuntimeDir"></param>
        /// <param name="BeforeChromiumInitialize"></param>
        /// <param name="BeforeCommandLineProcessing"></param>
        /// <returns></returns>
        public static bool InitializeChromium(string localRuntimeDir = null, Action<OnBeforeCfxInitializeEventArgs> BeforeChromiumInitialize = null, Action<CfxOnBeforeCommandLineProcessingEventArgs> BeforeCommandLineProcessing = null)
        {
            if (PrepareRuntime(localRuntimeDir))
            {
                ChromiumWebBrowser.OnBeforeCfxInitialize += (e) =>
                {
                    var cachePath = Path.Combine(ApplicationDataDir, Application.ProductName, "Cache");

                    if (!Directory.Exists(cachePath))
                        Directory.CreateDirectory(cachePath);

                    e.Settings.LocalesDirPath = DoubtsLocalesDir;
                    e.Settings.ResourcesDirPath = DoubtsLibCefDirPath;
                    e.Settings.Locale = "zh-CN";
                    e.Settings.CachePath = cachePath;
                    e.Settings.LogSeverity = CfxLogSeverity.Disable;
                    e.Settings.BrowserSubprocessPath = Path.Combine(localRuntimeDir, DoubtsBrowserSubprocessPath);

                    BeforeChromiumInitialize?.Invoke(e);
                };

                ChromiumWebBrowser.OnBeforeCommandLineProcessing += (args) =>
                {
                    Console.WriteLine("处理命令行参数。。。");

                    BeforeCommandLineProcessing?.Invoke(args);

                    Console.WriteLine(args.CommandLine.CommandLineString);
                };

                ChromiumWebBrowser.OnRegisterCustomSchemes += args =>
                {
                    args.Registrar.AddCustomScheme("embedded", false, false, false, false, false, false);
                };

                try
                {
                    ChromiumWebBrowser.Initialize();

                    RegisterLocalScheme();

                    RegisterDoubtsScheme();

                    return true;
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex);
                    Console.WriteLine(ex.InnerException);
                    MessageBox.Show(string.Format("初始化系统失败。\r\n{0}", ex.Message), "错误", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
            }
            return false;
        }



        private static void RegisterLocalScheme()
        {
            LocalSchemeHandlerFactory scheme = new LocalSchemeHandlerFactory();
            GCHandle gchandle = GCHandle.Alloc(scheme);

            SchemeHandlerGCHandles.Add(gchandle);

            RegisterScheme("local", null, scheme);
        }

        private static void RegisterDoubtsScheme()
        {
            DoubtsSchemeHandlerFactory scheme = new DoubtsSchemeHandlerFactory();
            GCHandle gchandle = GCHandle.Alloc(scheme);

            SchemeHandlerGCHandles.Add(gchandle);

            RegisterScheme("doubts", null, scheme);
        }

        public static void RegisterEmbeddedScheme(Assembly assembly, string schemeName = "http", string domainName = null)
        {
            if (string.IsNullOrEmpty(schemeName))
            {
                throw new ArgumentNullException("schemeName", "必须为scheme指定名称。");
            }

            EmbeddedSchemeHandlerFactory embedded = new EmbeddedSchemeHandlerFactory(schemeName, domainName, assembly);
            GCHandle gchandle = GCHandle.Alloc(embedded);

            SchemeHandlerGCHandles.Add(gchandle);

            RegisterScheme(embedded.SchemeName, domainName, embedded);
        }

        public static void RegisterScheme(string schemeName, string domain, CfxSchemeHandlerFactory factory)
        {
            if (string.IsNullOrEmpty(schemeName))
            {
                throw new ArgumentNullException("schemeName", "必须为scheme指定名称。");
            }

            CfxRuntime.RegisterSchemeHandlerFactory(schemeName, domain, factory);
        }
        /// <summary>
        /// 检测运行的环境
        /// </summary>
        /// <param name="localRuntimeDir"></param>
        /// <returns></returns>
        public static bool PrepareRuntime(string localRuntimeDir = null)
        {
            if (IsLocalRuntimeExisits(localRuntimeDir) == false)
            {
                MessageBox.Show($"CEF Runtime is not found.\r\nCEF Runtime should be in\r\n\"{System.IO.Path.Combine(Application.StartupPath, "fx\\")}\"", "CEF Runtime initialize faild", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return false;
            }

            CfxRuntime.LibCefDirPath = DoubtsLibCefDirPath;

            Application.ApplicationExit += (sender, args) =>
            {
                foreach (var handle in SchemeHandlerGCHandles)
                {
                    handle.Free();
                }

                CfxRuntime.Shutdown();
            };

            return true;
        }



        private static bool IsLocalRuntimeExisits(string localRuntimeDir = null)
        {
            if (string.IsNullOrWhiteSpace(localRuntimeDir))
                localRuntimeDir = Application.StartupPath;

            var libCfxDllName = "libcfx.dll";
            var libcfxLibName = "libcfx.lib";
            var libcfxExpName = "libcfx.exp";
            var libcfxIlkName = "libcfx.ilk";

            if (PlatformArch == RuntimeArch.x64)
            {
                libCfxDllName = "libcfx64.dll";
                libcfxLibName = "libcfx64.lib";
                libcfxExpName = "libcfx64.exp";
                libcfxIlkName = "libcfx64.ilk";
            }


            if (PlatformArch == RuntimeArch.x64)
                DoubtsLibCefDirPath = Path.Combine(localRuntimeDir, @"cef64");
            else
                DoubtsLibCefDirPath = Path.Combine(localRuntimeDir, @"cef");

            DoubtsLocalesDir = Path.Combine(localRuntimeDir, DoubtsLibCefDirPath, @"locales");

            var cfxDllFile = Path.Combine(DoubtsLibCefDirPath, libCfxDllName);

            Dictionary<string, bool> doubtsDetectResults = new Dictionary<string, bool>()
            {
                [libCfxDllName] = File.Exists(Path.Combine(localRuntimeDir, libCfxDllName)),
                [libcfxLibName] = File.Exists(Path.Combine(localRuntimeDir, libcfxLibName)),
                [libcfxIlkName] = File.Exists(Path.Combine(localRuntimeDir, libcfxIlkName)),
                [libcfxExpName] = File.Exists(Path.Combine(localRuntimeDir, libcfxExpName)),
                ["en-US.pak"] = File.Exists(Path.Combine(DoubtsLocalesDir, "en-US.pak")),
                ["cef.pak"] = File.Exists(Path.Combine(DoubtsLibCefDirPath, "cef.pak")),
                ["cef_sandbox.lib"] = File.Exists(Path.Combine(DoubtsLibCefDirPath, "cef_sandbox.lib")),
                ["cef_100_percent.pak"] = File.Exists(Path.Combine(DoubtsLibCefDirPath, "cef_100_percent.pak")),
                ["cef_200_percent.pak"] = File.Exists(Path.Combine(DoubtsLibCefDirPath, "cef_200_percent.pak")),
                ["cef_extensions.pak"] = File.Exists(Path.Combine(DoubtsLibCefDirPath, "cef_extensions.pak")),
                ["chrome_elf.dll"] = File.Exists(Path.Combine(DoubtsLibCefDirPath, "d3dcompiler_43.dll")),
                ["d3dcompiler_43.dll"] = File.Exists(Path.Combine(DoubtsLibCefDirPath, "d3dcompiler_43.dll")),
                ["d3dcompiler_47.dll"] = File.Exists(Path.Combine(DoubtsLibCefDirPath, "d3dcompiler_47.dll")),
                ["devtools_resources.pak"] = File.Exists(Path.Combine(DoubtsLibCefDirPath, "devtools_resources.pak")),
                ["icudtl.dat"] = File.Exists(Path.Combine(DoubtsLibCefDirPath, "icudtl.dat")),
                ["libcef.dll"] = File.Exists(Path.Combine(DoubtsLibCefDirPath, "libcef.dll")),
                ["libcef.lib"] = File.Exists(Path.Combine(DoubtsLibCefDirPath, "libcef.lib")),
                ["libEGL.dll"] = File.Exists(Path.Combine(DoubtsLibCefDirPath, "libEGL.dll")),
                ["libGLESv2.dll"] = File.Exists(Path.Combine(DoubtsLibCefDirPath, "libGLESv2.dll")),
                ["natives_blob.bin"] = File.Exists(Path.Combine(DoubtsLibCefDirPath, "natives_blob.bin")),
                ["snapshot_blob.bin"] = File.Exists(Path.Combine(DoubtsLibCefDirPath, "snapshot_blob.bin")),
                ["v8_context_snapshot.bin"] = File.Exists(Path.Combine(DoubtsLibCefDirPath, "v8_context_snapshot.bin")),
                ["widevinecdmadapter.dll"] = File.Exists(Path.Combine(DoubtsLibCefDirPath, "widevinecdmadapter.dll"))
            };

            return doubtsDetectResults.Count(p => p.Value == true) == doubtsDetectResults.Count;

        }

    }
}
