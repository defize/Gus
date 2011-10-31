namespace Defize.Gus
{
    using System;

    public interface IGusTask
    {
        event EventHandler<GusTaskExecutionEventArgs> ExecutionEvent;

        uint ExecutionStepCount { get; }
    }
}
