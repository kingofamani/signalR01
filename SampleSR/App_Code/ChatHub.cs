using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Microsoft.AspNet.SignalR;

public class ChatHub : Hub
{
    //Server端供Client呼叫的方法
    public void Send(string message)
    {
        //Server端呼叫Client端
        Clients.All.show(message);
    }       
}
