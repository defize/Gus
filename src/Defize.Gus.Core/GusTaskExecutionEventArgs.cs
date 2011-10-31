namespace Defize.Gus
{
    using System;

    public class GusTaskExecutionEventArgs : EventArgs
    {
        public GusTaskExecutionEventArgs(ExecutionEventType type, string message, uint executionStep)
        {
            Type = type;
            Message = message;
            ExecutionStep = executionStep;
        }

        public ExecutionEventType Type { get; private set; }
        public string Message { get; private set; }
        public uint ExecutionStep { get; private set; }
    }
}
