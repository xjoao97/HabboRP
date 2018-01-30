using Plus.Communication.Packets.Outgoing.Rooms.Engine;
using Plus.HabboHotel.GameClients;
using Plus.HabboHotel.Items;
using Plus.HabboHotel.Pathfinding;
using Plus.HabboHotel.Rooms;
using Plus.HabboRoleplay.Misc;
using Plus.Utilities;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static Plus.HabboRoleplay.Bots.Manager.TimerHandlers.TimerHandlerManager;

namespace Plus.HabboRoleplay.Bots.Manager.TimerHandlers.Types
{
    public class AttackHandler : IBotHandler
    {

        public bool Active { get; set; }
        public GameClient InteractingUser { get; set; }
        public RoleplayBot InteractingBot { get; set; }
        public ConcurrentDictionary<string, string> Values { get; set; }

        public AttackHandler(params object[] Params)
        {
            this.InteractingBot = (RoleplayBot)Params[0];
            this.InteractingUser = (GameClient)Params[1];
            this.Active = true;
            this.Values = new ConcurrentDictionary<string, string>();
        }

        public bool ExecuteHandler(params object[] Params)
        {

            #region Conditions

            if (!this.Active) return false;

            if (this.InteractingBot.Dead)
                return this.AbortHandler();

            if (this.GetRoomUser() == null)
                return this.AbortHandler();

            if (this.InteractingUser == null)
            {
                if (!this.FindNewTarget())
                    return this.AbortHandler();
            }

            if (this.InteractingUser.GetRoomUser() == null)
            {
                if (!this.FindNewTarget())
                    return this.AbortHandler();
            }

            if (this.GetRoomUser().GetRoom() == null)
                return this.AbortHandler();

            if (this.InteractingUser.GetRoomUser().GetRoom() != this.GetRoomUser().GetRoom())
            {
                if (!this.FindNewTarget())
                    return this.AbortHandler();
            }

            if (this.CanCombat(this.InteractingUser, this.GetRoomUser()) && !this.InteractingBot.TryGetCooldown("fist"))
            {
                if (this.HandleCombat(this.InteractingUser ,this.GetRoomUser()))
                    return this.AbortHandler();
            }

            if (this.InteractingUser.GetRoomUser() == null)
                return this.AbortHandler();
            #endregion

            Point AttackSpot;

            if (!this.InteractingBot.TryGetCooldown("fist"))
                if (this.GetAttackingPosition(out AttackSpot))
                    this.GetRoomUser().MoveTo(AttackSpot);
                else
                    this.AbortHandler();
            else
                this.GetRoomUser().MoveTo(GetRoomUser().GetRoom().GetGameMap().getRandomWalkableSquare(true));

            return true;
        }

        public bool AbortHandler(params object[] Params)
        {
            this.InteractingBot.Attacking = false;
            //this.InteractingBot.DRoomUser.Chat("Stopped attacking", true);
            this.Active = false;
            this.InteractingUser = null;

            return false;
        }

        public bool RestartHandler(params object[] Params)
        {
            this.Active = true;

            return true;
        }

        public void SetValues(string Key, string Value)
        {
            if (this.Values.ContainsKey(Key))
            {
                string OldValue = this.Values[Key];
                this.Values.TryUpdate(Key, Value, OldValue);
            }
            else
                this.Values.TryAdd(Key, Value);
        }

        public void AssignInteractingUser(GameClient InteractingUser)
        {
          //  GetRoomUser().Chat("begin attacking a new bitch", true);
            this.InteractingUser = InteractingUser;
        }

        private bool GetAttackingPosition(out Point Point)
        {

          
            Point = new Point(0, 0);

            if (!this.Values.ContainsKey("attack_pos"))
                return false;

            int AttackPosition = Convert.ToInt32(this.Values["attack_pos"]);

            if (this.InteractingUser == null)
                return false;

            if (this.InteractingUser.GetRoomUser() == null)
                return false;

            int AttackX = this.InteractingUser.GetRoomUser().GetUniqueSpot(this.InteractingBot.DefaultAttackPosition).X;
            int AttackY = this.InteractingUser.GetRoomUser().GetUniqueSpot(this.InteractingBot.DefaultAttackPosition).Y;
            
            Point = new Point(AttackX, AttackY);

            if (this.GetRoomUser().GetRoom() == null)
                return false;

            if (this.GetRoomUser().GetRoom().GetGameMap() == null)
                return false;

            if (!this.GetRoomUser().GetRoom().GetGameMap().IsValidStep2(GetRoomUser(), 
                new Vector2D(GetRoomUser().X, GetRoomUser().Y), new Vector2D(Point.X, Point.Y), false, false))
            {
                Point = this.InteractingUser.GetRoomUser().SquareInFront;
                if (!this.GetRoomUser().GetRoom().GetGameMap().IsValidStep2(GetRoomUser(),
               new Vector2D(GetRoomUser().X, GetRoomUser().Y), new Vector2D(Point.X, Point.Y), false, false))
                {
                    Point = this.InteractingUser.GetRoomUser().Coordinate;
                }
            }

            return true;
        }

