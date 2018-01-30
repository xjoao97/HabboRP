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

namespace Plus.HabboHotel.Rooms.Chat.Commands.Managers
{
    class UnBanChatterCommand : IChatCommand
    {
        public string PermissionRequired
        {
            get { return "command_websocket_chat_unban_chatter"; }
        }

        public string Parameters
        {
            get { return ""; }
        }

        public string Description
        {
            get { return "Desbane um usuário para de juntar a grupos do websocket."; }
        }

        public void Execute(GameClients.GameClient Session, Rooms.Room Room, string[] Params)
        {

            if (Params.Length < 2)
            {
                Session.SendWhisper("Comando inválido! Use :desbanwpp <usuário>", 1);
                return;
            }

            bool UnbanFromMakingChats = false;

            if (Params.Length > 2)
            {
                if (Convert.ToString(Params[2]).ToLower().StartsWith("sim"))
                    UnbanFromMakingChats = true;
            }

            string Chatter = Params[1].ToString();
            GameClient TargetSession = PlusEnvironment.GetGame().GetClientManager().GetClientByUsername(Chatter);

            if (TargetSession == null)
            {
                Session.SendWhisper("Alvo não encontrado!", 1);
                return;
            }

            Session.Shout("*Desbane imediatamente " + TargetSession.GetHabbo().Username + " de entrar em grupos " + ((UnbanFromMakingChats) ? " e também de fazer-los" : "") + "!*", 23);

            TargetSession.GetRoleplay().BannedFromChatting = false;
            
            if (UnbanFromMakingChats)
            {
                TargetSession.GetRoleplay().BannedFromMakingChat = false;
            }
        }
    }
}
