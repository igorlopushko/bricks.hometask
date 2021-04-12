namespace Bricks.Hometask.Sandbox
{
    public interface IServer
    {
        public void Run();
        public void Stop();
        public void RegisterClient(IClient client);
        public void UnregisterClient(IClient client);
    }
}