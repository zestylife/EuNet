using System;

namespace EuNet.Core
{
    public class NetDataSerializationException : Exception
    {
        public NetDataSerializationException(string msg)
            : base(msg)
        {

        }

        public NetDataSerializationException(string msg, Exception ex)
            : base(msg, ex)
        {

        }

    }
}
