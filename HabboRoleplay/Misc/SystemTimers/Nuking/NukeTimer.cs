using System;
using System.Collections.Generic;
using System.Linq;

using Plus.HabboHotel.GameClients;
using Plus.HabboHotel.Groups;
using Plus.HabboRoleplay.Misc;
using Plus.HabboRoleplay.RoleplayUsers;
using Plus.HabboHotel.Items;
using Plus.Core;
using Plus.HabboHotel.Rooms;
using Plus.Communication.Packets.Outgoing.Rooms.Notifications;
using System.Threading;

namespace Plus.HabboRoleplay.Timers.Types
{
    /// <summary>
    /// Countdown to nuke the RP
    /// </summary>
    public class NukeTimer : SystemRoleplayTimer
    {
        public NukeTimer(string Type, int Time, bool Forever, object[] Params) 
            : base(Type, Time, Forever, Params)
        {
            // 2 minutes converted to milliseconds
            int NukeTime = RoleplayManager.NukeMinutes;
            TimeLeft = NukeTime * 60000;
            TimeCount = 0;
        }

        /// <summary>
        /// Nuke process
        /// </summary>
        public override void Execute()
        {
            try
            {
                GameClient Nuker = (GameClient)Params[0];

                if (Nuker == null || Nuker.LoggingOut || Nuker.GetHabbo() == null || Nuker.GetRoleplay() == null || Nuker.GetRoleplay().IsDead || Nuker.GetRoleplay().IsJailed)
                {
                    lock (PlusEnvironment.GetGame().GetClientManager().GetClients)
                    {
                        foreach (GameClient Client in PlusEnvironment.GetGame().GetClientManager().GetClients.ToList())
                        {
                            if (Client == null || Client.GetHabbo() == null)
                                continue;

                            Client.SendWhisper("[NPA Alerta] Os membros da NPA protegeram a cidade da atividade de explosão suspeita e a cidade é marcada como segura! Agradecemos a eles!", 34);
                        }
                    }

                    base.EndTimer();
                    return;
                }

                Room Room = RoleplayManager.GenerateRoom(Nuker.GetHabbo().CurrentRoomId);

                if (Room == null)
                    return;

                List<Item> Items = Room.GetGameMap().GetRoomItemForSquare(Nuker.GetRoomUser().Coordinate.X, Nuker.GetRoomUser().Coordinate.Y);

                if (Items.Count < 1)
                {
                    RoleplayManager.Shout(Nuker, "*Para o processo de demolição da cidade*", 4);
                    base.EndTimer();
                    return;
                }

                bool HasCaptureTile = Items.ToList().Where(x => x.GetBaseItem().ItemName == "actionpoint01").ToList().Count() > 0;

                if (!HasCaptureTile)
                {
                    RoleplayManager.Shout(Nuker, "*Para o processo de demolição da cidade*", 4);
                    base.EndTimer();
                    return;
                }

                TimeCount++;
                TimeLeft -= 1000;

                if (TimeLeft > 0)
                {
                    if (TimeCount == 60)
                    {
                        RoleplayManager.Shout(Nuker, "*Chega mais perto de completar a demolição da cidade [" + (TimeLeft / 60000) + " minutos restantes]*", 4);

                        #region Warn all on-duty NPA associates

                        lock (PlusEnvironment.GetGame().GetClientManager().GetClients.ToList())
                        {
                            foreach (GameClient client in PlusEnvironment.GetGame().GetClientManager().GetClients.ToList())
                            {
                                if (client == null || client.GetHabbo() == null || client.GetRoleplay() == null)
                                    continue;

                                if (!GroupManager.HasJobCommand(client, "npa") && !client.GetHabbo().GetPermissions().HasRight("corporation_rights"))
                                    continue;

                                if (!client.GetRoleplay().IsWorking)
                                    continue;

                                if (client.GetRoleplay().DisableRadio)
                                    continue;

                                client.SendWhisper("[RÁDIO] [BOMBA NUCLERA] Atenção! Alguém entrou na máquina nuclear, e ordenou que explodisse a cidade! Você tem " + (TimeLeft / 60000) + " minuto(s) restantes para interromper o processo!", 30);
                            }
                        }

                        #endregion

                        TimeCount = 0;
                    }
                    return;
                }

                int Counter = 25;
                int KillsGained = 0;

                new Thread(() =>
                {
                    while (Counter > 0)
                    {
                        #region Global Warming

                        lock (PlusEnvironment.GetGame().GetClientManager().GetClients.ToList())
                        {
                            foreach (GameClient Client in PlusEnvironment.GetGame().GetClientManager().GetClients.ToList())
                            {
                                if (Client == null || Client.GetHabbo() == null)
                                    continue;

                                Client.SendWhisper("[PERIGO Global] [BOMBA NUCLEAR] Uma bomba nuclear está prestes a explodir, você deve se apressar rapidamente para o hospital... (" + Counter + " segundos)", 1);
                            }
                        }

                        #endregion

                        Counter--;
                        Thread.Sleep(1000);

                        if (Counter == 0)
                        {
                            #region Kill any un-safe citizens.

                            lock (PlusEnvironment.GetGame().GetClientManager().GetClients.ToList())
                            {
                                foreach (GameClient Client in PlusEnvironment.GetGame().GetClientManager().GetClients.ToList())
                                {
                                    if (Client == null || Client.GetHabbo() == null || Client.GetRoleplay() == null)
                                        continue;

                                    if (Client.GetHabbo().GetPermissions().HasRight("mod_tool") && Client.GetRoleplay().StaffOnDuty || Client.GetHabbo().VIPRank > 1)
                                        continue;

                                    if (Client.GetRoleplay().IsJailed || Client.GetRoleplay().IsDead)
                                        continue;

                                    if (Client.GetRoleplay().Game != null || Client.GetRoleplay().Team != null)
                                        continue;

                                    if (Client.GetHabbo().CurrentRoomId == Convert.ToInt32(RoleplayData.GetData("hospital", "roomid")) && Client.GetHabbo().CurrentRoomId == Convert.ToInt32(RoleplayData.GetData("hospital", "roomid2")))
                                        Client.SendWhisper("[Alerta SEGURO] A bomba explodiu, mas você foi refugiado no Hospital. Bom trabalho!", 1);
                                    else if (Client.GetHabbo().CurrentRoom.SafeZoneEnabled)
                                        Client.SendWhisper("[Alerta SEGURO] A bomba explodiu, mas você foi refugiado em uma Zona Segura. Bom trabalho!", 1);
                                    else if (Client.GetHabbo().CurrentRoomId == Convert.ToInt32(RoleplayData.GetData("npa", "insideroomid")))
                                        Client.SendWhisper("[Alerta SEGUR] A explosão de armas nucleares explodiu, mas você foi refugiado no quarto NPA. Bom trabalho!", 1);
                                    else
                                    {
                                        // Kill the un-safe users.
                                        KillsGained++;
                                        Client.GetRoleplay().CurHealth = 0;

                                        Client.SendNotification("A bomba nuclear explodiu - a radioatividade o matou!");
                                    }
                                }
                            }

                            #region Breaking News

                            lock (PlusEnvironment.GetGame().GetClientManager().GetClients.ToList())
                            {
                                foreach (GameClient ClientAll in PlusEnvironment.GetGame().GetClientManager().GetClients.ToList())
                                {
                                    if (ClientAll == null || ClientAll.GetHabbo() == null)
                                        continue;

                                    ClientAll.SendWhisper("[NOTÍCIAS DE ÚLTIMA HORA] [BOMBA NUCLEAR] Os associados da NPA relataram que " + KillsGained + " cidadãos morreram na bomba nuclear!", 33);
                                }
                            }

                            #endregion

                            #endregion
                        }
                    }

                }).Start();

                base.EndTimer();
                return;
            }
            catch (Exception e)
            {
                Logging.LogRPTimersError("Error in Execute() void: " + e);
                base.EndTimer();
            }
        }
    }
}