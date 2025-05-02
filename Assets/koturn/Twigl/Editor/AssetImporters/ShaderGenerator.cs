using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using UnityEditor;


namespace Koturn.Twigl.AssetImporters
{
    /// <summary>
    /// ShaderLab source generator.
    /// </summary>
    internal static class ShaderGenerator
    {
        /// <summary>
        /// GUID of shader template file.
        /// </summary>
        private const string TemplateGuid = "dba61e9f13b91444dbb632fe1fd9fa91";
        /// <summary>
        /// GUID of shader directory.
        /// </summary>
        private const string ShaderDirGuid = "933a8921bf87e9748abd5e318e6fa71e";


        /// <summary>
        /// <see cref="byte"/> array of shader template file.
        /// </summary>
        private static byte[] _templateData;
        /// <summary>
        /// <see cref="Regex"/> taht matches "#guidinclude" custom directive.
        /// </summary>
        private static readonly Regex _guidIncludeRegex = new Regex(
            "#guidinclude(\\s+)\"([^\"]+)\"",
            RegexOptions.Compiled | RegexOptions.CultureInvariant);

        /// <summary>
        /// Create ShaderLab source code from specified asset path and shader template file.
        /// </summary>
        /// <param name="snippetPath">Path to shader snippet.</param>
        /// <returns>ShaderLab source code.</returns>
        public static string GenerateShaderSourceFromTemplate(string snippetPath)
        {
            var twiSource = File.ReadAllText(snippetPath);
            var shaderDirPath = AssetDatabase.GUIDToAssetPath(ShaderDirGuid);

            var relPath = snippetPath.Replace(shaderDirPath, string.Empty).TrimStart('/');
            var lastSepIndex = relPath.LastIndexOf('/');
            var prefix = lastSepIndex >= 0 ? relPath.Substring(0, lastSepIndex + 1) : string.Empty;
            var name = prefix + Path.GetFileNameWithoutExtension(relPath);

            var sb = new StringBuilder();
            using (var ms = GetTemplateMemoryStream())
            using (var sr = new StreamReader(ms))
            {
                string line = null;
                while ((line = sr.ReadLine()) != null)
                {
                    line = line.Replace("#SCRIPTNAME#", name)
                        .Replace("#TWIHLSL#", twiSource);
                    line = _guidIncludeRegex.Replace(line, match =>
                    {
                        return new StringBuilder("#include")
                            .Append(match.Groups[1].Value)
                            .Append('"')
                            .Append(AssetDatabase.GUIDToAssetPath(match.Groups[2].Value))
                            .Append('"')
                            .ToString();
                    });
                    sb.Append(line).Append('\n');
                }
            }

            return sb.ToString();
        }

        /// <summary>
        /// Get <see cref="MemoryStream"/> of shader template file.
        /// </summary>
        /// <returns><see cref="MemoryStream"/> of shader template file.</returns>
        private static MemoryStream GetTemplateMemoryStream()
        {
            var templateData = _templateData;
            if (templateData == null)
            {
                var templatePath = AssetDatabase.GUIDToAssetPath(TemplateGuid);
                templateData = File.ReadAllBytes(templatePath);
                _templateData = templateData;
            }
            return new MemoryStream(templateData);
        }
    }
}
