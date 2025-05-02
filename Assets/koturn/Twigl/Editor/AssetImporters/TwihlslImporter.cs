using System.IO;
using System.Text;
using UnityEditor;
using UnityEngine;
#if UNITY_2020_2_OR_NEWER
using UnityEditor.AssetImporters;
#else
using UnityEditor.Experimental.AssetImporters;
#endif  // UNITY_2020_2_OR_NEWER


namespace Koturn.Twigl.AssetImporters
{
    /// <summary>
    /// "*.twihlsl" importer.
    /// </summary>
    [ScriptedImporter(0, "twihlsl")]
    public class TwihlslImporter : ScriptedImporter
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
        /// <para>Called when ".twihlsl" file is imported.</para>
        /// <para>Create shader source from ".twihlsl" file and register created shader as an asset.</para>
        /// </summary>
        /// <param name="ctx">Import context.</param>
        public override void OnImportAsset(AssetImportContext ctx)
        {
            var source = ShaderGenerator.GenerateShaderSourceFromTemplate(ctx.assetPath);
            var textAsset = new TextAsset(source)
            {
                name = "Shader Source",
                hideFlags = HideFlags.HideInHierarchy
            };

#if UNITY_2019_4_0 || UNITY_2019_4_1 || UNITY_2019_4_2 || UNITY_2019_4_3 || UNITY_2019_4_4 || UNITY_2019_4_5 || UNITY_2019_4_6 || UNITY_2019_4_7 || UNITY_2019_4_8 || UNITY_2019_4_9 || UNITY_2019_4_10
            var shader = ShaderUtil.CreateShaderAsset(source, true);
#else
            var shader = ShaderUtil.CreateShaderAsset(ctx, source, true);
#endif

            ctx.AddObjectToAsset("Main Shader", shader);
            ctx.AddObjectToAsset(textAsset.name, textAsset);
            ctx.SetMainObject(shader);
        }
    }
}
