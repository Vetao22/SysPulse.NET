using System;
using System.Diagnostics;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using TerminalGrid.NET;

namespace SysPulse.NET
{
    /// <summary>
    /// High-performance system resource monitor engine displaying real-time CPU, RAM, and Process metrics.
    /// </summary>
    public sealed class SysPulseEngine
    {
        private readonly DiagnosticGridRenderer _gridRenderer = new DiagnosticGridRenderer();
        private readonly Process _currentProcess = Process.GetCurrentProcess();
        private TimeSpan _prevCpuTime;
        private DateTime _prevTime;

        public SysPulseEngine()
        {
            _prevCpuTime = _currentProcess.TotalProcessorTime;
            _prevTime = DateTime.UtcNow;
        }

        /// <summary>
        /// Runs the live system diagnostic monitor loop until cancelled.
        /// </summary>
        public async Task RunMonitorAsync(CancellationToken cancellationToken = default)
        {
            Console.OutputEncoding = Encoding.UTF8;
            Console.CursorVisible = false;
            Console.Clear();

            try
            {
                while (!cancellationToken.IsCancellationRequested)
                {
                    SystemMetrics metrics = GatherMetrics();
                    RenderDashboard(metrics);

                    // Refresh every second smoothly
                    await Task.Delay(1000, cancellationToken);
                }
            }
            catch (TaskCanceledException)
            {
                // Graceful cancellation exit
            }
            finally
            {
                Console.ResetColor();
                Console.CursorVisible = true;
                Console.Clear();
            }
        }

        private SystemMetrics GatherMetrics()
        {
            // 1. CPU Usage Calculation for Current Process
            DateTime currentTime = DateTime.UtcNow;
            TimeSpan currentCpuTime = _currentProcess.TotalProcessorTime;

            double timePassed = (currentTime - _prevTime).TotalMilliseconds;
            double cpuTimePassed = (currentCpuTime - _prevCpuTime).TotalMilliseconds;

            _prevTime = currentTime;
            _prevCpuTime = currentCpuTime;

            double cpuUsage = 0;
            if (timePassed > 0)
            {
                cpuUsage = (cpuTimePassed / (timePassed * Environment.ProcessorCount)) * 100.0;
                cpuUsage = Math.Clamp(cpuUsage, 0, 100);
            }

            // 2. Memory & Thread Metrics
            long workingSetMB = _currentProcess.WorkingSet64 / (1024 * 1024);
            long privateMemoryMB = _currentProcess.PrivateMemorySize64 / (1024 * 1024);
            int threadCount = _currentProcess.Threads.Count;
            int handleCount = 0;
            
            try { handleCount = _currentProcess.HandleCount; } catch { /* Ignore platform restrictions */ }

            return new SystemMetrics(cpuUsage, workingSetMB, privateMemoryMB, threadCount, handleCount, Environment.ProcessorCount);
        }

        private void RenderDashboard(SystemMetrics metrics)
        {
            int screenWidth = Math.Max(80, Console.WindowWidth);
            int screenHeight = Math.Max(25, Console.WindowHeight);

            var buffer = new char[screenHeight, screenWidth];
            ClearBuffer(buffer);

            // Render Header title via TerminalGrid component
            _gridRenderer.DrawHeader(buffer, "=== SysPulse.NET - System Diagnostic Monitor (Press ESC to Exit) ===");

            // Render CPU Meter
            _gridRenderer.DrawProgressBar(buffer, startX: 4, startY: 4, width: 50, label: "Process CPU Usage", percentage: metrics.CpuUsage);

            // Render Memory Panel
            _gridRenderer.DrawPanelBox(buffer, startX: 4, startY: 8, width: 60, height: 6, title: "Memory & Resources");
            _gridRenderer.DrawText(buffer, startX: 6, startY: 10, text: $"Working Set (RAM): {metrics.WorkingSetMB} MB");
            _gridRenderer.DrawText(buffer, startX: 6, startY: 11, text: $"Private Memory:    {metrics.PrivateMemoryMB} MB");

            // Render Process Specs Panel
            _gridRenderer.DrawPanelBox(buffer, startX: 4, startY: 15, width: 60, height: 6, title: "Process Execution Details");
            _gridRenderer.DrawText(buffer, startX: 6, startY: 17, text: $"Active Threads:    {metrics.ThreadCount}");
            _gridRenderer.DrawText(buffer, startX: 6, startY: 18, text: $"Logical Cores:     {metrics.ProcessorCount} Cores");

            // Flush buffer to console atomically to eliminate flicker
            FlushBufferToConsole(buffer);
        }

        private void ClearBuffer(char[,] buffer)
        {
            int height = buffer.GetLength(0);
            int width = buffer.GetLength(1);
            for (int y = 0; y < height; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    buffer[y, x] = ' ';
                }
            }
        }

        private void FlushBufferToConsole(char[,] buffer)
        {
            int height = buffer.GetLength(0);
            int width = buffer.GetLength(1);
            var sb = new StringBuilder(height * (width + 1));

            for (int y = 0; y < height - 1; y++)
            {
                for (int x = 0; x < width; x++)
                {
                    sb.Append(buffer[y, x]);
                }
                sb.AppendLine();
            }

            Console.SetCursorPosition(0, 0);
            Console.Write(sb.ToString());
        }
    }

    public readonly struct SystemMetrics
    {
        public double CpuUsage { get; }
        public long WorkingSetMB { get; }
        public long PrivateMemoryMB { get; }
        public int ThreadCount { get; }
        public int HandleCount { get; }
        public int ProcessorCount { get; }

        public SystemMetrics(double cpuUsage, long workingSetMB, long privateMemoryMB, int threadCount, int handleCount, int processorCount)
        {
            CpuUsage = cpuUsage;
            WorkingSetMB = workingSetMB;
            PrivateMemoryMB = privateMemoryMB;
            ThreadCount = threadCount;
            HandleCount = handleCount;
            ProcessorCount = processorCount;
        }
    }
}
