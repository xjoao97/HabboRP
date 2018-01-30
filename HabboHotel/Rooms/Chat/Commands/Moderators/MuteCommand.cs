using System;
using System.Linq;
using System.Text;
using System.Collections.Generic;
using Plus.Database.Interfaces;
using Plus.Utilities;
using Plus.HabboHotel.Users;
using Plus.HabboHotel.GameClients;
using Plus.HabboRoleplay.Misc;
using Plus.Communication.Packets.Outgoing.Moderation;

namespace Plus.HabboHotel.Rooms.Chat.Commands.Moderators
{
    class MuteCommand : IChatCommand
    {
        public string PermissionRequired
        {
            get { return "command_mute"; }
        }

        public string Parameters
        {
            get { return "%usuário% %tempo%"; }
        }

        public string Description
        {
            get { return "Silenciar outro usuário por um certo período de tempo."; }
        }

        public void Execute(GameClients.GameClient Session, Rooms.Room Room, string[] Params)
        {
            if (Params.Length == 1)
            {
                Session.SendWhisper("Digite um nome de usuário e um tempo válido em segundos (max 600).", 1);
                return;
            }

            Habbo Habbo = PlusEnvironment.GetHabboByUsername(Params[1]);
            if (Habbo == null)
            {
                Session.SendWhisper("Ocorreu um erro ao encontrar esse usuário no banco de dados.", 1);
                return;
            }

            if (Habbo.GetPermissions().HasRight("mod_tool") && !Session.GetHabbo().GetPermissions().HasRight("mod_mute_any"))
            {
                Session.SendWhisper("Opa, você não pode silenciar esse usuário.", 1);
                return;
            }

            if (Habbo.VIPRank > 1)
            {
                Session.SendWhisper("Desculpe, você não pode silenciar membros da equipe!", 1);
                return;
            }

            double Time;
            if (double.TryParse(Params[2], out Time))
            {
                if (Time > 900 && !Session.GetHabbo().GetPermissions().HasRight("mod_mute_limit_override"))
                    Time = 900;

                using (IQueryAdapter dbClient = PlusEnvironment.GetDatabaseManager().GetQueryReactor())
                {
                    dbClient.RunQuery("UPDATE `users` SET `time_muted` = '" + Time + "' WHERE `id` = '" + Habbo.Id + "' LIMIT 1");
                }

                if (Habbo.GetClient() != null)
                {
                    Habbo.TimeMuted = Time;
                    DateTime origin = new DateTime(1970, 1, 1, 0, 0, 0, 0).AddSeconds(PlusEnvironment.GetUnixTimestamp() + Time).ToLocalTime();

                    Habbo.GetClient().SendNotification("Seu HabboRPG foi silenciado até " + origin.ToString("dd-MM-yyyy H:mm:ss") + ". Para o bem da comunidade, mantenha-se afastado de qualquer spam ou bate-papo negativo. A leitura das Regras e dos Termos do HabboRPG o ajudarão a evitar este problema no futuro.");
                }

                Session.SendWhisper("Você silenciou com sucesso " + Habbo.Username + " por " + Time + " segundos.", 1);
            }
            else
                Session.SendWhisper("Digite um número inteiro válido.", 1);
        }
    }
}