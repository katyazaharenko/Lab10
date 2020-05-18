using System;
using SomeProject.Library.Server;
using SomeProject.Library;
using SomeProject;

namespace SomeProject.TcpServer
{
    public class EnteringPointServer
    {
        public static void Main(string[] args)
        {
           try
            {
                Server server = new Server();
                // Включение сервера.
                server.TurnOnListener().Wait();
            }
            catch(Exception e)
            {
                Console.WriteLine(e.Message);
            }
        }
    }
}
