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
    class SoloQueueCommand : IChatCommand
    {
        public string PermissionRequired
        {
            get { return "command_events_soloqueue"; }
        }

        public string Parameters
        {
            get { return ""; }
        }

        public string Description
        {
            get { return "Junta-se a um soloqueue."; }
        }

        public void Execute(GameClients.GameClient Session, Rooms.Room Room, string[] Params)
        {
            if (RoleplayGameManager.RunningGames.ContainsKey(GameMode.SoloQueue))
            {
                if (Session.GetRoleplay().Game != null && Session.GetRoleplay().Game.GetGameMode() == GameMode.SoloQueue)
                {
                    Session.SendWhisper("Você já está no evento SoloQueue!", 1);
                    return;
                }
                else
                {
                    if (RoleplayGameManager.AddPlayerToGame(RoleplayGameManager.GetGame(GameMode.SoloQueue), Session, "") != "OK")
                        return;

                    if (Session.GetRoleplay().EquippedWeapon != null)
                        Session.GetRoleplay().EquippedWeapon = null;

                    if (Session.GetRoleplay().IsWorking)
                        Session.GetRoleplay().IsWorking = false;

                    if (Session.GetRoleplay().InsideTaxi)
                        Session.GetRoleplay().InsideTaxi = false;
                }
            }
            else
                return;
        }
    }
}