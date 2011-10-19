namespace Defize.Gus
{
    public class GusTaskBase : IGusTask
    {
        public event System.EventHandler<GusTaskExecutionEventArgs> ExecutionEvent;

        protected void OnExecutionEvent(GusTaskExecutionEventArgs e)
        {
            var handler = ExecutionEvent;
            if (handler != null)
            {
                handler(this, e);
            }
        }

        protected void OnExecutionEvent(ExecutionEventType type, string message, uint executionStep)
        {
            var e = new GusTaskExecutionEventArgs(type, message, executionStep);
            OnExecutionEvent(e);
        }

        protected void OnExecutionEvent(ExecutionEventType type, string message)
        {
            var e = new GusTaskExecutionEventArgs(type, message, 0);
            OnExecutionEvent(e);
        }

        protected void OnExecutionEvent(string message, uint executionStep)
        {
            var e = new GusTaskExecutionEventArgs(ExecutionEventType.Default, message, executionStep);
            OnExecutionEvent(e);
        }

        protected void OnExecutionEvent(string message)
        {
            var e = new GusTaskExecutionEventArgs(ExecutionEventType.Default, message, 0);
            OnExecutionEvent(e);
        }

        public uint ExecutionStepCount { get; protected set; }
    }
}
