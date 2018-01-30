using System;
using System.Linq;
using System.Text;
using System.Collections.Generic;

using Plus.Communication.Packets.Outgoing.Rooms.Engine;
using Plus.HabboHotel.GameClients;
using Plus.HabboRoleplay.Misc;

namespace Plus.HabboHotel.Rooms.Chat.Commands.SpecialRights
{
    class BanVIPCommand : IChatCommand
    {
        public string PermissionRequired
        {
            get { return "command_ban_vip_alert"; }
        }

        public string Parameters
        {
            get { return "%usuário% %tempo%"; }
        }

        public string Description
        {
            get { return "Proibe um usuário dos alertas vip."; }
        }

        public void Execute(GameClients.GameClient Session, Room Room, string[] Params)
        {
            #region Conditions

            if (Params.Length < 3)
            {
                Session.SendWhisper("Comando inválido, :banvip [usuário] [tempo]!", 1);
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

            #endregion

            #region Execute
            int Time;
            if (int.TryParse(Params[2], out Time))
            {
                if (Time < TargetClient.GetRoleplay().VIPBanned)
                {
                    Session.SendWhisper("Este usuário já está banido de Alertas VIP por " + String.Format("{0:N0}", TargetClient.GetRoleplay().VIPBanned) + " segundos!", 1);
                    return;
                }

                int Minutes = Convert.ToInt32(Math.Floor((double)Time / 60));
                int Seconds = Time - (Minutes * 60);

                TargetClient.GetRoleplay().VIPBanned = Time;
                if (Minutes > 0)
                    Session.Shout("*Usa seus poderes de Deus para proibir " + TargetClient.GetHabbo().Username + " de enviar alertas VIP por " + String.Format("{0:N0}", Minutes) + " minutos e " + String.Format("{0:N0}", Seconds) + " segundos!*", 23);
                else
                    Session.Shout("*Proíbe imediatamente " + TargetClient.GetHabbo().Username + " de enviar alertas VIP por " + String.Format("{0:N0}", Seconds) + " segundos!*", 23);
                TargetClient.SendWhisper("Um administrador baniu você de usar Alertas VIPs", 1);

                #region Notify of ban to other VIP users
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

                        client.SendWhisper("[Alerta VIP] *Um administrador proibiu " + TargetClient.GetHabbo().Username + " de enviar Alertas VIP!*", 11);
                    }
                }
                #endregion
                return;
            }
            else
            {
                Session.SendWhisper("Comando inválido! Use :banvip [usuário] [tempo]!", 1);
                return;
            }
            #endregion
        }
    }
}