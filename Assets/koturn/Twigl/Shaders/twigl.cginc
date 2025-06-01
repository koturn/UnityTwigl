#ifndef TWIGL_INCLUDED
#define TWIGL_INCLUDED


static const float F4 = 0.309016994374947451;
static const float PI = 3.141592653589793;
static const float PI2 = PI * 2.0;


float mod289(float x);
float2 mod289(float2 x);
float3 mod289(float3 x);
float4 mod289(float4 x);
float permute(float x);
float3 permute(float3 x);
float4 permute(float4 x);
float taylorInvSqrt(float r);
float4 taylorInvSqrt(float4 r);
float snoise2D(float2 v);
float snoise3D(float3 v);
float4 grad4(float j, float4 ip);
float snoise4D(float4 v);
float fsnoise(float2 c);
float fsnoiseDigits(float2 c);
float3 hsv(float h, float s, float v);
float2x2 rotate2D(float r);
float3x3 rotate3D(float angle, float3 axis);


float mod289(float x)
{
    return x - floor(x * (1.0 / 289.0)) * 289.0;
}

float2 mod289(float2 x)
{
    return x - floor(x * (1.0 / 289.0)) * 289.0;
}

float3 mod289(float3 x)
{
    return x - floor(x * (1.0 / 289.0)) * 289.0;
}

float4 mod289(float4 x)
{
    return x - floor(x * (1.0 / 289.0)) * 289.0;
}

float permute(float x)
{
    return mod289((x * 34.0 + 1.0) * x);
}

float3 permute(float3 x)
{
    return mod289((x * 34.0 + 1.0) * x);
}

float4 permute(float4 x)
{
    return mod289((x * 34.0 + 1.0) * x);
}

float taylorInvSqrt(float r)
{
    return 1.79284291400159 - 0.85373472095314 * r;
}

float4 taylorInvSqrt(float4 r)
{
    return 1.79284291400159 - 0.85373472095314 * r;
}

float snoise2D(float2 v)
{
    static const float CX = (3.0 - sqrt(3.0)) / 6.0;
    static const float4 C = float4(CX, 0.5 * sqrt(3.0) - 0.5, 2.0 * CX - 1.0, 1.0 / 41.0);

    // First corner
    float2 i = floor(v + dot(v, C.yy));
    float2 x0 = v - i + dot(i, C.xx);

    // Other corners
    float2 i1;
    // i1.x = step( x0.y, x0.x ); // x0.x > x0.y ? 1.0 : 0.0
    // i1.y = 1.0 - i1.x;
    i1 = (x0.x > x0.y) ? float2(1.0, 0.0) : float2(0.0, 1.0);
    // x0 = x0 - 0.0 + 0.0 * C.xx ;
    // x1 = x0 - i1 + 1.0 * C.xx ;
    // x2 = x0 - 1.0 + 2.0 * C.xx ;
    float4 x12 = x0.xyxy + C.xxzz;
    x12.xy -= i1;

    // Permutations
    i = mod289(i); // Avoid truncation effects in permutation
    float3 p = permute(permute(i.y + float3(0.0, i1.y, 1.0)) + i.x + float3(0.0, i1.x, 1.0));
    float3 m = max(0.5 - float3(dot(x0, x0), dot(x12.xy, x12.xy), dot(x12.zw, x12.zw)), 0.0);
    m = m * m;
    m = m * m;

    // Gradients: 41 points uniformly over a line, mapped onto a diamond.
    // The ring size 17*17 = 289 is close to a multiple of 41 (41*7 = 287)

    float3 x = 2.0 * frac(p * C.www) - 1.0;
    float3 h = abs(x) - 0.5;
    float3 ox = floor(x + 0.5);
    float3 a0 = x - ox;

    // Normalise gradients implicitly by scaling m
    // Approximation of: m *= inversesqrt( a0*a0 + h*h );
    m *= 1.79284291400159 - 0.85373472095314 * (a0 * a0 + h * h);

    // Compute final noise value at P
    float3 g;
    g.x = a0.x * x0.x + h.x * x0.y;
    g.yz = a0.yz * x12.xz + h.yz * x12.yw;
    return 130.0 * dot(m, g);
}

