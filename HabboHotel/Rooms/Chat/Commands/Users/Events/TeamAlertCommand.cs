using System;
using System.Linq;
using System.Text;
using System.Collections.Generic;
using System.Collections.Concurrent;

using Plus.HabboRoleplay.Turfs;
using Plus.HabboHotel.Rooms;
using Plus.HabboHotel.Pathfinding;
using Plus.HabboRoleplay.Misc;
using Plus.HabboHotel.Groups;
using Plus.HabboRoleplay.Games;

namespace Plus.HabboHotel.Rooms.Chat.Commands.Users.Events
{
    class TeamAlertCommand : IChatCommand
    {
        public string PermissionRequired
        {
            get { return "command_events_team_alert"; }
        }

        public string Parameters
        {
            get { return "%mensagem%"; }
        }

        public string Description
        {
            get { return "Envia uma mensagem para todos os seus colegas de equipe."; }
        }

        public void Execute(GameClients.GameClient Session, Rooms.Room Room, string[] Params)
        {
            if (Params.Length == 1)
            {
                Session.SendWhisper("Digite uma mensagem para enviar.", 1);
                return;
            }

            if (Session.GetRoleplay().Game == null)
            {
                Session.SendWhisper("Você não está dentro de um evento para usar este comando!", 1);
                return;
            }

            if (Session.GetRoleplay().Team == null)
            {
                Session.SendWhisper("Este evento não possui alertas de equipe!", 1);
                return;
            }

            string Message = CommandManager.MergeParams(Params, 1);
            RoleplayTeam Team = Session.GetRoleplay().Team;

            if (RoleplayGameManager.RunningGames.ContainsKey(Session.GetRoleplay().Game.GetGameMode()))
            {
                lock (Team.Members)
                {
                    foreach (var Member in Team.Members)
                    {
                        var Client = PlusEnvironment.GetGame().GetClientManager().GetClientByUserID(Member);

                        if (Client == null)
                            continue;

                        Client.SendWhisper("[Alerta do TIME] [" + Session.GetHabbo().Username + "] " + Message, 11);
                    }
                }
            }
            else
            {
                Session.SendWhisper("Um erro ocorreu!", 1);
                return;
            }
        }
    }
}