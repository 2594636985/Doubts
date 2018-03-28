using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using Doubts.AomiEx.Properties;

namespace Doubts.AomiEx
{
    public class AddInReference
    {
        private string name;
        private Version minimumVersion;
        private Version maximumVersion;
        private bool requirePreload;

        static Version entryVersion;

        public Version MinimumVersion
        {
            get { return minimumVersion; }
        }

        public Version MaximumVersion
        {
            get { return maximumVersion; }
        }

        public bool RequirePreload
        {
            get { return requirePreload; }
        }


        public string Name
        {
            get
            {
                return name;
            }
            set
            {
                if (value == null)
                    throw new ArgumentNullException("name");

                if (value.Length == 0)
                    throw new ArgumentException(StringResources.Modules_AddInReference_NameNotEmpty, "name");

                name = value;
            }
        }

        public AddInReference(string name) : this(name, new Version(0, 0, 0, 0), new Version(int.MaxValue, int.MaxValue)) { }

        public AddInReference(string name, Version specificVersion) : this(name, specificVersion, specificVersion) { }

        public AddInReference(string name, Version minimumVersion, Version maximumVersion)
        {
            this.Name = name;

            if (minimumVersion == null)
                throw new ArgumentNullException("minimumVersion");
            if (maximumVersion == null)
                throw new ArgumentNullException("maximumVersion");

            this.minimumVersion = minimumVersion;
            this.maximumVersion = maximumVersion;
        }


        public bool Check(Dictionary<string, Version> addIns, out Version versionFound)
        {
            if (addIns.TryGetValue(name, out versionFound))
            {
                return CompareVersion(versionFound, minimumVersion) >= 0 && CompareVersion(versionFound, maximumVersion) <= 0;
            }
            else
            {
                return false;
            }
        }

        private int CompareVersion(Version a, Version b)
        {
            if (a.Major != b.Major)
            {
                return a.Major > b.Major ? 1 : -1;
            }
            if (a.Minor != b.Minor)
            {
                return a.Minor > b.Minor ? 1 : -1;
            }

            if (a.Build < 0 || b.Build < 0)
                return 0;

            if (a.Build != b.Build)
            {
                return a.Build > b.Build ? 1 : -1;
            }

            if (a.Revision < 0 || b.Revision < 0)
                return 0;

            if (a.Revision != b.Revision)
            {
                return a.Revision > b.Revision ? 1 : -1;
            }

            return 0;
        }

        public static AddInReference Create(AddInProperties properties, string hintPath)
        {
            AddInReference reference = new AddInReference(properties["addin"]);

            string version = properties["version"];

            if (version != null && version.Length > 0)
            {
                int pos = version.IndexOf('-');

                if (pos > 0)
                {
                    reference.minimumVersion = ParseVersion(version.Substring(0, pos), hintPath);
                    reference.maximumVersion = ParseVersion(version.Substring(pos + 1), hintPath);
                }
                else
                {
                    reference.maximumVersion = reference.minimumVersion = ParseVersion(version, hintPath);
                }

                if (reference.Name == "SharpDevelop")
                {
                    if (reference.maximumVersion == new Version("4.1") || reference.maximumVersion == new Version("4.2") || reference.maximumVersion == new Version("4.3"))
                    {
                        reference.maximumVersion = new Version("4.4");
                    }
                }
            }

            reference.requirePreload = string.Equals(properties["requirePreload"], "true", StringComparison.OrdinalIgnoreCase);

            return reference;
        }

        internal static Version ParseVersion(string version, string hintPath)
        {
            if (version == null || version.Length == 0)
                return new Version(0, 0, 0, 0);

            if (version.StartsWith("@"))
            {
                //if (version == "@SharpDevelopCoreVersion")
                //{
                //    if (entryVersion == null)
                //        entryVersion = new Version(RevisionClass.Major + "." + RevisionClass.Minor + "." + RevisionClass.Build + "." + RevisionClass.Revision);

                //    return entryVersion;
                //}

                if (hintPath != null)
                {
                    string fileName = Path.Combine(hintPath, version.Substring(1));

                    try
                    {
                        FileVersionInfo info = FileVersionInfo.GetVersionInfo(fileName);

                        return new Version(info.FileMajorPart, info.FileMinorPart, info.FileBuildPart, info.FilePrivatePart);
                    }
                    catch (FileNotFoundException ex)
                    {
                        throw new AddInException("Cannot get version '" + version + "': " + ex.Message);
                    }
                }

                return new Version(0, 0, 0, 0);
            }
            else
            {
                return new Version(version);
            }
        }
    }
}
