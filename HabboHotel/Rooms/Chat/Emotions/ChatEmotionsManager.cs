using System;
using System.Linq;
using System.Text;
using System.Collections.Generic;

namespace Plus.HabboHotel.Rooms.Chat.Emotions
{
    public sealed class ChatEmotionsManager
    {
        private readonly Dictionary<string, ChatEmotions> Emotions = new Dictionary<string, ChatEmotions>()
            {
                // Smile kk, legal, empolgante, ok, blz, xvideos
                { ":)", ChatEmotions.Smile },
                { ";)", ChatEmotions.Smile },
                { ":d", ChatEmotions.Smile },
                { ";d", ChatEmotions.Smile },
                { ":]", ChatEmotions.Smile },
                { ";]", ChatEmotions.Smile },
                { "=)", ChatEmotions.Smile },
                { "=]", ChatEmotions.Smile },
                { ":-)", ChatEmotions.Smile },
				{ "kk", ChatEmotions.Smile },
				{ "kj", ChatEmotions.Smile },
				{ "legal", ChatEmotions.Smile },
				{ "empolgante", ChatEmotions.Smile },
				{ "ok", ChatEmotions.Smile },
				{ "blz", ChatEmotions.Smile },
				{ "xvideos", ChatEmotions.Smile },
				{ "foda-se", ChatEmotions.Smile },
     
                // Angry  fdp, arrombado, lixo, cuzao, bosta, pnc, retardado, caralho, escroto
                { ">:(", ChatEmotions.Angry },
                { ">:[", ChatEmotions.Angry },
                { ">;[", ChatEmotions.Angry },
                { ">;(", ChatEmotions.Angry },
                { ">=(", ChatEmotions.Angry },
				{ "fdp", ChatEmotions.Angry },
				{ "arrombado", ChatEmotions.Angry },
				{ "lixo", ChatEmotions.Angry },
				{ "cuzao", ChatEmotions.Angry },
				{ "bosta", ChatEmotions.Angry },
				{ "pnc", ChatEmotions.Angry },
				{ "retardado", ChatEmotions.Angry },
				{ "caralho", ChatEmotions.Angry },
				{ "escroto", ChatEmotions.Angry },
				{ "xxx", ChatEmotions.Angry },
				{ "carlos", ChatEmotions.Angry },
				{ "byxhp", ChatEmotions.Angry },
   
                // Shocked oloco, nossa, uau, wow, maneiro, brabo, incrivel
                { ":o", ChatEmotions.Shocked },
                { ";o", ChatEmotions.Shocked },
                { ">;o", ChatEmotions.Shocked },
                { ">:o", ChatEmotions.Shocked },
                { "=o", ChatEmotions.Shocked },
                { ">=o", ChatEmotions.Shocked },
				{ "oloco", ChatEmotions.Shocked },
				{ "uau", ChatEmotions.Shocked },
				{ "wow", ChatEmotions.Shocked },
				{ "maneiro", ChatEmotions.Shocked },
				{ "brabo", ChatEmotions.Shocked },
				{ "incrivel", ChatEmotions.Shocked },
     
                // Sad triste, feio, 
                { ";'(", ChatEmotions.Sad },
                { ";[", ChatEmotions.Sad },
                { ":[", ChatEmotions.Sad },
                { ";(", ChatEmotions.Sad },
                { "=(", ChatEmotions.Sad },
                { "='(", ChatEmotions.Sad },
                { "=[", ChatEmotions.Sad },
                { "='[", ChatEmotions.Sad },
                { ":(", ChatEmotions.Sad },
                { ":-(", ChatEmotions.Sad },
				{ "triste", ChatEmotions.Sad },
				{ "feio", ChatEmotions.Sad },
				{ "sad", ChatEmotions.Sad },
            };

        /// <summary>
        /// Searches the provided text for any emotions that need to be applied and returns the packet number.
        /// </summary>
        /// <param name="Text">The text to search through</param>
        /// <returns></returns>
        public int GetEmotionsForText(string Text)
        {
            foreach (KeyValuePair<string, ChatEmotions> Kvp in Emotions)
            {
                if (Text.ToLower().Contains(Kvp.Key.ToLower()))
                {
                    return GetEmoticonPacketNum(Kvp.Value);
                }
            }

            return 0;
        }

        /// <summary>
        /// Trys to get the packet number for the provided chat emotion.
        /// </summary>
        /// <param name="e">Chat Emotion</param>
        /// <returns></returns>
        private static int GetEmoticonPacketNum(ChatEmotions e)
        {
            switch (e)
            {
                case ChatEmotions.Smile:
                    return 1;

                case ChatEmotions.Angry:
                    return 2;

                case ChatEmotions.Shocked:
                    return 3;

                case ChatEmotions.Sad:
                    return 4;

                case ChatEmotions.None:
                default:
                    return 0;
            }
        }
    }
}
