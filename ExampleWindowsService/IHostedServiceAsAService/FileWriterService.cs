using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;

namespace IHostedServiceAsAService
{
    public class FileWriterService : IHostedService, IDisposable
    {
        private const string FileName = "TestApplication.txt";

        private Timer _timer;

        private static string FilePath =>
            Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), FileName);

        public void Dispose()
        {
            _timer?.Dispose();
        }

        public Task StartAsync(CancellationToken cancellationToken)
        {
            _timer = new Timer(
                e => WriteTimeToFile(),
                null,
                TimeSpan.Zero,
                TimeSpan.FromMinutes(1));

            return Task.CompletedTask;
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            _timer?.Change(Timeout.Infinite, 0);

            return Task.CompletedTask;
        }

        public void WriteTimeToFile()
        {
            if (!File.Exists(FilePath))
                using (var sw = File.CreateText(FilePath))
                {
                    sw.WriteLine(DateTime.UtcNow.ToString("O"));
                }
            else
                using (var sw = File.AppendText(FilePath))
                {
                    sw.WriteLine(DateTime.UtcNow.ToString("O"));
                }
        }
    }
}