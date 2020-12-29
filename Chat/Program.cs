using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Hosting;
using Serilog;
using Serilog.Events;
using System;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Security.Authentication;

#pragma warning disable 1591

namespace Chat
{
    public class Program
    {
        public static int Main(string[] args)
        {
            Log.Logger = new LoggerConfiguration()
                .MinimumLevel.Debug()
                .MinimumLevel.Override("Microsoft", LogEventLevel.Information)
                .Enrich.FromLogContext()
                .WriteTo.Console()
                .CreateLogger();
            try
            {
                Log.Information("Iniciando Teste de WebChat");
                CreateHostBuilder(args).Build().Run();
                return 0;
            }
            catch (Exception exception)
            {
                Log.Fatal(exception, "WebChat API Finalizada sem sucesso!");
                return 1;
            }
            finally
            {
                Log.CloseAndFlush();
            }
        }
        private static int FreePort()
        {
            TcpListener tcpListener = new TcpListener(IPAddress.Loopback, 0);
            tcpListener.Start();
            int port = ((IPEndPoint)tcpListener.LocalEndpoint).Port;
            tcpListener.Stop();
            return port;
        }

        /// <summary>
        /// Cria o ambiente para se trabalhar
        /// </summary>
        /// <param name="args"></param>
        /// <returns></returns>
        public static IHostBuilder CreateHostBuilder(string[] args)
        {
            int port = FreePort();
            int port_nosecurity = 5000; //59354;
            int port_security = 5001; //44393;
#if DEBUG
            port_nosecurity = 5000;
            port_security = 5001; //44393;
#endif
            return Host.CreateDefaultBuilder(args)
                .ConfigureWebHostDefaults(webBuilder =>
                {
                    webBuilder.UseStartup<Startup>();
                    webBuilder.UseUrls($"http://*:{port_nosecurity}", $"https://*:{port_security}", $"http://localhost:{port_nosecurity}",$"http://localhost:{port_security}")
                        .UseKestrel(options =>
                        {
                            options.ConfigureHttpsDefaults(listenOptions =>
                            {
                                listenOptions.SslProtocols = SslProtocols.Tls12;
                            });
                            options.ListenAnyIP(port_nosecurity);
                            options.ListenAnyIP(port_security, listenOptions =>
                            {
                                //listenOptions.UseHttps();
                            });
                        })
                        .UseContentRoot(Directory.GetCurrentDirectory());
                })
                .UseSerilog();
        }

    }
}
