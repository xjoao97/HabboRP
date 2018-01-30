using System;
using System.Linq;
using System.Text;
using System.Collections.Generic;

using Plus.HabboHotel.Rooms;
using Plus.HabboHotel.GameClients;

namespace Plus.HabboHotel.Rooms.Chat.Commands.Developers
{
    class FixWeaponsCommand : IChatCommand
    {
        public string PermissionRequired
        {
            get { return "command_fixweapons"; }
        }

        public string Parameters
        {
            get { return "%usuário%"; }
        }

        public string Description
        {
            get { return "Recarrega a lista de armas de usuários."; }
        }

        public void Execute(GameClients.GameClient Session, Rooms.Room Room, string[] Params)
        {
            if (Params.Length == 1)
            {
                Session.SendWhisper("Digite o usuário que deseja usar este comando ", 1);
                return;
            }

            GameClient TargetClient = PlusEnvironment.GetGame().GetClientManager().GetClientByUsername(Params[1]);
            if (TargetClient == null)
            {
                Session.SendWhisper("Ocorreu um erro ao encontrar esse usuário, talvez ele não esteja online.", 1);
                return;
            }

            RoomUser TargetUser = Session.GetHabbo().CurrentRoom.GetRoomUserManager().GetRoomUserByHabbo(Params[1]);

            if (TargetClient.GetRoleplay().EquippedWeapon != null)
                TargetClient.GetRoleplay().EquippedWeapon = null;

            TargetClient.GetRoleplay().OwnedWeapons = null;
            TargetClient.GetRoleplay().OwnedWeapons = TargetClient.GetRoleplay().LoadAndReturnWeapons();

            Session.Shout("*Atualiza automaticamente as armas de " + TargetClient.GetHabbo().Username + "*", 23);
            TargetClient.SendWhisper("Suas armas foram atualizadas por " + Session.GetHabbo().Username + "!", 1);
        }
    }
}
