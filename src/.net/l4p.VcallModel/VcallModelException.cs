using System;

namespace l4p.VcallModel
{
    public abstract class VcallModelException : Exception
    {
        protected VcallModelException() { }
        protected VcallModelException(string message) : base(message) { }
        protected VcallModelException(string message, Exception inner) : base(message, inner) { }
    }
}