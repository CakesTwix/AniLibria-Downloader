using System;
using System.Diagnostics;
using System.Text;
using System.Threading.Tasks;

namespace Anilibria_Downloader.Utility
{
    class ffmpeg
    {
        public class ProcessAsync
        {

            private string _fileName;
            private string _arguments;

            public ProcessAsync(string fileName, string arguments)
            {
                _fileName = fileName;
                _arguments = arguments;
            }

            public async Task<int> Run(StringBuilder stdin = null)
            {

                // Initialise
                var cmd = new Process();
                cmd.StartInfo.FileName = _fileName;
                cmd.StartInfo.RedirectStandardInput = true;
                cmd.StartInfo.RedirectStandardOutput = true;
                cmd.StartInfo.CreateNoWindow = true;
                cmd.StartInfo.UseShellExecute = false;
                cmd.StartInfo.Arguments = _arguments;

                // Create a task that waits for the Process to finish
                var cmdExited = new CmdExitedTaskWrapper();
                cmd.EnableRaisingEvents = true;
                cmd.Exited += cmdExited.EventHandler;

                // Start process
                cmd.Start();

                // Pass any stdin if necessary
                if (stdin != null)
                {
                    await cmd.StandardInput.WriteAsync(stdin.ToString());
                    await cmd.StandardInput.FlushAsync();
                    cmd.StandardInput.Close();
                }

                // Wait for process to end and return stdout
                await cmdExited.Task;
                return cmd.ExitCode;

            }

            /// <remarks>
            /// We can't wait on a Process directly, so create a wrapper for a
            /// task that waits for the <see cref="Process.Exited"/> Event to be
            /// raised.
            /// </remarks>
            private class CmdExitedTaskWrapper
            {

                private TaskCompletionSource<bool> _tcs = new TaskCompletionSource<bool>();

                public void EventHandler(object sender, EventArgs e)
                {
                    _tcs.SetResult(true);
                }

                public Task Task => _tcs.Task;

            }

        }
    }
}
