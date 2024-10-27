using UnityEngine;
using UnityEngine.Rendering;
using Koturn.Twigl.Enums;


namespace Koturn.Twigl.Inspectors
{
    /// <summary>
    /// Provides utility methods for <see cref="Material"/>.
    /// </summary>
    static class MaterialUtil
    {
        /// <summary>
        /// Tag name of "RenderType".
        /// </summary>
        private const string TagRenderType = "RenderType";

        /// <summary>
        /// Set render queue value if the value is differ from the default.
        /// </summary>
        /// <param name="material">Target material.</param>
        /// <param name="renderQueue"><see cref="RenderQueue"/> to set.</param>
        public static void SetRenderTypeTag(Material material, RenderType renderType)
        {
            // Set to default and get the default.
            material.SetOverrideTag(TagRenderType, string.Empty);
            var defaultTagval = material.GetTag(TagRenderType, false, "Transparent");

            // Set specified render type value if the value differs from the default.
            var renderTypeValue = renderType.ToString();
            if (renderTypeValue != defaultTagval)
            {
                material.SetOverrideTag(TagRenderType, renderTypeValue);
            }
        }

        /// <summary>
        /// Set render queue value if the value is differ from the default.
        /// </summary>
        /// <param name="material">Target material.</param>
        /// <param name="renderQueue"><see cref="RenderQueue"/> to set.</param>
        public static void SetRenderQueue(Material material, RenderQueue renderQueue)
        {
            // Set to default and get the default.
            material.renderQueue = -1;
            var defaultRenderQueue = material.renderQueue;

            // Set specified render queue value if the value differs from the default.
            var renderQueueValue = (int)renderQueue;
            if (defaultRenderQueue != renderQueueValue)
            {
                material.renderQueue = renderQueueValue;
            }
        }
    }
}
