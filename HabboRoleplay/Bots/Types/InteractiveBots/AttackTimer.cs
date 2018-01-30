using System;
using Plus.HabboHotel.GameClients;
using Plus.HabboHotel.Groups;
using Plus.HabboRoleplay.Misc;
using Plus.HabboRoleplay.RoleplayUsers;
using Plus.HabboRoleplay.Bots;
using Plus.HabboRoleplay.Bots.Manager;
using Plus.HabboHotel.Items;
using System.Linq;
using System.Drawing;
using Plus.HabboHotel.Pathfinding;
using System.Threading;
using Plus.HabboHotel.Rooms;
using Plus.Utilities;
using System.Collections.Generic;
using Plus.Communication.Packets.Outgoing.Rooms.Notifications;

namespace Plus.HabboRoleplay.Timers.Types
{
    /// <summary>
    /// Attack timer
    /// </summary>
    public class AttackTimer : BotRoleplayTimer
    {

        public AttackTimer(string Type, RoleplayBot CachedBot, int Time, bool Forever, object[] Params) 
            : base(Type, CachedBot, Time, Forever, Params)
        {
            this.TimeCount = 0;
        }
 
        /// <summary>
        /// Begins chasing the client
        /// </summary>
        public override void Execute()
        {
            try
            {
                if (base.CachedBot == null || base.CachedBot.DRoomUser.GetBotRoleplay() == null || base.CachedBot.DRoom == null)
                {
                    this.StopAttacking();
                    return;
                }

                if (GetAttacking() == null || GetAttacking().GetHabbo() == null)
                {
                    this.StopAttacking();
                    return;
                }

                if (GetAttacking().GetRoomUser() == null && GetAttacking().GetHabbo().CurrentRoomId != 0)
                    return;

                if (GetAttacking() != null && GetAttacking().GetHabbo() != null && GetAttacking().GetRoleplay() != null)
                {
                    if (GetAttacking().GetRoleplay().IsDead)
                    {
                        base.CachedBot.DRoomUser.GetBotRoleplay().UserAttacking = null;
                        this.StopAttacking();
                        return;
                    }
                }

                TimeCount++;

                // Waits 1.5 seconds before chasing the user down
                if (TimeCount < 150)
                    return;

                #region Chase user through rooms
                if (GetAttacking().GetHabbo().CurrentRoom != base.CachedBot.DRoom)
                {

                    if (base.CachedBot.DRoomUser.GetBotRoleplay().AIType == RoleplayBotAIType.MAFIAWARS)
                    {
                        this.StopAttacking();
                        return;
                    }

                    List<Item> AllArrows = base.CachedBot.DRoom.GetRoomItemHandler().GetFloor.Where(x => x.GetBaseItem().InteractionType == InteractionType.ARROW).ToList();
                    List<Item> PossibleArrows = AllArrows.Where(x => ItemTeleporterFinder.GetTeleRoomId(ItemTeleporterFinder.GetLinkedTele(x.Id, base.CachedBot.DRoom), base.CachedBot.DRoom) == GetAttacking().GetRoomUser().RoomId).ToList();

                    if (PossibleArrows.Count <= 0)
                    {
                        if (GetAttacking() != null)
                            base.CachedBot.DRoomUser.Chat("Merda! Você conseguiu, eu vou te pegar na próxima vez " + GetAttacking().GetHabbo().Username + "!", true, 4);

                        base.CachedBot.DRoomUser.GetBotRoleplay().UserAttacking = null;
                        this.StopAttacking();
                        return;
                    }

                    Item RandTele = PossibleArrows.FirstOrDefault();

                    int LinkedTeleId = ItemTeleporterFinder.GetLinkedTele(RandTele.Id, base.CachedBot.DRoom);
                    int LinkedTeleRoomId = ItemTeleporterFinder.GetTeleRoomId(LinkedTeleId, base.CachedBot.DRoom);
                    Room LinkedRoom = RoleplayManager.GenerateRoom(LinkedTeleRoomId);

                    //object[] Params = { RandTele, LinkedTeleId, LinkedTeleRoomId, LinkedRoom };
                    object[] Params = { CachedBot, RandTele };

                    if (LinkedRoom.HitEnabled)
                        base.CachedBot.DRoomUser.GetBotRoleplay().TimerManager.CreateTimer("teleport", CachedBot, 100, true, Params);
                    else
                    {
                        if (GetAttacking() != null)
                            base.CachedBot.DRoomUser.Chat("Merda! Você conseguiu, eu vou te pegar na próxima vez" + GetAttacking().GetHabbo().Username + "!", true, 4);

                        base.CachedBot.DRoomUser.GetBotRoleplay().UserAttacking = null;
                    }

                    this.StopAttacking();
                    return;
                }
               
                else
                #endregion

                {

                    if (CanCombat(GetAttacking(), base.CachedBot.DRoomUser) && !base.CachedBot.DRoomUser.GetBotRoleplay().TryGetCooldown("fist"))
                    {
                        if (HandleCombat(GetAttacking(), base.CachedBot.DRoomUser))
                        {
                            this.StopAttacking();
                            return;
                        }
                    }

                    if (!base.CachedBot.DRoomUser.GetBotRoleplay().TryGetCooldown("fist"))
                        base.CachedBot.DRoomUser.MoveTo(GetAttacking().GetRoomUser().Coordinate);
                    else
                        base.CachedBot.DRoomUser.MoveTo(base.CachedBot.DRoom.GetGameMap().getRandomWalkableSquare());

                    return;
                }

            }
            catch
            {
                this.StopAttacking();
            }
        }

