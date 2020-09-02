using System;
namespace Relua {
    /// <summary>
    /// Base class for Relua exceptions. By catching this type, you can
    /// catch both types of exceptions (while tokenizing and while parsing).
    /// </summary>
    public abstract class ReluaException : Exception {
        public int Line;
        public int Column;

        protected ReluaException(string msg) : base(msg) { }
    }

    /// <summary>
    /// Exception thrown when the tokenizer runs into invalid syntax.
    /// </summary>
    public class TokenizerException : ReluaException {
        public TokenizerException(string msg, int line, int @char)
                        : base($"Failed tokenizing: {msg} [{line}:{@char}]") {
            Line = line;
            Column = @char;
        }

        public TokenizerException(string msg, Tokenizer.Region region)
                        : base($"Failed tokenizing: {msg} [{region.BoundsToString()}]") {
            Line = region.StartLine;
            Column = region.StartColumn;
        }
    }

    /// <summary>
    /// Exception thrown when the parser runs into invalid syntax.
    /// </summary>
    public class ParserException : ReluaException {
        public ParserException(string msg, Tokenizer.Region region)
                        : base($"Failed parsing: {msg} [{region.BoundsToString()}]") {
            Line = region.StartLine;
            Column = region.StartColumn;
        }

        public ParserException(string msg) : base(msg) { }
    }

    public class SyntaxErrorException : Exception
    {
        internal Token Token { get; private set; }

        /// <summary>
        /// Gets or sets a value indicating whether this exception was caused by premature stream termination (that is, unexpected EOF).
        /// This can be used in REPL interfaces to tell between unrecoverable errors and those which can be recovered by extra input.
        /// </summary>
        public bool IsPrematureStreamTermination { get; set; }

        internal SyntaxErrorException(Token t, string format, params object[] args)
            : base(string.Format(format, args))
        {
            Token = t;
        }

        internal SyntaxErrorException(Token t, string message)
            : base(message)
        {
            Token = t;
        }
    }

    public class InternalErrorException : Exception
    {
        internal InternalErrorException(string message)
            : base(message)
        {

        }

        internal InternalErrorException(string format, params object[] args)
            : base(string.Format(format, args))
        {

        }
    }
}
