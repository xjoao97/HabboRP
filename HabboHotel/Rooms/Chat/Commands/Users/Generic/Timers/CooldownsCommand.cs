using System;
using System.Linq;
using System.Text;
using System.Collections.Generic;

using Plus.Communication.Packets.Outgoing.Notifications;
using Plus.HabboHotel.Rooms;
using Plus.HabboHotel.Rooms.Chat.Styles;
using Plus.HabboRoleplay.RoleplayUsers;
using Plus.HabboRoleplay.Misc;

namespace Plus.HabboHotel.Rooms.Chat.Commands.Users.Generic.Timers
{
    class CooldownsCommand : IChatCommand
    {
        public string PermissionRequired
        {
            get { return "command_timers_cooldowns"; }
        }

        public string Parameters
        {
            get { return ""; }
        }

        public string Description
        {
            get { return "Avisa quais contagens que tens restante."; }
        }

        public void Execute(GameClients.GameClient Session, Rooms.Room Room, string[] Params)
        {
            StringBuilder Message = new StringBuilder().Append("<----- Contagens ativos atuais ----->\n\n");

            if (Session.GetRoleplay().CooldownManager.ActiveCooldowns.Count <= 0)
                Message.Append("Você atualmente não possui contagens!\n");

            lock (Session.GetRoleplay().CooldownManager.ActiveCooldowns.Values)
            {
                foreach (var Cooldown in Session.GetRoleplay().CooldownManager.ActiveCooldowns.Values)
                {
                    if (Cooldown == null)
                        continue;

                    // Don't show combat timers
                    if (Cooldown.Type.ToLower() == "arma" || Cooldown.Type.ToLower() == "soco" || Cooldown.Type.ToLower() == "recarregar")
                        continue;

                    // Capital at the start of type
                    int TotalSeconds = Cooldown.TimeLeft / 1000;
                    int Minutes = Convert.ToInt32(Math.Floor((double)TotalSeconds / 60));
                    int Seconds = TotalSeconds - (Minutes * 60);

                    string TypeOfCooldown = Cooldown.Type.Substring(0, 1).ToUpper() + Cooldown.Type.Substring(1);
                    Message.Append(TypeOfCooldown + " Contagem: " + Minutes + " minuto(s) e " + Seconds + " segundo(s) restantes!\n");
                    Message.Append("----------\n");
                }
                Message.Append("\nUse o comando :temporestante para verificar os tempos de alguma coisa, se houver!");
            }

            Session.SendMessage(new MOTDNotificationComposer(Message.ToString()));
        }
    }
}