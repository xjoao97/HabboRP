using System;
using Plus.HabboHotel.GameClients;
using Plus.HabboHotel.Rooms;
using Plus.HabboRoleplay.Misc;
using Plus.Communication.Packets.Outgoing.Rooms.Avatar;

namespace Plus.Communication.Packets.Incoming.Rooms.Avatar
{
    public class ActionEvent : IPacketEvent
    {
        public void Parse(GameClient Session, ClientPacket Packet)
        {
            if (!Session.GetHabbo().InRoom)
                return;

            int Action = Packet.PopInt();

            Room Room = null;
            if (!PlusEnvironment.GetGame().GetRoomManager().TryGetRoom(Session.GetHabbo().CurrentRoomId, out Room))
                return;

            RoomUser User = Room.GetRoomUserManager().GetRoomUserByHabbo(Session.GetHabbo().Id);
            if (User == null)
                return;

            if (Action != 1 && Action != 7)
                User.UnIdle();

            if (Action == 3) // giggle, it removes the gun enable
            {
                return;
            }
            else if (Action == 5) // idle
            {
                if (!Session.GetHabbo().GetPermissions().HasCommand("command_idle"))
                {
                    Session.SendWhisper("Desculpe, mas :ausente está desabilitado para evitar abusos.", 1);
                    return;
                }
                else
                {
                    if (User.DanceId > 0)
                        User.DanceId = 0;

                    if (Session.GetHabbo().Effects().CurrentEffect > 0)
                        Room.SendMessage(new AvatarEffectComposer(User.VirtualId, 0));

                    User.IsAsleep = true;
                    Room.SendMessage(new SleepComposer(User, true));

                    if (!Session.GetRoleplay().IsJailed && !Session.GetRoleplay().IsDead && Session.GetRoleplay().Game == null)
                    {
                        Session.GetHabbo().Motto = "[AUSENTE] - " + Session.GetRoleplay().Class; 
                        Session.GetHabbo().Poof(false);
                    }
                }
            }
            else
            {
                if (User.DanceId > 0)
                    User.DanceId = 0;

                if (Session.GetHabbo().Effects().CurrentEffect > 0)
                    Room.SendMessage(new AvatarEffectComposer(User.VirtualId, 0));

                Room.SendMessage(new ActionComposer(User.VirtualId, Action));
            }
        }
    }
}