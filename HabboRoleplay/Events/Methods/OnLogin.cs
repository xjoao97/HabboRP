using System;
using System.Threading;
using System.Collections.Generic;
using Plus.HabboHotel.GameClients;
using Plus.HabboRoleplay.Misc;
using Plus.HabboHotel.Groups;
using Plus.HabboHotel.Rooms;
using Plus.HabboHotel.Items;
using Plus.Communication.Packets.Outgoing.Navigator;
using Plus.Core;

namespace Plus.HabboRoleplay.Events.Methods
{
    public class OnLogin : IEvent
    {
        /// <summary>
        /// Responds to the event
        /// </summary>
        public void Execute(object Source, object[] Params)
        {
            GameClient Client = (GameClient)Source;
            if (Client == null || Client.GetRoleplay() == null || Client.GetHabbo() == null)
                return;

            if (Client.GetRoleplay().OriginalOutfit == null)
                Client.GetRoleplay().OriginalOutfit = Client.GetHabbo().Look;

            if (Client.GetRoleplay().Cuffed && !Client.GetRoleplay().TimerManager.ActiveTimers.ContainsKey("cuff"))
                Client.GetRoleplay().TimerManager.CreateTimer("cuff", 1000, false);

            DeathCheck(Client);
            JailCheck(Client);
            NoobCheck(Client);
            SocketConnection(Client);
        }

        #region SocketConnection
        public void SocketConnection(GameClient Client)
        {
            Client.GetRoleplay().RefreshStatDialogue();
            Logging.WriteLine(Client.GetHabbo().Username + " has logged in!", ConsoleColor.DarkGreen);
        }
        #endregion

        #region DeathCheck
        /// <summary>
        /// Checks if the client is dead, if so send the user to hospital
        /// </summary>
        public void DeathCheck(GameClient Client)
        {
            if (!Client.GetRoleplay().IsDead)
                return;

            int HospitalRID = Convert.ToInt32(RoleplayData.GetData("hospital", "roomid2"));

            if (Client.GetHabbo().HomeRoom != HospitalRID)
                Client.GetHabbo().HomeRoom = HospitalRID;

            RoleplayManager.SendUser(Client, HospitalRID, "");
            Client.GetRoleplay().TimerManager.CreateTimer("death", 1000, true);
        }
        #endregion

        #region JailCheck
        /// <summary>
        /// Checks if the client is jailed, if so send the user to jail
        /// </summary>
        public void JailCheck(GameClient Client)
        {
            if (!Client.GetRoleplay().IsJailed)
                return;

            if (JailbreakManager.JailbreakActivated)
            {
                Client.GetRoleplay().Jailbroken = true;
                Client.SendNotification("Someone has initiated a jailbreak while you were offline! Better run before you get caught!");
                return;
            }

            int JailRID = Convert.ToInt32(RoleplayData.GetData("jail", "insideroomid"));

            if (Client.GetRoleplay().IsWanted || Client.GetRoleplay().WantedLevel != 0 || Client.GetRoleplay().WantedTimeLeft != 0)
            {
                Client.GetRoleplay().IsWanted = false;
                Client.GetRoleplay().WantedLevel = 0;
                Client.GetRoleplay().WantedTimeLeft = 0;
            }

            if (Client.GetHabbo().HomeRoom != JailRID)
                Client.GetHabbo().HomeRoom = JailRID;

            RoleplayManager.SendUser(Client, JailRID, "");
            Client.GetRoleplay().TimerManager.CreateTimer("jail", 1000, true);
        }
        #endregion

        #region NoobCheck

        /// <summary>
        /// Checks if the client is jailed, if so send the user to jail
        /// </summary>
        public void NoobCheck(GameClient Client)
        {
            if (!Client.GetRoleplay().IsNoob)
                return;

            Client.GetRoleplay().TimerManager.CreateTimer("noob", 1000, true);
        }
        #endregion
    }
}