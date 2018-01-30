using System;
using System.Linq;
using System.Text;
using System.Collections.Generic;

using Plus.Communication.Packets.Outgoing.Rooms.Engine;
using Plus.HabboHotel.GameClients;
using Plus.HabboRoleplay.Misc;

namespace Plus.HabboHotel.Rooms.Chat.Commands.SpecialRights
{
    class MakePetCommand : IChatCommand
    {
        public string PermissionRequired
        {
            get { return "command_pet_transform"; }
        }

        public string Parameters
        {
            get { return "%usuário% %animal%"; }
        }

        public string Description
        {
            get { return "Permite transformar um usuário em um animal de estimação."; }
        }

        public void Execute(GameClients.GameClient Session, Room Room, string[] Params)
        {
            if (Params.Length < 3)
            {
                Session.SendWhisper("Comando inválido! Use :tpet <usuário> <animal>", 1);
                return;
            }

            GameClient TargetClient = PlusEnvironment.GetGame().GetClientManager().GetClientByUsername(Params[1]);

            if (TargetClient == null)
            {
                Session.SendWhisper("Desculpe, este usuário não pôde ser encontrado!", 1);
                return;
            }

            if (TargetClient.GetRoomUser() == null)
            {
                Session.SendWhisper("Desculpe, este usuário não está na mesma sala que você!", 1);
                return;
            }

            int TargetPetId;
            if (!int.TryParse(Params[2], out TargetPetId))
                TargetPetId = RoleplayManager.GetPetIdByString(Params[2].ToString());

            if (TargetPetId == 0)
            {
                Session.SendWhisper("Opa, não existe um animal de estimação com esse nome!!", 1);
                return;
            }

            //Change the users Pet Id.         
            TargetClient.GetHabbo().PetId = (TargetPetId == -1 ? 0 : TargetPetId);

            //Quickly remove the old user instance.
            TargetClient.GetRoomUser().GetRoom().SendMessage(new UserRemoveComposer(TargetClient.GetRoomUser().VirtualId));

            //Add the new one, they won't even notice a thing!!11 8-)
            TargetClient.GetRoomUser().GetRoom().SendMessage(new UsersComposer(TargetClient.GetRoomUser()));

            //Tell them a quick message.
            if (TargetClient.GetHabbo().PetId > 0)
            {
                Session.Shout("*Transforma " + TargetClient.GetHabbo().Username + " em " + Params[2].ToString() + "*", 23);
                TargetClient.SendWhisper("Um administrador transformou você em " + Params[1].ToString(), 1);
            }
        }
    }
}