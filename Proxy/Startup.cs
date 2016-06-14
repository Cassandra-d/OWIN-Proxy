using System;
using System.Threading.Tasks;
using Microsoft.Owin;
using Owin;
using Microsoft.Owin.Cors;
using System.Net.Http;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Collections.Generic;
using System.Text;

[assembly: OwinStartup(typeof(Proxy.Startup))]

namespace Proxy
{
    public class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            app.UseCors(CorsOptions.AllowAll);
            app.Use<MyMiddleWare>();
        }
    }

    public class MyMiddleWare : OwinMiddleware
    {
        protected OwinMiddleware Next { get; set; }

        public MyMiddleWare(OwinMiddleware next) : base(next)
        {
            Next = next;
        }

        public async override Task Invoke(IOwinContext context)
        {
            var clientReq = context.Request;

            using (TcpClient client = new TcpClient(clientReq.Host.ToUriComponent(), 80))
            {
                var stream = client.GetStream();
                var connect = $"CONNECT {clientReq.Uri.OriginalString} HTTP/1.1\r\nHost: {clientReq.Host.ToUriComponent()}\r\n\r\n";
                
                byte[] byteData = Encoding.UTF8.GetBytes(connect);
                await stream.WriteAsync(byteData, 0, byteData.Length);

                int recvLen = 0;
                byte[] recvData = new byte[1000];
                while (recvLen == 0)
                {
                    recvLen = await stream.ReadAsync(recvData, 0, 1000);
                }

                var str1 = Encoding.ASCII.GetString(recvData);
                str1 = str1.Replace("\r\nConnection: keep-alive\r\n", "\r\nConnection: close\r\n");

                recvData = Encoding.ASCII.GetBytes(str1);
                recvLen = Encoding.ASCII.GetByteCount(str1);
                await context.Response.WriteAsync(recvData, 0, recvLen, new CancellationToken());

                connect = "";
                foreach (var header in clientReq.Headers)
                {
                    if (header.Key.StartsWith("Host"))
                        continue;
                    connect = connect + header.Key + ": " + header.Value[0] + "\r\n";
                }
                connect = connect + "\r\n";
                recvData = new byte[1000];
                recvLen = 0;
                while (true)
                {
                    recvLen = await stream.ReadAsync(recvData, 0, 1000);
                    if (recvLen != 0)
                    {
                        await context.Response.WriteAsync(recvData, 0, recvLen, new CancellationToken());
                    }
                    else
                        break;
                }

            }
            await Next.Invoke(context);
        }
    }
}
