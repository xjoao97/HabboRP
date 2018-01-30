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
using Plus.HabboHotel.Rooms.Games;
using Plus.HabboHotel.Groups;

namespace Plus.HabboHotel.Rooms.Chat.Commands.Managers
{
    class TLockCommand : IChatCommand
    {
        public string PermissionRequired
        {
            get { return "command_websocket_chat_lock"; }
        }

        public string Parameters
        {
            get { return ""; }
        }

        public string Description
        {
            get { return "Bloqueia um bate-papo."; }
        }

        public void Execute(GameClients.GameClient Session, Rooms.Room Room, string[] Params)
        {

            #region Variables
            if (Params.Length < 2)
            {
                Session.SendWhisper("Comando inválido! Use :ctrancar <nomedochat>", 1);
                return;
            }

            string ChatName = Convert.ToString(Params[2]);
            #endregion

            #region Conditions

            if (!WebSocketChatManager.RunningChatRooms.ContainsKey(ChatName.ToLower()))
            {
                Session.SendWhisper("Este chat '" + ChatName.ToLower() + "' não existe!", 1);
                return;
            }

            WebSocketChatRoom Chat = WebSocketChatManager.RunningChatRooms[ChatName.ToLower()];

            if (Chat == null)
            {
                Session.SendWhisper("Ocorreu um erro, este bate-papo não existe!", 1);
                return;
            }

            if (Chat.ChatOwner != Session.GetHabbo().Id && Session.GetHabbo().VIPRank <= 1)
            {
                Session.SendWhisper("Você deve ser o proprietário desta conversa para fazer isso!", 1);
                return;
            }

            #endregion

            #region Execute          

            bool CurrentLocked = Convert.ToBoolean(Chat.ChatValues["locked"]);

            if (CurrentLocked)
            {
                Chat.ChatValues["locked"] = false;
                Session.SendWhisper("Desbloqueado com sucesso o bate-papo '" + Chat.ChatName + "'", 1);
            }
            else
            {
                Chat.ChatValues["locked"] = true;
                Session.SendWhisper("Bloqueou com sucesso o bate-papo '" + Chat.ChatName + "'", 1);
            }

            #endregion

        }

    }
}
