
namespace Pipeline.Infrastructure
{
    public static class FloatExtensions
    {
        public static bool IsNearZero(this float v, float epsilon = 0.00001F)
        {
            return v > -epsilon && v < epsilon;
        }
    }
}