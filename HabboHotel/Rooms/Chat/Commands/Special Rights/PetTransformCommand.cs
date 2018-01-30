using System;
using System.Linq;
using System.Text;
using System.Collections.Generic;

using Plus.Communication.Packets.Outgoing.Rooms.Engine;
using Plus.HabboRoleplay.Misc;

namespace Plus.HabboHotel.Rooms.Chat.Commands.SpecialRights
{
    class PetTransformCommand : IChatCommand
    {
        public string PermissionRequired
        {
            get { return "command_pet_transform"; }
        }

        public string Parameters
        {
            get { return ""; }
        }

        public string Description
        {
            get { return "Permite que você se transforme em um animal de estimação."; }
        }

        public void Execute(GameClients.GameClient Session, Room Room, string[] Params)
        {
            RoomUser RoomUser = Session.GetRoomUser();
            if (RoomUser == null)
                return;

            if (!Room.PetMorphsAllowed)
            {
                Session.SendWhisper("O proprietário do quarto desativou a capacidade de utilizar uma transformação de pet nesta sala.", 1);
                if (Session.GetHabbo().PetId > 0)
                {
                    Session.SendWhisper("Ops, você ainda tem um morph, des-morphing.", 1);
                    //Change the users Pet Id.
                    Session.GetHabbo().PetId = 0;

                    //Quickly remove the old user instance.
                    Room.SendMessage(new UserRemoveComposer(RoomUser.VirtualId));

                    //Add the new one, they won't even notice a thing!!11 8-)
                    Room.SendMessage(new UsersComposer(RoomUser));
                }
                return;
            }

            if (Params.Length == 1)
            {
                Session.SendWhisper("Opa, você esqueceu de escolher o tipo de animal de estimação que você gostaria de transformar! Digite :pet lista para ver os pets disponíveis!", 1);
                return;
            }

            if (Params[1].ToString().ToLower() == "lista")
            {
                Session.SendWhisper("Habbo, Cachorro, Gato, Terrier, Crocodilo, Urso, Porco, Leao, Rinoceronte, Aranha, Tartaruga, Pintinho, Sapo, Dragao, Macaco, Cavalo, Coelho, Pombo, Demonio e Gnomo.", 1);
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

            //Change the users Pet Id.
            Session.GetHabbo().PetId = (TargetPetId == -1 ? 0 : TargetPetId);

            //Quickly remove the old user instance.
            Room.SendMessage(new UserRemoveComposer(RoomUser.VirtualId));

            //Add the new one, they won't even notice a thing!!11 8-)
            Room.SendMessage(new UsersComposer(RoomUser));

            //Tell them a quick message.
            if (Session.GetHabbo().PetId > 0)
            {
                Session.Shout("*Transforma-se em " + Params[1].ToString() + "*", 23);
                Session.SendWhisper("Digite ':pet habbo' Para voltar ao normal!", 1);
            }
            else
            {
                Session.Shout("*Transforma de volta a um cidadão*", 23);
            }
        }

    }
}