using System;
using System.Linq;
using System.Collections.Concurrent;
using Plus.HabboHotel.GameClients;
using Plus.HabboRoleplay.Timers.Types;
using Plus.HabboRoleplay.Bots;
using Plus.HabboRoleplay.Web.Util.ChatRoom;
using Plus.HabboHotel.Rooms;
using Plus.HabboRoleplay.Bots.Manager;

namespace Plus.HabboRoleplay.Timers
{
    public class TimerManager
    {
        /// <summary>
        /// The client
        /// </summary>
        public GameClient Client;

        /// <summary>
        /// Contains all running timers
        /// </summary>
        public ConcurrentDictionary<string, RoleplayTimer> ActiveTimers;

        /// <summary>
        /// Constructs our manager
        /// </summary>
        public TimerManager(GameClient Client)
        {
            this.Client = Client;
            ActiveTimers = new ConcurrentDictionary<string, RoleplayTimer>();

            // Start up our Forever timers
            CreateTimer("hunger", 1000, true);
            CreateTimer("hygiene", 1000, true);
            CreateTimer("conditioncheck", 1000, true);
            CreateTimer("interest", 1000, true);
        }
        /// <summary>
        /// Creates a timer
        /// </summary>
        public RoleplayTimer CreateTimer(string Type, int Time, bool Forever, params object[] Params)
        {
            if (ActiveTimers.ContainsKey(Type))
                return null;

            RoleplayTimer Timer = GetTimerFromType(Type, Time, Forever, Params);

            if (Timer == null)
                return null;

            ActiveTimers.TryAdd(Type, Timer);

            return Timer;
        }

        /// <summary>
        /// Returns a new timer based on the type
        /// </summary>
        /// <param name="TypeOfTimer"></param>
        private RoleplayTimer GetTimerFromType(string TypeOfTimer, int Time, bool Forever, object[] Params)
        {
            switch (TypeOfTimer)
            {
                case "hunger":
				case "fome":
                    return new HungerTimer(TypeOfTimer, Client, Time, Forever, Params);
                case "hygiene":
				case "higiene":
                    return new HygieneTimer(TypeOfTimer, Client, Time, Forever, Params);
                case "conditioncheck":
                    return new ConditionCheckTimer(TypeOfTimer, Client, Time, Forever, Params);
                case "heal":
				case "curar":
                    return new HealTimer(TypeOfTimer, Client, Time, Forever, Params);
                case "shower":
                    return new ShowerTimer(TypeOfTimer, Client, Time, Forever, Params);
                case "work":
				case "trabalhar":
                    return new WorkTimer(TypeOfTimer, Client, Time, Forever, Params);
                case "sendhome":
                    return new SendhomeTimer(TypeOfTimer, Client, Time, Forever, Params);
                case "death":
				case "morto":
                    return new DeathTimer(TypeOfTimer, Client, Time, Forever, Params);
                case "jail":
				case "preso":
                    return new JailTimer(TypeOfTimer, Client, Time, Forever, Params);
                case "stun":
				case "atordoado":
                    return new StunTimer(TypeOfTimer, Client, Time, Forever, Params);
                case "spray":
				case "pulverizado":
                    return new SprayTimer(TypeOfTimer, Client, Time, Forever, Params);
                case "cuff":
				case "algemado":
                    return new CuffTimer(TypeOfTimer, Client, Time, Forever, Params);
                case "wanted":
				case "procurado":
                    return new WantedTimer(TypeOfTimer, Client, Time, Forever, Params);
                case "probation":
                    return new ProbationTimer(TypeOfTimer, Client, Time, Forever, Params);
                case "turfcapture":
                    return new TurfCaptureTimer(TypeOfTimer, Client, Time, Forever, Params);
                case "workout":
				case "capturar":
                    return new WorkoutTimer(TypeOfTimer, Client, Time, Forever, Params);
                case "capture":
                    return new EventCaptureTimer(TypeOfTimer, Client, Time, Forever, Params);
                case "interest":
                    return new InterestTimer(TypeOfTimer, Client, Time, Forever, Params);
                case "noob":
                    return new NoobTimer(TypeOfTimer, Client, Time, Forever, Params);
                case "repair":
				case "reparar":
                    return new RepairTimer(TypeOfTimer, Client, Time, Forever, Params);
                default:
                    return null;
            }
        }

        /// <summary>
        /// Ends all of the timers
        /// </summary>
        public void EndAllTimers()
        {
            lock (ActiveTimers.Values)
            {
                foreach (RoleplayTimer Timer in ActiveTimers.Values)
                    Timer.EndTimer();
            }
        }
    }

    public class BotTimerManager
    {
        /// <summary>
        /// The bot
        /// </summary>
        public RoleplayBot CachedBot;

        /// <summary>
        /// Contains all running timers
        /// </summary>
        public ConcurrentDictionary<string, BotRoleplayTimer> ActiveTimers;

        /// <summary>
        /// Constructs our manager
        /// </summary>
        public BotTimerManager(RoleplayBot CachedBot)
        {
            this.CachedBot = CachedBot;
            ActiveTimers = new ConcurrentDictionary<string, BotRoleplayTimer>();

            if (this.CachedBot == null)
                return;

            if (this.CachedBot.DRoomUser == null)
                return;

            if (this.CachedBot.RoamBot)
                this.CachedBot.MoveRandomly();

        }

