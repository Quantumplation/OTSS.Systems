using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Web;
using Microsoft.AspNet.SignalR;

namespace Website.Hubs
{
    public class SampleHub : Hub
    {
        private static int Viewers = 0;

        public async override Task OnConnected()
        {
            Interlocked.Increment(ref Viewers);
            Clients.All.UpdateViewerCount(Viewers);
            await base.OnConnected();
        }

        public async override Task OnDisconnected()
        {
            Interlocked.Decrement(ref Viewers);
            Clients.All.UpdateViewerCount(Viewers);
            await base.OnDisconnected();
        }

        public void Broadcast(string message)
        {
            Clients.All.Broadcast(message);
        }
    }
}