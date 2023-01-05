using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Windows.Forms;

namespace Elevator
{
    static class Elevator
    {
        public static void Main()
        {
            try
            {
                // User config
                string relativeScriptPath = ".\\gui.ps1";
                // Set up variables
                int processExitCode = 60010;
                string currentAppFolder = AppDomain.CurrentDomain.BaseDirectory;
                string scriptPath = Path.Combine(currentAppFolder, relativeScriptPath);
                string powershellExePath = Path.Combine(Environment.GetEnvironmentVariable("WinDir"), "System32\\WindowsPowerShell\\v1.0\\PowerShell.exe");
                string powershellArgs = "-ExecutionPolicy Bypass -NoProfile -NoLogo -WindowStyle Hidden";
                List<string> commandLineArgs = new List<string>(Environment.GetCommandLineArgs());
                bool isRequireAdmin = true;

                // Trim ending & starting empty space from each element in the command-line
                commandLineArgs = commandLineArgs.ConvertAll(s => s.Trim());
                // Remove first command-line argument as this is always the executable name
                commandLineArgs.RemoveAt(0);

                // Define the command line arguments to pass to PowerShell
                powershellArgs = powershellArgs + " -Command & { & '" + scriptPath + "'";
                if (commandLineArgs.Count > 0)
                {
                    powershellArgs = powershellArgs + " " + string.Join(" ", commandLineArgs.ToArray());
                }
                powershellArgs = powershellArgs + "; Exit $LastExitCode }";

                // Verify if the script file exists
                if (!File.Exists(scriptPath))
                {
                    throw new Exception("A critical component is missing." + Environment.NewLine + Environment.NewLine + "Unable to find the Script file: " + scriptPath + "." + Environment.NewLine + Environment.NewLine + "Please ensure you have all of the required files available to start the installation.");
                }

                // Define PowerShell process
                WriteDebugMessage("PowerShell Path: " + powershellExePath);
                WriteDebugMessage("PowerShell Parameters: " + powershellArgs);
                ProcessStartInfo processStartInfo = new ProcessStartInfo
                {
                    FileName = powershellExePath,
                    Arguments = powershellArgs,
                    WorkingDirectory = Path.GetDirectoryName(powershellExePath),
                    WindowStyle = ProcessWindowStyle.Hidden,
                    UseShellExecute = true
                };
                // Set the RunAs flag if the XML specifically calls for Admin Rights and OS Vista or higher
                if (((isRequireAdmin) & (Environment.OSVersion.Version.Major >= 6)))
                {
                    processStartInfo.Verb = "runas";
                }

                // Start the PowerShell process and wait for completion
                processExitCode = 60011;
                Process process = new Process();
                try
                {
                    process.StartInfo = processStartInfo;
                    process.Start();
                    process.WaitForExit();
                    processExitCode = process.ExitCode;
                }
                catch (Exception)
                {
                    throw;
                }
                finally
                {
                    if ((process != null))
                    {
                        process.Dispose();
                    }
                }

                // Exit
                WriteDebugMessage("Exit Code: " + processExitCode);
                Environment.Exit(processExitCode);
            }
            catch (Exception ex)
            {
                WriteDebugMessage(ex.Message, true, MessageBoxIcon.Error);
                Environment.Exit(processExitCode);
            }
        }

        public static void WriteDebugMessage(string debugMessage = null, bool IsDisplayError = false, MessageBoxIcon MsgBoxStyle = MessageBoxIcon.Information)
        {
            // Output to the Console
            Console.WriteLine(debugMessage);

            // If we are to display an error message...
            IntPtr handle = Process.GetCurrentProcess().MainWindowHandle;
            if (IsDisplayError == true)
            {
                MessageBox.Show(new WindowWrapper(handle), debugMessage, Application.ProductName + " " + Application.ProductVersion, MessageBoxButtons.OK, (MessageBoxIcon)MsgBoxStyle, MessageBoxDefaultButton.Button1);
            }
        }

        public class WindowWrapper : System.Windows.Forms.IWin32Window
        {
            public WindowWrapper(IntPtr handle)
            {
                _hwnd = handle;
            }

            public IntPtr Handle
            {
                get { return _hwnd; }
            }

            private IntPtr _hwnd;
        }

        public static int processExitCode { get; set; }
    }
}
