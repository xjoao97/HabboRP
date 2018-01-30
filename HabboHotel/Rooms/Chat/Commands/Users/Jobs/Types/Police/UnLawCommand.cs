using System;
using System.Linq;
using System.Text;
using System.Drawing;
using System.Collections.Generic;

using Plus.HabboHotel.Rooms;
using Plus.HabboHotel.GameClients;
using Plus.HabboHotel.Users;
using Plus.HabboHotel.Rooms.Chat.Styles;
using Plus.HabboRoleplay.RoleplayUsers;
using Plus.HabboHotel.Groups;
using Plus.HabboRoleplay.Misc;
using Plus.Communication.Packets.Outgoing.Rooms.Chat;

namespace Plus.HabboHotel.Rooms.Chat.Commands.Users.Jobs.Types.Police
{
    class UnLawCommand : IChatCommand
    {
        public string PermissionRequired
        {
            get { return "command_police_law_undo"; }
        }

        public string Parameters
        {
            get { return "%usuario%"; }
        }

        public string Description
        {
            get { return "Remove um cidadão da lista de procurados."; }
        }

        public void Execute(GameClient Session, Rooms.Room Room, string[] Params)
        {
            #region Conditions
            if (Params.Length == 1)
            {
                Session.SendWhisper("Opa, você esqueceu de inserir um nome de usuário!", 1);
                return;
            }

            Habbo Target = PlusEnvironment.GetHabboByUsername(Params[1]);

            if (Target == null)
            {
                Session.SendWhisper("Ocorreu um erro ao tentar encontrar esse usuário, talvez ele esteja offline.", 1);
                return;
            }

            var RoomUser = Session.GetRoomUser();

            if (RoomUser == null)
                return;

            if (!GroupManager.HasJobCommand(Session, "unlaw"))
            {
                Session.SendWhisper("Apenas um policial pode usar esse comando!", 1);
                return;
            }

            if (!Session.GetRoleplay().IsWorking)
            {
                Session.SendWhisper("Você deve estar trabalhando para usar este comando!", 1);
                return;
            }

            if (!RoleplayManager.WantedList.ContainsKey(Target.Id))
            {
                Session.SendWhisper("Este cidadão não é procurado!", 1);
                return;
            }

            if (Target.GetClient() != null && Target.GetClient().GetRoomUser() != null)
            {
                if (Target.GetClient().GetRoomUser().IsAsleep)
                {
                    Session.SendWhisper("Você não pode retirar alguém da que está ausente!", 1);
                    return;
                }
            }
            #endregion

            #region Execute
            Wanted Junk;
            RoleplayManager.WantedList.TryRemove(Target.Id, out Junk);
            Session.Shout("*Remove " + Target.Username + " da lista de procurados e limpa seu nome*", 37);
		
            if (Target.GetClient() != null)
                Target.GetClient().GetRoleplay().IsWanted = false;
            #endregion
        }
    }
}