        /// <summary>
        /// Creates a timer
        /// </summary>
        public BotRoleplayTimer CreateTimer(string Type, RoleplayBot CachedBot, int Time, bool Forever, params object[] Params)
        {
            if (ActiveTimers.ContainsKey(Type))
                return null;

            BotRoleplayTimer Timer = GetTimerFromType(Type, CachedBot, Time, Forever, Params);

            if (Timer == null)
                return null;

            ActiveTimers.TryAdd(Type, Timer);

            return Timer;
        }

        /// <summary>
        /// Returns a new timer based on the type
        /// </summary>
        /// <param name="TypeOfTimer"></param>
        private BotRoleplayTimer GetTimerFromType(string TypeOfTimer, RoleplayBot CachedBot, int Time, bool Forever, object[] Params)
        {
            switch (TypeOfTimer)
            {
                // Default Bots
                case "startwork":
				case "trabalhar":
                    return new StartWorkTimer(TypeOfTimer, CachedBot, Time, Forever, Params);
                case "stopwork":
				case "ptrabalhar":
                    return new StopWorkTimer(TypeOfTimer, CachedBot, Time, Forever, Params);

                // Hospital Bots
                case "discharge":
				case "reviver":
                    return new DischargeTimer(TypeOfTimer, CachedBot, Time, Forever, Params);

                // Serving Bots
                case "serving":
				case "serve":
				case "servir":
                    return new ServingTimer(TypeOfTimer, CachedBot, Time, Forever, Params);

                // Gun Store Bots
                case "deliverywait":
                    return new DeliveryWaitTimer(TypeOfTimer, CachedBot, Time, Forever, Params);
                case "pickupdelivery":
                    return new PickupDeliveryTimer(TypeOfTimer, CachedBot, Time, Forever, Params);

                // Delivery Bots
                case "startdelivery":
                    return new StartDeliveryTimer(TypeOfTimer, CachedBot, Time, Forever, Params);
                case "stopdelivery":
                    return new StopDeliveryTimer(TypeOfTimer, CachedBot, Time, Forever, Params);

                // Thug Bots
                case "attack":
				case "atacar":
                    return new AttackTimer(TypeOfTimer, CachedBot, Time, Forever, Params);
                case "botdeath":
                    return new BotDeathTimer(TypeOfTimer, CachedBot, Time, Forever, Params);

                // Jury Bots
                case "jury":
				case "juiz":
                    return new JuryTimer(TypeOfTimer, CachedBot, Time, Forever, Params);

                default:
                    return null;
            }
        }

        /// <summary>
        /// Ends all of the timers
        /// </summary>
        public void EndAllTimers()
        {
            lock (ActiveTimers.Values)
            {
                foreach (BotRoleplayTimer Timer in ActiveTimers.Values)
                    Timer.EndTimer();
            }
        }
    }

    public class SystemTimerManager
    {
        /// <summary>
        /// Contains all running timers
        /// </summary>
        public ConcurrentDictionary<string, SystemRoleplayTimer> ActiveTimers;

        /// <summary>
        /// Constructs our manager
        /// </summary>
        public SystemTimerManager()
        {
            ActiveTimers = new ConcurrentDictionary<string, SystemRoleplayTimer>();
            CreateTimer("farmingspace", 1000, true);
            CreateTimer("daynight", 5000, true);
        }

        /// <summary>
        /// Creates a timer
        /// </summary>
        public SystemRoleplayTimer CreateTimer(string Type, int Time, bool Forever, params object[] Params)
        {
            if (ActiveTimers.ContainsKey(Type))
                return null;

            SystemRoleplayTimer Timer = GetTimerFromType(Type, Time, Forever, Params);

            if (Timer == null)
                return null;

            ActiveTimers.TryAdd(Type, Timer);

            return Timer;
        }

        /// <summary>
        /// Returns a new timer based on the type
        /// </summary>
        /// <param name="TypeOfTimer"></param>
        private SystemRoleplayTimer GetTimerFromType(string TypeOfTimer, int Time, bool Forever, object[] Params)
        {
            switch (TypeOfTimer)
            {
                case "farmingspace":
				case "eagricola":
                    return new FarmingSpaceTimer(TypeOfTimer, Time, Forever, Params);
                case "jailbreak":
				case "fugirprisao":
                    return new JailbreakTimer(TypeOfTimer, Time, Forever, Params);
                case "dynamite":
				case "dinamite":
                    return new DynamiteTimer(TypeOfTimer, Time, Forever, Params);
                case "matchingpoll":
				case "pergunta":
                    return new MatchingPollTimer(TypeOfTimer, Time, Forever, Params);
                case "texasholdem":
				case "texashold":
                    return new TexasHoldEmTimer(TypeOfTimer, Time, Forever, Params);
                case "juryinvitation":
				case "juriconvite":
				case "juizconvite":
                    return new InvitationTimer(TypeOfTimer, Time, Forever, Params);
                case "websocketchatmanager":
				case "chats":
				case "grupos":
                    return new WebSocketChatManagerMainTimer(TypeOfTimer, Time, Forever, Params);
                case "daynight":
				case "dianoite":
                    return new DayNightCycleTimer(TypeOfTimer, Time, Forever, Params);
                case "nuking":
				case "explosao":
                    return new NukeTimer(TypeOfTimer, Time, Forever, Params);
                case "nuking_bd":
                    return new BreakdownTimer(TypeOfTimer, Time, Forever, Params);
                default:
                    return null;
            }
        }

        /// <summary>
        /// Ends all of the timers
        /// </summary>
        public void EndAllTimers()
        {
            lock (ActiveTimers.Values)
            {
                foreach (SystemRoleplayTimer Timer in ActiveTimers.Values)
                    Timer.EndTimer();
            }
        }
    }
}