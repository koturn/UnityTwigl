using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;
using Koturn.Twigl.Enums;


namespace Koturn.Twigl.Inspectors
{
    /// <summary>
    /// Custom editor of twigl shaders.
    /// </summary>
    public class TwiglGUI : ShaderGUI
    {
        /// <summary>
        /// Property name of "_Lighting".
        /// </summary>
        private const string PropNameLighting = "_Lighting";
        /// <summary>
        /// Property name of "_Glossiness".
        /// </summary>
        private const string PropNameGlossiness = "_Glossiness";
        /// <summary>
        /// Property name of "_Metallic".
        /// </summary>
        private const string PropNameMetallic = "_Metallic";
        /// <summary>
        /// Property name of "_SpecColor".
        /// </summary>
        private const string PropNameSpecColor = "_SpecColor";
        /// <summary>
        /// Property name of "_SpecPower".
        /// </summary>
        private const string PropNameSpecPower = "_SpecPower";
        /// <summary>
        /// Property name of "_ForwardAdd".
        /// </summary>
        private const string PropNameForwardAdd = "_ForwardAdd";
        /// <summary>
        /// Property name of "_Cull".
        /// </summary>
        private const string PropNameCull = "_Cull";
        /// <summary>
        /// Property name of "_Mode".
        /// </summary>
        private const string PropNameMode = "_Mode";
        /// <summary>
        /// Property name of "_AlphaTest".
        /// </summary>
        private const string PropNameAlphaTest = "_AlphaTest";
        /// <summary>
        /// Property name of "_Cutoff".
        /// </summary>
        private const string PropNameCutoff = "_Cutoff";
        /// <summary>
        /// Property name of "_SrcBlend".
        /// </summary>
        private const string PropNameSrcBlend = "_SrcBlend";
        /// <summary>
        /// Property name of "_DstBlend".
        /// </summary>
        private const string PropNameDstBlend = "_DstBlend";
        /// <summary>
        /// Property name of "_SrcBlendAlpha".
        /// </summary>
        private const string PropNameSrcBlendAlpha = "_SrcBlendAlpha";
        /// <summary>
        /// Property name of "_DstBlendAlpha".
        /// </summary>
        private const string PropNameDstBlendAlpha = "_DstBlendAlpha";
        /// <summary>
        /// Property name of "_BlendOp".
        /// </summary>
        private const string PropNameBlendOp = "_BlendOp";
        /// <summary>
        /// Property name of "_BlendOpAlpha".
        /// </summary>
        private const string PropNameBlendOpAlpha = "_BlendOpAlpha";
        /// <summary>
        /// Property name of "_ZTest".
        /// </summary>
        private const string PropNameZTest = "_ZTest";
        /// <summary>
        /// Property name of "_ZClip".
        /// </summary>
        private const string PropNameZClip = "_ZClip";
        /// <summary>
        /// Property name of "_ZWrite".
        /// </summary>
        private const string PropNameZWrite = "_ZWrite";
        /// <summary>
        /// Property name of "_OffsetFactor".
        /// </summary>
        private const string PropNameOffsetFactor = "_OffsetFactor";
        /// <summary>
        /// Property name of "_OffsetUnit".
        /// </summary>
        private const string PropNameOffsetUnit = "_OffsetUnit";
        /// <summary>
        /// Property name of "_ColorMask".
        /// </summary>
        private const string PropNameColorMask = "_ColorMask";
        /// <summary>
        /// Property name of "_AlphaToMask".
        /// </summary>
        private const string PropNameAlphaToMask = "_AlphaToMask";
        /// <summary>
        /// Property name of "_StencilRef".
        /// </summary>
        private const string PropNameStencilRef = "_StencilRef";
        /// <summary>
        /// Property name of "_StencilReadMask".
        /// </summary>
        private const string PropNameStencilReadMask = "_StencilReadMask";
        /// <summary>
        /// Property name of "_StencilWriteMask".
        /// </summary>
        private const string PropNameStencilWriteMask = "_StencilWriteMask";
        /// <summary>
        /// Property name of "_StencilComp".
        /// </summary>
        private const string PropNameStencilComp = "_StencilComp";
        /// <summary>
        /// Property name of "_StencilPass".
        /// </summary>
        private const string PropNameStencilPass = "_StencilPass";
        /// <summary>
        /// Property name of "_StencilFail".
        /// </summary>
        private const string PropNameStencilFail = "_StencilFail";
        /// <summary>
        /// Property name of "_StencilZFail".
        /// </summary>
        private const string PropNameStencilZFail = "_StencilZFail";

