using System;
using Plus.HabboHotel.GameClients;
using Plus.HabboHotel.Groups;
using Plus.HabboRoleplay.Misc;
using Plus.HabboRoleplay.RoleplayUsers;
using Plus.HabboRoleplay.Bots;
using Plus.HabboHotel.Items;
using System.Linq;
using Plus.HabboHotel.Pathfinding;
using System.Threading;
using Plus.HabboHotel.Rooms;

namespace Plus.HabboRoleplay.Timers.Types
{
    /// <summary>
    /// Discharge timer
    /// </summary>
    public class DischargeTimer : BotRoleplayTimer
    {
        public DischargeTimer(string Type, RoleplayBot CachedBot, int Time, bool Forever, object[] Params) 
            : base(Type, CachedBot, Time, Forever, Params)
        {
            TimeCount = 0;
        }
 
        /// <summary>
        /// Discharge sequence
        /// </summary>
        public override void Execute()
        {
            try
            {
                if (base.CachedBot == null || base.CachedBot.DRoomUser == null || base.CachedBot.DRoomUser.GetBotRoleplay() == null || base.CachedBot.DRoom == null)
                {
                    base.EndTimer();
                    return;
                }

                GameClient Client = (GameClient)Params[0];

                if (Client != null && Client.GetRoomUser() != null && Client.GetRoleplay() != null && !Client.LoggingOut && Client.GetRoleplay().IsDead)
                {
                    var UserPoint = new System.Drawing.Point(Client.GetRoomUser().X, Client.GetRoomUser().Y);
                    var Items = base.CachedBot.DRoom.GetGameMap().GetAllRoomItemForSquare(UserPoint.X, UserPoint.Y);
                    bool HasBed = Items.ToList().Where(x => x.GetBaseItem().ItemName == "hosptl_bed").ToList().Count() > 0;

                    Item Item = null;
                    if (HasBed)
                        Item = Items.ToList().FirstOrDefault(x => x.GetBaseItem().ItemName == "hosptl_bed");

                    if (Item != null)
                    {
                        var GoToPoint = new System.Drawing.Point(Item.SquareLeft.X, Item.SquareLeft.Y);

                        if (base.CachedBot.DRoomUser.Coordinate != GoToPoint)
                            return;

                        int Rot = Rotation.Calculate(Client.GetRoomUser().Coordinate.X, Client.GetRoomUser().Coordinate.Y, base.CachedBot.DRoomUser.Coordinate.X, base.CachedBot.DRoomUser.Coordinate.Y);
                        base.CachedBot.DRoomUser.SetRot(Rot - 4, false);
                        base.CachedBot.DRoomUser.Chat("Deixe-me injetá-lo com esta agulha para reviver você " + Client.GetHabbo().Username + "!", true);

                        BeginDischargingUser(Client);
                    }
                }

                base.CachedBot.WalkingToItem = false;
                InitiateGoHome();
                base.EndTimer();
            }
            catch
            {
                base.EndTimer();
            }
        }

        public void BeginDischargingUser(GameClient Client)
        {
            try
            {
                if (Client == null)
                    return;

                if (Client.GetHabbo() == null)
                    return;

                if (Client.GetRoleplay() == null)
                    return;

                if (Client.GetRoomUser() == null)
                    return;

                if (!Client.GetRoleplay().IsDead)
                    return;

                if (Client.GetRoleplay().DeadTimeLeft <= 1)
                    return;

                if (Client.LoggingOut)
                    return;

                Client.GetRoleplay().UpdateTimerDialogue("Dead-Timer", "remove", Client.GetRoleplay().DeadTimeLeft, 0);

                Client.GetRoleplay().BeingHealed = true;

                if (Client.GetRoomUser().CurrentEffect != (23))
                    Client.GetRoomUser().ApplyEffect(23);

                new Thread(() =>
                {
                    int count = 0;
                    int limit = Random.Next(35, 61);
                    while (count < limit)
                    {
                        if (Client == null)
                            break;

                        if (Client.GetRoomUser() == null)
                            break;

                        if (Client.GetRoleplay() == null)
                            break;

                        if (Client.LoggingOut)
                            break;

                        if (!Client.GetRoleplay().BeingHealed)
                            break;

                        if (!Client.GetRoleplay().IsDead)
                            break;

                        if ((limit - count) == 60)
                            Client.SendWhisper("Você irá reviver em 1 minuto!", 1);
                        else if ((limit - count) == 30)
                            Client.SendWhisper("Você irá reviver em 30 segundos!", 1);
                        else if ((limit - count) == 10)
                            Client.SendWhisper("Você irá reviver em 10 segundos!", 1);

                        count++;
                        Thread.Sleep(1000);
                    }

                    if (count >= limit && Client != null && Client.GetRoomUser() != null && Client.GetRoleplay() != null && !Client.LoggingOut)
                    {
                        if (Client.GetRoleplay().IsDead)
                        {
                            Client.GetRoleplay().IsDead = false;
                            Client.GetRoleplay().DeadTimeLeft = 0;
                            Client.SendWhisper("Você reviveu antes da hora, graças à ajuda dos médicos!", 1);
                        }
                    }
                }).Start();
            }
            catch
            {

            }
        }

        public void InitiateGoHome()
        {
            if (!base.CachedBot.DRoomUser.GetBotRoleplayAI().OnDuty)
                return;

            var Point = new System.Drawing.Point(base.CachedBot.DRoomUser.GetBotRoleplay().oX, base.CachedBot.DRoomUser.GetBotRoleplay().oY);

            if (base.CachedBot.DRoomUser.Coordinate != Point)
                base.CachedBot.DRoomUser.MoveTo(Point);
        }
    }
}