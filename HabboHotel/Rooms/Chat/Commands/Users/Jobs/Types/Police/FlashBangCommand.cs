using System;
using System.Linq;
using System.Text;
using System.Drawing;
using System.Collections.Generic;

using Plus.HabboHotel.Rooms;
using Plus.HabboHotel.GameClients;
using Plus.HabboHotel.Rooms.Chat.Styles;
using Plus.HabboRoleplay.RoleplayUsers;
using Plus.HabboHotel.Groups;
using Plus.HabboRoleplay.Misc;
using Plus.Communication.Packets.Outgoing.Rooms.Chat;
using Plus.Utilities;

namespace Plus.HabboHotel.Rooms.Chat.Commands.Users.Jobs.Types.Police
{
    class FlashBangCommand : IChatCommand
    {
        public string PermissionRequired
        {
            get { return "command_police_flashbang"; }
        }

        public string Parameters
        {
            get { return ""; }
        }

        public string Description
        {
            get { return "Atordoe todos os usuários procurados em uma sala para detê-los."; }
        }

        public void Execute(GameClient Session, Rooms.Room Room, string[] Params)
        {
            #region Conditions

            RoomUser RoomUser = Session.GetRoomUser();       

            if (!GroupManager.HasJobCommand(Session, "flashbang"))
            {
                Session.SendWhisper("Somente um tenente da polícia pode usar esse comando!", 1);
                return;
            }

            if (!Session.GetRoleplay().IsWorking)
            {
                Session.SendWhisper("Você deve estar trabalhando para usar esse comando!", 1);
                return;
            }

            List<RoomUser> WantedUsers = Room.GetRoomUserManager().GetRoomUsers().Where(x => x != null && x.GetClient() != null && x.GetClient().GetHabbo() != null && x.GetClient().GetRoleplay() != null && RoleplayManager.WantedList.ContainsKey(x.UserId)).ToList();
            if (WantedUsers.Count <= 0)
            {
                Session.SendWhisper("Não há usuários procurados nesta sala!", 1);
                return;
            }

            if (Session.GetRoleplay().TryGetCooldown("flashbang"))
                return;

            Point ClientPos = new Point(RoomUser.Coordinate.X, RoomUser.Coordinate.Y);

            #endregion

            #region Execute

            lock (Room.GetRoomUserManager().GetRoomUsers())
            {
                foreach (RoomUser User in Room.GetRoomUserManager().GetRoomUsers())
                {
                    if (User == null)
                        continue;

                    if (User.GetClient() == null)
                        continue;

                    if (User.GetClient().GetHabbo() == null)
                        continue;

                    if (User.GetClient().GetRoleplay() == null)
                        continue;

                    if (Session.GetHabbo().Id == User.UserId)
                        continue;

                    if (User.IsAsleep)
                        continue;

                    if (!RoleplayManager.WantedList.ContainsKey(User.UserId))
                        continue;

                    Point TargetClientPos = new Point(User.Coordinate.X, User.Coordinate.Y);
                    double Distance = RoleplayManager.GetDistanceBetweenPoints2D(ClientPos, TargetClientPos);

                    if (Distance <= 6)
                    {
                        User.GetClient().GetRoleplay().TimerManager.CreateTimer("atordoar", 1000, false);
                        User.GetClient().SendMessage(new FloodControlComposer(15));

                        if (User.GetClient().GetRoleplay().InsideTaxi)
                            User.GetClient().GetRoleplay().InsideTaxi = false;

                        User.Frozen = true;
                        User.CanWalk = false;
                        User.ClearMovement(true);
                    }
                }
            }

            Session.Shout("*Atira sua granada flashbang em todos os suspeitos que estão na sala*", 37);
            Session.GetRoleplay().CooldownManager.CreateCooldown("flashbang", 1000, 30);
            return;

            #endregion
        }
    }
}