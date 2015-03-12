using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Microsoft.AspNet.SignalR;
using System.Threading.Tasks;

public class NotifyHub : Hub
{
    private const string ADMIN_GROUP = "adminGroup";
    private const string USER_GROUP = "userGroup";
    public static class Users
    {
        public static Dictionary<string, string> ConnectionIds = new Dictionary<string, string>();
    }

    //管理者進入
    //★Groups.Add是非同步執行，如果要新增至group後，馬上執行getList，要寫成
    public async Task AdminConnected()
    {
        //將管理者加入群組
        await Groups.Add(Context.ConnectionId, ADMIN_GROUP);
        //顯示user列表
        Clients.Group(ADMIN_GROUP).getList(Users.ConnectionIds.Select(u => new { id = u.Key, name = u.Value }).ToList());
    }
    
    //新使用者連線
    public async Task userConnected(string name)
    {
        //將目前使用者新增至user清單
        Users.ConnectionIds.Add(Context.ConnectionId, name);

        //發送給管理者，列出user清單
        Clients.Group(ADMIN_GROUP).getList(Users.ConnectionIds.Select(u => new { id = u.Key, name = u.Value }).ToList());

        //將user加入群組，並列出notify清單
        await Groups.Add(Context.ConnectionId, USER_GROUP);
        Clients.Client(Context.ConnectionId).showNotify("Welcome " + name);
    }

    public void SendNotify(string toId,string message)
    {
        Clients.Client(toId).showNotify(message);
    }

    //當使用者斷線時呼叫
    public override Task OnDisconnected(bool stopCalled)
    {        
        Users.ConnectionIds.Remove(Context.ConnectionId);
        Clients.Group(ADMIN_GROUP).getList(Users.ConnectionIds.Select(u => new { id = u.Key, name = u.Value }).ToList());
        return base.OnDisconnected(stopCalled);
    }
          
}
