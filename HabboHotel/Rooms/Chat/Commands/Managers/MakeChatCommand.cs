using System;
using System.Linq;
using System.Text;
using System.Collections.Generic;

using Plus.HabboHotel.Rooms;
using Plus.HabboHotel.GameClients;
using Fleck;
using Plus.Communication.Packets.Outgoing.Notifications;
using Plus.HabboRoleplay.Web.Util.ChatRoom;
using Plus.HabboRoleplay.Misc;
using System.Text.RegularExpressions;

namespace Plus.HabboHotel.Rooms.Chat.Commands.Users.Generic.General
{ 
    class MakeChatCommand : IChatCommand
    {
        public string PermissionRequired
        {
            get { return "command_websocket_chat_create"; }
        }

        public string Parameters
        {
            get { return ""; }
        }

        public string Description
        {
            get { return "Faça um grupo de WhatsApp."; }
        }

        public void Execute(GameClients.GameClient Session, Rooms.Room Room, string[] Params)
        {

            #region Params
            if (Params.Length < 2)
            {
                Session.SendWhisper("Comando inválido, use :fazerchat <nomedochat>", 1);
                return;
            }

            string NewChatName = Convert.ToString(Params[1]);
            #endregion

            #region Conditions

            if (Session.GetRoleplay().BannedFromMakingChat)
            {
                Session.SendWhisper("Você está permanentemente banido de fazer salas de bate-papo!", 1);
                return;
            }
            if (!Session.GetRoleplay().PhoneApps.Contains("whatsapp"))
            {
                Session.SendWhisper("Você precisa do App WhatsApp para fazer isso!", 1);
                return;
            }
            
            if (WebSocketChatManager.RunningChatRooms.ContainsKey(NewChatName.ToLower()))
            {
                Session.SendWhisper("Esse chat (" + NewChatName.ToLower() + ") já existe! Escolha outro nome!", 1);
                return;
            }

            if ((WebSocketChatManager.RunningChatRooms.Values.Where(Runningchat => Runningchat != null).Where(Runningchat => Runningchat.ChatOwner == Session.GetHabbo().Id).ToList().Count > 0) && Session.GetHabbo().VIPRank < 2)
            {
                Session.SendWhisper("Você só pode criar um bate-papo por vez!", 1);
                return;
            }

            Regex regexItem = new Regex("^[a-zA-Z0-9]*$");
            if (!regexItem.IsMatch(NewChatName))
            {
                Session.SendWhisper("Nome do bate-papo inválido! Remova todos os caracteres especiais!", 1);
                return;
            }
            #endregion

            #region Execute
            WebSocketChatRoom NewChatRoom = new WebSocketChatRoom(NewChatName, Session.GetHabbo().Id, new Dictionary<object, object>() { { "password", "" }, {"gang", 0}, {"locked", false} }, new List<int>() { }, false);
            if (NewChatRoom.OnUserJoin(Session))
            {
                NewChatRoom.BeginChatJoin(Session);
                Session.Shout("*Cria um novo bate-papo no WhatsApp com seu " + RoleplayManager.GetPhoneName(Session) + "*", 4);
            }
            #endregion

        }

    }
}
