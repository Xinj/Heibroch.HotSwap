using System.IO;
using System.Linq;

namespace Heibroch.HotSwapper
{
    internal static class HotSwapExtensions
    {
        public static string ToInstance1Directory(this string instanceRoot) => Path.Combine(instanceRoot, "Instance1");
        public static string ToInstance2Directory(this string instanceRoot) => Path.Combine(instanceRoot, "Instance2");

        public static string ToInstanceDirectory(this string instanceRoot, int instance) => Path.Combine(instanceRoot, instance == 1 ? "Instance1" : "Instance2");
        
        public static bool IsInstance1Directory(this string instanceDirectory) => instanceDirectory.EndsWith('1');
        public static bool IsInstance2Directory(this string instanceDirectory) => instanceDirectory.EndsWith('2');

        public static bool IsAny<T>(this T val, params T[] values) => values.Contains(val);
    }
}
