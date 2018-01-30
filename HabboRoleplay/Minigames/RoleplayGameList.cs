using System;
using System.Collections.Generic;

namespace Plus.HabboRoleplay.Games
{
    public enum GameMode
    {
        /// <summary>
        /// Brawl
        /// </summary>
        Brawl,

        /// <summary>
        /// Team Brawl
        /// </summary>
        TeamBrawl,

        /// <summary>
        /// Soloqueue
        /// </summary>
        SoloQueue,

        /// <summary>
        /// Soloqueue with guns
        /// </summary>
        SoloQueueGuns,

        /// <summary>
        /// Colour Wars
        /// </summary>
        ColourWars,

        /// <summary>
        /// Mafia Wars
        /// </summary>
        MafiaWars,

        /// <summary>
        /// Hunger Games
        /// </summary>
        HungerGames,

        /// <summary>
        /// Game mode doesn't exist
        /// </summary>
        None
    };

    public static class GameList
    {
        /// <summary>
        /// Gets the game mode type
        /// </summary>
        /// <param name="gameMode">The game mode</param>
        public static GameMode GetGameModeType(string gameMode)
        {
            switch (gameMode.ToLower())
            {
                case "brawl":
				case "briga":
                    return GameMode.Brawl;

                case "colourwars":
				case "guerradecores":
				case "guerracores":
				case "gc":
				case "cw":
                    return GameMode.ColourWars;

                case "mafiawars":
				case "guerrademafias":
				case "guerramafias":
				case "guerrademafia":
				case "guerramafia":
				case "gm":
				case "mw":
                    return GameMode.MafiaWars;

                case "hungergames":
				case "vorazes":
                    return GameMode.HungerGames;

                case "teambrawl":
				case "brigadetimes":
				case "brigadetime":
				case "brigatimes":
				case "brigatime":
				case "bt":
				case "tb":
				case "tbriga":
                    return GameMode.TeamBrawl;

                case "soloqueue":
                    return GameMode.SoloQueue;

                case "soloqueueguns":
				case "soloarmas":
				case "sarmas":
				case "asoloqueue":
                    return GameMode.SoloQueueGuns;

                default:
                    return GameMode.None;
            }
        }
    }
}
