using System;
using System.Linq;
using System.Text;
using System.Collections.Generic;

using Plus.Communication.Packets.Outgoing.Rooms.Engine;
using Plus.HabboHotel.GameClients;
using Plus.HabboRoleplay.Misc;

namespace Plus.HabboHotel.Rooms.Chat.Commands.SpecialRights
{
    class UnBanVIPCommand : IChatCommand
    {
        public string PermissionRequired
        {
            get { return "command_ban_vip_alert_undo"; }
        }

        public string Parameters
        {
            get { return "%usuario%"; }
        }

        public string Description
        {
            get { return "Desbanir um usuário dos alertas vip."; }
        }

        public void Execute(GameClients.GameClient Session, Room Room, string[] Params)
        {
            #region Conditions

            if (Params.Length < 2)
            {
                Session.SendWhisper("Comando inválido! Use :desbanvip [usuário]!", 1);
                return;
            }

            GameClient TargetClient = PlusEnvironment.GetGame().GetClientManager().GetClientByUsername(Params[1]);

            if (TargetClient == null)
            {
                Session.SendWhisper("Desculpe, mas este usuário não pôde ser encontrado!", 1);
                return;
            }

            if (TargetClient.GetHabbo().VIPRank == 0)
            {
                Session.SendWhisper("Desculpe, mas este usuário não é VIP!", 1);
                return;
            }

            if (TargetClient.GetRoleplay().VIPBanned <= 0)
            {
                Session.SendWhisper("Este utilizador não está proibido de receber alertas VIP!", 1);
                return;
            }
            #endregion

            #region Execute

            TargetClient.GetRoleplay().VIPBanned = 0;
            Session.Shout("*Desbane imediatamente " + TargetClient.GetHabbo().Username + " de enviar alertas VIP!*" , 23);
            TargetClient.SendWhisper("Um administrador desbaniu você de enviar alertas VIP!", 1);

            #region Notify of unban to other VIP users
            lock (PlusEnvironment.GetGame().GetClientManager().GetClients.ToList())
            {
                foreach (GameClient client in PlusEnvironment.GetGame().GetClientManager().GetClients.ToList())
                {
                    if (client == null || client.GetHabbo() == null)
                        continue;

                    if (!client.GetHabbo().GetPermissions().HasRight("mod_tool") && client.GetHabbo().VIPRank == 0)
                        continue;

                    if (client.GetRoleplay().DisableVIPA == true)
                        continue;

                    client.SendWhisper("[Alerta VIP] *Um administrador desbaniu você " + TargetClient.GetHabbo().Username + " de enviar Alertas VIP!*", 11);
                }
            }
            #endregion

            #endregion
        }
    }
}