namespace IHostedServiceAsAService
{
    internal interface ILogger
    {
        void Info(string str);
        void InportantInfo(string str);
        void Warning(string str);
        void Error(string str);
    }
}