        public void StopAttacking()
        {
            BotRoleplayTimer Timer;

            base.CachedBot.DRoomUser.GetBotRoleplay().UserAttacking = null;

            if (base.CachedBot.DRoomUser.GetBotRoleplay().ActiveTimers.ContainsKey("attack"))
                base.CachedBot.DRoomUser.GetBotRoleplay().ActiveTimers.TryRemove("attack", out Timer);

            base.EndTimer();
        }

        public bool CanCombat(GameClient Client, RoomUser RoleplayBot)
        {
            double Distance = RoleplayManager.GetDistanceBetweenPoints2D(RoleplayBot.Coordinate, Client.GetRoomUser().Coordinate);

            if (Distance <= 1)
                return true;

            return false;
        }

        public bool HandleCombat(GameClient Client, RoomUser RoleplayBot)
        {
            int Damage = GetDamage(RoleplayBot);

            bool Died = false;
            if ((Client.GetRoleplay().CurHealth - Damage) <= 0)
            {
                Client.GetRoleplay().CurHealth = 0;
                RoleplayBot.Chat("*Dá um soco em " + Client.GetHabbo().Username + ", e mata-o*", true, 6);
                Died = true;
				
				lock (PlusEnvironment.GetGame().GetClientManager().GetClients)
            {
				foreach (var client in PlusEnvironment.GetGame().GetClientManager().GetClients.ToList())
                {
                    if (client == null || client.GetHabbo() == null)
                        continue;
                
                    client.SendMessage(new RoomNotificationComposer("staff_notice", "message", "[Notícia Urgente] " + Client.GetHabbo().Username + " matou com socos o cidadão " + Client.GetHabbo().Username + ", tome cuidado!"));
                }
            }

            }
            else
            {
                Client.GetRoleplay().CurHealth -= Damage;
                RoleplayBot.Chat("*Dá um soco em " + Client.GetHabbo().Username + ", e causa " + Damage + " damage*", true, 6);
            }

            RoleplayBot.GetBotRoleplay().CooldownManager.CreateCooldown("fist", 1000, RoleplayBot.GetBotRoleplay().AttackInterval);
            return Died;
        }

        public int GetDamage(RoomUser RoleplayBot)
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

        private GameClient GetAttacking()
        {
            return base.CachedBot.DRoomUser.GetBotRoleplay().UserAttacking;
        }
    }
}