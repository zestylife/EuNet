using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using CodeWriter;
using EuNet.Core;

namespace CodeGenerator
{
    public class RpcCodeGenerator
    {
        public Options Options { get; set; }

        public void GenerateCode(Type type, Dictionary<int, string> rpcEnumMap, CodeWriter.CodeWriter w)
        {
            Console.WriteLine("GenerateCode: " + type.GetSymbolDisplay(true));

            w._($"#region {type.GetSymbolDisplay(true)}");
            w._();

            var namespaceHandle = (string.IsNullOrEmpty(type.Namespace) == false)
                ? w.B($"namespace {type.Namespace}")
                : null;

            var baseTypes = type.GetInterfaces().Where(t => t.FullName != "EuNet.Rpc.IRpc").ToArray();
            var infos = new List<Tuple<Type, List<Tuple<MethodInfo, Tuple<string, string>>>>>();
            foreach (var t in new[] { type }.Concat(baseTypes))
            {
                var methods = GetInvokableMethods(t);
                var method2PayloadTypeNameMap = GetPayloadTypeNames(t, methods);
                infos.Add(Tuple.Create(t, GetPayloadTypeNames(t, methods)));
            }

            // Generate all

            GenerateRpcCode(type, w, baseTypes, infos.ToArray(), rpcEnumMap);
            GenerateRpcServiceAbstractCode(type, w, infos.First().Item2, infos.ToArray());
            GenerateRpcServiceSessionCode(type, w, infos.First().Item2, infos.ToArray());

            namespaceHandle?.Dispose();

            w._();
            w._($"#endregion");
        }

