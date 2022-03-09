namespace Nekres.Mistwar
{
    internal static class MathExtensions
    {
        public static double Normalize(this double val, double valMin, double valMax, double min, double max)
        {
            return (val - valMin) / (valMax - valMin) * (max - min) + min;
        }
        public static int Normalize(this int val, int valMin, int valMax, int min, int max)
        {
            return (val - valMin) / (valMax - valMin) * (max - min) + min;
        }
    }
}
