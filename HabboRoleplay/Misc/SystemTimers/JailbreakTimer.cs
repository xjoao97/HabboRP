using System;
using Plus.HabboHotel.GameClients;
using Plus.HabboHotel.Groups;
using Plus.HabboRoleplay.Misc;
using Plus.HabboRoleplay.RoleplayUsers;
using Plus.HabboHotel.Items;
using Plus.Core;
using System.Linq;
using System.Collections.Generic;
using Plus.HabboHotel.Rooms;

namespace Plus.HabboRoleplay.Timers.Types
{
    /// <summary>
    /// Countdown to stop jailbreak
    /// </summary>
    public class JailbreakTimer : SystemRoleplayTimer
    {
        public JailbreakTimer(string Type, int Time, bool Forever, object[] Params) 
            : base(Type, Time, Forever, Params)
        {
            // minutes converted to miliseconds
            int JailbreakTime = Convert.ToInt32(RoleplayData.GetData("jailbreak", "timer"));
            TimeLeft = JailbreakTime * 60000;
            TimeCount = 0;
        }
 
        /// <summary>
        /// Ends the jailbreak
        /// </summary>
        public override void Execute()
        {
            try
            {
                if (!JailbreakManager.JailbreakActivated)
                {
                    base.EndTimer();
                    return;
                }

                List<GameClient> CurrentJailbrokenUsers = PlusEnvironment.GetGame().GetClientManager().GetClients.Where(x => x != null && x.GetHabbo() != null && x.GetRoleplay() != null && x.GetRoleplay().Jailbroken).ToList();
                GameClient UserJailbreaking = JailbreakManager.UserJailbreaking;

                if (CurrentJailbrokenUsers.Count <= 0)
                {
                    JailbreakManager.JailbreakActivated = false;
                    if (JailbreakManager.FenceBroken)
                    {
                        Room Room = RoleplayManager.GenerateRoom(Convert.ToInt32(RoleplayData.GetData("jail", "outsideroomid")));

                        if (Room != null)
                            JailbreakManager.GenerateFence(Room);
                        JailbreakManager.FenceBroken = false;
                    }
                    MessagePoliceOfficers();
                    base.EndTimer();
                    return;
                }

                if (UserJailbreaking != null || UserJailbreaking.GetHabbo().CurrentRoom != null || UserJailbreaking.GetRoomUser() != null)
                {
                    if (UserJailbreaking.GetHabbo().CurrentRoomId != Convert.ToInt32(RoleplayData.GetData("jail", "outsideroomid")))
                    {
                        JailbreakManager.JailbreakActivated = false;
                        if (JailbreakManager.FenceBroken)
                        {
                            Room Room = RoleplayManager.GenerateRoom(Convert.ToInt32(RoleplayData.GetData("jail", "outsideroomid")));

                            if (Room != null)
                                JailbreakManager.GenerateFence(Room);
                            JailbreakManager.FenceBroken = false;
                        }

                        foreach (GameClient Client in CurrentJailbrokenUsers)
                        {
                            if (Client == null || Client.GetRoleplay() == null || Client.GetHabbo() == null)
                                continue;

                            if (Client.GetRoleplay().Jailbroken && !JailbreakManager.FenceBroken)
                                Client.GetRoleplay().Jailbroken = false;

                            if (Client.GetHabbo().CurrentRoomId == Convert.ToInt32(RoleplayData.GetData("jail", "insideroomid")))
                            {
                                RoleplayManager.GetLookAndMotto(Client);
                                RoleplayManager.SpawnBeds(Client, "bed_silo_one");
                                Client.SendNotification("O processo para fuga da prisão parou, então você voltou para ela!");
                            }
                            else
                            {
                                Client.SendNotification("O processo para fuga da prisão parou, então você voltou para ela!");
                                RoleplayManager.SendUser(Client, Convert.ToInt32(RoleplayData.GetData("jail", "insideroomid")));
                            }
                        }

                        MessagePoliceOfficers();

                        RoleplayManager.Shout(UserJailbreaking, "*Para a fuga*", 4);
                        base.EndTimer();
                        return;
                    }
                }

                TimeCount++;
                TimeLeft -= 1000;

                if (TimeLeft > 0)
                {
                    if (TimeCount == 60)
                    {
                        if (UserJailbreaking != null || UserJailbreaking.GetHabbo().CurrentRoom != null || UserJailbreaking.GetRoomUser() != null)
                            RoleplayManager.Shout(UserJailbreaking, "*Estou mais perto de terminar a fuga dos prisioneiros [" + (TimeLeft / 60000) + " minutos restantes]*", 4);

                        TimeCount = 0;
                    }
                    return;
                }

                foreach (GameClient Client in CurrentJailbrokenUsers)
                {
                    if (Client == null || Client.GetRoleplay() == null || Client.GetHabbo() == null)
                        continue;

                    RoleplayManager.Shout(Client, "*Escapa completamente da prisão graças a um parceiro*", 4);
                    PlusEnvironment.GetGame().GetAchievementManager().ProgressAchievement(Client, "ACH_Jailbreak", 1);
                    Client.GetRoleplay().Jailbroken = false;
                    Client.GetRoleplay().IsWanted = false;
                    Client.GetRoleplay().IsJailed = false;
                    Client.GetHabbo().Poof();
                }

                if (JailbreakManager.FenceBroken)
                {
                    Room Room = RoleplayManager.GenerateRoom(Convert.ToInt32(RoleplayData.GetData("jail", "outsideroomid")));

                    if (Room != null)
                        JailbreakManager.GenerateFence(Room);
                    JailbreakManager.FenceBroken = false;
                }
                JailbreakManager.JailbreakActivated = false;
                JailbreakManager.UserJailbreaking = null;
                base.EndTimer();
                return;
            }
            catch (Exception e)
            {
                Logging.LogRPTimersError("Error in Execute() void: " + e);
                base.EndTimer();
            }
        }

        public void MessagePoliceOfficers()
        {
            lock (PlusEnvironment.GetGame().GetClientManager().GetClients.ToList())
            {
                foreach (GameClient client in PlusEnvironment.GetGame().GetClientManager().GetClients.ToList())
                {
                    if (client == null || client.GetHabbo() == null || client.GetRoleplay() == null)
                        continue;

                    if (!GroupManager.HasJobCommand(client, "radio") && !client.GetHabbo().GetPermissions().HasRight("corporation_rights"))
                        continue;

                    if (GroupManager.HasJobCommand(client, "radio"))
                    {
                        if (!client.GetRoleplay().IsWorking)
                            continue;

                        if (!client.GetRoleplay().HandlingJailbreaks)
                            continue;

                        if (client.GetRoleplay().DisableRadio)
                            continue;
                    }

                    client.SendWhisper("[RÁDIO] [Fuga da Prisão] Parece que todos os prisioneiros foram capturados e a cerca foi reparada! Grande trabalho a todos!", 30);
                }
            }
        }
    }
}