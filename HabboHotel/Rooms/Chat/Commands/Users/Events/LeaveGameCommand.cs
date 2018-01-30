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
using Plus.HabboRoleplay.Gambling;

namespace Plus.HabboHotel.Rooms.Chat.Commands.Users.Events
{
    class LeaveGameCommand : IChatCommand
    {
        public string PermissionRequired
        {
            get { return "command_events_leave_game"; }
        }

        public string Parameters
        {
            get { return ""; }
        }

        public string Description
        {
            get { return "Permite deixar o jogo atual em que você está."; }
        }

        public void Execute(GameClients.GameClient Session, Rooms.Room Room, string[] Params)
        {
            if (Session.GetRoleplay().TexasHoldEmPlayer > 0)
            {
                Session.Shout("*Sai do Texas Hold 'Em *", 4);
                TexasHoldEmManager.RemovePlayer(Session.GetHabbo().Id);
                return;
            }

            if (Session.GetRoleplay().Game == null)
            {
                Session.SendWhisper("Você não está dentro de um evento!", 1);
                return;
            }

            if (Session.GetRoleplay().Game.IsGameStarting() && !Session.GetRoleplay().Game.HasGameStarted())
            {
                Session.SendWhisper("Você não pode deixar um jogo enquanto está começando!", 1);
                return;
            }

            if (RoleplayGameManager.RunningGames.ContainsKey(Session.GetRoleplay().Game.GetGameMode()))
            {
                if (Session.GetRoleplay().Game.GetGameMode() == GameMode.Brawl || Session.GetRoleplay().Game.GetGameMode() == GameMode.SoloQueue || Session.GetRoleplay().Game.GetGameMode() == GameMode.SoloQueueGuns)
                {
                    if (Session.GetRoomUser() != null)
                        Session.GetRoomUser().ClearMovement(true);
                    RoleplayGameManager.GetGame(Session.GetRoleplay().Game.GetGameMode()).RemovePlayerFromGame(Session);
                    RoleplayManager.SpawnChairs(Session, "es_bench");
                }
                else
                    RoleplayGameManager.GetGame(Session.GetRoleplay().Game.GetGameMode()).RemovePlayerFromGame(Session);

                Session.GetRoleplay().Game = null;
                Session.GetRoleplay().Team = null;
                return;
            }
            else
            {
                Session.SendWhisper("Um erro ocorreu!", 1);
                return;
            }
        }
    }
}