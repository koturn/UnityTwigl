#ifndef VERT_COMMON_INCLUDED
#define VERT_COMMON_INCLUDED

#include "UnityCG.cginc"
#include "UnityStandardUtils.cginc"
#include "AutoLight.cginc"

#if defined(SHADER_API_D3D11_9X)
#    undef _FLIPNORMAL_ON
#endif  // defined(SHADER_API_D3D11_9X)
#if defined(SHADER_STAGE_FRAGMENT) && defined(_FLIPNORMAL_ON)
#    define USE_FACING_PARAMETER
#endif


#if defined(SHADER_API_GLCORE) || defined(SHADER_API_GLES) || defined(SHADER_API_D3D9)
typedef fixed face_t;
#    define FACE_SEMANTICS VFACE
#else
typedef bool face_t;
#    define FACE_SEMANTICS SV_IsFrontFace
#endif  // defined(SHADER_API_GLCORE) || defined(SHADER_API_GLES) || defined(SHADER_API_D3D9)


/*!
 * @brief Input data type for vertex shader function, vertTwigl().
 * @see vertTwigl
 */
struct appdata_twigl
{
    //! Object space position of the vertex.
    float4 vertex : POSITION;
    // float4 tangent : TANGENT;
    //! Normal vector of the vertex.
    float3 normal : NORMAL;
    //! UV coordinate of the vertex.
    float4 texcoord : TEXCOORD0;
#if defined(LIGHTMAP_ON)
    //! Lightmap coordinate.
    float4 texcoord1 : TEXCOORD1;
#endif  // defined(LIGHTMAP_ON)
#if defined(DYNAMICLIGHTMAP_ON)
    //! Dynamic Lightmap coordinate.
    float4 texcoord2 : TEXCOORD2;
#endif  // defined(DYNAMICLIGHTMAP_ON)
    UNITY_VERTEX_INPUT_INSTANCE_ID
};


/*!
 * @brief Input of the vertex shader, vertTwiglShadowCaster().
 * @see vertTwiglShadowCaster
 */
struct appdata_twigl_shadowcaster
{
    //! Object space position of the vertex.
    float4 vertex : POSITION;
#if !defined(SHADOWS_CUBE) || defined(SHADOWS_CUBE_IN_DEPTH_TEX)
    //! Normal vector of the vertex.
    float3 normal : NORMAL;
#endif  // !defined(SHADOWS_CUBE) || defined(SHADOWS_CUBE_IN_DEPTH_TEX)
    //! instanceID for single pass instanced rendering.
    UNITY_VERTEX_INPUT_INSTANCE_ID
};


/*!
 * @brief Output of the vertex shader, vertTwigl()
 * and input of fragment shader, fragTwigl().
 * @see vertTwigl
 * @see fragTwigl
 */
struct v2f_twigl
{
    //! Clip space position of the vertex.
    float4 pos : SV_POSITION;
    //! UV coordinate.
    float2 uv : TEXCOORD0;
    //! World space position.
    float3 worldPos: TEXCOORD1;
    //! World space normal.
    float3 normal : TEXCOORD2;
#if defined(LIGHTMAP_ON)
#    if defined(DYNAMICLIGHTMAP_ON)
    //! Lightmap and Dynamic Lightmap coordinate.
    float4 lmap: TEXCOORD3;
#    else
    //! Lightmap coordinate.
    float2 lmap: TEXCOORD3;
#    endif  // defined(DYNAMICLIGHTMAP_ON)
#elif defined(UNITY_SHOULD_SAMPLE_SH)
    //! Ambient light color.
    half3 ambient: TEXCOORD3;
#endif  // UNITY_SHOULD_SAMPLE_SH
    //! Members abourt ligting coordinates, _LightCoord and _ShadowCoord.
    UNITY_LIGHTING_COORDS(4, 5)
    //! Member abourt fog coordinates, _fogCoord.
    UNITY_FOG_COORDS(6)
    //! Instance ID for single pass instanced rendering, instanceID.
    UNITY_VERTEX_INPUT_INSTANCE_ID
    //! Stereo target eye index for single pass instanced rendering, stereoTargetEyeIndex.
    UNITY_VERTEX_OUTPUT_STEREO
#if defined(USE_FACING_PARAMETER)
    face_t facing : FACE_SEMANTICS;
#endif  // defined(USE_FACING_PARAMETER)
};


/*!
 * @brief Output of the vertex shader, vertShadowCaster()
 * and input of fragment shader, fragShadowCaster().
 * @see vertShadowCaster
 * @see fragShadowCaster
 */
