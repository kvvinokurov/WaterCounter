namespace WaterCounter.Server.ComPortReader
{
    public interface IRawDataRepository
    {
        long Add(int counterNumber);
    }
}