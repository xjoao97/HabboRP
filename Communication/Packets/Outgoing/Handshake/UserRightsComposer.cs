using Plus.HabboHotel.GameClients;

namespace Plus.Communication.Packets.Outgoing.Handshake
{
    public class UserRightsComposer : ServerPacket
    {
        public UserRightsComposer(GameClient Session, int Rank)
            : base(ServerPacketHeader.UserRightsMessageComposer)
        {
            //base.WriteInteger(Session.GetHabbo().GetSubscriptionManager().HasSubscription ? 2 : 0);//Club level
            base.WriteInteger(2);//Club level
            base.WriteInteger(Rank);
            base.WriteBoolean(Session.GetHabbo().GetPermissions().HasRight("ambassador") ? true : false);//Is an ambassador
        }
    }
}