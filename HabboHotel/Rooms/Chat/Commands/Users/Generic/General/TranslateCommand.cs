using System;
using System.Linq;
using System.Text;
using System.Collections.Generic;

using Plus.Communication.Packets.Outgoing.Notifications;
using Plus.HabboHotel.Rooms;
using Plus.HabboHotel.Rooms.Chat.Styles;
using Plus.HabboRoleplay.RoleplayUsers;
using Plus.HabboHotel.Groups;
using Plus.HabboRoleplay.Misc;
using Plus.HabboRoleplay.Weapons;

namespace Plus.HabboHotel.Rooms.Chat.Commands.Users.Generic.General
{
    class TranslateCommand : IChatCommand
    {
        public string PermissionRequired
        {
            get { return "command_translate"; }
        }

        public string Parameters
        {
            get { return "%sua_lingua% %outra_lingua%"; }
        }

        public string Description
        {
            get { return "Traduz uma mensagem de um idioma para outro."; }
        }

        public void Execute(GameClients.GameClient Session, Rooms.Room Room, string[] Params)
        {
            if (Params.Length < 3)
            {
                Session.SendWhisper("Comando inválido! Use :traduzir <sua linguagem> <linguagem para traduzir>. Exemplo ':traduzir PT EN'", 1);
                return;
            }

            string Language1 = Params[1].ToLower();
            string Language2 = Params[2].ToLower();

            #region Available Languages
            List<string> AvailableLanguages = new List<string>();

            AvailableLanguages.Add("af"); // Afrikaans
            AvailableLanguages.Add("sq"); // Albanian 
            AvailableLanguages.Add("az"); // Azerbaijani
            AvailableLanguages.Add("eu"); // Basque
            AvailableLanguages.Add("be"); // Belarusian
            AvailableLanguages.Add("bg"); // Bulgarian
            AvailableLanguages.Add("ca"); // Catalan
            AvailableLanguages.Add("hr"); // Croatian
            AvailableLanguages.Add("cs"); // Czech
            AvailableLanguages.Add("da"); // Danish
            AvailableLanguages.Add("nl"); // Dutch
            AvailableLanguages.Add("en"); // English
            AvailableLanguages.Add("eo"); // Esperanto
            AvailableLanguages.Add("et"); // Estonian
            AvailableLanguages.Add("tl"); // Filipino
            AvailableLanguages.Add("fi"); // Finnish
            AvailableLanguages.Add("fr"); // French
            AvailableLanguages.Add("gl"); // Galician
            AvailableLanguages.Add("de"); // German
            AvailableLanguages.Add("el"); // Greek
            AvailableLanguages.Add("ht"); // Haitian Creole
            AvailableLanguages.Add("hu"); // Hungarian
            AvailableLanguages.Add("is"); // Icelandic
            AvailableLanguages.Add("id"); // Indonesian
            AvailableLanguages.Add("ga"); // Irish
            AvailableLanguages.Add("it"); // Italian
            AvailableLanguages.Add("la"); // Latin
            AvailableLanguages.Add("lv"); // Latvian
            AvailableLanguages.Add("lt"); // Lithuanian
            AvailableLanguages.Add("mk"); // Macedonian
            AvailableLanguages.Add("ms"); // Malay
            AvailableLanguages.Add("mt"); // Maltese
            AvailableLanguages.Add("no"); // Norwegian
            AvailableLanguages.Add("pl"); // Polish
            AvailableLanguages.Add("pt"); // Portuguese
            AvailableLanguages.Add("ro"); // Romanian
            AvailableLanguages.Add("sk"); // Slovak
            AvailableLanguages.Add("sl"); // Slovenian
            AvailableLanguages.Add("es"); // Spanish
            AvailableLanguages.Add("sw"); // Swahili
            AvailableLanguages.Add("sv"); // Swedish
            AvailableLanguages.Add("tr"); // Turkish
            AvailableLanguages.Add("vi"); // Vietnamese
            AvailableLanguages.Add("cy"); // Welsh
            #endregion

            if (Session.GetHabbo().FromLanguage == Language1 && Session.GetHabbo().ToLanguage == Language2)
            {
                Session.SendWhisper("Você já está traduzindo de " + Language1.ToUpper() + " para " + Language2.ToUpper() + "!", 1);
                return;
            }

            if (!AvailableLanguages.Contains(Language1) || !AvailableLanguages.Contains(Language2))
            {
                Session.SendWhisper("Uma das línguas que você selecionou não está disponível! Aqui está uma lista de idiomas disponíveis: " + String.Join(", ", AvailableLanguages.ToArray()), 1);
                return;
            }

            Session.GetHabbo().Translating = true;
            Session.GetHabbo().FromLanguage = Language1;
            Session.GetHabbo().ToLanguage = Language2;

            Session.SendWhisper("Agora você está traduzindo de " + Language1.ToUpper() + " para " + Language2.ToUpper() + "!", 1);
        }
    }
}