using System;
using System.Linq;
using System.Threading;
using System.Text;
using System.Collections.Generic;

using Plus.HabboRoleplay.Misc;
using Plus.HabboHotel.GameClients;
using Plus.Communication.Packets.Outgoing.Rooms.Session;
using Plus.Communication.Packets.Outgoing.Navigator;
using Plus.Communication.Packets.Outgoing.Notifications;
using Plus.Communication.Packets.Outgoing.Rooms.Engine;
using Plus.Communication.Packets.Outgoing.Rooms.Chat;

namespace Plus.HabboHotel.Rooms.Chat.Commands.Moderators.Seniors
{
    class SummonCommand : IChatCommand
    {
        public string PermissionRequired
        {
            get { return "command_summon"; }
        }

        public string Parameters
        {
            get { return "%usuário%"; }
        }

        public string Description
        {
            get { return "Bring another user to your current room."; }
        }

        public void Execute(GameClients.GameClient Session, Rooms.Room Room, string[] Params)
        {
            if (Params.Length == 1)
            {
                Session.SendWhisper("Digite o usuário que deseja convocar..", 1);
                return;
            }

            GameClient TargetClient = PlusEnvironment.GetGame().GetClientManager().GetClientByUsername(Params[1]);
            if (TargetClient == null || TargetClient.GetHabbo() == null)
            {
                Session.SendWhisper("Ocorreu um erro ao encontrar esse usuário, talvez ele não esteja online.", 1);
                return;
            }

            if (TargetClient.GetRoleplay().Game != null)
            {
                Session.SendWhisper("Desculpe, mas " + TargetClient.GetHabbo().Username + " está em um evento!", 1);
                return;
            }

            if (TargetClient.GetHabbo().CurrentRoom != null)
            {
                if (TargetClient.GetHabbo().CurrentRoom.TutorialEnabled)
                {
                    Session.SendWhisper("Você não pode puxar alguém que está em uma sala de tutorial!", 1);
                    return;
                }
            }

            if (TargetClient.GetRoleplay().IsDead)
            {
                TargetClient.GetRoleplay().IsDead = false;
                TargetClient.GetRoleplay().ReplenishStats(true);
                TargetClient.GetHabbo().Poof();
            }

            if (TargetClient.GetRoleplay().IsJailed)
            {
                TargetClient.GetRoleplay().IsJailed = false;
                TargetClient.GetRoleplay().JailedTimeLeft = 0;
                TargetClient.GetHabbo().Poof();
            }

            Session.Shout("*Traz imediatamente " + TargetClient.GetHabbo().Username + " para o quarto*", 23);
            RoleplayManager.SendUser(TargetClient, Room.Id, "Você foi puxado por " + Session.GetHabbo().Username + "!");
        }
    }
}