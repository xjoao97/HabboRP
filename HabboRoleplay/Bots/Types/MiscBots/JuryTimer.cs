using System;
using Plus.HabboHotel.GameClients;
using Plus.HabboHotel.Groups;
using Plus.HabboRoleplay.Misc;
using Plus.HabboRoleplay.RoleplayUsers;
using Plus.HabboRoleplay.Bots;
using Plus.HabboRoleplay.Bots.Manager;
using Plus.HabboHotel.Items;
using System.Linq;
using System.Drawing;
using Plus.HabboHotel.Pathfinding;
using System.Threading;
using Plus.HabboHotel.Rooms;
using Plus.Utilities;
using System.Collections.Generic;
using Plus.Communication.Packets.Outgoing.Polls;
using Plus.HabboHotel.Polls;
using Plus.Core;

namespace Plus.HabboRoleplay.Timers.Types
{
    /// <summary>
    /// Attack timer
    /// </summary>
    public class JuryTimer : BotRoleplayTimer
    {
        public JuryTimer(string Type, RoleplayBot CachedBot, int Time, bool Forever, object[] Params) 
            : base(Type, CachedBot, Time, Forever, Params)
        {
            TimeCount = 0;
        }
 
        /// <summary>
        /// Begins chasing the client
        /// </summary>
        public override void Execute()
        {
            try
            {
                if (base.CachedBot == null || base.CachedBot.DRoomUser == null || base.CachedBot.DRoom == null)
                {

                    SpawnJuryUsersOut(false);
                    Thread.Sleep(200);

                    RoleplayManager.CourtVoteEnabled = false;
                    RoleplayManager.InnocentVotes = 0;
                    RoleplayManager.GuiltyVotes = 0;

                    RoleplayManager.CourtJuryTime = 0;
                    RoleplayManager.CourtTrialIsStarting = false;
                    RoleplayManager.CourtTrialStarted = false;
                    RoleplayManager.Defendant = null;
                    RoleplayManager.InvitedUsersToJuryDuty.Clear();

                    base.EndTimer();

                    return;
                }

                GameClient Client = RoleplayManager.Defendant;
                int CourtRoomId = Convert.ToInt32(RoleplayData.GetData("court", "roomid"));
                Room Room = RoleplayManager.GenerateRoom(CourtRoomId);

                if (Client == null || Client.LoggingOut || Client.GetHabbo() == null || Client.GetRoleplay() == null)
                {
                    SpawnJuryUsersOut(false);

                    RoleplayManager.CourtVoteEnabled = false;
                    RoleplayManager.InnocentVotes = 0;
                    RoleplayManager.GuiltyVotes = 0;

                    RoleplayManager.CourtJuryTime = 0;
                    RoleplayManager.CourtTrialIsStarting = false;
                    RoleplayManager.CourtTrialStarted = false;
                    RoleplayManager.Defendant = null;
                    RoleplayManager.InvitedUsersToJuryDuty.Clear();

                    if (base.CachedBot != null)
                    {
                        if (base.CachedBot.DRoomUser != null)
                        {
                            base.CachedBot.DRoomUser.Chat("Olá, como o réu saiu, o processo judicial foi suspenso. Desculpas por qualquer inconveniente!", false, 2);
                        }
                    }
                    base.EndTimer();
                    return;
                }

                RoleplayManager.CourtJuryTime++;

                if (RoleplayManager.CourtJuryTime < 151)
                {
                    if (RoleplayManager.CourtJuryTime == 2)
                    {
                        RoleplayManager.SendUser(Client, CourtRoomId, "");
                        return;
                    }
                    else if (RoleplayManager.CourtJuryTime == 5)
                    {
                        base.CachedBot.DRoomUser.Chat("Bom dia senhoras e senhores, agora irá começar o julgamento da Polícia do HabboRPG contra o(a) " + Client.GetHabbo().Username + ", seja honesto.", false, 2);
                        return;
                    }
                    else if (RoleplayManager.CourtJuryTime == 10)
                    {
                        if (Client.GetRoleplay().WantedFor != "")
                            base.CachedBot.DRoomUser.Chat("Esta pessoa é acusada de " + Client.GetRoleplay().WantedFor.TrimEnd(',', ' ') + ".", false, 2);
                        else
                            base.CachedBot.DRoomUser.Chat("Infelizmente, não recebi receber o relatório das acusações para o julgamento deste tribunal, então o juiz ouvirá sua explicação " + Client.GetHabbo().Username + "!", false, 2);
                        return;
                    }
                    else if (RoleplayManager.CourtJuryTime == 18)
                    {
                        base.CachedBot.DRoomUser.Chat("Apenas um lembrete. A Polícia do HabboRPG afirma que o Réu é culpado dos crimes, vamos prosseguir.", false, 2);
                        return;
                    }
                    else if (RoleplayManager.CourtJuryTime == 26)
                    {
                        base.CachedBot.DRoomUser.Chat(Client.GetHabbo().Username + ", explique para mim e para os juris, o que realmente aconteceu?", false, 2);
                        return;
                    }
                    else if (RoleplayManager.CourtJuryTime == 75)
                    {
                        base.CachedBot.DRoomUser.Chat("Obrigado por sua explicação.", false, 2);
                        return;
                    }
                    else if (RoleplayManager.CourtJuryTime == 81)
                    {
                        base.CachedBot.DRoomUser.Chat(Client.GetHabbo().Username + ", Se os jurados achar que você é inocente, você será libertado da prisão.", false, 2);
                        return;
                    }
                    else if (RoleplayManager.CourtJuryTime == 87)
                    {
                        base.CachedBot.DRoomUser.Chat("No entanto, se acharem que você é culpado, você permanecerá na prisão para servir o resto da sua sentença.", false, 2);
                        return;
                    }
                    else if (RoleplayManager.CourtJuryTime == 93)
                    {
                        base.CachedBot.DRoomUser.Chat("Advogado(a), por favor, pode remover " + Client.GetHabbo().Username + " para prosseguirmos com o julgamento.", false, 2);
                        return;
                    }
                    else if (RoleplayManager.CourtJuryTime == 96)
                    {
                        int JailRID = Convert.ToInt32(RoleplayData.GetData("jail", "insideroomid"));
                        RoleplayManager.SendUser(Client, JailRID);
                        return;
                    }
                    else if (RoleplayManager.CourtJuryTime == 100)
                    {
                        base.CachedBot.DRoomUser.Chat("Agora são vocês, juris, decidir o destino do Réu", false, 2);
                        return;
                    }
                    else if (RoleplayManager.CourtJuryTime == 105)
                    {
                        RoleplayManager.CourtVoteEnabled = true;
                        base.CachedBot.DRoomUser.Chat("Por favor, vote digitando ':votar (culpado/inocente)'!", false, 2);
                        return;
                    }
                    else if (RoleplayManager.CourtJuryTime == 125)
                    {
                        int CourtResult = Math.Max(RoleplayManager.InnocentVotes, RoleplayManager.GuiltyVotes);

                        if (CourtResult == 0)
                        {
                            base.CachedBot.DRoomUser.Chat("Obrigado pelos seus votos. O júri definiu que o réu, " + Client.GetHabbo().Username + ", é culpado de todos os crimes.", false, 2);
                            Thread.Sleep(4000);
                            base.CachedBot.DRoomUser.Chat("O réu permanecerá na prisão e servirá o resto de sua sentença lá.", false, 2);
                            Client.SendNotification("Os juris definiu que você é culpado de todos os crimes. Você permanecerá na prisão e servirá o resto da sua sentença!");
                            return;
                        }
                        else if (CourtResult == RoleplayManager.GuiltyVotes)
                        {
                            base.CachedBot.DRoomUser.Chat("Obrigado pelo seu voto. O júri definiu que o réu, " + Client.GetHabbo().Username + ", é culpado de todos os crimes.", false, 2);
                            Thread.Sleep(4000);
                            base.CachedBot.DRoomUser.Chat("O réu permanecerá na prisão e servirá o resto de sua sentença lá.", false, 2);
                            Client.SendNotification("Os juris definiu que você é culpado de todos os crimes. Você permanecerá na prisão e servirá o resto da sua sentença!");
                            return;

                        }
                        else
                        {
                            base.CachedBot.DRoomUser.Chat("Obrigado por seu voto. Os juris definiu que o réu, " + Client.GetHabbo().Username + ", é inocente de todos os crimes.", false, 2);
                            Thread.Sleep(4000);
                            base.CachedBot.DRoomUser.Chat("Eu liberto o réu e perdoo de todos os crimes.", false, 2);
                            Client.SendNotification("Os juris definiu que você é inocente de todos os crimes. Você foi libertado da prisão!");
                            Client.GetRoleplay().IsJailed = false;
                            Client.GetRoleplay().JailedTimeLeft = 0;
                        }
                        return;
                    }
                    else if (RoleplayManager.CourtJuryTime == 150)
                    {
                        base.CachedBot.DRoomUser.Chat("Obrigado juris. Caso encerrado.", false, 2);

                        if (Room != null)
                        {
                            SpawnJuryUsersOut(true);
                        }
                        return;
                    }
                    return;
                }

                RoleplayManager.CourtVoteEnabled = false;
                RoleplayManager.InnocentVotes = 0;
                RoleplayManager.GuiltyVotes = 0;

                RoleplayManager.CourtJuryTime = 0;
                RoleplayManager.CourtTrialIsStarting = false;
                RoleplayManager.CourtTrialStarted = false;
                RoleplayManager.Defendant = null;
                RoleplayManager.InvitedUsersToJuryDuty.Clear();
                base.EndTimer();

            }
            catch (Exception e)
            {
                Logging.LogRPTimersError("Error in Execute() void: " + e);
                base.EndTimer();
            }
        }

