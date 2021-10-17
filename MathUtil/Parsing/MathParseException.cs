using System;
using System.Runtime.Serialization;

namespace MathUtil.Parsing
{
    public class MathParseException : Exception
    {
        public MathParseException() { }
        public MathParseException(string message) : base(message) { }
        public MathParseException(string message, Exception innerException) : base(message, innerException) { }
        protected MathParseException(SerializationInfo info, StreamingContext context) : base(info, context) { }

        public int Offset { get; set; } = -1;
    }
}
