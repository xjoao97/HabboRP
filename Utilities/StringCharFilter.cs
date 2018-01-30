using System;
using System.Linq;
using System.Text;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using Plus.HabboHotel.GameClients;


namespace Plus.Utilities
{
    static class StringCharFilter
    {
        /// 

        /// Escapes the characters used for injecting special chars from a user input.
        /// 

        /// The string/text to escape.
        /// Allow line breaks to be used (\r\n).
        /// 
        public static string Escape(string str, bool allowBreaks = false, GameClient Session = null)
        {
            str = str.Trim();
            str = str.Replace(Convert.ToChar(1), ' ');
            str = str.Replace(Convert.ToChar(2), ' ');
            str = str.Replace(Convert.ToChar(3), ' ');
            str = str.Replace(Convert.ToChar(9), ' ');

            if (!allowBreaks)
            {
                str = str.Replace(Convert.ToChar(10), ' ');
                str = str.Replace(Convert.ToChar(13), ' ');
            }
            
            HabboRC4.HabboRC4 MessageEncryption = new HabboRC4.HabboRC4(Session, str);

            str = Regex.Replace(str, "<(.|\\n)*?>", string.Empty);
              
            return str;
        }
    }
}