        /// <summary>
        /// Editor UI mode names.
        /// </summary>
        private static readonly string[] _editorModeNames;

        /// <summary>
        /// Current editor UI mode.
        /// </summary>
        private static EditorMode _editorMode;
        /// <summary>
        /// Key list of cache of MaterialPropertyHandlers.
        /// </summary>
        private static List<string> _propStringList;


        /// <summary>
        /// Initialize <see cref="_editorMode"/>, <see cref="_editorModeNames"/>.
        /// </summary>
        static TwiglGUI()
        {
            _editorMode = (EditorMode)(-1);
            _editorModeNames = Enum.GetNames(typeof(EditorMode));
        }

        /// <summary>
        /// Draw property items.
        /// </summary>
        /// <param name="me">The <see cref="MaterialEditor"/> that are calling this <see cref="OnGUI(MaterialEditor, MaterialProperty[])"/> (the 'owner').</param>
        /// <param name="mps">Material properties of the current selected shader.</param>
        public override void OnGUI(MaterialEditor me, MaterialProperty[] mps)
        {
            if (!Enum.IsDefined(typeof(EditorMode), _editorMode))
            {
                MaterialPropertyUtil.ClearDecoratorDrawers(((Material)me.target).shader, mps);
                _editorMode = EditorMode.Custom;
            }
            using (var ccScope = new EditorGUI.ChangeCheckScope())
            {
                _editorMode = (EditorMode)GUILayout.Toolbar((int)_editorMode, _editorModeNames);
                if (ccScope.changed)
                {
                    if (_propStringList != null)
                    {
                        MaterialPropertyUtil.ClearPropertyHandlerCache(_propStringList);
                    }

                    var shader = ((Material)me.target).shader;
                    if (_editorMode == EditorMode.Custom)
                    {
                        _propStringList = MaterialPropertyUtil.ClearDecoratorDrawers(shader, mps);
                    }
                    else
                    {
                        _propStringList = MaterialPropertyUtil.ClearCustomDrawers(shader, mps);
                    }
                }
            }
            if (_editorMode == EditorMode.Default)
            {
                base.OnGUI(me, mps);
                return;
            }

            EditorGUILayout.LabelField("Lighting Parameters", EditorStyles.boldLabel);
            using (new EditorGUI.IndentLevelScope())
            using (new EditorGUILayout.VerticalScope(GUI.skin.box))
            {
                var mpLighting = FindAndDrawProperty(me, mps, PropNameLighting, false);
                var lightingMethod = (LightingMethod)(mpLighting == null ? -1 : (int)mpLighting.floatValue);

                using (new EditorGUI.IndentLevelScope())
                using (new EditorGUI.DisabledScope(lightingMethod == LightingMethod.UnityLambert || lightingMethod == LightingMethod.Unlit))
                {
                    ShaderProperty(me, mps, PropNameGlossiness, false);
                    using (new EditorGUI.DisabledScope(lightingMethod != LightingMethod.UnityStandard))
                    {
                        ShaderProperty(me, mps, PropNameMetallic, false);
                    }
                    using (new EditorGUI.DisabledScope(lightingMethod != LightingMethod.UnityBlinnPhong && lightingMethod != LightingMethod.UnityStandardSpecular))
                    {
                        ShaderProperty(me, mps, PropNameSpecColor, false);
                    }
                    using (new EditorGUI.DisabledScope(lightingMethod != LightingMethod.UnityBlinnPhong))
                    {
                        ShaderProperty(me, mps, PropNameSpecPower, false);
                    }
                }
            }

            EditorGUILayout.Space();

            EditorGUILayout.LabelField("Rendering Options", EditorStyles.boldLabel);
            using (new EditorGUI.IndentLevelScope())
            using (new EditorGUILayout.VerticalScope(GUI.skin.box))
            {
                ShaderProperty(me, mps, PropNameForwardAdd, false);
                ShaderProperty(me, mps, PropNameCull, false);
                DrawRenderingMode(me, mps);
                ShaderProperty(me, mps, PropNameZTest, false);
                ShaderProperty(me, mps, PropNameZClip, false);
                DrawOffsetProperties(me, mps, PropNameOffsetFactor, PropNameOffsetUnit);
                ShaderProperty(me, mps, PropNameColorMask, false);
                ShaderProperty(me, mps, PropNameAlphaToMask, false);

                EditorGUILayout.Space();
                DrawBlendProperties(me, mps);
                EditorGUILayout.Space();
                DrawStencilProperties(me, mps);
                EditorGUILayout.Space();
                DrawAdvancedOptions(me, mps);
            }
        }

