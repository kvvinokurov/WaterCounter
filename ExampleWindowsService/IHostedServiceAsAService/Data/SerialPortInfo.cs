namespace IHostedServiceAsAService.Data
{
    public class SerialPortInfo
    {
        private string _portName;

        public SerialPortInfo()
        {
        }

        public SerialPortInfo(string title, string portName)
        {
            Title = title;
            PortName = portName;
        }

        public SerialPortInfo(string portName) : this(portName, portName)
        {
        }

        public string Title { get; set; }

        public string PortName
        {
            get => _portName;
            set => _portName = value?.Trim().ToUpper();
        }

        public override string ToString()
        {
            return Title;
        }
    }
}