using System;
using System.Linq;
using System.Text;
using System.Collections.Generic;


using Plus.HabboHotel.Rooms.Trading;
using Plus.HabboHotel.Items;


namespace Plus.Communication.Packets.Outgoing.Inventory.Trading
{
    class TradingUpdateComposer : ServerPacket
    {
        public TradingUpdateComposer(Trade Trade)
            : base(ServerPacketHeader.TradingUpdateMessageComposer)
        {


            if (Trade.Users.Count() < 2)
                return;


            var User1 = Trade.Users.First();
            var User2 = Trade.Users.Last();


            base.WriteInteger(User1.GetClient().GetHabbo().Id);
            SerializeUserItems(User1);



            base.WriteInteger(0);
            base.WriteInteger(0);
            base.WriteInteger(1);


            SerializeUserItems(User2);




            base.WriteInteger(0);
            base.WriteInteger(0);






            /*base.WriteInteger(User.GetClient().GetHabbo().Id);
            base.WriteInteger(User.OfferedItems.Count);


            foreach (Item Item in User.OfferedItems.ToList())
            {
                base.WriteInteger(Item.Id);
               base.WriteString(Item.GetBaseItem().Type.ToString().ToLower());
                base.WriteInteger(Item.Id);
                base.WriteInteger(Item.Data.SpriteId);
                base.WriteInteger(0);//Not sure.
                if (Item.LimitedNo > 0)
                {
                    base.WriteBoolean(false);//Stackable
                    base.WriteInteger(256);
                   base.WriteString("");
                    base.WriteInteger(Item.LimitedNo);
                    base.WriteInteger(Item.LimitedTot);
                }
                else
                {
                    base.WriteBoolean(true);//Stackable
                    base.WriteInteger(0);
                   base.WriteString("");
                }


                base.WriteInteger(0);
                base.WriteInteger(0);
                base.WriteInteger(0);


                if (Item.GetBaseItem().Type == 's')
                    base.WriteInteger(0);


                base.WriteInteger(0);
                base.WriteInteger(0);
                base.WriteInteger(-1);*/
        }
        private void SerializeUserItems(TradeUser User)
        {
            base.WriteInteger(User.OfferedItems.Count);//While
            foreach (Item Item in User.OfferedItems.ToList())
            {
                base.WriteInteger(Item.Id);
                base.WriteString(Item.Data.Type.ToString().ToUpper());
                base.WriteInteger(Item.Id);
                base.WriteInteger(Item.Data.SpriteId);
                base.WriteInteger(1);
                base.WriteBoolean(true);


                //Func called _SafeStr_15990
                base.WriteInteger(0);
                base.WriteString("");


                //end Func called
                base.WriteInteger(0);
                base.WriteInteger(0);
                base.WriteInteger(0);
                if (Item.Data.Type.ToString().ToUpper() == "S")
                    base.WriteInteger(0);
            }
            //End of while
        }
    }
}