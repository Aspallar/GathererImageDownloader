using System;
using System.Runtime.Serialization;

namespace GathererImageDownloader
{
    [Serializable]
    internal class NoSetCodeException : Exception
    {
        public NoSetCodeException()
        {
        }

        public NoSetCodeException(string message) : base(message)
        {
        }

        public NoSetCodeException(string message, Exception innerException) : base(message, innerException)
        {
        }

        protected NoSetCodeException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }
    }
}