using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Plus.Communication.Packets.Outgoing.Catalog
{
    class ClubGiftsComposer : ServerPacket
    {
        public ClubGiftsComposer() 
            : base(ServerPacketHeader.ClubGiftsMessageComposer)
        {
            base.WriteInteger(31); // Days until next gift.
            base.WriteInteger(0); // Gifts available
            base.WriteInteger(1); // Count?
            {
                base.WriteInteger(200054);
                base.WriteString("CFC_100000_diam");
                base.WriteBoolean(false);
                base.WriteInteger(5);
                base.WriteInteger(0);
                base.WriteInteger(0);
                base.WriteBoolean(true);
                base.WriteInteger(1); // Count for some reason
                {
                    base.WriteString("s");
                    base.WriteInteger(200054);
                    base.WriteString("");
                    base.WriteInteger(1);
                    base.WriteBoolean(false);
                }
                base.WriteInteger(0);
                base.WriteBoolean(false);
                base.WriteBoolean(false);
                base.WriteString(String.Empty);
            }

            base.WriteInteger(1);//Count
            {
                //int, bool, int, bool
                base.WriteInteger(200054);//Maybe the item id?
                base.WriteBoolean(true);//Can we get?
                base.WriteInteger(-1);//idk
                base.WriteBoolean(true);//idk
            }
        }
    }
}
