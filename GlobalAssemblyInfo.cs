using System.Resources;
using System.Reflection;

[assembly: System.Runtime.InteropServices.ComVisible(false)]
[assembly: AssemblyCompany("Aomi")]
[assembly: AssemblyProduct("Taomi")]
[assembly: AssemblyCopyright("aomi")]
[assembly: AssemblyVersion(RevisionClass.Major + "." + RevisionClass.Minor + "." + RevisionClass.Build + "." + RevisionClass.Revision)]
[assembly: AssemblyInformationalVersion(RevisionClass.FullVersion + "-00000000")]
[assembly: NeutralResourcesLanguage("zh-CN")]

[assembly: System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2243:AttributeStringLiteralsShouldParseCorrectly", Justification = "AssemblyInformationalVersion does not need to be a parsable version")]

internal static class RevisionClass
{
	public const string Major = "1";
	public const string Minor = "0";
	public const string Build = "0";
	public const string Revision = "0";
	public const string VersionName = null; 
	
	public const string FullVersion = Major + "." + Minor + "." + Build + ".0";
}
