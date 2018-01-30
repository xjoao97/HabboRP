/*using System;
using System.Linq;
using System.Collections.Generic;
using System.Text;

using Plus.HabboHotel.GameClients;
using Plus.HabboHotel.Rooms;
using Plus.HabboHotel.Users.UserDataManagement;
using Plus.Communication.Packets.Incoming;

using Plus.Communication.Packets.Outgoing.Rooms.Avatar;
using Plus.Communication.Packets.Outgoing.Inventory.AvatarEffects;
using Plus.Communication.Packets.Outgoing.Avatar;

namespace Plus.HabboHotel.Users.Inventory.Effects
{
    public class AvatarEffectsInventoryComponent
    {
        public  List<AvatarEffect> Effects;
        private readonly int UserId;
        public int CurrentEffect;
        private int EffectCount;
        private GameClient mClient;

        public AvatarEffectsInventoryComponent(int UserId, GameClient pClient, UserData data)
        {
            mClient = pClient;
            Effects = new List<AvatarEffect>();
            this.UserId = UserId;
            CurrentEffect = -1;
            Effects.Clear();

            var QueryBuilder = new StringBuilder();
            foreach (AvatarEffect effect in data.effects)
            {
                if (!effect.HasExpired)
                {
                    Effects.Add(effect);
                    EffectCount++;
                }
                else
                    QueryBuilder.Append("DELETE FROM user_effects WHERE user_id = " + UserId + " AND effect_id = " + effect.EffectId + "; ");
            }

            if (QueryBuilder.Length > 0)
            {
                using (IQueryAdapter dbClient = PlusEnvironment.GetDatabaseManager().GetQueryReactor())
                {
                    dbClient.runFastQuery(QueryBuilder.ToString());
                }
            }
        }

        public void Dispose()
        {
            EffectCount = 0;
            Effects.Clear();
            mClient = null;
        }

        public void AddEffect(int EffectId, int Duration)
        {
            using (IQueryAdapter dbClient = PlusEnvironment.GetDatabaseManager().GetQueryReactor())
            {
                dbClient.runFastQuery("INSERT INTO user_effects (user_id,effect_id,total_duration,is_activated,activated_stamp) VALUES (" + UserId + "," + EffectId + "," + Duration + ",'0',0)");
            }

            EffectCount++;
            Effects.Add(new AvatarEffect(EffectId, Duration, false, 0));
            GetClient().SendMessage(new AvatarEffectAddedComposer(EffectId, Duration));
        }

        public void StopEffect(int EffectId)
        {
            // REMOVE EFFECT!!!
            AvatarEffect Effect = GetEffect(EffectId, true);

            if (Effect == null || !Effect.HasExpired)
            {
                return;
            }

            using (IQueryAdapter dbClient = PlusEnvironment.GetDatabaseManager().GetQueryReactor())
            {
                dbClient.runFastQuery("DELETE FROM user_effects WHERE user_id = " + UserId + " AND effect_id = " +   EffectId + " AND is_activated = 1");
            }

            Effects.Remove(Effect);
            EffectCount--;

            GetClient().SendMessage(new AvatarEffectExpiredComposer(EffectId));

            if (CurrentEffect >= 0)
            {
                ApplyEffect(-1);
            }
        }

        public void ApplyCustomEffect(int EffectId)
        {
            Room Room = GetUserRoom();
            if (Room == null)
                return;

            RoomUser User = Room.GetRoomUserManager().GetRoomUserByHabbo(GetClient().GetHabbo().Id);
            if (User == null)
                return;

            CurrentEffect = EffectId;
            Room.SendMessage(new AvatarEffectComposer(User.VirtualId, EffectId));
        }

        public void ApplyEffect(int EffectId)
        {
            if (!HasEffect(EffectId, true))
                return;    

            Room Room = GetUserRoom();
            if (Room == null)
                return;

            RoomUser User = Room.GetRoomUserManager().GetRoomUserByHabbo(GetClient().GetHabbo().Id);
            if (User == null)
                return;
            

            CurrentEffect = EffectId;
            Room.SendMessage(new AvatarEffectComposer(User.VirtualId, EffectId));
        }

   
        public bool HasEffect(int EffectId, bool IfEnabledOnly)
        {
            if (EffectId == -1 || EffectId == 28 || EffectId == 29)
            {
                return true;
            }

            foreach (AvatarEffect Effect in Effects)
            {
                if (IfEnabledOnly && !Effect.Activated)
                {
                    continue;
                }

                if (Effect.HasExpired)
                {
                    continue;
                }

                if (Effect.EffectId == EffectId)
                {
                    return true;
                }
            }

            return false;
        }

        public AvatarEffect GetEffect(int EffectId, bool IfEnabledOnly)
        {
            foreach (AvatarEffect Effect in Effects)
            {
                if (IfEnabledOnly && !Effect.Activated)
                {
                    continue;
                }

                if (Effect.EffectId == EffectId)
                {
                    return Effect;
                }
            }

            return null;
        }

        public void CheckExpired()
        {
            if (Effects.Count <= 0)
                return;

            List<AvatarEffect> ToRemove = new List<AvatarEffect>();
            foreach (AvatarEffect Effect in Effects.ToList())
            {
                if (Effect == null)
                    continue;

                if (Effect.HasExpired)
                    ToRemove.Add(Effect);
            }

            foreach (AvatarEffect Effect in ToRemove.ToList())
            {
                if (Effect == null)
                    continue;

                StopEffect(Effect.EffectId);
            }

            if (ToRemove.Count > 0)
                ToRemove.Clear();
        }

        private GameClient GetClient()
        {
            return mClient;
        }

        private Room GetUserRoom()
        {
            return mClient.GetHabbo().CurrentRoom;
        }

        public void OnRoomExit()
        {
            CurrentEffect = 0;
        }
    }
}*/