        private void GenerateRpcCode(
            Type type, CodeWriter.CodeWriter w, Type[] baseTypes,
            Tuple<Type, List<Tuple<MethodInfo, Tuple<string, string>>>>[] typeInfos,
            Dictionary<int, string> rpcEnumMap)
        {
            // NoReply Interface

            var baseNoReplys = baseTypes.Select(t => Utility.GetNoReplyInterfaceName(t));
            var baseNoReplysInherit = baseNoReplys.Any() ? " : " + string.Join(", ", baseNoReplys) : "";
            using (w.B($"public interface {Utility.GetNoReplyInterfaceName(type)}{type.GetGenericParameters()}{baseNoReplysInherit}{type.GetGenericConstraintClause()}"))
            {
                foreach (var m in typeInfos.First().Item2)
                {
                    var method = m.Item1;
                    var parameters = method.GetParameters();
                    var paramStr = string.Join(", ", parameters.Select(p => p.GetParameterDeclaration(true)));
                    w._($"void {method.Name}{method.GetGenericParameters()}({paramStr}){method.GetGenericConstraintClause()};");
                }
            }
            
            // Rpc Enum
            using (w.B($"public enum {Utility.GetInterfaceEnumName(type)} : int"))
            {
                foreach (var m in typeInfos.First().Item2)
                {
                    var method = m.Item1;
                    /*var parameters = method.GetParameters();
                    var paramStr = string.Join(", ", parameters.Select(p => p.GetParameterDeclaration(true)));
                    var returnType = method.ReturnType.GenericTypeArguments.FirstOrDefault();
                    var returnTaskType = (returnType != null) ? $"Task<{returnType.GetSymbolDisplay(true)}>" : "Task";*/
                    var methodHash = Utility.GetMethodHashCode(Utility.GetInterfaceEnumName(type) + "." + method.Name);

                    w._($"{method.Name} = {methodHash},");

                    rpcEnumMap.Add(methodHash, $"{type.GetPureName()}.{method.Name}");
                }
            }

            // Rpc
            var rpcClassName = Utility.GetRpcClassName(type);
            var rpcClassGenericName = rpcClassName + type.GetGenericParameters();
            var noReplyInterfaceName = Utility.GetNoReplyInterfaceName(type);
            var noReplyInterfaceGenericName = noReplyInterfaceName + type.GetGenericParameters();

            using (w.B($"public class {rpcClassName}{type.GetGenericParameters()} : RpcRequester, {type.GetSymbolDisplay()}, {noReplyInterfaceName}{type.GetGenericParameters()}{type.GetGenericConstraintClause()}"))
            {
                // InterfaceType property

                w._($"public override Type InterfaceType => typeof({type.GetSymbolDisplay()});");
                w._();

                // Constructors

                using (w.B($"public {rpcClassName}() : base(null)"))
                {
                    w._("DeliveryMethod = DeliveryMethod.Tcp;");
                }

                using (w.B($"public {rpcClassName}(ISession target) : base(target)"))
                {
                    w._("DeliveryMethod = DeliveryMethod.Tcp;");
                }

                using (w.B($"public {rpcClassName}(ISession target, IRequestWaiter requestWaiter, TimeSpan? timeout = null) : base(target, requestWaiter, timeout)"))
                {
                    w._("DeliveryMethod = DeliveryMethod.Tcp;");
                }

                // With Helpers

                using (w.B($"public {noReplyInterfaceGenericName} WithNoReply()"))
                {
                    w._("return this;");
                }

                using (w.B($"public {rpcClassGenericName} WithRequestWaiter(IRequestWaiter requestWaiter)"))
                {
                    w._($"return new {rpcClassGenericName}(Target, requestWaiter, Timeout);");
                }

                using (w.B($"public {rpcClassGenericName} WithTimeout(TimeSpan? timeout)"))
                {
                    w._($"return new {rpcClassGenericName}(Target, RequestWaiter, timeout);");
                }

                // IInterface message methods

                foreach (var t in typeInfos)
                {
                    var payloadTableClassName = Utility.GetRpcServiceClassName(t.Item1) + type.GetGenericParameters();

                    foreach (var m in t.Item2)
                    {
                        var method = m.Item1;
                        var payloadTypes = m.Item2;
                        var parameters = method.GetParameters();

                        var parameterTypeNames = string.Join(", ", parameters.Select(p => p.GetParameterDeclaration(true)));
                        var parameterInits = string.Join(", ", parameters.Select(Utility.GetParameterAssignment));
                        var returnType = method.ReturnType.GenericTypeArguments.FirstOrDefault();

                        // Request Methods

                        var returnTaskType = (returnType != null) ? $"Task<{returnType.GetSymbolDisplay(true)}>" : "Task";
                        var prototype = $"public async {returnTaskType} {method.Name}{method.GetGenericParameters()}({parameterTypeNames}){method.GetGenericConstraintClause()}";
                        using (w.B(prototype))
                        {
                            w._("var _writer_ = NetPool.DataWriterPool.Alloc();");
                            w._("try");
                            using (w.i("{", "}"))
                            {
                                w._($"_writer_.Write((int){Utility.GetInterfaceEnumName(type)}.{method.Name});");

                                foreach (var param in parameters)
                                {
                                    w._($"{Utility.GetWriteMethod(param)}");
                                }

                                if (returnType != null)
                                {
                                    using (w.B($"using(var _reader_ = await SendRequestAndReceive(_writer_))"))
                                    {
                                        w._($"return {Utility.GetReaderMethod(returnType)}");
                                    }
                                }
                                else
                                {
                                    w._($"await SendRequestAndWait(_writer_);");
                                }
                            }
                            w._("finally");
                            using (w.i("{", "}"))
                            {
                                w._($"NetPool.DataWriterPool.Free(_writer_);");
                            }
                        }
                    }
                }

                // IInterface_NoReply message methods

                foreach (var t in typeInfos)
                {
                    var interfaceName = Utility.GetNoReplyInterfaceName(t.Item1);
                    var interfaceGenericName = interfaceName + t.Item1.GetGenericParameters();

                    var payloadTableClassName = Utility.GetRpcServiceClassName(t.Item1) + type.GetGenericParameters();

                    foreach (var m in t.Item2)
                    {
                        var method = m.Item1;
                        var payloadTypes = m.Item2;
                        var parameters = method.GetParameters();

                        var parameterTypeNames = string.Join(", ", parameters.Select(p => p.GetParameterDeclaration(false)));
                        var parameterInits = string.Join(", ", parameters.Select(Utility.GetParameterAssignment));

                        // Request Methods

                        using (w.B($"void {interfaceGenericName}.{method.Name}{method.GetGenericParameters()}({parameterTypeNames})"))
                        {
                            w._("var _writer_ = NetPool.DataWriterPool.Alloc();");
                            w._("try");
                            using (w.i("{", "}"))
                            {
                                w._($"_writer_.Write((int){Utility.GetInterfaceEnumName(type)}.{method.Name});");

                                foreach (var param in parameters)
                                {
                                    w._($"{Utility.GetWriteMethod(param)}");
                                }

                                w._("SendRequest(_writer_);");
                            }
                            w._("finally");
                            using (w.i("{", "}"))
                            {
                                w._($"NetPool.DataWriterPool.Free(_writer_);");
                            }
                        }
                    }
                }
            }
        }

