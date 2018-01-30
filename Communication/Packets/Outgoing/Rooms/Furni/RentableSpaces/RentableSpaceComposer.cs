using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Plus.HabboRoleplay.Misc;
using Plus.HabboHotel.Items;
using Plus.HabboHotel.Items.Data.RentableSpace;

namespace Plus.Communication.Packets.Outgoing.Rooms.Furni.RentableSpaces
{
    public class RentableSpaceComposer : ServerPacket
    {
        public RentableSpaceComposer(Item Item, HabboHotel.GameClients.GameClient Session) : base(ServerPacketHeader.RentableSpaceMessageComposer)
        {
            int Cost = GetCost(Item);

            if (Item != null)
            {
                var RentableSpaceData = Item.RentableSpaceData;

                if (RentableSpaceData != null)
                {
                    base.WriteBoolean(RentableSpaceData.Enabled);
                    base.WriteInteger(0);
                    base.WriteInteger(-1); // nothing??
                    base.WriteString(PlusEnvironment.GetHabboById(RentableSpaceData.OwnerId).Username);
                    base.WriteInteger(RentableSpaceData.TimeLeft);
                    base.WriteInteger(Cost); // Rentable Space Cost

                    var House = PlusEnvironment.GetGame().GetHouseManager().GetHouseBySignItem(Item);

                    if (House != null)
                    {
                        if (House.OwnerId == Session.GetHabbo().Id)
                        {
                            if (!House.ForSale)
                            {
                                Session.SendWhisper("Olá, " + Session.GetHabbo().Username + "! Se você gostaria de vender sua casa, use o comando ':vender [quantidade]' e clique em 'Vender'!");
                                return;
                            }
                            else
                            {
                                Session.SendWhisper("Olá, " + Session.GetHabbo().Username + "! Sua casa já está à venda por R$" + House.Cost + "! Use o comando ':vender [quantidade]' para mudar o preço da casa!");
                                return;
                            }
                        }
                    }
                }
                else
                    WriteNullData();
            }
            else
                WriteNullData();
        }

        public void WriteNullData()
        {
            base.WriteBoolean(true);
            base.WriteInteger(-1);
            base.WriteInteger(-1);
            base.WriteString("HabboRPG");
            base.WriteInteger(360); 
            base.WriteInteger(GetCost(null));
        }

        public int GetCost(Item Item)
        {
            if (Item == null)
                return 2000;

            if (Item.RentableSpaceData.FarmingSpace != null)
                return Item.RentableSpaceData.FarmingSpace.Cost;

            if (Item.GetBaseItem().Id == 3618)
            {
                var House = PlusEnvironment.GetGame().GetHouseManager().GetHouseBySignItem(Item);

                if (House != null)
                    return House.Cost;
                else
                    return 20000;
            }

            int Cost;
            string ItemName = Item.GetBaseItem().ItemName;

            switch (ItemName.ToLower())
            {
                // 3x4 Space
                case "hblooza_spacerent3x4":
                    {
                        Cost = 250;
                        break;
                    }
                // 5x5 Space
                case "hblooza_spacerent5x5":
                    {
                        Cost = 500;
                        break;
                    }
                // 6x6 Space
                case "hblooza_spacerent6x6":
                    {
                        Cost = 1000;
                        break;
                    }
                // 7x7 Space
                case "hblooza_spacerent7x7":
                    {
                        Cost = 2000;
                        break;
                    }
                // Any Other Size
                default:
                    {
                        Cost = 2000;
                        break;
                    }
            }
            return Cost;
        }
    }
}