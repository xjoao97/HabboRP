using Plus.Communication.Packets.Outgoing.Catalog;
using Plus.HabboHotel.GameClients;
using Plus.HabboHotel.Rooms.AI;
using Plus.Communication.Packets.Incoming;

namespace Plus.Communication.Packets.Incoming.Catalog
{
    public class GetSellablePetBreedsEvent : IPacketEvent
    {
        public void Parse(GameClient Session, ClientPacket Packet)
        {
            string Type = Packet.PopString();
            string PacketType = "";
            int PetId = PlusEnvironment.GetGame().GetCatalog().GetPetRaceManager().GetPetId(Type, out PacketType);

            Session.SendMessage(new SellablePetBreedsComposer(PacketType, PetId, PlusEnvironment.GetGame().GetCatalog().GetPetRaceManager().GetRacesForRaceId(PetId)));
        }
    }
}