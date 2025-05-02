#ifndef TWIGL_CORE_INCLUDED
#define TWIGL_CORE_INCLUDED

#include "include/VertCommon.cginc"
#include "include/LightingUtils.cginc"
#include "twigl.cginc"


//! pseudo frame count.
static float f = floor(_Time.y / 60.0);
//! Mouse position.
static float2 m = float2(0.0, 0.0);
//! Resolution.
static float2 r = float2(1280.0, 1280.0);
//! Sound frequency.
static float s = 0.0;
//! Elapse time.
static float t = _Time.y;


/*!
 * @brief G-Buffer data which is output of fragTwigl in deferred rendering pass.
 * @see fragTwigl
 */
struct gbuffer_twigl
{
    //! Diffuse and occlustion. (rgb: diffuse, a: occlusion)
    half4 diffuse : SV_Target0;
    //! Specular and smoothness. (rgb: specular, a: smoothness)
    half4 specular : SV_Target1;
    //! Normal. (rgb: normal, a: unused)
    half4 normal : SV_Target2;
    //! Emission. (rgb: emission, a: unused)
    half4 emission : SV_Target3;
};


#if defined(_ALPHATEST_ON)
UNITY_INSTANCING_BUFFER_START(Props)
//! Alpha Cutoff.
UNITY_DEFINE_INSTANCED_PROP(float, _Cutoff)
UNITY_INSTANCING_BUFFER_END(Props)
#endif  // defined(_ALPHATEST_ON)


void geekestDefault(inout half4 o, float4 FC);

#if !defined(GEEKEST)
#    define GEEKEST geekestDefault
#endif  // !defined(GEEKEST)


#if defined(UNITY_PASS_FORWARDADD) && (defined(_FORWARDADD_OFF) || defined(_LIGHTING_UNLIT))
/*!
 * @brief Fragment shader function.
 * @param [in] fi  Input data from vertex shader
 * @return Output of each texels (fout_raymarching).
 */
half4 fragTwigl() : SV_Target
{
    return half4(0.0, 0.0, 0.0, 0.0);
}
#else
#    if defined(UNITY_PASS_DEFERRED)
/*!
 * @brief Fragment shader function.
 * @param [in] fi  Input data from vertex shader.
 * @return G-Buffer data.
 */
gbuffer_twigl fragTwigl(v2f_twigl fi)
#    else
/*!
 * @brief Fragment shader function.
 * @param [in] fi  Input data from vertex shader.
 * @return Color of texel.
 */
half4 fragTwigl(v2f_twigl fi) : SV_Target
#    endif  // defined(UNITY_PASS_DEFERRED)
{
    UNITY_SETUP_INSTANCE_ID(fi);
    UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(fi);

#    if defined(USE_FACING_PARAMETER)
    fi.normal = isFacing(fi.facing) ? fi.normal : -fi.normal;
#    endif  // defined(USE_FACING_PARAMETER)

    half4 mainCol = half4(0.0, 0.0, 0.0, 1.0);
    GEEKEST(/* inout */ mainCol, float4(fi.uv * r, 0.0, 1.0));

#    if defined(_ALPHATEST_ON)
    clip(mainCol.a - UNITY_ACCESS_INSTANCED_PROP(Props, _Cutoff));
#    endif  // defined(_ALPHATEST_ON)

#    if defined(LIGHTMAP_ON)
#        if defined(DYNAMICLIGHTMAP_ON)
    const float4 lmap = fi.lmap;
#        else
    const float4 lmap = float4(fi.lmap, 0.0, 0.0);
#        endif  // defined(DYNAMICLIGHTMAP_ON)
    const half3 ambient = half3(0.0, 0.0, 0.0);
#    elif defined(UNITY_SHOULD_SAMPLE_SH)
    const float4 lmap = float4(0.0, 0.0, 0.0, 0.0);
    const half3 ambient = fi.ambient;
#    else
    const float4 lmap = float4(0.0, 0.0, 0.0, 0.0);
    const half3 ambient = half3(0.0, 0.0, 0.0);
#    endif  // defined(LIGHTMAP_ON)

    UNITY_LIGHT_ATTENUATION(atten, fi, fi.worldPos);

#    if defined(UNITY_PASS_DEFERRED)
    gbuffer_twigl gb;
    UNITY_INITIALIZE_OUTPUT(gbuffer_twigl, gb);
    gb.emission = calcLightingUnityDeferred(
        mainCol,
        fi.worldPos,
        fi.normal,
        atten,
        lmap,
        ambient,
        /* out */ gb.diffuse,
        /* out */ gb.specular,
        /* out */ gb.normal);
    return gb;
#    else
    half4 color = calcLightingUnity(
        mainCol,
        fi.worldPos,
        fi.normal,
        atten,
        lmap,
        ambient);
    UNITY_APPLY_FOG(fi.fogCoord, color);
    return color;
#    endif  // defined(UNITY_PASS_DEFERRED)
}
#endif  // defined(UNITY_PASS_FORWARDADD) && (defined(_FORWARDADD_OFF) || defined(_LIGHTING_UNLIT))


/*!
 * @brief Fragment shader function for ShadowCaster pass.
 * @param [in] fi  Input data from vertex shader.
 * @return Color of texel.
 */
fixed4 fragTwiglShadowCaster(v2f_twigl_shadowcaster fi) : SV_Target
{
    UNITY_SETUP_INSTANCE_ID(fi);
    UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(fi);

    SHADOW_CASTER_FRAGMENT(fi)
}


/*!
 * @brief Default fragment shader core function in twigl-geekest form.
 * @param [in,out] o  Output color.
 * @param [in] FC  Fragment coordinate.
 * @return Color of texel.
 */
void geekestDefault(inout half4 o, float4 FC)
{
    o = half4(0.0, 0.0, 0.0, 1.0);
}


#endif  // !defined(TWIGL_CORE_INCLUDED)
