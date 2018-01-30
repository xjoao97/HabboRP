using System;
using System.Linq;
using System.Text;
using System.Collections.Generic;

using Plus.HabboHotel.Rooms;
using Plus.HabboHotel.Users.Effects;

namespace Plus.HabboHotel.Rooms.Chat.Commands.Managers
{
    class FreezeRoomCommand : IChatCommand
    {
        public string PermissionRequired
        {
            get { return "command_freeze_room"; }
        }

        public string Parameters
        {
            get { return ""; }
        }

        public string Description
        {
            get { return "Congele todos os usuários na sala atual!"; }
        }

        public void Execute(GameClients.GameClient Session, Rooms.Room Room, string[] Params)
        {
            if (Room.GetRoomUserManager().GetUserList().ToList().Count <= 1)
            {
                Session.SendWhisper("Você é a única pessoa na sala!", 1);
                return;
            }

            foreach (RoomUser User in Room.GetRoomUserManager().GetUserList().ToList())
            {
                if (User == null || User.GetClient() == null || User.GetClient().GetHabbo() == null)
                    continue;

                if (User.GetClient() == Session)
                    continue;

                if (User.Frozen)
                    continue;

                if (User.GetClient().GetRoleplay().EquippedWeapon != null)
                    User.GetClient().GetRoleplay().EquippedWeapon = null;

                User.Frozen = true;
                User.ClearMovement(true);

                if (User.CurrentEffect != 12)
                    User.ApplyEffect(EffectsList.Ice);

                User.GetClient().SendWhisper("Você foi congelador por " + Session.GetHabbo().Username + "!", 1);
            }

            Session.Shout("*Congela imediatamente todos usuários do quarto*", 23);
            return;
        }
    }
}
