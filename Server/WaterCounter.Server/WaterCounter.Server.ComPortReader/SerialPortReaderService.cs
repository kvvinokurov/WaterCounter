using System;
using System.IO.Ports;
using System.Linq;
using System.Management;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using WaterCounter.Server.ComPortReader.Data;

namespace WaterCounter.Server.ComPortReader
{
    public class SerialPortReaderService : IHostedService, IDisposable
    {
        private const double SerialPortSearchPeriod = 10;
        private const double SerialPortHealthCheckPeriod = 10;
        private const string SearchString = "CH340";
        private const string SerialPortSearchString = "(COM";
        private const string CaptionPropertyName = "Caption";

        private readonly ILogger<SerialPortReaderService> _logger;
        private SerialPort _serialPort;
        private Timer _serialPortHealthTimer;
        private SerialPortInfo _serialPortInfo;
        private Timer _serialPortSearchTimer;


        public SerialPortReaderService(ILogger<SerialPortReaderService> logger)
        {
            _logger = logger;
        }

        public void Dispose()
        {
            _serialPortSearchTimer?.Dispose();
            _serialPortHealthTimer?.Dispose();
            _serialPort?.Dispose();
        }


        public Task StartAsync(CancellationToken cancellationToken)
        {
            var comPortInfo = SearchComPort();

            ProcessComPortInfo(comPortInfo, StartSearchTimer, InitializeSerialPort);

            return Task.CompletedTask;
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            if (_serialPortSearchTimer != null)
                StopSearchTimer();

            if (_serialPort != null && _serialPort.IsOpen)
                _serialPort.Close();

            return Task.CompletedTask;
        }

        private void StartSearchTimer()
        {
            _serialPortSearchTimer = new Timer(
                e => SearchSerialPortByTimer(),
                null,
                TimeSpan.Zero,
                TimeSpan.FromSeconds(SerialPortSearchPeriod));
        }

        private void SearchSerialPortByTimer()
        {
            _logger.LogInformation("Searching Com Port...");

            var comPortInfo = SearchComPort();
            ProcessComPortInfo(comPortInfo, null, info =>
            {
                StopSearchTimer();
                InitializeSerialPort(info);
            });
        }

        private void ProcessComPortInfo(
            SerialPortInfo comPortInfo,
            Action actionWhileComPortNotFound = null,
            Action<SerialPortInfo> actionIfComPortIsFound = null
        )
        {
            if (comPortInfo == null)
            {
                _logger.LogError("Не удалось найти соответствующий COM порт!");
                actionWhileComPortNotFound?.Invoke();
            }
            else
            {
                _logger.LogInformation($"COM порт найден: {comPortInfo.Title}");
                actionIfComPortIsFound?.Invoke(comPortInfo);
            }
        }

        private void StopSearchTimer()
        {
            StopTimer(_serialPortSearchTimer);
        }

        private void StopHealthTimer()
        {
            StopTimer(_serialPortHealthTimer);
        }

        private static void StopTimer(Timer timer)
        {
            timer?.Change(Timeout.Infinite, 0);
            timer?.Dispose();
        }

        private void InitializeSerialPort(SerialPortInfo serialPortInfo)
        {
            _serialPort = new SerialPort(serialPortInfo.PortName, 9600)
            {
                DtrEnable = true,
                RtsEnable = true
            };
            _serialPort.DataReceived += SerialPortOnDataReceived;
            if (!_serialPort.IsOpen)
                _serialPort.Open();

            _serialPortInfo = serialPortInfo;

            _serialPortHealthTimer ??= new Timer(
                e => SerialPortHealthCheckByTimer(),
                null,
                TimeSpan.FromSeconds(SerialPortHealthCheckPeriod),
                TimeSpan.FromSeconds(SerialPortHealthCheckPeriod)
                );
        }

        private void ClosePort()
        {
            if (_serialPort == null)
                return;

            if (_serialPort.IsOpen)
                _serialPort.Close();

            _serialPort.Dispose();
            _serialPort = null;
        }

        private void SerialPortHealthCheckByTimer()
        {
            if (_serialPortInfo == null)
                return;

            var comPortInfo = SearchComPort();
            if (comPortInfo == null)
            {
                _logger.LogWarning("COM порт не найден!!!");

                StopHealthTimer();
                ClosePort();

                StartSearchTimer();
                return;
            }

            if (comPortInfo.PortName.Equals(_serialPortInfo.PortName, StringComparison.InvariantCultureIgnoreCase))
            {
                _logger.LogInformation("Port OK!");
                if (_serialPort.IsOpen)
                    return;

                //_logger.LogInformation("Com порт закрыт!");
                _serialPort.Open();
                //_logger.InportantInfo("Открыли");

                return;
            }

            ClosePort();
            InitializeSerialPort(comPortInfo);
        }

        private static SerialPortInfo SearchComPort()
        {
            using var managementObjectSearcher =
                new ManagementObjectSearcher("root\\CIMV2", "SELECT * FROM Win32_PnPEntity");
            var reg = new Regex("\\((.+)\\)");

            var comPortInfo = managementObjectSearcher.Get()
                .Cast<ManagementObject>()
                .Where(obj =>
                    obj[CaptionPropertyName] != null &&
                    obj[CaptionPropertyName].ToString().Contains(SerialPortSearchString) &&
                    obj[CaptionPropertyName].ToString().Contains(SearchString)
                )
                .Select(obj => new
                {
                    Name = obj[CaptionPropertyName].ToString(),
                    ComPort = reg.Matches(obj[CaptionPropertyName].ToString()).Count > 0
                        ? reg.Matches(obj[CaptionPropertyName].ToString()).First().Groups.Last<Capture>().Value
                        : null
                })
                .Where(data => !string.IsNullOrEmpty(data.ComPort))
                .Select(data => new SerialPortInfo(data.Name, data.ComPort))
                .FirstOrDefault();

            return comPortInfo;
        }

        private void SerialPortOnDataReceived(object sender,
            SerialDataReceivedEventArgs serialDataReceivedEventArgs)
        {
            var comPort = (SerialPort) sender;
            var waterCounter = comPort.ReadLine()?.Trim();
            var wcNumber = !string.IsNullOrEmpty(waterCounter) ? Convert.ToUInt16(waterCounter) : 0;
            _logger.LogInformation(wcNumber.ToString());
        }
    }
}