        private void GenerateRpcServiceAbstractCode(
            Type type,
            CodeWriter.CodeWriter w,
            List<Tuple<MethodInfo, Tuple<string, string>>> method2PayloadTypeNames,
            Tuple<Type, List<Tuple<MethodInfo, Tuple<string, string>>>>[] typeInfos)
        {
            using (w.B($"public abstract class {Utility.GetRpcServiceClassName(type)}{type.GetGenericParameters()} : IRpcInvokable, {type.GetSymbolDisplay()}"))
            {
                foreach (var m in typeInfos.First().Item2)
                {
                    var method = m.Item1;
                    var parameters = method.GetParameters();
                    var paramStr = string.Join(", ", parameters.Select(p => p.GetParameterDeclaration(true)));
                    var returnType = method.ReturnType.GenericTypeArguments.FirstOrDefault();
                    var returnTaskType = (returnType != null) ? $"Task<{returnType.GetSymbolDisplay(true)}>" : "Task";

                    w._($"public abstract {returnTaskType} {method.Name}{method.GetGenericParameters()}({paramStr}){method.GetGenericConstraintClause()};");
                }

                using (w.B($"public async Task<bool> Invoke(object _target_, NetDataReader _reader_, NetDataWriter _writer_)"))
                {
                    w._("ISession session = _target_ as ISession;");

                    w._($"var typeEnum = ({Utility.GetInterfaceEnumName(type)})_reader_.ReadInt32();");
                    using (w.B($"switch(typeEnum)"))
                    {
                        foreach (var m in typeInfos.First().Item2)
                        {
                            var method = m.Item1;
                            var parameters = method.GetParameters();
                            var paramStr = string.Join(", ", parameters.Select(p => p.GetParameterDeclaration(true)));
                            var returnType = method.ReturnType.GenericTypeArguments.FirstOrDefault();
                            var returnTaskType = (returnType != null) ? $"Task<{returnType.GetSymbolDisplay(true)}>" : "Task";

                            w._($"case {Utility.GetInterfaceEnumName(type)}.{method.Name}:");
                            using (w.i())
                            {
                                using (w.i("{", "}"))
                                {
                                    var parameterNames = string.Join(", ", method.GetParameters().Select(p => p.Name));

                                    foreach (var param in method.GetParameters())
                                    {
                                        w._($"{Utility.GetReaderMethod(param)}");
                                    }

                                    if (returnType != null)
                                    {
                                        w._($"var _result_ = await {method.Name}{method.GetGenericParameters()}({parameterNames});");
                                        w._($"{Utility.GetWriteMethod(returnType, "_result_")}");
                                    }
                                    else
                                    {
                                        w._($"await {method.Name}{method.GetGenericParameters()}({parameterNames});");
                                    }
                                }

                                w._($"break;");
                            }
                        }

                        w._($"default: return false;");
                    }
                    w._($"return true;");
                }
            }
        }

