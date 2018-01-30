using System;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Fleck;

using Plus.HabboHotel.GameClients;
using System.IO;

namespace Plus.HabboHotel.Roleplay.Web.OutGoing.Misc
{
    /// <summary>
    /// CaptchaWebEvent class.
    /// </summary>
    class CaptchaWebEvent : IWebEvent
    {
        /// <summary>
        /// Executes socket data.
        /// </summary>
        /// <param name="Client"></param>
        /// <param name="Data"></param>
        /// <param name="Socket"></param>
        public void Execute(GameClient Client, string Data, IWebSocketConnection Socket)
        {
            if (!PlusEnvironment.GetGame().GetWebEventManager().SocketReady(Client, true) || !PlusEnvironment.GetGame().GetWebEventManager().SocketReady(Socket))
                return;

            string Action = Data.Split(',')[0];
            string Title = Data.Split(',')[1];

            switch (Action)
            {
                case "create":
				case "criar":
                    Socket.Send("compose_captchabox:" + Data);
                    Client.GetRoleplay().CaptchaSent = true;
                    break;

                case "complete":
				case "completo":
				case "completar":
                    Client.GetRoleplay().CaptchaSent = false;
                    Client.GetRoleplay().CaptchaTime = 0;
                    break;

                case "regenerate":
				case "regenerar":
                    Client.GetRoleplay().CreateCaptcha(Title);
                    break;
            }
        }
    }
}
