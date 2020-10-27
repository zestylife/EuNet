using System;
using System.Linq;
using System.Reflection;
using Microsoft.CodeAnalysis.CSharp;
using EuNet.Core;

namespace CodeGenerator
{
    public static class TypeExtensions
    {
        // Output: where T : new(), IEquatable<T> where U : struct
        public static string GetGenericConstraintClause(this Type type)
        {
            if (type.IsGenericType == false && type.ContainsGenericParameters == false)
                return "";

            var constraints = type.GetTypeInfo().GenericTypeParameters.Select(t => t.GetParameterGenericConstraintClause()).Where(c => c.Any()).ToList();
            return constraints.Any()
                ? " " + string.Join(" ", constraints)
                : "";
        }

        // Output: where T : new(), IEquatable<T> where U : struct
        public static string GetGenericConstraintClause(this MethodInfo method)
        {
            if (method.IsGenericMethod == false && method.ContainsGenericParameters == false)
                return "";

            var constraints = method.GetGenericArguments().Select(t => t.GetParameterGenericConstraintClause()).Where(c => c.Any()).ToList();
            return constraints.Any()
                ? " " + string.Join(" ", constraints)
                : "";
        }

        // Output: where T : new(), IEquatable<T>
        public static string GetParameterGenericConstraintClause(this Type type)
        {
            var gpa = type.GenericParameterAttributes;
            var specialContrains = new[]
            {
                gpa.HasFlag(GenericParameterAttributes.ReferenceTypeConstraint) ? "class" : "",
                gpa.HasFlag(GenericParameterAttributes.NotNullableValueTypeConstraint) ? "struct" : "",
                gpa.HasFlag(GenericParameterAttributes.DefaultConstructorConstraint) ? "new()" : "",
            };

            var contraints = specialContrains.Where(t => t.Length > 0)
                                             .Concat(type.GetGenericParameterConstraints().Select(t => t.GetSymbolDisplay(true)))
                                             .ToList();
            return (contraints.Count > 0)
                ? string.Format("where {0} : {1}", type.Name, string.Join(", ", contraints))
                : "";
        }

        // Output: "Literal"
        public static string GetValueLiteral(this object value)
        {
            var literal = SymbolDisplay.FormatPrimitive(value, true, false);

            if (value != null)
            {
                var type = value.GetType();
                if (type.IsEnum)
                    return $"({type.FullName}){literal}";
                if (type == typeof(float))
                    return literal + "f";
                if (type == typeof(double))
                    return literal + "d";
            }

            return literal;
        }

        public static string GetParameterDeclaration(this ParameterInfo pi, bool includeDefaultExpression)
        {
            var defaultValue = pi.HasDefaultValue ? GetValueLiteral(pi.DefaultValue) : "";
            return (pi.GetCustomAttribute<ParamArrayAttribute>() != null ? "params " : "") +
                   (pi.ParameterType.GetSymbolDisplay(true) + " " + pi.Name) +
                   (includeDefaultExpression && defaultValue != "" ? " = " + defaultValue : "");
        }

        public static bool HasNonTrivialDefaultValue(this ParameterInfo pi)
        {
            if (pi.HasDefaultValue == false || pi.DefaultValue == null)
                return false;

            if (pi.DefaultValue.GetType().IsValueType == false)
                return true;

            return pi.DefaultValue.Equals(Activator.CreateInstance(pi.ParameterType)) == false;
        }
    }
}
