using System;
using System.Threading;
using Plus.HabboHotel.GameClients;
using Plus.HabboRoleplay.Bots;
using Plus.Core;

namespace Plus.HabboRoleplay.Cooldowns
{
    public abstract class Cooldown
    {
        /// <summary>
        /// The client
        /// </summary>
        public GameClient Client;

        /// <summary>
        /// The timer for cooldowns
        /// </summary>
        private Timer CooldownTimer;

        /// <summary>
        /// The type of timer
        /// </summary>
        public string Type;

        /// <summary>
        /// Time interval
        /// </summary>
        private int Time;

        /// <summary>
        /// The amount of ticks (usually in seconds)
        /// </summary>
        public int Amount;

        /// <summary>
        /// Random number generator
        /// </summary>
        public Random Random = new Random();

        /// <summary>
        /// Represents the time left if specified
        /// </summary>
        public int TimeLeft = 0;

        public bool Ended = false;

        /// <summary>
        /// Constructor
        /// </summary>
        public Cooldown(string Type, GameClient Client, int Time, int Amount)
        {
            this.Type = Type;
            this.Client = Client;
            this.Time = Time;
            this.Amount = Amount;
            this.CooldownTimer = new Timer(Finished, null, Time, Time);
        }

        /// <summary>
        /// Called when the timer finishes/ticks
        /// </summary>
        private void Finished(object State)
        {
            try
            {
                Execute();

                if (TimeLeft <= 0)
                    EndCooldown();
            }
            catch (Exception e)
            {
                Logging.LogRPTimersError("Ocorreu um erro ao tentar terminar um temporizador de contagem: " + e);
                EndCooldown();
            }
        }

        /// <summary>
        /// Ends our timer
        /// </summary>
        public void EndCooldown()
        {
            try
            {
                if (this.Ended)
                    return;

                if (CooldownTimer == null)
                    return;

                CooldownTimer.Change(Timeout.Infinite, Timeout.Infinite);
                CooldownTimer.Dispose();
                CooldownTimer = null;

                if (Client != null && Client.GetRoleplay() != null)
                {
                    Cooldown Junk;
                    Client.GetRoleplay().CooldownManager.ActiveCooldowns.TryRemove(Type, out Junk);
                }

                this.Ended = true;
            }
            catch (Exception e)
            {
                Logging.LogRPTimersError("Ocorreu um erro em fechar a contagem() vazio: " + e);
            }
        }

        /// <summary>
        /// Called when the cooldown finishes/ticks
        /// </summary>
        public abstract void Execute();
    }

    public abstract class BotCooldown
    {
        /// <summary>
        /// The bot
        /// </summary>
        public RoleplayBot Bot;

        /// <summary>
        /// The timer for cooldowns
        /// </summary>
        private Timer CooldownTimer;

        /// <summary>
        /// The type of timer
        /// </summary>
        public string Type;

        /// <summary>
        /// Time interval
        /// </summary>
        private int Time;

        /// <summary>
        /// The amount of ticks (usually in seconds)
        /// </summary>
        public int Amount;

        /// <summary>
        /// Random number generator
        /// </summary>
        public Random Random = new Random();

        /// <summary>
        /// Represents the time left if specified
        /// </summary>
        public int TimeLeft = 0;

        public bool Ended = false;

        /// <summary>
        /// Constructor
        /// </summary>
        public BotCooldown(string Type, RoleplayBot CachedBot, int Time, int Amount)
        {
            this.Type = Type;
            this.Bot = CachedBot;
            this.Time = Time;
            this.Amount = Amount;
            this.CooldownTimer = new Timer(Finished, null, Time, Time);
        }

        /// <summary>
        /// Called when the timer finishes/ticks
        /// </summary>
        private void Finished(object State)
        {
            try
            {
                Execute();

                if (TimeLeft <= 0)
                    EndCooldown();
            }
            catch (Exception e)
            {
                Logging.LogRPTimersError("Ocorreu um erro ao tentar terminar um temporizador: " + e);
                EndCooldown();
            }
        }

        /// <summary>
        /// Ends our timer
        /// </summary>
        public void EndCooldown()
        {
            try
            {
                if (this.Ended)
                    return;

                if (CooldownTimer == null)
                    return;


                CooldownTimer.Change(Timeout.Infinite, Timeout.Infinite);
                CooldownTimer.Dispose();
                CooldownTimer = null;

                if (Bot != null && Bot.DRoomUser.GetBotRoleplay() != null)
                {
                    BotCooldown Junk;
                    Bot.CooldownManager.ActiveCooldowns.TryRemove(Type, out Junk);
                }

                this.Ended = true;
            }
            catch (Exception e)
            {
                Logging.LogRPTimersError("Ocorreu um erro em fechar a contagem() vazio: " + e);
            }
        }

        /// <summary>
        /// Called when the cooldown finishes/ticks
        /// </summary>
        public abstract void Execute();
    }

}