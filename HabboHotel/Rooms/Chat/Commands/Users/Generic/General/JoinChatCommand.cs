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
    class JoinChatCommand : IChatCommand
    {
        public string PermissionRequired
        {
            get { return "command_websocket_chat_join"; }
        }

        public string Parameters
        {
            get { return ""; }
        }

        public string Description
        {
            get { return "Entra em um chat."; }
        }

        public void Execute(GameClients.GameClient Session, Rooms.Room Room, string[] Params)
        {

            #region Params
            if (Params.Length < 2)
            {
                Session.SendWhisper("Comando inválido!", 1);
                return;
            }

            string InputtedChatName = Convert.ToString(Params[1]);

            WebSocketChatRoom NewChatRoom = WebSocketChatManager.GetChatByName(InputtedChatName.ToLower());

            #endregion

            #region Conditions
            if (Session.GetRoleplay().BannedFromChatting)
            {
                Session.SendWhisper("Você está proibido de se juntar a qualquer Grupo do WhatsApp!", 1);
                return;
            }

            if (NewChatRoom == null)
            {
                Session.SendWhisper("O bate-papo não existe!", 1);
                return;
            }

            if (!Session.GetRoleplay().PhoneApps.Contains("whatsapp"))
            {
                Session.SendWhisper("Você precisa do Aplicativo Whatsapp para fazer isso! Digite :baixar whatsapp.", 1);
                return;
            }
            #endregion

            #region Execute            

            PlusEnvironment.GetGame().GetWebEventManager().ExecuteWebEvent(Session, "event_chatroom", JsonConvert.SerializeObject(new Dictionary<object, object>()
            {
                { "action", "requestjoin" },
                { "chatname", NewChatRoom.ChatName },

            }));

            #endregion

        }

    }
}
