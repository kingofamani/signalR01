using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Microsoft.AspNet.SignalR;
using System.Threading.Tasks;
using Newtonsoft.Json;
using System.Runtime.Serialization.Formatters;

public class ChatHub : Hub
{
    //過關點擊次數
    public const int PASS_COUNTS = 10;

    //宣告靜態類別，儲存user清單
    public static class Users
    {
        public class Info
        {
            public Info(string n) { name = n; x = 0; y = 0; count = 0; }
            public string name { get; set; }
            public int x { get; set; }
            public int y { get; set; }
            public int count { get; set; }
        }

        public static Dictionary<string, Users.Info> ConnectionIds = new Dictionary<string, Users.Info>();
    }

    //遊戲初始
    public void gameConnected(string name)
    {
        //將目前使用者新增至user清單        
        Users.ConnectionIds.Add(Context.ConnectionId, new Users.Info(name));
    }

    //遊戲
    //★傳參至Client方法一：用LINQ
    public void Game(Users.Info user)
    {
        if (Users.ConnectionIds.ContainsKey(Context.ConnectionId))
        {
            Users.ConnectionIds[Context.ConnectionId] = user;
        }

        Clients.Others.game(Users.ConnectionIds.Select(u => new { id = u.Key, name = u.Value.name, x = u.Value.x, y = u.Value.y, count = u.Value.count }).ToList());//用Others，自己不用更新座標，更具即時性

        //判斷是否過關
        if (user.count == PASS_COUNTS)
        {
            
            //清除遊戲分數
            foreach (KeyValuePair<string, Users.Info> u in Users.ConnectionIds)
            {
                Users.ConnectionIds[u.Key].count = 0;
            }

            //通知玩家過關
            Clients.All.pass(user.name, Users.ConnectionIds.Select(u => new { id = u.Key, name = u.Value.name, x = u.Value.x, y = u.Value.y, count = u.Value.count }).ToList());
        }
    }

    //★傳參至Client方法二：用JSON
    //public void Game2(Users.Info user)
    //{
    //    string json = string.Empty;

    //    if (Users.ConnectionIds.ContainsKey(Context.ConnectionId))
    //    {
    //        Users.ConnectionIds[Context.ConnectionId] = user;

    //        //轉JSON
    //        Dictionary<string, Users.Info> users = new Dictionary<string, Users.Info>();
    //        users = Users.ConnectionIds;

    //        json = JsonConvert.SerializeObject(users, Formatting.Indented, new JsonSerializerSettings 
    //        { 
    //            TypeNameHandling = TypeNameHandling.None,
    //            TypeNameAssemblyFormat = FormatterAssemblyStyle.Simple
    //        });
    //    }
    //    else
    //    {
    //        json = string.Empty;
    //    }

    //    Clients.Others.game(json);
    //}

    
    
    //儲存上一個加入的群組
    public static string OriginGroup = string.Empty;

    //加群組(只能在一個群組)
    public Task JoinOnlyOneGroup(string groupId)
    {
        Groups.Remove(Context.ConnectionId, OriginGroup);
        OriginGroup = groupId;
        return Groups.Add(Context.ConnectionId, groupId);
    }
    //加群組(可多個群組)
    public Task JoinGroup(string groupId)
    {
        OriginGroup = groupId;
        return Groups.Add(Context.ConnectionId, groupId);
    }
    //退群組
    public Task LeaveGroup(string groupId)
    {
        return Groups.Remove(Context.ConnectionId, groupId);
    }
    //傳送訊息給群組
    public void SendGroup(string groupId, string message)
    {
        var user = Users.ConnectionIds.Where(u => u.Key == Context.ConnectionId).FirstOrDefault();
        Clients.Group(groupId).show(user.Value.name + "說：" + message);
    }
    

    //傳送訊息給所有User
    public void Send(string message)
    {
        var user = Users.ConnectionIds.Where(u => u.Key == Context.ConnectionId).FirstOrDefault();
        Clients.All.show(user.Value.name + "說：" + message);
    }  

    //傳送訊息給某人
    public void SendOne(string id, string message)
    {
        var from = Users.ConnectionIds.Where(u => u.Key == Context.ConnectionId).FirstOrDefault();
        Clients.Client(id).show("<span style='color:red'>" + from.Value.name + "密你:" + message + "</span>");
    }

    //新使用者連線進入聊天室
    public void userConnected(string name)
    {
        //將目前使用者新增至user清單
        Users.ConnectionIds.Add(Context.ConnectionId, new Users.Info(name));

        //發送給所有人，取得user清單
        Clients.All.getList(Users.ConnectionIds.Select(u => new { id=u.Key,name=u.Value.name}).ToList());

        //通知其他人，有新使用者
        Clients.Others.show("歡迎" + name + "進入聊天室");
    }

    //當使用者斷線時呼叫
    //stopCalled是SignalR 2.1.0版新增的參數
    public override Task OnDisconnected(bool stopCalled)
    {
        Clients.Others.removeList(Context.ConnectionId);
        Users.ConnectionIds.Remove(Context.ConnectionId);
        return base.OnDisconnected(stopCalled);
    }
    
    
}
