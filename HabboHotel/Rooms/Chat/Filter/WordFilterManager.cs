using System;
using System.Linq;
using System.Text;
using System.Data;
using System.Collections.Generic;

using System.Text.RegularExpressions;

using Plus.Database.Interfaces;


namespace Plus.HabboHotel.Rooms.Chat.Filter
{
    public sealed class WordFilterManager
    {
        private List<WordFilter> _filteredWords;

        public WordFilterManager()
        {
            this._filteredWords = new List<WordFilter>();
        }

        public void Init()
        {
            if (this._filteredWords.Count > 0)
            this._filteredWords.Clear();

            DataTable Data = null;
            using (IQueryAdapter dbClient = PlusEnvironment.GetDatabaseManager().GetQueryReactor())
            {
                dbClient.SetQuery("SELECT * FROM `wordfilter`");
                Data = dbClient.getTable();

                if (Data != null)
                {
                    foreach (DataRow Row in Data.Rows)
                    {
                        this._filteredWords.Add(new WordFilter(Convert.ToString(Row["word"]), Convert.ToString(Row["replacement"]), PlusEnvironment.EnumToBool(Row["strict"].ToString()), PlusEnvironment.EnumToBool(Row["bannable"].ToString())));
                    }
                }
            }
        }

        public string ReplaceSpecial(string Message)
        {
            byte[] data = Encoding.Default.GetBytes(Message);
            Message = Encoding.UTF8.GetString(data);
            Message = Regex.Replace(Message, "[àâäàáâãäåÀÁÂÃÄÅ@4ª?]", "a");
            Message = Regex.Replace(Message, "[ß8]", "b");
            Message = Regex.Replace(Message, "[©çÇ¢]", "c");
            Message = Regex.Replace(Message, "[Ð]", "d");
            Message = Regex.Replace(Message, "[éèëêðÉÈËÊ£3?]", "e");
            Message = Regex.Replace(Message, "[ƒ]", "f");
            Message = Regex.Replace(Message, "[ìíîïÌÍÎÏ]", "i");
            Message = Regex.Replace(Message, "[1]", "l");
            Message = Regex.Replace(Message, "[ñÑp]", "n");
            Message = Regex.Replace(Message, "[òóôõöøÒÓÔÕÖØ0|ºO]", "o");
            Message = Regex.Replace(Message, "[®]", "r");
            Message = Regex.Replace(Message, "[šŠ$5?§]", "s");
            Message = Regex.Replace(Message, "[ùúûüµÙÚÛÜ]", "u");
            Message = Regex.Replace(Message, "[ÿŸ¥]", "y");
            Message = Regex.Replace(Message, "[žŽ]", "z");
            Message = Message.Replace("œ", "oe");
            Message = Message.Replace("Œ", "Oe");
            Message = Message.Replace("™", "TM");
            Message = Message.Replace("æ", "ae");
            Message = Message.Replace("8", "oo");
            Message = Message.Replace("dot", ".");
            return Message;
        }

        public string CheckMessage(string Message)
        {
            foreach (WordFilter Filter in this._filteredWords.ToList())
            {
                if (Message.ToLower().Contains(Filter.Word) && Filter.IsStrict || Message == Filter.Word)
                {
                    Message = Regex.Replace(Message, Filter.Word, Filter.Replacement, RegexOptions.IgnoreCase);
                }
                else if (Message.ToLower().Contains(Filter.Word) && !Filter.IsStrict || Message == Filter.Word)
                {
                    string[] Words = Message.Split(' ');

                    Message = "";
                    foreach (string Word in Words.ToList())
                    {
                        if (Word.ToLower() == Filter.Word)
                            Message += Filter.Replacement + " ";
                        else
                            Message += Word + " ";
                    }
                }
            }

            return Message.TrimEnd(' ');
        }

        public bool CheckBannedWords(string Message, out string Phrase)
        {
            Phrase = "";
            //First check replace all special characters and numbers
            Message = ReplaceSpecial(Message);

            //Second check Remove everything except letters
            Message = Regex.Replace(Message.ToLower(), @"[^\w]", "");
            foreach (WordFilter Filter in this._filteredWords.ToList())
            {
                if (!Filter.IsBannable)
                    continue;

                if (Message.Contains(Regex.Replace(Filter.Word, @"[^\w]", "")))
                {
                    Phrase = Filter.Replacement;
                    return true;
                }
            }
            return false;
        }

        public bool IsFiltered(string Message)
        {
            Message = Regex.Replace(Message.ToLower(), @"[^\w]", "");
            foreach (WordFilter Filter in this._filteredWords.ToList())
            {
                if (Message.Contains(Filter.Word))
                    return true;
            }
            return false;
        }
    }
}