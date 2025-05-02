using System.IO;
using System.Text;
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
        /// <see cref="byte"/> array of shader template file.
        /// </summary>
        private static byte[] _templateData;

        /// <summary>
        /// Create ShaderLab source code from specified asset path and shader template file.
        /// </summary>
        /// <param name="snippetPath">Path to shader snippet.</param>
        /// <returns>ShaderLab source code.</returns>
        public static string GenerateShaderSourceFromTemplate(string snippetPath)
        {
            var twiSource = File.ReadAllText(snippetPath);
            var name = Path.GetFileNameWithoutExtension(snippetPath);

            var sb = new StringBuilder();
            using (var ms = GetTemplateMemoryStream())
            using (var sr = new StreamReader(ms))
            {
                string line = null;
                while ((line = sr.ReadLine()) != null)
                {
                    line = line.Replace("#SCRIPTNAME#", name)
                        .Replace("#TWIHLSL#", twiSource);
                    sb.AppendLine(line);
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
