using System;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Fleck;

using Plus.HabboHotel.GameClients;

namespace Plus.HabboHotel.Roleplay.Web
{
    public interface IWebEvent
    {
        void Execute(GameClient Client, string Data, IWebSocketConnection Socket);
    }
}
