using System;

namespace EuNet.Core
{
    public static class NetDataSerializer
    {
        public static NetDataSerializerOptions DefaultOptions { get; set; } = NetDataSerializerOptions.Standard;

        public static void Serialize<T>(NetDataWriter writer, T value, NetDataSerializerOptions options = null)
        {
            options = options ?? DefaultOptions;

            try
            {
                options.Resolver.GetFormatter<T>().Serialize(writer, value, options);
            }
            catch (TypeInitializationException ex)
            {
                var type = typeof(T);
                string exString = $"*** Ex) new {type.GetPureName()}Formatter{type.GetGenericParameters()}(); ***\n*** Please see https://docs.unity3d.com/kr/2020.2/Manual/ScriptingRestrictions.html ***";
                throw new NetDataSerializationException($"Failed to serialize {type.GetSymbolDisplay(true)} value.\n*** If you are using IL2PP, you need to specify the Generic Type. ***\n{exString}", ex);
            }
            catch (Exception ex)
            {
                throw new NetDataSerializationException($"Failed to serialize {typeof(T).GetSymbolDisplay(true)} value.\n*** If you haven't generated code, please do it! ***\n*** If you have generated it, please register with the following code. CustomResolver.Register(GeneratedResolver.Instance); ***", ex);
            }
        }

        public static T Deserialize<T>(NetDataReader reader, NetDataSerializerOptions options = null)
        {
            options = options ?? DefaultOptions;

            try
            {
                return options.Resolver.GetFormatter<T>().Deserialize(reader, options);
            }
            catch (TypeInitializationException ex)
            {
                var type = typeof(T);
                string exString = $"*** Ex) new {type.GetPureName()}Formatter{type.GetGenericParameters()}(); ***\n*** Please see https://docs.unity3d.com/kr/2020.2/Manual/ScriptingRestrictions.html ***";
                throw new NetDataSerializationException($"Failed to deserialize {type.GetSymbolDisplay(true)} value.\n*** If you are using IL2PP, you need to specify the Generic Type. ***\n{exString}", ex);
            }
            catch (Exception ex)
            {
                throw new NetDataSerializationException($"Failed to deserialize {typeof(T).GetSymbolDisplay(true)} value.\n*** If you haven't generated code, please do it! ***\n*** If you have generated it, please register with the following code. CustomResolver.Register(GeneratedResolver.Instance); ***", ex);
            }
        }
    }
}
