namespace StarterServer
{
    class Program
    {
        static void Main(string[] args)
        {
            new Server().Start().Wait();
        }
    }
}
