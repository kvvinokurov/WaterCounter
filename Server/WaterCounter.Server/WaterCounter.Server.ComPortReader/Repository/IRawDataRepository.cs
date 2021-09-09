namespace WaterCounter.Server.ComPortReader.Repository
{
    public interface IRawDataRepository
    {
        long Add(int counterNumber);
    }
}