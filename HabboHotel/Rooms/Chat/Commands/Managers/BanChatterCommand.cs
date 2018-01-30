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
    class BanChatterCommand : IChatCommand
    {
        public string PermissionRequired
        {
            get { return "command_websocket_chat_ban_chatter"; }
        }

        public string Parameters
        {
            get { return ""; }
        }

        public string Description
        {
            get { return "Proíbe a um usuário se juntar a bate-papos do Whatsapp."; }
        }

        public void Execute(GameClients.GameClient Session, Rooms.Room Room, string[] Params)
        {

            #region Params
            if (Params.Length < 2)
            {
                Session.SendWhisper("Comando inválido, use :banwpp <usuário>", 1);
                return;
            }

            string Chatter = Params[1].ToString();
            GameClient TargetSession = PlusEnvironment.GetGame().GetClientManager().GetClientByUsername(Chatter);

            bool BanFromMaking = false;

            if (Params.Length == 3)
            {
                if (Convert.ToString(Params[2]).ToLower().StartsWith("sim"))
                    BanFromMaking = true;
            }
            #endregion

            #region Conditions
            if (TargetSession == null)
            {
                Session.SendWhisper("Alvo não encontrado!", 1);
                return;
            }
            #endregion

            #region Execute

            if (TargetSession.GetRoleplay().ChatRooms.Count > 0)
            {
                foreach (WebSocketChatRoom Chat in TargetSession.GetRoleplay().ChatRooms.Values)
                {
                    if (Chat == null)
                        continue;

                    WebSocketChatManager.Disconnect(TargetSession, Chat.ChatName);
                }
            }

            Session.Shout("*Proíbe imediatamente " + TargetSession.GetHabbo().Username + " de se juntar ou criar Grupos de WhatsApp " + ((BanFromMaking) ? ""  : "") + "!*", 23);
            TargetSession.SendWhisper("Você foi banido de se juntar a uma sala de bate-papo por um membro da equipe!", 1);
            TargetSession.GetRoleplay().BannedFromChatting = true;

            if (BanFromMaking)
            {
                TargetSession.SendWhisper("Você foi proibido de fazer uma sala de bate-papo por um membro da equipe!", 1);
                TargetSession.GetRoleplay().BannedFromMakingChat = true;
            }

            RoleplayManager.Shout(TargetSession, "*Pega meu " + RoleplayManager.GetPhoneName(TargetSession) + " e deleta o Aplicativo WhatsApp*", 4);
           
            #endregion
        }
    }
}
