using Plus.HabboHotel.GameClients;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Plus.HabboRoleplay.Bots.Manager.TimerHandlers
{
    public interface IBotHandler
    {
        GameClient InteractingUser { get; set; }
        RoleplayBot InteractingBot { get; set; }
        bool Active { get; set; }
        ConcurrentDictionary<string, string> Values { get; set; }


        bool ExecuteHandler(params object[] Params);
        bool AbortHandler(params object[] Params);
        bool RestartHandler(params object[] Params);
        void AssignInteractingUser(GameClient InteractingUser);
        void SetValues(string Key, string Value);
    }
}
