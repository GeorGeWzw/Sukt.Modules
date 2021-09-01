﻿using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Sukt.WebSocketServer
{
    public static class WebSocketRouteMiddlewareExtensions
    {
        public static IApplicationBuilder UseSuktWebSocketServer(this IApplicationBuilder app, IServiceProvider serviceProvider)
        {
            app.UseMiddleware<WebSocketRouteMiddleware>();
            WebSocketRouteOption.ApplicationServices = serviceProvider;
            return app;
        }
        /// <summary>
        /// Add WebSocket debug ui.
        /// </summary>
        /// <param name="app"></param>
        /// <param name="path"></param>
        /// <returns></returns>
        public static IApplicationBuilder UseWebSocketServerUI(this IApplicationBuilder app, string path = "/sukt/wsdebug")
        {
            // debug ui
            app.Map(path, (appbuilder) =>
            {
                appbuilder.Run(async context =>
                {
                    await context.Response.WriteAsync("<h1>Sukt------------->WebSocket   Test</h1>");
                });
            });

            return app;
        }

        /// <summary>
        /// Use websocket cluster start service.
        /// Add Cyaim.WebSocketServer.Infrastructure.Middlewares.WebSocketRouteMiddleware Middleware.
        /// The websocket request will execute the with relation endpoint methods.
        /// </summary>
        /// <param name="app"></param>
        /// <param name="serviceProvider"></param>
        /// <param name="clusterOption"></param>
        /// <returns></returns>
        //public static IApplicationBuilder UseWebSocketServer(this IApplicationBuilder app, IServiceProvider serviceProvider, Action<ClusterOption> clusterOption)
        //{
        //    throw new NotImplementedException();

        //    if (app == null)
        //    {
        //        throw new ArgumentNullException(nameof(app));
        //    }

        //    if (serviceProvider == null)
        //    {
        //        throw new ArgumentNullException(nameof(serviceProvider));
        //    }

        //    if (clusterOption == null)
        //    {
        //        throw new ArgumentNullException(nameof(clusterOption));
        //    }


        //    ClusterOption cluster = new ClusterOption();
        //    clusterOption(cluster);
        //    if (cluster == null)
        //    {
        //        throw new ArgumentNullException(nameof(clusterOption));
        //    }

        //    WebSocketRouteOption wsro = serviceProvider.GetService(typeof(WebSocketRouteOption)) as WebSocketRouteOption;
        //    bool hasClusterChannel = wsro.WebSocketChannels.ContainsKey(cluster.ChannelName);
        //    if (!hasClusterChannel)
        //    {
        //        throw new InvalidOperationException($"WebSocket集群 -> WebSocketRouteOption中没有定义集群数据交换通道");
        //    }

        //    Console.WriteLine($"WebSocket集群 -> {(cluster.NodeLevel == ServiceLevel.Master ? "主节点" : "从节点")}");

        //    GlobalClusterCenter.ClusterContext = cluster;
        //    //foreach (var item in GlobalClusterCenter.ClusterContext.Nodes)
        //    //{
        //    //    var exitEvent = new ManualResetEvent(false);
        //    //    var url = new Uri($"ws://{item}/{cluster.ChannelName}");
        //    //    var factory = new Func<ClientWebSocket>(() => new ClientWebSocket
        //    //    {
        //    //        Options =
        //    //            {
        //    //                KeepAliveInterval = TimeSpan.FromSeconds(5),
        //    //                //ClientCertificates = ...
        //    //            }
        //    //    });
        //    //    using (var client = new WebsocketClient(url, factory))
        //    //    {
        //    //        client.ReconnectTimeout = TimeSpan.FromSeconds(10);

        //    //        client.ReconnectionHappened.Subscribe(info => Console.WriteLine($"Reconnection happened, type: {info.Type}"));

        //    //        client.MessageReceived.Subscribe(msg => Console.WriteLine($"Message received: {msg}"));
        //    //        client.Start();

        //    //        Task.Run(() => client.Send("{ message }"));

        //    //        exitEvent.WaitOne();
        //    //    }
        //    //}



        //    app.UseMiddleware<WebSocketRouteMiddleware>();
        //    WebSocketRouteOption.ApplicationServices = serviceProvider;


        //    return app;
        //}
    }
}