float snoise3D(float3 v)
{
    static const float2 C = float2(1.0 / 6.0, 1.0 / 3.0);
    static const float4 D = float4(0.0, 0.5, 1.0, 2.0);

    // First corner
    float3 i = floor(v + dot(v, C.yyy));
    float3 x0 = v - i + dot(i, C.xxx);

    // Other corners
    float3 g = step(x0.yzx, x0.xyz);
    float3 l = 1.0 - g;
    float3 i1 = min(g.xyz, l.zxy);
    float3 i2 = max(g.xyz, l.zxy);

    //   x0 = x0 - 0.0 + 0.0 * C.xxx;
    //   x1 = x0 - i1  + 1.0 * C.xxx;
    //   x2 = x0 - i2  + 2.0 * C.xxx;
    //   x3 = x0 - 1.0 + 3.0 * C.xxx;
    float3 x1 = x0 - i1 + C.xxx;
    float3 x2 = x0 - i2 + C.yyy; // 2.0*C.x = 1/3 = C.y
    float3 x3 = x0 - D.yyy; // -1.0+3.0*C.x = -0.5 = -D.y

    // Permutations
    i = mod289(i);
    float4 p = permute(
        permute(
            permute(
                i.z + float4(0.0, i1.z, i2.z, 1.0))
            + i.y + float4(0.0, i1.y, i2.y, 1.0))
        + i.x + float4(0.0, i1.x, i2.x, 1.0));

    // Gradients: 7x7 points over a square, mapped onto an octahedron.
    // The ring size 17*17 = 289 is close to a multiple of 49 (49*6 = 294)
    float n_ = 1.0 / 7.0;
    float3 ns = n_ * D.wyz - D.xzx;

    float4 j = p - 49.0 * floor(p * ns.z * ns.z); //  mod(p,7*7)

    float4 x_ = floor(j * ns.z);
    float4 y_ = floor(j - 7.0 * x_); // mod(j,N)

    float4 x = x_ * ns.x + ns.yyyy;
    float4 y = y_ * ns.x + ns.yyyy;
    float4 h = 1.0 - abs(x) - abs(y);

    float4 b0 = float4(x.xy, y.xy);
    float4 b1 = float4(x.zw, y.zw);

    // float4 s0 = float4(lessThan(b0,0.0))*2.0 - 1.0;
    // float4 s1 = float4(lessThan(b1,0.0))*2.0 - 1.0;
    float4 s0 = floor(b0) * 2.0 + 1.0;
    float4 s1 = floor(b1) * 2.0 + 1.0;
    float4 sh = -step(h, (0.0).xxxx);

    float4 a0 = b0.xzyw + s0.xzyw * sh.xxyy;
    float4 a1 = b1.xzyw + s1.xzyw * sh.zzww;

    float3 p0 = float3(a0.xy, h.x);
    float3 p1 = float3(a0.zw, h.y);
    float3 p2 = float3(a1.xy, h.z);
    float3 p3 = float3(a1.zw, h.w);

    // Normalise gradients
    float4 norm = taylorInvSqrt(float4(dot(p0, p0), dot(p1, p1), dot(p2, p2), dot(p3, p3)));
    p0 *= norm.x;
    p1 *= norm.y;
    p2 *= norm.z;
    p3 *= norm.w;

    // Mix final noise value
    float4 m = max(0.6 - float4(dot(x0, x0), dot(x1, x1), dot(x2, x2), dot(x3, x3)), 0.0);
    m = m * m;
    return 42.0 * dot(m * m, float4(dot(p0, x0), dot(p1, x1), dot(p2, x2), dot(p3, x3)));
}

float4 grad4(float j, float4 ip)
{
    static const float4 ones = float4(1.0, 1.0, 1.0, -1.0);
    float4 p, s;

    p.xyz = floor(frac(j.xxx * ip.xyz) * 7.0) * ip.z - 1.0;
    p.w = 1.5 - dot(abs(p.xyz), ones.xyz);
    // s = float4(lessThan(p, float4(0.0)));
    s = float4(p < (0.0).xxxx);
    p.xyz = p.xyz + (s.xyz * 2.0 - 1.0) * s.www;

    return p;
}