        /// <summary>
        /// Draw default item of specified shader property.
        /// </summary>
        /// <param name="me">A <see cref="MaterialEditor"/>.</param>
        /// <param name="mps"><see cref="MaterialProperty"/> array.</param>
        /// <param name="propName">Name of shader property.</param>
        /// <param name="isMandatory">If <c>true</c> then this method will throw an exception
        /// if a property with <<paramref name="propName"/> was not found.</param>
        protected static void ShaderProperty(MaterialEditor me, MaterialProperty[] mps, string propName, bool isMandatory = true)
        {
            var prop = FindProperty(propName, mps, isMandatory);
            if (prop != null)
            {
                ShaderProperty(me, prop);
            }
        }

        /// <summary>
        /// Draw default item of specified shader property.
        /// </summary>
        /// <param name="me">A <see cref="MaterialEditor"/>.</param>
        /// <param name="mp">Target <see cref="MaterialProperty"/>.</param>
        protected static void ShaderProperty(MaterialEditor me, MaterialProperty mp)
        {
            if (mp != null)
            {
                me.ShaderProperty(mp, mp.displayName);
            }
        }

        /// <summary>
        /// Draw default item of specified shader property and return the property.
        /// </summary>
        /// <param name="me">A <see cref="MaterialEditor"/>.</param>
        /// <param name="mps"><see cref="MaterialProperty"/> array.</param>
        /// <param name="propName">Name of shader property.</param>
        /// <param name="isMandatory">If <c>true</c> then this method will throw an exception
        /// if a property with <<paramref name="propName"/> was not found.</param>
        /// <return>Found property.</return>
        protected static MaterialProperty FindAndDrawProperty(MaterialEditor me, MaterialProperty[] mps, string propName, bool isMandatory = true)
        {
            var prop = FindProperty(propName, mps, isMandatory);
            if (prop != null)
            {
                ShaderProperty(me, prop);
            }

            return prop;
        }

        /// <summary>
        /// Find properties which has specified names.
        /// </summary>
        /// <param name="propNames">Names of shader property.</param>
        /// <param name="mps"><see cref="MaterialProperty"/> array.</param>
        /// <param name="isMandatory">If <c>true</c> then this method will throw an exception
        /// if one of properties with <<paramref name="propNames"/> was not found.</param>
        /// <return>Found properties.</return>
        protected static List<MaterialProperty> FindProperties(string[] propNames, MaterialProperty[] mps, bool isMandatory = true)
        {
            var mpList = new List<MaterialProperty>(propNames.Length);
            foreach (var propName in propNames)
            {
                var prop = FindProperty(propName, mps, isMandatory);
                if (prop != null)
                {
                    mpList.Add(prop);
                }
            }

            return mpList;
        }

        /// <summary>
        /// Draw inspector items of <see cref="RenderingMode"/>.
        /// </summary>
        /// <param name="me">A <see cref="MaterialEditor"/></param>
        /// <param name="mps"><see cref="MaterialProperty"/> array</param>
        private void DrawRenderingMode(MaterialEditor me, MaterialProperty[] mps)
        {
            var mpMMode = FindProperty(PropNameMode, mps, false);
            var mode = RenderingMode.Custom;
            if (mpMMode != null)
            {
                using (var ccScope = new EditorGUI.ChangeCheckScope())
                {
                    mode = (RenderingMode)EditorGUILayout.EnumPopup(mpMMode.displayName, (RenderingMode)mpMMode.floatValue);
                    mpMMode.floatValue = (float)mode;
                    if (ccScope.changed)
                    {
                        if (mode != RenderingMode.Custom)
                        {
                            ApplyRenderingMode(me, mps, mode);
                        }
                    }
                }
            }

            using (new EditorGUI.DisabledScope(mode != RenderingMode.Cutout && mode != RenderingMode.Custom))
            {
                var mpAlphaTest = FindAndDrawProperty(me, mps, PropNameAlphaTest, false);
                if (mpAlphaTest != null)
                {
                    using (new EditorGUI.IndentLevelScope())
                    using (new EditorGUI.DisabledScope(!ValueConverter.ToBool(mpAlphaTest.floatValue)))
                    {
                        ShaderProperty(me, mps, PropNameCutoff);
                    }
                }
            }

            using (new EditorGUI.DisabledScope(mode != RenderingMode.Custom))
            {
                ShaderProperty(me, mps, PropNameZWrite, false);
            }
        }