        private void GenerateRpcServiceSessionCode(
            Type type,
            CodeWriter.CodeWriter w,
            List<Tuple<MethodInfo, Tuple<string, string>>> method2PayloadTypeNames,
            Tuple<Type, List<Tuple<MethodInfo, Tuple<string, string>>>>[] typeInfos)
        {
            using (w.B($"public class {Utility.GetRpcServiceSessionClassName(type)}{type.GetGenericParameters()} : IRpcInvokable"))
            {
                using (w.B($"public async Task<bool> Invoke(object _target_, NetDataReader _reader_, NetDataWriter _writer_)"))
                {
                    w._("ISession session = _target_ as ISession;");
                    w._($"var typeEnum = ({Utility.GetInterfaceEnumName(type)})_reader_.ReadInt32();");
                    using (w.B($"switch(typeEnum)"))
                    {
                        foreach (var m in typeInfos.First().Item2)
                        {
                            var method = m.Item1;
                            var parameters = method.GetParameters();
                            var paramStr = string.Join(", ", parameters.Select(p => p.GetParameterDeclaration(true)));
                            var returnType = method.ReturnType.GenericTypeArguments.FirstOrDefault();
                            var returnTaskType = (returnType != null) ? $"Task<{returnType.GetSymbolDisplay(true)}>" : "Task";

                            w._($"case {Utility.GetInterfaceEnumName(type)}.{method.Name}:");
                            using (w.i())
                            {
                                using (w.i("{", "}"))
                                {
                                    var parameterNames = string.Join(", ", method.GetParameters().Select(p => p.Name));

                                    foreach (var param in method.GetParameters())
                                    {
                                        w._($"{Utility.GetReaderMethod(param)}");
                                    }

                                    if (returnType != null)
                                    {
                                        w._($"var _result_ = await (session as {type.GetSymbolDisplay()}).{method.Name}{method.GetGenericParameters()}({parameterNames});");
                                        w._($"{Utility.GetWriteMethod(returnType, "_result_")}");
                                    }
                                    else
                                    {
                                        w._($"await (session as {type.GetSymbolDisplay()}).{method.Name}{method.GetGenericParameters()}({parameterNames});");
                                    }
                                }

                                w._($"break;");
                            }
                        }

                        w._($"default: return false;");
                    }
                    w._($"return true;");
                }
            }
        }

        private MethodInfo[] GetInvokableMethods(Type type)
        {
            var methods = type.GetMethods();
            if (methods.Any(m => m.ReturnType.Name.StartsWith("Task") == false))
                throw new Exception(string.Format("All methods of {0} should return Task or Task<T>", type.FullName));
            return methods.OrderBy(m => m, new MethodInfoComparer()).ToArray();
        }

        private List<Tuple<MethodInfo, Tuple<string, string>>> GetPayloadTypeNames(Type type, MethodInfo[] methods)
        {
            var method2PayloadTypeNames = new List<Tuple<MethodInfo, Tuple<string, string>>>();
            for (var i = 0; i < methods.Length; i++)
            {
                var method = methods[i];
                var returnType = method.ReturnType.GenericTypeArguments.FirstOrDefault();
                var ordinal = methods.Take(i).Count(m => m.Name == method.Name) + 1;
                var ordinalStr = (ordinal <= 1) ? "" : string.Format("_{0}", ordinal);

                method2PayloadTypeNames.Add(Tuple.Create(method, Tuple.Create(
                    string.Format("{0}{1}_Invoke", method.Name, ordinalStr),
                    returnType != null
                        ? string.Format("{0}{1}_Return", method.Name, ordinalStr)
                        : "")));
            }
            return method2PayloadTypeNames;
        }
    }
}