        private bool CanCombat(GameClient Client, RoomUser RoleplayBot)
        {

            Point Point;

            if (!this.GetAttackingPosition(out Point))
                return false;

            double Distance = RoleplayManager.GetDistanceBetweenPoints2D(RoleplayBot.Coordinate, Point);

            if (Distance <= 1.5)
            {
                if (this.InteractingUser.GetRoomUser().IsWalking)
                {
                    int Odds = new Random().Next(this.InteractingBot.MinOdds, this.InteractingBot.MaxOdds);
                    int Odds2 = new Random().Next(this.InteractingBot.MinOdds, this.InteractingBot.MaxOdds);

                    if (Odds == Odds2)
                    {
                        RoleplayBot.Chat("*Swings at " + Client.GetHabbo().Username + ", but misses*", true, 6);
                        RoleplayBot.GetBotRoleplay().CooldownManager.CreateCooldown("fist", 1000, 3);
                    }
                }

                int Rot = Rotation.Calculate(GetRoomUser().X, GetRoomUser().Y, Client.GetRoomUser().X, Client.GetRoomUser().Y);

                GetRoomUser().SetRot(Rot, false);
                GetRoomUser().UpdateNeeded = true;

                return true;
            }
            

            return false;
        }

        private bool HandleCombat(GameClient Client, RoomUser RoleplayBot)
        {
            int Damage = GetDamage(RoleplayBot);

            bool Died = false;
            if ((Client.GetRoleplay().CurHealth - Damage) <= 0)
            {
                Client.GetRoleplay().CurHealth = 0;
                RoleplayBot.Chat("*Swings at " + Client.GetHabbo().Username + ", knocking them out*", true, 6);
                Died = true;

                if (!this.FindNewTarget())
                    return this.AbortHandler();
            }
            else
            {
                Client.GetRoleplay().CurHealth -= Damage;
                RoleplayBot.Chat("*Swings at " + Client.GetHabbo().Username + ", causing " + Damage + " damage*", true, 6);
            }

            RoleplayBot.GetBotRoleplay().CooldownManager.CreateCooldown("fist", 1000, 3);
            return Died;
        }

        private bool FindNewTarget()
        {
            RoomUser NextTarget = this.NewTarget();

            if (NextTarget == null)
                return this.AbortHandler();
            if (NextTarget.GetClient() == null)
                return this.AbortHandler();
            if (this.InteractingBot == null)
                return this.AbortHandler();
            if (this.InteractingBot.GetDeployedInstance() == null)
                return this.AbortHandler();
            if (this.InteractingBot.GetDeployedInstance().GetBotRoleplayAI() == null)
                return this.AbortHandler();

            this.InteractingBot.GetDeployedInstance().GetBotRoleplayAI().OnAttacked(NextTarget.GetClient());

            return true;
        }

        private RoomUser NewTarget()
        {

            List<RoomUser> Targets = new List<RoomUser>();
            RoomUser SelectedUser = null;

            if (this.GetRoomUser() == null) return null;
            if (this.GetRoomUser().GetRoom() == null) return null;
            if (this.GetRoomUser().GetRoom().GetRoomUserManager() == null) return null;
            if (this.GetRoomUser().GetRoom().GetRoomUserManager().GetRoomUsers() == null) return null;

            foreach (RoomUser RoomUser in this.GetRoomUser().GetRoom().GetRoomUserManager().GetRoomUsers())
            {
                if (RoomUser == null) continue;
                if (RoomUser.GetClient() == null) continue;
                if (this.BeingAttacked(RoomUser.GetClient()) && 
                    this.GetRoomUser().GetRoom().GetRoomUserManager().GetRoomUsers().Count > 1) continue;

                Targets.Add(RoomUser);
            }

            if (Targets.Count > 0)
            SelectedUser = Targets[new Random().Next(0, Targets.Count)];

            return SelectedUser;
        }

        private bool BeingAttacked(GameClient Client)
        {
            foreach (RoomUser MafiaBot in this.GetRoomUser().GetRoom().GetRoomUserManager().GetBotList())
            {
                if (MafiaBot == this.GetRoomUser())
                    continue;

                if (MafiaBot.GetBotRoleplay() == null)
                    continue;

                if (!MafiaBot.GetBotRoleplay().Attacking)
                    continue;

                if (!MafiaBot.GetBotRoleplay().ActiveHandlers.ContainsKey(Handlers.ATTACK))
                    continue;

                if (MafiaBot.GetBotRoleplay().ActiveHandlers[Handlers.ATTACK].InteractingUser == Client)
                    return true;
            }

            return false;
        }

        private int GetDamage(RoomUser RoleplayBot)
        {
            CryptoRandom Random = new CryptoRandom();

            int Strength = RoleplayBot.GetBotRoleplay().Strength;
            int MinDamage = (Strength - 4) <= 0 ? 1 : (Strength - 4);
            int MaxDamage = Strength + 4;

            // Lucky shot?
            if (Random.Next(0, 100) < 10)
            {
                MinDamage = Strength + 10;
                MaxDamage = MinDamage + 1;
            }

            return Random.Next(MinDamage, MaxDamage);
        }

        private RoomUser GetRoomUser()
        {
            if (this.InteractingBot == null) return null;
            return this.InteractingBot.DRoomUser;
        }

    }
}
