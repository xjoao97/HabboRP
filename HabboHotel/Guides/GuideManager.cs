using System;
using System.Linq;
using Plus.Utilities;
using System.Collections.Generic;
using Plus.HabboHotel.GameClients;
using Plus.HabboHotel.Groups;

namespace Plus.HabboHotel.Guides
{
    internal class GuideManager
    {
        internal List<GameClient> GuardiansOnDuty = new List<GameClient>();
        internal List<GameClient> GuidesOnDuty = new List<GameClient>();
        internal List<GameClient> HelpersOnDuty = new List<GameClient>();
        internal List<GameClient> AllPolice = new List<GameClient>();

        public int GuidesCount
        {
            get { return GuidesOnDuty.Count; }
        }

        public int HelpersCount
        {
            get { return HelpersOnDuty.Count; }
        }

        public int GuardiansCount
        {
            get { return GuardiansOnDuty.Count; }
        }

        public int AllPoliceCount
        {
            get { return AllPolice.Count; }
        }

        public GameClient GetRandomPolice()
        {
            return AllPolice[new CryptoRandom().Next(0, AllPoliceCount - 1)];
        }

        public List<GameClient> HandlingCalls()
        {
            List<GameClient> handlingCalls = new List<GameClient>();

            foreach (GameClient Client in AllPolice)
            {
                if (Client.GetRoleplay() == null)
                    continue;

                if (Client.GetRoleplay().GuideOtherUser != null)
                    continue;

                if (Client.GetRoleplay().HandlingCalls)
                        handlingCalls.Add(Client);
            }
            return handlingCalls;
        }

        public List<GameClient> HandlingJailbreaks()
        {
            List<GameClient> handlingJailbreaks = new List<GameClient>();

            foreach (GameClient Client in AllPolice)
            {
                if (Client.GetRoleplay() == null)
                    continue;

                if (Client.GetRoleplay().GuideOtherUser != null)
                    continue;

                if (Client.GetRoleplay().HandlingJailbreaks)
                    handlingJailbreaks.Add(Client);
            }
            return handlingJailbreaks;
        }

        public List<GameClient> HandlingHeists()
        {
            List<GameClient> handlingHeists = new List<GameClient>();

            foreach (GameClient Client in AllPolice)
            {
                if (Client.GetRoleplay() == null)
                    continue;

                if (Client.GetRoleplay().GuideOtherUser != null)
                    continue;

                if (Client.GetRoleplay().HandlingHeists)
                    handlingHeists.Add(Client);
            }
            return handlingHeists;
        }

        public void AddGuide(GameClient guide)
        {
            int DutyLevel = 0;

            if (guide.GetRoleplay().JobRank >= 1 && guide.GetRoleplay().JobRank <= 2)
                DutyLevel = 1;
            else if (guide.GetRoleplay().JobRank >= 3 && guide.GetRoleplay().JobRank <= 4)
                DutyLevel = 2;
            else
                DutyLevel = 3;

            switch (DutyLevel)
            {
                case 1:
                    if (!GuidesOnDuty.Contains(guide))
                        GuidesOnDuty.Add(guide);
                    if (!AllPolice.Contains(guide))
                        AllPolice.Add(guide);
                    break;

                case 2:
                    if (!HelpersOnDuty.Contains(guide))
                        HelpersOnDuty.Add(guide);
                    if (!AllPolice.Contains(guide))
                        AllPolice.Add(guide);
                    break;

                case 3:
                    if (!GuardiansOnDuty.Contains(guide))
                        GuardiansOnDuty.Add(guide);
                    if (!AllPolice.Contains(guide))
                        AllPolice.Add(guide);
                    break;

                default:
                    if (!GuidesOnDuty.Contains(guide))
                        GuidesOnDuty.Add(guide);
                    if (!AllPolice.Contains(guide))
                        AllPolice.Add(guide);
                    break;
            }
        }

        public void RemoveGuide(GameClient guide)
        {
            int DutyLevel = 0;

            if (guide.GetRoleplay().JobRank >= 1 && guide.GetRoleplay().JobRank <= 2)
                DutyLevel = 1;
            else if (guide.GetRoleplay().JobRank >= 3 && guide.GetRoleplay().JobRank <= 4)
                DutyLevel = 2;
            else
                DutyLevel = 3;

            switch (DutyLevel)
            {
                case 1:
                    if (GuidesOnDuty.Contains(guide))
                        GuidesOnDuty.Remove(guide);
                    if (AllPolice.Contains(guide))
                        AllPolice.Remove(guide);
                    break;

                case 2:
                    if (HelpersOnDuty.Contains(guide))
                        HelpersOnDuty.Remove(guide);
                    if (AllPolice.Contains(guide))
                        AllPolice.Remove(guide);
                    break;

                case 3:
                    if (GuardiansOnDuty.Contains(guide))
                        GuardiansOnDuty.Remove(guide);
                    if (AllPolice.Contains(guide))
                        AllPolice.Remove(guide);
                    break;

                default:
                    if (GuidesOnDuty.Contains(guide))
                        GuidesOnDuty.Remove(guide);
                    if (AllPolice.Contains(guide))
                        AllPolice.Remove(guide);
                    break;
            }
        }
    }
}