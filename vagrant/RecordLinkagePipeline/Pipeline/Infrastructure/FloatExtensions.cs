using System.Runtime.CompilerServices;

namespace Pipeline.Infrastructure
{
    public static class FloatExtensions
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool IsNearZero(this float v)
        {
            return (v == 0F) && (v < 0F + float.Epsilon);
        }
    }
}