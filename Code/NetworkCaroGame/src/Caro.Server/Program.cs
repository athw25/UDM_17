
using System;
using System.Threading;
using Caro.Server.Core;

namespace Caro.Server
{
    class Program
    {
        static void Main()
        {
            var server = new ServerManager();
            server.Start(8888);
            
            // Keep the server running
            Thread.Sleep(Timeout.Infinite);
        }
    }
}
