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
    class TimeLeftCommand : IChatCommand
    {
        public string PermissionRequired
        {
            get { return "command_timers_timeleft"; }
        }

        public string Parameters
        {
            get { return ""; }
        }

        public string Description
        {
            get { return "Avisa quais os temporizadores que você está executando, se houver."; }
        }

        public void Execute(GameClients.GameClient Session, Rooms.Room Room, string[] Params)
        {
            StringBuilder Message = new StringBuilder().Append("<----- Temporizadores ativos atuais ----->\n\n");

            var RealTimers = Session.GetRoleplay().TimerManager.ActiveTimers.Values.Where(x => x.TimeLeft > 0).ToList();

            if (RealTimers.Count == 0)
                Message.Append("Você atualmente não possui tempos restantes!\n");
            else
            { 
                foreach (var Timer in RealTimers)
                {
                    if (Timer == null)
                        continue;

                    int TotalSeconds = Timer.TimeLeft / 1000;
                    int Minutes = Convert.ToInt32(Math.Floor((double)TotalSeconds / 60));
                    int Seconds = TotalSeconds - (Minutes * 60);

                    // Capital at the start of type
                    string TypeOfTimer = Timer.Type.Substring(0, 1).ToUpper() + Timer.Type.Substring(1);

                    if (TypeOfTimer == "Turfcapture")
                        TypeOfTimer = "Capturar Território";

                    if (TypeOfTimer != "Morto")
                        Message.Append(TypeOfTimer + " Tempo: " + Minutes + " minuto(s) e " + Seconds + " segundo(s) restantes!\n");
                    else
                        Message.Append(TypeOfTimer + " Tempo: 2 minutos!\n");
                    Message.Append("----------\n");
                }
                Message.Append("\nUse o comando :contagens para verificar as contagens restantes, se houver!");
            }
            Session.SendMessage(new MOTDNotificationComposer(Message.ToString()));
        }
    }
}