struct v2f_twigl_shadowcaster
{
    //! Shadow caster members.
    V2F_SHADOW_CASTER;
    //! instanceID for single pass instanced rendering.
    UNITY_VERTEX_INPUT_INSTANCE_ID
    //! stereoTargetEyeIndex for single pass instanced rendering.
    UNITY_VERTEX_OUTPUT_STEREO
};


bool isFacing(face_t face);


#if defined(UNITY_PASS_FORWARDADD) && defined(_FORWARDADD_OFF) || defined(_LIGHTING_UNLIT)
/*!
 * @brief Vertex shader function for ForwardAdd pass.
 *
 * This function outputs NaN vertice to skip fragment shader.
 *
 * @param [in] v  Input data.
 * @return Interpolation source data for fragment shader function, fragForwardAdd().
 * @see fragForwardAdd
 */
float4 vertTwigl() : SV_POSITION
{
    return asfloat(0x7fc00000).xxxx;  // qNaN
}
#else
/*!
 * @brief Vertex shader function.
 * @param [in] v  Input data.
 * @return Interpolation source data for fragment shader function, frag().
 * @see frag
 */
v2f_twigl vertTwigl(appdata_twigl v)
{
    v2f_twigl o;
    UNITY_INITIALIZE_OUTPUT(v2f_twigl, o);

    UNITY_SETUP_INSTANCE_ID(v);
    UNITY_TRANSFER_INSTANCE_ID(v, o);
    UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);

    o.pos = UnityObjectToClipPos(v.vertex);
    o.uv = v.texcoord;
    o.worldPos = mul(unity_ObjectToWorld, v.vertex).xyz;
    o.normal = UnityObjectToWorldNormal(v.normal);

    half4 ambientOrLightmapUV = half4(0.0, 0.0, 0.0, 0.0);
    // Static lightmaps
#    if defined(LIGHTMAP_ON)
    o.lmap.xy = v.texcoord1.xy * unity_LightmapST.xy + unity_LightmapST.zw;
#        if defined(DYNAMICLIGHTMAP_ON)
    o.lmap.zw = v.texcoord2.xy * unity_DynamicLightmapST.xy + unity_DynamicLightmapST.zw;
#        endif  // defined(DYNAMICLIGHTMAP_ON)
#    elif UNITY_SHOULD_SAMPLE_SH
#        if defined(VERTEXLIGHT_ON)
    // Approximated illumination from non-important point lights
    o.ambient.rgb = Shade4PointLights(
        unity_4LightPosX0,
        unity_4LightPosY0,
        unity_4LightPosZ0,
        unity_LightColor[0].rgb,
        unity_LightColor[1].rgb,
        unity_LightColor[2].rgb,
        unity_LightColor[3].rgb,
        unity_4LightAtten0,
        o.worldPos,
        v.normal);
#        endif  // defined(VERTEXLIGHT_ON)
    o.ambient.rgb = ShadeSHPerVertex(v.normal, o.ambient.rgb);
#    endif  // defined(LIGHTMAP_ON)

    UNITY_TRANSFER_LIGHTING(o, v.uv2);
    UNITY_TRANSFER_FOG(o, o.pos);

    return o;
}
#endif  // defined(UNITY_PASS_FORWARDADD) && defined(_FORWARDADD_OFF) || defined(_LIGHTING_UNLIT)


/*!
 * @brief Vertex shader function for ShadowCaster pass.
 * @param [in] v  Input data.
 * @return Interpolation source data for fragment shader function, fragTwiglShadowCaster().
 * @see fragTwiglShadowCaster
 */
v2f_twigl_shadowcaster vertTwiglShadowCaster(appdata_twigl_shadowcaster v)
{
    v2f_twigl_shadowcaster o;

    UNITY_SETUP_INSTANCE_ID(v);
    UNITY_TRANSFER_INSTANCE_ID(v, o);
    UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);

    TRANSFER_SHADOW_CASTER_NORMALOFFSET(o)

    return o;
}


/*!
 * @brief Identify whether surface is facing the camera or facing away from the camera.
 * @param [in] facing  Facing variable (fixed or bool).
 * @return True if surface facing the camera, otherwise false.
 */
bool isFacing(face_t facing)
{
#if defined(SHADER_API_GLCORE) || defined(SHADER_API_GLES) || defined(SHADER_API_D3D9)
    return facing >= 0.0;
#else
    return facing;
#endif  // defined(SHADER_API_GLCORE) || defined(SHADER_API_GLES) || defined(SHADER_API_D3D9)
}


#endif  // !defined(VERT_COMMON_INCLUDED)
