using Plus.Communication.Packets.Outgoing.Catalog;
using Plus.HabboHotel.GameClients;
using Plus.Communication.Packets.Incoming;

namespace Plus.Communication.Packets.Incoming.Catalog
{
    public class CheckPetNameEvent : IPacketEvent
    {
        public void Parse(GameClient Session, ClientPacket Packet)
        {
            string PetName = Packet.PopString();

            if (PetName.Length < 2)
            {
                Session.SendMessage(new CheckPetNameComposer(2, "2"));
                return;
            }
            else if (PetName.Length > 15)
            {
                Session.SendMessage(new CheckPetNameComposer(1, "15"));
                return;
            }
            else if (!PlusEnvironment.IsValidAlphaNumeric(PetName))
            {
                Session.SendMessage(new CheckPetNameComposer(3, ""));
                return;
            }
            else if (PlusEnvironment.GetGame().GetChatManager().GetFilter().IsFiltered(PetName))
            {
                Session.SendMessage(new CheckPetNameComposer(4, ""));
                return;
            }

            Session.SendMessage(new CheckPetNameComposer(0, ""));
        }
    }
}