using System;
using System.Collections.Concurrent;
using Plus.HabboRoleplay.Events.Methods;
using log4net;

namespace Plus.HabboRoleplay.Events
{
    public static class EventManager
    {
        /// <summary>
        /// log4net
        /// </summary>
        private static readonly ILog log = LogManager.GetLogger("Plus.HabboRoleplay.Misc.RoleplayData");

        /// <summary>
        /// Dictionary containing the events
        /// </summary>
        public static ConcurrentDictionary<string, IEvent> Events;

        /// <summary>
        /// Registers the events
        /// </summary>
        public static void Initialize()
        {
            Events = new ConcurrentDictionary<string, IEvent>();

            Events.TryAdd("OnAddedToRoom", new OnAddedToRoom());
            Events.TryAdd("OnHealthChange", new OnHealthChange());
            Events.TryAdd("OnLogin", new OnLogin());
            Events.TryAdd("OnDisconnect", new OnDisconnect());
            Events.TryAdd("OnTeleport", new OnTeleport());

            //log.Info("Gerenciado de Eventos: (" + Events.Count + ") -> Carregados!");
        }

        /// <summary>
        /// Triggers an event
        /// </summary>
        public static void TriggerEvent(string EventName, object Source, params object[] Params)
        {
            if (!Events.ContainsKey(EventName))
                return;

            IEvent Event = Events[EventName];
            Event.Execute(Source, Params);
        }
    }
}