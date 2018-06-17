﻿using Microsoft.AspNetCore.SignalR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;


namespace AssetCalendarApi.Hubs
{ 
    public class CalendarHub : Hub
    {
        public async Task AddToGroup(string organizationId)
        {
            Console.WriteLine($"SignalR -- Client {Clients.Caller.ToString()} added to group");
            await Groups.AddToGroupAsync(Context.ConnectionId, organizationId);
            await Clients.Caller.SendAsync("AddedToGroup", organizationId);
        }
    }
}