        private void SpawnJuryUsersOut(bool Reward)
        {

            #region Null checks & Court Variables
            if (RoleplayManager.InvitedUsersToJuryDuty == null) return;
            if (RoleplayManager.InvitedUsersToJuryDuty.Count <= 0) return;
            int CourtRoomId = Convert.ToInt32(RoleplayData.GetData("court", "roomid"));
            Room Room = RoleplayManager.GenerateRoom(CourtRoomId);
            if (Room == null) return;
            int Award = new CryptoRandom().Next(Convert.ToInt32(RoleplayData.GetData("court", "minaward")), Convert.ToInt32(RoleplayData.GetData("court", "maxaward")));
            #endregion

            lock (RoleplayManager.InvitedUsersToJuryDuty)
            {
                foreach (RoomUser User in Room.GetRoomUserManager().GetRoomUsers())
                {
                    #region Null checks
                    if (User == null) continue;
                    if (User.GetClient() == null) continue;
                    if (User.GetClient().GetHabbo() == null) continue;
                    if (User.GetClient().GetRoleplay() == null) continue;
                    if (!RoleplayManager.InvitedUsersToJuryDuty.Contains(User.GetClient())) continue;
                    #endregion

                    #region Reward
                    if (Reward)
                    {
                        User.GetClient().GetHabbo().Credits += Award;
                        User.GetClient().GetHabbo().UpdateCreditsBalance();
                        User.GetClient().SendWhisper("O Juíz deu para os juris R$" + String.Format("{0:N0}", Award) + " pelo seu tempo. Obrigado!", 1);
                    }
                    #endregion

                    RoleplayManager.SpawnChairs(User.GetClient(), "sofachair_silo*2");
                    User.Frozen = false;
                }
            }
        }
    }
}