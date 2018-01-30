using System;
using System.Linq;
using System.Text;
using System.Collections.Generic;

using Plus.HabboHotel.Rooms;
using Plus.HabboHotel.GameClients;
using Plus.HabboHotel.Users.Effects;

namespace Plus.HabboHotel.Rooms.Chat.Commands.Managers
{
    class FreezeCommand : IChatCommand
    {
        public string PermissionRequired
        {
            get { return "command_freeze"; }
        }

        public string Parameters
        {
            get { return "%usuário%"; }
        }

        public string Description
        {
            get { return "Evite que outros usuários caminhem."; }
        }

        public void Execute(GameClients.GameClient Session, Rooms.Room Room, string[] Params)
        {
            if (Params.Length == 1)
            {
                Session.SendWhisper("Digite o usuário que deseja congelar.", 1);
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

            if (TargetUser != null)
            {
                TargetUser.Frozen = true;
                TargetUser.ClearMovement(true);
                TargetUser.ApplyEffect(EffectsList.Ice);
            }

            Session.Shout("*Congela imediatamente o usuário " + TargetClient.GetHabbo().Username + "*", 23);
            Session.SendWhisper("Congelou " + TargetClient.GetHabbo().Username + " com sucesso!", 1);
        }
    }
}
