using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Serilog;
using Serilog.Events;
using System;
using System.IO;
using System.Threading;

namespace Sukt.AuthServer.DemoApi
{
    public class Program
    {
        public static void Main(string[] args)
        {
            ThreadPool.GetMinThreads(out var workerThreads, out var completionPortThreads);
            Console.WriteLine($"{workerThreads}, {completionPortThreads}");
            ThreadPool.SetMinThreads(workerThreads * 16, completionPortThreads * 16);
            //Log.Logger = new LoggerConfiguration()

            //    .MinimumLevel.Information()
            //    .MinimumLevel.Override("Microsoft", LogEventLevel.Information)
            //    .Enrich.FromLogContext()
            //    .WriteTo.Console()
            //    .WriteTo.File(Path.Combine("logs", @"log.txt"), rollingInterval: RollingInterval.Day)
            //    .CreateLogger();
            //SeriLogLogger.SetSeriLoggerToFile("logs");
            CreateHostBuilder(args).Build().Run();
        }

        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                //.UseServiceContext()
                .ConfigureWebHostDefaults(webBuilder =>
                {
                    //���API��Ŀ��Ҫ����GRPC������Ҫ��������Kestrel�������ֱ�ָ��������ͨ�˿ڣ���ΪGRPCĬ����ʹ��https 
                    //webBuilder.ConfigureKestrel(opt =>
                    //{
                    //    opt.ListenLocalhost(8852, o => o.Protocols = Microsoft.AspNetCore.Server.Kestrel.Core.HttpProtocols.Http1);
                    //    opt.ListenLocalhost(9852, o => o.Protocols = Microsoft.AspNetCore.Server.Kestrel.Core.HttpProtocols.Http2);
                    //});
                    webBuilder.UseStartup<Startup>()
                    //                    .ConfigureKestrel(options =>
                    //                    {

                    //#if DEBUG

                    //                        options.ListenLocalhost(8361, o => o.Protocols =
                    //                            HttpProtocols.Http2);

                    //                        // ADDED THIS LINE to fix the problem
                    //                        options.ListenLocalhost(8001, o => o.Protocols =
                    //                            HttpProtocols.Http1);
                    //#else

                    //                        // ADDED THIS LINE to fix the problem
                    //                        options.ListenAnyIP(80, o => o.Protocols =
                    //                            HttpProtocols.Http1);
                    //                        options.ListenAnyIP(8331, o => o.Protocols =
                    //                                                    HttpProtocols.Http2);

                    //#endif
                    //                    })
                    .UseSerilog((webHost, configuration) =>
                    {

                        //�õ������ļ�
                        var serilog = webHost.Configuration.GetSection("Serilog");
                        //��С����
                        var minimumLevel = serilog["MinimumLevel:Default"];
                        //��־�¼�����
                        var logEventLevel = (LogEventLevel)Enum.Parse(typeof(LogEventLevel), minimumLevel);


                        configuration.ReadFrom.
                        Configuration(webHost.Configuration.GetSection("Serilog")).Enrich.FromLogContext().WriteTo.Console(logEventLevel);

                        configuration.WriteTo.Map(le => MapData(le),
                (key, log) => log.Async(o => o.File(Path.Combine("logs", @$"{key.time:yyyy-MM-dd}\{key.level.ToString().ToLower()}.txt"), logEventLevel)));

                        (DateTime time, LogEventLevel level) MapData(LogEvent logEvent)
                        {

                            return (new DateTime(logEvent.Timestamp.Year, logEvent.Timestamp.Month, logEvent.Timestamp.Day, logEvent.Timestamp.Hour, logEvent.Timestamp.Minute, logEvent.Timestamp.Second), logEvent.Level);
                        }

                    })//ע��Serilog��־�м��//����������log��
                    .ConfigureLogging((hostingContext, builder) =>
                    {
                        builder.ClearProviders();
                        builder.SetMinimumLevel(LogLevel.Information);
                        builder.AddConfiguration(hostingContext.Configuration.GetSection("Logging"));
                        builder.AddConsole();
                        builder.AddDebug();
                    });
                });
    }
}
