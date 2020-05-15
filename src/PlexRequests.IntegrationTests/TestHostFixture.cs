using System;
using System.Diagnostics;
using System.Net;
using System.Net.Sockets;
using System.Threading;

namespace PlexRequests.IntegrationTests
{
    public class TestHostFixture : IDisposable
    {
        public static string BaseUrl { get; private set; }

        private static string _functionDirectory;

        private static Process _host;

        private static readonly int CurrentPort = GetFreeTcpPort();

#if DEBUG
        private const string BuildConfiguration = "Debug";
#else
        private const string BuildConfiguration = "Release";
#endif

        public TestHostFixture()
        {
            Console.WriteLine("Initialising integration test content");

            SetIntegrationContextLocations();
            Environment.SetEnvironmentVariable("AZURE_FUNCTIONS_ENVIRONMENT", "Development");

            var workingDirectory = $"{System.IO.Directory.GetCurrentDirectory()}/{_functionDirectory}";
            var hostProcess = new ProcessStartInfo
            {
                FileName = "func",
                Arguments = $"host start --port {CurrentPort}",
                WorkingDirectory = workingDirectory,
                UseShellExecute = true,
                RedirectStandardOutput = false
            };

            StartHost(hostProcess);
        }
        
        public void Dispose()
        {
            Console.WriteLine("Disposing of integration test context");

            _host.CloseMainWindow();
        }

        private static void StartHost(ProcessStartInfo hostProcess)
        {
            Console.WriteLine($"Starting TestHost on port: {CurrentPort}");
            _host = Process.Start(hostProcess);

            if (_host == null)
            {
                throw new Exception("TestHost failed to start");
            }

            _host.OutputDataReceived += HandleStdOut;
            _host.ErrorDataReceived += HandleStdOut;

            //Not sure of a better way to do this. Wait for the function app to start up and load all functions
            Thread.Sleep(5000);

            if (_host.HasExited)
            {
                throw new Exception(_host.StandardOutput.ReadToEnd());
            }
        }

        private static void SetIntegrationContextLocations()
        {
            BaseUrl = $"http://localhost:{CurrentPort}";
            _functionDirectory = $"./../../../../../src/PlexRequests.Functions/bin/{BuildConfiguration}/netcoreapp3.0";
        }

        private static void HandleStdOut(object sendingProcess, DataReceivedEventArgs outLine)
        {
            Console.WriteLine(outLine.Data);
        }

        private static int GetFreeTcpPort()
        {
            var tcpListener = new TcpListener(IPAddress.Loopback, 0);
            tcpListener.Start();

            var port = ((IPEndPoint)tcpListener.LocalEndpoint).Port;
            tcpListener.Stop();

            return port;
        }
    }
}