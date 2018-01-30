using System;
using System.Linq;
using System.Text;
using System.Threading;
using System.Collections.Generic;

using Plus.HabboRoleplay.Misc;
using Plus.HabboHotel.Rooms;
using Plus.HabboHotel.Groups;
using Plus.Communication.Packets.Outgoing.Groups;
using Plus.Communication.Packets.Outgoing.Catalog;
using Plus.Communication.Packets.Outgoing.Inventory.Purse;
using Plus.Communication.Packets.Outgoing.Moderation;

namespace Plus.Communication.Packets.Incoming.Groups
{
    class PurchaseGroupEvent : IPacketEvent
    {
        public void Parse(HabboHotel.GameClients.GameClient Session, ClientPacket Packet)
        {
            if (Session.GetRoleplay().GangId > 1000)
            {
                Group Gang = GroupManager.GetGang(Session.GetRoleplay().GangId);

                if (Gang != null)
                {
                    if (Gang.CreatorId == Session.GetHabbo().Id)
                    {
                        Session.SendMessage(new BroadcastMessageAlertComposer("Por favor, exclua sua gangue atual antes de tentar fazer uma nova!"));
                        return;
                    }
                }
            }

            if (Session.GetHabbo().Credits < PlusStaticGameSettings.GroupPurchaseAmount)
            {
                Session.SendMessage(new BroadcastMessageAlertComposer("Uma gangue custa " + PlusStaticGameSettings.GroupPurchaseAmount + " créditos! Você tem atualmente " + Session.GetHabbo().Credits + "!"));
                return;
            }
            else
            {
                Session.GetHabbo().Credits -= PlusStaticGameSettings.GroupPurchaseAmount;
                Session.SendMessage(new CreditBalanceComposer(Session.GetHabbo().Credits));
            }

            String Name = PlusEnvironment.GetGame().GetChatManager().GetFilter().CheckMessage(Packet.PopString());
            String Description = PlusEnvironment.GetGame().GetChatManager().GetFilter().CheckMessage(Packet.PopString());
            int RoomId = Packet.PopInt();
            int Colour1 = Packet.PopInt();
            int Colour2 = Packet.PopInt();
            int groupID3 = Packet.PopInt();
            int groupID4 = Packet.PopInt();
            int groupID5 = Packet.PopInt();
            int groupID6 = Packet.PopInt();
            int groupID7 = Packet.PopInt();
            int groupID8 = Packet.PopInt();
            int groupID9 = Packet.PopInt();
            int groupID10 = Packet.PopInt();
            int groupID11 = Packet.PopInt();
            int groupID12 = Packet.PopInt();
            int groupID13 = Packet.PopInt();
            int groupID14 = Packet.PopInt();
            int groupID15 = Packet.PopInt();
            int groupID16 = Packet.PopInt();
            int groupID17 = Packet.PopInt();
            int groupID18 = Packet.PopInt();

            //RoomData Room = PlusEnvironment.GetGame().GetRoomManager().GenerateRoomData(RoomId);
            //if (Room == null || Room.OwnerId != Session.GetHabbo().Id || Room.Group != null)
            //    return;

            string Base = "b" + ((groupID4 < 10) ? "0" + groupID4.ToString() : groupID4.ToString()) + ((groupID5 < 10) ? "0" + groupID5.ToString() : groupID5.ToString()) + groupID6;
            string Symbol1 = "s" + ((groupID7 < 10) ? "0" + groupID7.ToString() : groupID7.ToString()) + ((groupID8 < 10) ? "0" + groupID8.ToString() : groupID8.ToString()) + groupID9;
            string Symbol2 = "s" + ((groupID10 < 10) ? "0" + groupID10.ToString() : groupID10.ToString()) + ((groupID11 < 10) ? "0" + groupID11.ToString() : groupID11.ToString()) + groupID12;
            string Symbol3 = "s" + ((groupID13 < 10) ? "0" + groupID13.ToString() : groupID13.ToString()) + ((groupID14 < 10) ? "0" + groupID14.ToString() : groupID14.ToString()) + groupID15;
            string Symbol4 = "s" + ((groupID16 < 10) ? "0" + groupID16.ToString() : groupID16.ToString()) + ((groupID17 < 10) ? "0" + groupID17.ToString() : groupID17.ToString()) + groupID18;

            Symbol1 = PlusEnvironment.GetGame().GetGroupManager().CheckActiveSymbol(Symbol1);
            Symbol2 = PlusEnvironment.GetGame().GetGroupManager().CheckActiveSymbol(Symbol2);
            Symbol3 = PlusEnvironment.GetGame().GetGroupManager().CheckActiveSymbol(Symbol3);
            Symbol4 = PlusEnvironment.GetGame().GetGroupManager().CheckActiveSymbol(Symbol4);

            string Badge = Base + Symbol1 + Symbol2 + Symbol3 + Symbol4;

            Group Group = null;
            if (!PlusEnvironment.GetGame().GetGroupManager().TryCreateGroup(Session.GetHabbo(), Name, Description, RoomId, Badge, Colour1, Colour2, out Group))
            {
                Session.SendNotification("Ocorreu um erro ao tentar criar este grupo.\n\nIsso pode ter acontecido porque você clicou em comprar mais de uma vez!\n\nNesse caso, sua gangue já foi criada.\n\n(Verifique clicando em seu usuário e visualizando a caveira no canto inferior direito ou seu perfil).\n\nCaso contrário, você pode tentar criar outra gangue!");
                return;
            }

            Session.SendMessage(new PurchaseOKComposer());

            RoomData Room = PlusEnvironment.GetGame().GetRoomManager().GenerateRoomData(Session.GetHabbo().CurrentRoomId);

            Session.SendMessage(new NewGroupInfoComposer(Room.Id, 0));
        }
    }
}