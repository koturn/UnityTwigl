using System;


namespace Koturn.Twigl
{
    /// <summary>
    /// Provides some value converter methods.
    /// </summary>
    public static class ValueConverter
    {
        /// <summary>
        /// Convert a <see cref="float"/> value to <see cref="bool"/> value.
        /// </summary>
        /// <param name="floatValue">Source <see cref="float"/> value.</param>
        /// <returns>True if <paramref name="floatValue"/> is greater than 0.5, otherwise false.</returns>
        public static bool ToBool(float floatValue)
        {
            return floatValue >= 0.5f;
        }

        /// <summary>
        /// Convert a <see cref="bool"/> value to <see cref="float"/> value.
        /// </summary>
        /// <param name="boolValue">Source <see cref="bool"/> value.</param>
        /// <returns>1.0f if <paramref name="boolValue"/> is true, otherwise 0.0f.</returns>
        public static float ToFloat(bool boolValue)
        {
            return boolValue ? 1.0f : 0.0f;
        }

        /// <summary>
        /// Cast generic enum to <see cref="int"/>.
        /// </summary>
        /// <typeparam name="T">Type of enum.</typeparam>
        /// <param name="val">Enum value.</param>
        /// <returns><see cref="int"/> value converted from <typeparamref name="T"/>.</returns>
        public static int ToInt<T>(T val)
            where T : unmanaged, Enum
        {
            unsafe
            {
                return sizeof(T) == 8 ? (int)*(long*)&val
                    : sizeof(T) == 4 ? *(int*)&val
                    : sizeof(T) == 2 ? (int)*(short*)&val
                    : (int)*(byte*)&val;
            }
        }
    }
}
