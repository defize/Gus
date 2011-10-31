namespace Defize.Gus
{
    public class GusTaskExecutionContext
    {
        public event System.EventHandler<GusTaskExecutionEventArgs> ExecutionEvent;

        public uint ExecutionStepCount { get; set; }

        public void RaiseExecutionEvent(GusTaskExecutionEventArgs e)
        {
            var handler = ExecutionEvent;
            if (handler != null)
            {
                handler(this, e);
            }
        }

        public void RaiseExecutionEvent(ExecutionEventType type, string message, uint executionStep)
        {
            var e = new GusTaskExecutionEventArgs(type, message, executionStep);
            RaiseExecutionEvent(e);
        }

        public void RaiseExecutionEvent(ExecutionEventType type, string message)
        {
            var e = new GusTaskExecutionEventArgs(type, message, 0);
            RaiseExecutionEvent(e);
        }

        public void RaiseExecutionEvent(string message, uint executionStep)
        {
            var e = new GusTaskExecutionEventArgs(ExecutionEventType.Default, message, executionStep);
            RaiseExecutionEvent(e);
        }

        public void RaiseExecutionEvent(string message)
        {
            var e = new GusTaskExecutionEventArgs(ExecutionEventType.Default, message, 0);
            RaiseExecutionEvent(e);
        }
    }
}
