using System;
using Plus.HabboHotel.GameClients;
using Plus.HabboRoleplay.Misc;
using Plus.Core;
using Plus.HabboHotel.Guides;
using Plus.HabboHotel.Items;
using System.Collections.Generic;
using System.Linq;
using Plus.Communication.Packets.Outgoing.Guides;
using Plus.HabboRoleplay.Gambling;

namespace Plus.HabboRoleplay.Events.Methods
{
    /// <summary>
    /// Triggered when the user disconnects
    /// </summary>
    public class OnDisconnect : IEvent
    {
        /// <summary>
        /// Responds to the event
        /// </summary>
        public void Execute(object Source, object[] Params)
        {
            GameClient Client = (GameClient)Source;

            if (Client == null)
                return;

            if (Client.GetRoleplay() == null)
                return;

            if (Client.GetHabbo() == null)
                return;

            if (Client.GetHabbo()._disconnected)
                return;

            if (Client.GetRoleplay().IsWorking)
                WorkManager.RemoveWorkerFromList(Client);

            if (RoleplayManager.InvitedUsersToJuryDuty.Contains(Client))
                RoleplayManager.InvitedUsersToJuryDuty.Remove(Client);

            if (Client.GetRoleplay().TexasHoldEmPlayer > 0)
            {
                var Game = TexasHoldEmManager.GetGameForUser(Client.GetHabbo().Id);

                if (Game != null)
                    Game.RemovePlayerFromGame(Client.GetHabbo().Id);
            }
            
            Client.GetRoleplay().CloseInteractingUserDialogues();

            var GuideManager = PlusEnvironment.GetGame().GetGuideManager();
            if (GuideManager != null && Client != null)
            {
                if (GuideManager.AllPolice.Contains(Client))
                    GuideManager.AllPolice.Remove(Client);
                if (GuideManager.GuardiansOnDuty.Contains(Client))
                    GuideManager.GuardiansOnDuty.Remove(Client);
                if (GuideManager.GuidesOnDuty.Contains(Client))
                    GuideManager.GuidesOnDuty.Remove(Client);
                if (GuideManager.HelpersOnDuty.Contains(Client))
                    GuideManager.HelpersOnDuty.Remove(Client);

                #region End Existing Calls
                if (Client.GetRoleplay() != null && Client.GetRoleplay().GuideOtherUser != null)
                {
                    Client.GetRoleplay().GuideOtherUser.SendMessage(new OnGuideSessionDetachedComposer(0));
                    Client.GetRoleplay().GuideOtherUser.SendMessage(new OnGuideSessionDetachedComposer(1));
                    if (Client.GetRoleplay().GuideOtherUser.GetRoleplay() != null)
                    {
                        Client.GetRoleplay().GuideOtherUser.GetRoleplay().Sent911Call = false;
                        Client.GetRoleplay().GuideOtherUser.GetRoleplay().GuideOtherUser = null;
                    }

                    Client.GetRoleplay().GuideOtherUser = null;
                    Client.SendMessage(new OnGuideSessionDetachedComposer(0));
                    Client.SendMessage(new OnGuideSessionDetachedComposer(1));
                }
                #endregion
            }

            if (RoleplayData.GetData("farming", "room") != null)
            {
                int RoomId = Convert.ToInt32(RoleplayData.GetData("farming", "room"));
                var Room = RoleplayManager.GenerateRoom(RoomId);

                if (Room != null && Room.GetRoomItemHandler() != null && Room.GetRoomItemHandler().GetFloor != null)
                {
                    List<Item> Items = Room.GetRoomItemHandler().GetFloor.Where(x => x.GetBaseItem().InteractionType == InteractionType.FARMING && x.FarmingData != null && x.FarmingData.OwnerId == Client.GetHabbo().Id).ToList();

                    foreach (Item Item in Items)
                    {
                        Room.GetRoomItemHandler().RemoveFurniture(null, Item.Id);
                    }
                }
            }

            Client.GetRoleplay().EndCycle();

            if (RoleplayManager.WantedList != null)
            {
                if (RoleplayManager.WantedList.ContainsKey(Client.GetHabbo().Id))
                {
                    Wanted Junk;
                    RoleplayManager.WantedList.TryRemove(Client.GetHabbo().Id, out Junk);
                }
            }

            Logging.WriteLine(Client.GetHabbo().Username + " has logged out!", ConsoleColor.DarkGray);
        }
    }
}