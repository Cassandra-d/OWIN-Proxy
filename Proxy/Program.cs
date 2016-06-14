using Microsoft.Owin.Hosting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Proxy
{
    class Program
    {
        const string Host = "http://127.0.0.1";
        const string Port = "8080";

        static void Main(string[] args)
        {

            using (WebApp.Start<Startup>(new StartOptions($"{Host}:{Port}")))
            {
                Console.WriteLine("Proxy is up and running");
                Console.ReadLine();
            }
            Console.WriteLine("Exited");
        }
    }
}
