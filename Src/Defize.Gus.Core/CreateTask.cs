namespace Defize.Gus
{
    using System;
    using System.IO;

    public class CreateTask
    {
        public bool Execute(CreateTaskConfiguration configuration, GusTaskExecutionContext context)
        {
            try
            {
                var path = (configuration.Name.ToLower().EndsWith(".sql")) ? configuration.Name : configuration.Name + ".sql";
                path = Path.GetFullPath(path);

                var name = Path.GetFileName(path);
                var directory = Path.GetDirectoryName(path);

                var timestamp = DateTime.Now.ToString("yyyyMMdd-HHmmss");
                var newFileName = string.Format("{0}-{1}", timestamp, name);

                var newPath = Path.Combine(directory, newFileName);

                using (var sw = File.CreateText(newPath))
                {
                    sw.WriteLine("//");
                    sw.WriteLine("// Created by Defize Gus.");
                    sw.WriteLine("//");
                }
            }
            catch (Exception ex)
            {
                context.RaiseExecutionEvent(ExecutionEventType.Error, string.Format("Failed to create script file: {0}" , ex.Message));
                return false;
            }

            return true;
        }
    }
}