        /// <summary>
        /// <para>Change rendeing mode.</para>
        /// <para>In other words, change following values.
        /// <list type="bullet">
        ///   <item>Value of material tag, "RenderType".</item>
        ///   <item>Value of render queue.</item>
        ///   <item>Shader property, "_AlphaTest" and related tag, "_ALPHATEST_ON".</item>
        ///   <item>Shader property, "_ZWrite".</item>
        ///   <item>Shader property, "_SrcBlend".</item>
        ///   <item>Shader property, "_DstBlend".</item>
        ///   <item>Shader property, "_SrcBlendAlpha".</item>
        ///   <item>Shader property, "_DstBlendAlpha".</item>
        ///   <item>Shader property, "_BlendOp".</item>
        ///   <item>Shader property, "_BlendOpAlpha".</item>
        /// </list>
        /// </para>
        /// </summary>
        /// <param name="me">A <see cref="MaterialEditor"/>.</param>
        /// <param name="mps"><see cref="MaterialProperty"/> array.</param>
        /// <param name="renderingMode">Rendering mode.</param>
        private static void ApplyRenderingMode(MaterialEditor me, MaterialProperty[] mps, RenderingMode renderingMode)
        {
            var config = new RenderingModeConfig(renderingMode);

            foreach (var material in me.targets.Cast<Material>())
            {
                MaterialUtil.SetRenderTypeTag(material, config.RenderType);
                MaterialUtil.SetRenderQueue(material, config.RenderQueue);
            }

            var mpAlphaTest = FindProperty(PropNameAlphaTest, mps, false);
            if (mpAlphaTest != null)
            {
                mpAlphaTest.floatValue = ValueConverter.ToFloat(config.IsAlphaTestEnabled);
                MaterialPropertyUtil.ToggleKeyword(((Material)me.target).shader, mpAlphaTest);
            }

            SetPropertyValue(PropNameZWrite, mps, config.IsZWriteEnabled, false);
            SetPropertyValue(PropNameSrcBlend, mps, config.SrcBlend, false);
            SetPropertyValue(PropNameDstBlend, mps, config.DstBlend, false);
            SetPropertyValue(PropNameSrcBlendAlpha, mps, config.SrcBlendAlpha, false);
            SetPropertyValue(PropNameDstBlendAlpha, mps, config.DstBlendAlpha, false);
            SetPropertyValue(PropNameBlendOp, mps, config.BlendOp, false);
            SetPropertyValue(PropNameBlendOp, mps, config.BlendOpAlpha, false);
        }

        /// <summary>
        /// Draw inspector items of "Blend".
        /// </summary>
        /// <param name="me">A <see cref="MaterialEditor"/></param>
        /// <param name="mps"><see cref="MaterialProperty"/> array</param>
        private void DrawBlendProperties(MaterialEditor me, MaterialProperty[] mps)
        {
            var mpMode = FindProperty(PropNameMode, mps, false);
            using (new EditorGUI.DisabledScope(mpMode != null && (RenderingMode)mpMode.floatValue != RenderingMode.Custom))
            {
                var propSrcBlend = FindProperty(PropNameSrcBlend, mps, false);
                var propDstBlend = FindProperty(PropNameDstBlend, mps, false);
                if (propSrcBlend == null || propDstBlend == null)
                {
                    return;
                }

                EditorGUILayout.LabelField("Blend", EditorStyles.boldLabel);
                using (new EditorGUI.IndentLevelScope())
                using (new EditorGUILayout.VerticalScope(GUI.skin.box))
                {
                    ShaderProperty(me, propSrcBlend);
                    ShaderProperty(me, propDstBlend);

                    var propSrcBlendAlpha = FindProperty(PropNameSrcBlendAlpha, mps, false);
                    var propDstBlendAlpha = FindProperty(PropNameDstBlendAlpha, mps, false);
                    if (propSrcBlendAlpha != null && propDstBlendAlpha != null)
                    {
                        ShaderProperty(me, propSrcBlendAlpha);
                        ShaderProperty(me, propDstBlendAlpha);
                    }

                    ShaderProperty(me, mps, PropNameBlendOp, false);
                    ShaderProperty(me, mps, PropNameBlendOpAlpha, false);
                }
            }
        }

