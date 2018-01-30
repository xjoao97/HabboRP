using System;
using System.Linq;
using System.Text;
using System.Collections.Generic;

using Plus.Communication.Packets.Outgoing.Rooms.Engine;
using Plus.HabboHotel.GameClients;
using Plus.HabboRoleplay.Misc;

namespace Plus.HabboHotel.Rooms.Chat.Commands.SpecialRights
{
    class RoomMakePetCommand : IChatCommand
    {
        public string PermissionRequired
        {
            get { return "command_room_make_pet"; }
        }

        public string Parameters
        {
            get { return "%animal%"; }
        }

        public string Description
        {
            get { return "Permite que você transformar todos na sala em um animal de estimação."; }
        }

        public void Execute(GameClients.GameClient Session, Room Room, string[] Params)
        {
            #region Conditions
            if (Params.Length < 2)
            {
                Session.SendWhisper("Comando inválido! Use :qpet <animal>", 1);
                return;
            }

            int TargetPetId;
            if (!int.TryParse(Params[1], out TargetPetId))
                TargetPetId = RoleplayManager.GetPetIdByString(Params[1].ToString());

            if (TargetPetId == 0)
            {
                Session.SendWhisper("Opa, não existe um animal de estimação com esse nome!!", 1);
                return;
            }
            #endregion

            #region Execute
            lock (Room.GetRoomUserManager().GetRoomUsers())
            {
                foreach (var user in Room.GetRoomUserManager().GetRoomUsers())
                {
                    if (user == null || user.IsBot || user.IsPet || user.GetClient() == null || user.GetClient().GetHabbo() == null)
                        continue;

                    if (user.GetClient().GetHabbo().PetId == TargetPetId)
                        continue;

                    //Change the users Pet Id.
                    user.GetClient().GetHabbo().PetId = (TargetPetId == -1 ? 0 : TargetPetId);

                    //Quickly remove the old user instance.
                    user.GetRoom().SendMessage(new UserRemoveComposer(user.VirtualId));

                    //Add the new one, they won't even notice a thing!!11 8-)
                    user.GetRoom().SendMessage(new UsersComposer(user));

                    //Tell them a quick message.
                    if (user.GetClient().GetHabbo().PetId > 0)
                        user.GetClient().SendWhisper("Um administrador transformou você em " + Params[1].ToString(), 1);
                }
                Session.Shout("*Transforma todos na sala em " + Params[1].ToString() + "*", 23);
            }
            #endregion
        }
    }
}