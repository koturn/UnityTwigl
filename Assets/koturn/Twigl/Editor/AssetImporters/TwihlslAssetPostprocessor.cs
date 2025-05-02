using System;
using UnityEditor;
using UnityEngine;


namespace Koturn.Twigl.AssetImporters
{
    /// <summary>
    /// <see cref="AssetPostprocessor"/> for ".twihlsl" file.
    /// </summary>
    public class TwihlslAssetPostprocessor : AssetPostprocessor
    {
        /// <summary>
        /// <para>This is called after importing of any number of assets is complete (when the Assets progress bar has reached the end).</para>
        /// <para>Load assets from ".twihlsl" and register it as a shader.</para>
        /// </summary>
        /// <param name="importedAssets">Pathes of imported assets.</param>
        /// <param name="deletedAssets">Pathes of deleted assets.</param>
        /// <param name="movedAssets">Pathes of moved assets.</param>
        /// <param name="movedFromAssetPaths">Paths of assets before moving.</param>
        private static void OnPostprocessAllAssets(string[] importedAssets, string[] deletedAssets, string[] movedAssets, string[] movedFromAssetPaths)
        {
            foreach (var path in importedAssets)
            {
                if (!path.EndsWith(".twihlsl", StringComparison.InvariantCultureIgnoreCase))
                {
                    continue;
                }

                var mainShader = AssetDatabase.LoadMainAssetAtPath(path) as Shader;
                if (mainShader != null)
                {
                    ShaderUtil.RegisterShader(mainShader);
                }

                foreach (var obj in AssetDatabase.LoadAllAssetRepresentationsAtPath(path))
                {
                    var shader = obj as Shader;
                    if (shader != null)
                    {
                        ShaderUtil.RegisterShader(shader);
                    }
                }
            }
        }
    }
}
