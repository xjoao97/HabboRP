using System;
using System.Linq;
using System.Text;
using System.Collections.Generic;

using Plus.HabboHotel.Rooms;
using Plus.HabboHotel.GameClients;

namespace Plus.HabboHotel.Rooms.Chat.Commands.Managers
{
    class UnFreezeCommand : IChatCommand
    {
        public string PermissionRequired
        {
            get { return "command_freeze_undo"; }
        }

        public string Parameters
        {
            get { return "%usuário%"; }
        }

        public string Description
        {
            get { return "Permita que outro usuário caminhe novamente."; }
        }

        public void Execute(GameClients.GameClient Session, Rooms.Room Room, string[] Params)
        {
            if (Params.Length == 1)
            {
                Session.SendWhisper("Digite o usuário que deseja descongelar.", 1);
                return;
            }

            RoomUser TargetUser = Room.GetRoomUserManager().GetRoomUserByHabbo(Params[1]);
            if (TargetUser == null || TargetUser.GetClient() == null)
            {
                Session.SendWhisper("Ocorreu um erro ao encontrar esse usuário, talvez ele não esteja online.", 1);
                return;
            }

            if (!TargetUser.Frozen)
            {
                Session.SendWhisper("Este usuário não está congelado!", 1);
                return;
            }

            TargetUser.Frozen = false;

            if (TargetUser.CurrentEffect != 0)
                TargetUser.ApplyEffect(0);

            Session.Shout("*Descongela imediatamente " + TargetUser.GetClient().GetHabbo().Username + "*", 23);
            Session.SendWhisper("Descongelou com sucesso " + TargetUser.GetClient().GetHabbo().Username + "!", 1);
            return;
        }
    }
}
