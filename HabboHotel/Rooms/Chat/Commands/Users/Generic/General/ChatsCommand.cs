using System;
using System.Linq;
using System.Text;
using System.Collections.Generic;

using Plus.HabboHotel.Rooms;
using Plus.HabboHotel.GameClients;
using Fleck;
using Plus.Communication.Packets.Outgoing.Notifications;
using Plus.HabboRoleplay.Web.Util.ChatRoom;
using Newtonsoft.Json;

namespace Plus.HabboHotel.Rooms.Chat.Commands.Users.Generic.General
{
    class ChatsCommand : IChatCommand
    {
        public string PermissionRequired
        {
            get { return "command_websocket_chat_joinable_chats"; }
        }

        public string Parameters
        {
            get { return ""; }
        }

        public string Description
        {
            get { return "Dá uma lista de bate-papos."; }
        }

        public void Execute(GameClients.GameClient Session, Rooms.Room Room, string[] Params)
        {

            Dictionary<object, object> AllChats = new Dictionary<object, object>();
            
            foreach(WebSocketChatRoom ChatRoom in WebSocketChatManager.RunningChatRooms.Values)
            {
                AllChats.Add(ChatRoom.ChatName, JsonConvert.SerializeObject(new Dictionary<object, object>() { { "chatName", ChatRoom.ChatName }, { "chatVisitors", ChatRoom.ChatUsers.Count }, { "chatType", (ChatRoom.GetChatType()) } }));
            }

            PlusEnvironment.GetGame().GetWebEventManager().SendDataDirect(Session, JsonConvert.SerializeObject(new Dictionary<object, object>()
            {
                { "event", "chatManager" },
                { "chatname", "getchats" },
                { "action", "getchatrooms" },
                { "returnedChats" , JsonConvert.SerializeObject(AllChats) },
             }));
        }

    }
}