float snoise4D(float4 v)
{
    static const float G4 = (5.0 - sqrt(5.0)) / 20.0;
    static const float4 C = float4(G4, 2.0 * G4, 3.0 * G4, 4.0 * G4 - 1.0);

    // First corner
    float4 i = floor(v + dot(v, F4.xxxx));
    float4 x0 = v - i + dot(i, C.xxxx);

    // Other corners

    // Rank sorting originally contributed by Bill Licea-Kane, AMD (formerly ATI)
    float4 i0;
    float3 isX = step(x0.yzw, x0.xxx);
    float3 isYZ = step(x0.zww, x0.yyz);
    //  i0.x = dot( isX, float3( 1.0 ) );
    i0.x = isX.x + isX.y + isX.z;
    i0.yzw = 1.0 - isX;
    //  i0.y += dot( isYZ.xy, float2( 1.0 ) );
    i0.y += isYZ.x + isYZ.y;
    i0.zw += 1.0 - isYZ.xy;
    i0.z += isYZ.z;
    i0.w += 1.0 - isYZ.z;

    // i0 now contains the unique values 0,1,2,3 in each channel
    float4 i3 = saturate(i0);
    float4 i2 = saturate(i0 - 1.0);
    float4 i1 = saturate(i0 - 2.0);

    //  x0 = x0 - 0.0 + 0.0 * C.xxxx
    //  x1 = x0 - i1  + 1.0 * C.xxxx
    //  x2 = x0 - i2  + 2.0 * C.xxxx
    //  x3 = x0 - i3  + 3.0 * C.xxxx
    //  x4 = x0 - 1.0 + 4.0 * C.xxxx
    float4 x1 = x0 - i1 + C.xxxx;
    float4 x2 = x0 - i2 + C.yyyy;
    float4 x3 = x0 - i3 + C.zzzz;
    float4 x4 = x0 + C.wwww;

    // Permutations
    i = mod289(i);
    float j0 = permute(
        permute(
            permute(
                permute(i.w) + i.z)
            + i.y)
        + i.x);
    float4 j1 = permute(
        permute(
            permute(
                permute(
                    i.w + float4(i1.w, i2.w, i3.w, 1.0))
                + i.z + float4(i1.z, i2.z, i3.z, 1.0))
            + i.y + float4(i1.y, i2.y, i3.y, 1.0))
        + i.x + float4(i1.x, i2.x, i3.x, 1.0));

    // Gradients: 7x7x6 points over a cube, mapped onto a 4-cross polytope
    // 7*7*6 = 294, which is close to the ring size 17*17 = 289.
    float4 ip = float4(1.0 / 294.0, 1.0 / 49.0, 1.0 / 7.0, 0.0);

    float4 p0 = grad4(j0, ip);
    float4 p1 = grad4(j1.x, ip);
    float4 p2 = grad4(j1.y, ip);
    float4 p3 = grad4(j1.z, ip);
    float4 p4 = grad4(j1.w, ip);

    // Normalise gradients
    float4 norm = taylorInvSqrt(float4(dot(p0, p0), dot(p1, p1), dot(p2, p2), dot(p3, p3)));
    p0 *= norm.x;
    p1 *= norm.y;
    p2 *= norm.z;
    p3 *= norm.w;
    p4 *= taylorInvSqrt(dot(p4, p4));

    // Mix contributions from the five corners
    float3 m0 = max(0.6 - float3(dot(x0, x0), dot(x1, x1), dot(x2, x2)), 0.0);
    float2 m1 = max(0.6 - float2(dot(x3, x3), dot(x4, x4)), 0.0);
    m0 = m0 * m0;
    m1 = m1 * m1;
    return 49.0 * (dot(m0 * m0, float3(dot(p0, x0), dot(p1, x1), dot(p2, x2))) + dot(m1 * m1, float2(dot(p3, x3), dot(p4, x4))));
}

float fsnoise(float2 c)
{
    return frac(sin(dot(c, float2(12.9898, 78.233))) * 43758.5453);
}

float fsnoiseDigits(float2 c)
{
    return frac(sin(dot(c, float2(0.129898, 0.78233))) * 437.585453);
}

float3 hsv(float h, float s, float v)
{
    static const float4 t = float4(1.0, 2.0 / 3.0, 1.0 / 3.0, 3.0);
    float3 p = abs(frac(h.xxx + t.xyz) * 6.0 - t.www);
    return v * lerp(t.xxx, saturate(p - t.xxx), s);
}

float2x2 rotate2D(float r)
{
    return float2x2(cos(r), sin(r), -sin(r), cos(r));
}

float3x3 rotate3D(float angle, float3 axis)
{
    float3 a = normalize(axis);
    float s = sin(angle);
    float c = cos(angle);
    float r = 1.0 - c;
    return float3x3(
        a.x * a.x * r + c,
        a.y * a.x * r + a.z * s,
        a.z * a.x * r - a.y * s,
        a.x * a.y * r - a.z * s,
        a.y * a.y * r + c,
        a.z * a.y * r + a.x * s,
        a.x * a.z * r + a.y * s,
        a.y * a.z * r - a.x * s,
        a.z * a.z * r + c);
}

float asinh(float x)
{
    return log(x + sqrt(x * x + 1.0));
}

float2 asinh(float2 x)
{
    return log(x + sqrt(x * x + (1.0).xx));
}

float3 asinh(float3 x)
{
    return log(x + sqrt(x * x + (1.0).xxx));
}

float4 asinh(float4 x)
{
    return log(x + sqrt(x * x + (1.0).xxxx));
}


float acosh(float x)
{
    return log(x + sqrt(x * x - 1.0));
}

float2 acosh(float2 x)
{
    return log(x + sqrt(x * x - (1.0).xx));
}

float3 acosh(float3 x)
{
    return log(x + sqrt(x * x - (1.0).xxx));
}

float4 acosh(float4 x)
{
    return log(x + sqrt(x * x - (1.0).xxxx));
}


float atanh(float x)
{
    return 0.5 * log((1.0 + x) / (1.0 - x));
}

float2 atanh(float2 x)
{
    return 0.5 * log(((1.0).xx + x) / ((1.0).xx - x));
}

float3 atanh(float3 x)
{
    return 0.5 * log(((1.0).xxx + x) / ((1.0).xxx - x));
}

float4 atanh(float4 x)
{
    return 0.5 * log(((1.0).xxxx + x) / ((1.0).xxxx - x));
}

typedef float2 vec2;
typedef float3 vec3;
typedef float4 vec4;
typedef float2x2 mat2;
typedef float3x3 mat3;
typedef float4x4 mat4;

#define dFdx(v) ddx(v)
#define dFdy(v) ddy(v)
#define fract(x) frac(x)
#define mix(x, y, a) lerp((x), (y), (a))
#define mod(x, y) ((x) - (y) * floor((x) / (y)))
#define atan(x, y) atan2((x), (y))


#endif  // TWIGL_INCLUDED