        /// <summary>
        /// Draw inspector items of "Offset".
        /// </summary>
        /// <param name="me">A <see cref="MaterialEditor"/></param>
        /// <param name="mps"><see cref="MaterialProperty"/> array</param>
        /// <param name="propNameFactor">Property name for the first argument of "Offset"</param>
        /// <param name="propNameUnit">Property name for the second argument of "Offset"</param>
        private static void DrawOffsetProperties(MaterialEditor me, MaterialProperty[] mps, string propNameFactor, string propNameUnit)
        {
            var propFactor = FindProperty(propNameFactor, mps, false);
            var propUnit = FindProperty(propNameUnit, mps, false);
            if (propFactor == null || propUnit == null)
            {
                return;
            }
            EditorGUILayout.LabelField("Offset");
            using (new EditorGUI.IndentLevelScope())
            {
                ShaderProperty(me, propFactor);
                ShaderProperty(me, propUnit);
            }
        }

        /// <summary>
        /// Draw inspector items of Stencil.
        /// </summary>
        /// <param name="me">A <see cref="MaterialEditor"/></param>
        /// <param name="mps"><see cref="MaterialProperty"/> array</param>
        private static void DrawStencilProperties(MaterialEditor me, MaterialProperty[] mps)
        {
            var stencilProps = FindProperties(new []
            {
                PropNameStencilRef,
                PropNameStencilReadMask,
                PropNameStencilWriteMask,
                PropNameStencilComp,
                PropNameStencilPass,
                PropNameStencilFail,
                PropNameStencilZFail
            }, mps, false);

            if (stencilProps.Count == 0)
            {
                return;
            }

            EditorGUILayout.LabelField("Stencil", EditorStyles.boldLabel);
            using (new EditorGUI.IndentLevelScope())
            using (new EditorGUILayout.VerticalScope(GUI.skin.box))
            {
                foreach (var prop in stencilProps)
                {
                    me.ShaderProperty(prop, prop.displayName);
                }
            }
        }

        /// <summary>
        /// Draw inspector items of advanced options.
        /// </summary>
        /// <param name="me">A <see cref="MaterialEditor"/>.</param>
        /// <param name="mps"><see cref="MaterialProperty"/> array.</param>
        private static void DrawAdvancedOptions(MaterialEditor me, MaterialProperty[] mps)
        {
            EditorGUILayout.LabelField("Advanced Options", EditorStyles.boldLabel);
            using (new EditorGUI.IndentLevelScope())
            using (new EditorGUILayout.VerticalScope(GUI.skin.box))
            {
                me.RenderQueueField();
#if UNITY_5_6_OR_NEWER
                me.EnableInstancingField();
                me.DoubleSidedGIField();
#endif  // UNITY_5_6_OR_NEWER
            }
        }

        /// <summary>
        /// Set property value.
        /// </summary>
        /// <param name="propName">Names of shader property.</param>
        /// <param name="mps"><see cref="MaterialProperty"/> array.</param>
        /// <param name="val">Value to set. (true: 1.0f, false 0.0f).</param>
        /// <param name="isMandatory">
        /// If true then this method will throw an exception if property <paramref name="propName"/> is not found.
        /// Otherwise do nothing if property with <paramref name="propName"/> is not found.
        /// </param>
        private static void SetPropertyValue(string propName, MaterialProperty[] mps, bool val, bool isMandatory = true)
        {
            var prop = FindProperty(propName, mps, isMandatory);
            if (prop != null)
            {
                prop.floatValue = ValueConverter.ToFloat(val);
            }
        }

        /// <summary>
        /// Set property value.
        /// </summary>
        /// <typeparam name="T">Type of enum.</typeparam>
        /// <param name="propName">Names of shader property.</param>
        /// <param name="mps"><see cref="MaterialProperty"/> array.</param>
        /// <param name="val">Value to set, which is cast to <see cref="float"/>.</param>
        /// <param name="isMandatory">
        /// If true then this method will throw an exception if property <paramref name="propName"/> was not found.
        /// Otherwise do nothing if property with <paramref name="propName"/> was not found.
        /// </param>
        private static void SetPropertyValue<T>(string propName, MaterialProperty[] mps, T val, bool isMandatory = true)
            where T : unmanaged, Enum
        {
            var prop = FindProperty(propName, mps, isMandatory);
            if (prop != null)
            {
                prop.floatValue = ValueConverter.ToInt(val);
            }
        }
    }
}
