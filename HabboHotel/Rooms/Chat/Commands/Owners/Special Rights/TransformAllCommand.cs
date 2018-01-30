using System;
using System.Linq;
using System.Text;
using System.Collections.Generic;

using Plus.Communication.Packets.Outgoing.Rooms.Engine;
using Plus.HabboHotel.GameClients;
using Plus.HabboRoleplay.Misc;

namespace Plus.HabboHotel.Rooms.Chat.Commands.SpecialRights
{
    class TransformAllCommand : IChatCommand
    {
        public string PermissionRequired
        {
            get { return "command_transform_all"; }
        }

        public string Parameters
        {
            get { return "%animal%"; }
        }

        public string Description
        {
            get { return "Permite que você transformar todos no hotel em um animal de estimação."; }
        }

        public void Execute(GameClients.GameClient Session, Room Room, string[] Params)
        {
            #region Conditions
            if (Params.Length < 2)
            {
                Session.SendWhisper("Comando inválido! Use :todospet <animal>", 1);
                return;
            }

            int TargetPetId;
            if (!int.TryParse(Params[1], out TargetPetId))
                TargetPetId = RoleplayManager.GetPetIdByString(Params[1].ToString());

            if (TargetPetId == 0)
            {
                Session.SendWhisper("Opa, não existe um animal de estimação com esse nome!", 1);
                return;
            }
            #endregion

            #region Execute
            lock (PlusEnvironment.GetGame().GetClientManager().GetClients)
            {
                foreach (var Client in PlusEnvironment.GetGame().GetClientManager().GetClients.ToList())
                {
                    if (Client == null || Client.GetHabbo() == null || Client.GetRoleplay() == null || Client.GetRoomUser() == null)
                        continue;

                    if (Client.GetHabbo().PetId == TargetPetId)
                        continue;

                    //Change the Clients Pet Id.
                    Client.GetHabbo().PetId = (TargetPetId == -1 ? 0 : TargetPetId);

                    //Quickly remove the old Client instance.
                    Client.GetRoomUser().GetRoom().SendMessage(new UserRemoveComposer(Client.GetRoomUser().VirtualId));

                    //Add the new one, they won't even notice a thing!!11 8-)
                    Client.GetRoomUser().GetRoom().SendMessage(new UsersComposer(Client.GetRoomUser()));

                    //Tell them a quick message.
                    if (Client.GetHabbo().PetId > 0)
                        Client.SendWhisper("Um administrador transformou você em " + Params[1].ToString(), 1);
                }
                Session.Shout("Transforma todos do hotel em " + Params[1].ToString() + "*", 23);
            }
            #endregion
        }
    }
}