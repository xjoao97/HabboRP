using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Plus.HabboHotel.Rewards
{
    public class RewardTypeUtility
    {
        public static RewardType GetType(string Type)
        {
            switch (Type.ToLower())
            {
                case "badge":
				case "emblema":
                    return RewardType.BADGE;

                case "credits":
				case "creditos":
				case "moedas":
				case "grana":
                    return RewardType.CREDITS;

                case "duckets":
                    return RewardType.DUCKETS;

                case "diamonds":
				case "diamantes":
                    return RewardType.DIAMONDS;

                default:
                case "none":
				case "nenhum":
                    return RewardType.NONE;
            }
        }
    }
}
