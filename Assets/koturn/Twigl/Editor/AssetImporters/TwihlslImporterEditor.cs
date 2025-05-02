using System.IO;
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
    /// <see cref="CustomEditor"/> for TwihlslImporter.
    /// </summary>
    [CustomEditor(typeof(TwihlslImporter))]
    public class TwihlslImporterEditor : ScriptedImporterEditor
    {
        /// <summary>
        /// <para>Called when drawing Inspector.</para>
        /// <para>Add export button.</para>
        /// </summary>
        public override void OnInspectorGUI()
        {
            do
            {
                if (!GUILayout.Button("Export Shader"))
                {
                    break;
                }

                var assetPath = AssetDatabase.GetAssetPath(target);
                var shaderSource = ShaderGenerator.GenerateShaderSourceFromTemplate(assetPath);
                var exportPath = EditorUtility.SaveFilePanel(
                    "Export Shader",
                    Path.GetDirectoryName(assetPath),
                    Path.GetFileNameWithoutExtension(assetPath),
                    "shader");
                if (string.IsNullOrEmpty(exportPath))
                {
                    break;
                }

                File.WriteAllText(exportPath, shaderSource);
            }
            while (false);

            ApplyRevertGUI();
        }
    }
}
