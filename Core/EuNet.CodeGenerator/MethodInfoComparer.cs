using System;
using System.Collections.Generic;
using System.Reflection;

namespace CodeGenerator
{
    public class MethodInfoComparer : IComparer<MethodInfo>
    {
        public int Compare(MethodInfo x, MethodInfo y)
        {
            var ret = string.Compare(x.Name, y.Name, StringComparison.Ordinal);
            if (ret != 0)
                return ret;

            var xp = x.GetParameters();
            var yp = y.GetParameters();
            for (var i = 0; i < Math.Min(xp.Length, yp.Length); i++)
            {
                var ret2 = string.Compare(xp[i].Name, yp[i].Name, StringComparison.Ordinal);
                if (ret2 != 0)
                    return ret2;

                var ret3 = string.Compare(xp[i].ParameterType.FullName ?? xp[i].ParameterType.Name,
                                          yp[i].ParameterType.FullName ?? yp[i].ParameterType.Name,
                                          StringComparison.Ordinal);
                if (ret3 != 0)
                    return ret3;
            }

            return xp.Length - yp.Length;
        }
    }
}

