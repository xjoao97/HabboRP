using System;
using System.Linq;
using System.Text;
using System.Collections.Generic;

using Plus.HabboHotel.Rooms.AI;
using Plus.HabboHotel.Rooms;
using Plus.Communication.Packets.Outgoing.Inventory.Pets;

using System.Drawing;
using Plus.HabboHotel.GameClients;
using Plus.Communication.Packets.Outgoing.Rooms.Engine;
using Plus.Database.Interfaces;

namespace Plus.Communication.Packets.Incoming.Rooms.AI.Pets
{
    class PickUpPetEvent : IPacketEvent
    {
        public void Parse(HabboHotel.GameClients.GameClient Session, ClientPacket Packet)
        {
            if (!Session.GetHabbo().InRoom)
                return;

            if (Session == null || Session.GetHabbo() == null || Session.GetHabbo().GetInventoryComponent() == null)
                return;

            Room Room;

            if (!PlusEnvironment.GetGame().GetRoomManager().TryGetRoom(Session.GetHabbo().CurrentRoomId, out Room))
                return;

            int PetId = Packet.PopInt();

            RoomUser Pet = null;
            if (!Room.GetRoomUserManager().TryGetPet(PetId, out Pet))
            {
                //Check kick rights, just because it seems most appropriate.
                if ((!Room.CheckRights(Session) && Room.WhoCanKick != 2 && Room.Group == null) || (Room.Group != null && !Room.CheckRights(Session, false, true)))
                    return;

                //Okay so, we've established we have no pets in this room by this virtual Id, let us check out users, maybe they're creeping as a pet?!
                RoomUser TargetUser = Session.GetHabbo().CurrentRoom.GetRoomUserManager().GetRoomUserByHabbo(PetId);
                if (TargetUser == null)
                    return;

                //Check some values first, please!
                if (TargetUser.GetClient() == null || TargetUser.GetClient().GetHabbo() == null)
                    return;

                //Update the targets PetId.
                TargetUser.GetClient().GetHabbo().PetId = 0;

                //Quickly remove the old user instance.
                Room.SendMessage(new UserRemoveComposer(TargetUser.VirtualId));

                //Add the new one, they won't even notice a thing!!11 8-)
                Room.SendMessage(new UsersComposer(TargetUser));
                return;
            }

            if (Session.GetHabbo().Id != Pet.PetData.OwnerId && !Room.CheckRights(Session, true, false))
            {
                Session.SendWhisper("Você só pode pegar seus próprios animais de estimação, para expulsar um animal de estimação você deve ter direitos de sala.");
                return;
            }

            //Only if user is riding horse duuuh
            if (Pet.RidingHorse)
            {
                RoomUser UserRiding = Room.GetRoomUserManager().GetRoomUserByVirtualId(Pet.HorseID);
                if (UserRiding != null)
                {
                    //Set user is not longer riding a horse
                    UserRiding.RidingHorse = false;
                    //Set user effect to normal
                    UserRiding.ApplyEffect(-1);
                    UserRiding.MoveTo(new Point(UserRiding.X + 1, UserRiding.Y + 1));
                }
                else
                    Pet.RidingHorse = false;
            }

            Pet.PetData.RoomId = 0;
            Pet.PetData.PlacedInRoom = false;

            Pet pet = Pet.PetData;
            if (pet != null)
            {
                using (IQueryAdapter dbClient = PlusEnvironment.GetDatabaseManager().GetQueryReactor())
                {
                    dbClient.RunQuery("UPDATE `bots` SET `room_id` = '0', `x` = '0', `Y` = '0', `Z` = '0' WHERE `id` = '" + pet.PetId + "' LIMIT 1");
                    dbClient.RunQuery("UPDATE `bots_petdata` SET `experience` = '" + pet.experience + "', `energy` = '" + pet.Energy + "', `nutrition` = '" + pet.Nutrition + "', `respect` = '" + pet.Respect + "' WHERE `id` = '" + pet.PetId + "' LIMIT 1");
                }

                Session.GetHabbo().GetInventoryComponent().TryAddPet(Pet.PetData);
                Session.SendMessage(new PetInventoryComposer(Session.GetHabbo().GetInventoryComponent().GetPets()));
            }

            if (pet.OwnerId != Session.GetHabbo().Id)
            {
                GameClient Target = PlusEnvironment.GetGame().GetClientManager().GetClientByUserID(pet.OwnerId);
                if (Target != null)
                {
                    Target.GetHabbo().GetInventoryComponent().TryAddPet(Pet.PetData);
                    Session.SendMessage(new PetInventoryComposer(Session.GetHabbo().GetInventoryComponent().GetPets()));
                    Room.GetRoomUserManager().RemoveBot(Pet.VirtualId, false);

                    Target.SendMessage(new PetInventoryComposer(Target.GetHabbo().GetInventoryComponent().GetPets()));
                    return;
                }
            }

            Session.SendMessage(new PetInventoryComposer(Session.GetHabbo().GetInventoryComponent().GetPets()));
            Room.GetRoomUserManager().RemoveBot(Pet.VirtualId, false);
        }
    }
}