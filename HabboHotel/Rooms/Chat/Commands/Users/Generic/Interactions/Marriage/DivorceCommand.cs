using System;
using System.Linq;
using System.Text;
using System.Collections.Generic;

using Plus.HabboHotel.Groups;
using Plus.HabboHotel.GameClients;
using Plus.HabboRoleplay.Misc;
using Plus.HabboHotel.Cache;

namespace Plus.HabboHotel.Rooms.Chat.Commands.Users.Generic.Interactions.Marriage
{
    class DivorceCommand : IChatCommand
    {
        public string PermissionRequired
        {
            get { return "command_interactions_divorce"; }
        }

        public string Parameters
        {
            get { return "%usuário%"; }
        }

        public string Description
        {
            get { return "Divorcia de seu marido ou esposa."; }
        }

        public void Execute(GameClients.GameClient Session, Rooms.Room Room, string[] Params)
        {
            #region Conditions
            if (Params.Length == 1)
            {
                Session.SendWhisper("Comando inválido! Use ':divorciar <usuário>'.", 1);
                return;
            }

            if (Session.GetRoleplay().MarriedTo == 0)
            {
                Session.SendWhisper("Você não é casado(a) com ninguém!", 1);
                return;
            }

            GameClient TargetClient = PlusEnvironment.GetGame().GetClientManager().GetClientByUsername(Params[1]);

            #endregion

            #region Execute
            if (TargetClient == null)
            {
                using (var dbClient = PlusEnvironment.GetDatabaseManager().GetQueryReactor())
                {
                    var Habbo = PlusEnvironment.GetHabboByUsername(Params[1]);

                    if (Habbo == null)
                    {
                        Session.SendWhisper("Não foi possível encontrar esse usuário! Talvez você tenha digitado o nome errado", 1);
                        return;
                    }

                    dbClient.SetQuery("SELECT `id` FROM `rp_stats` WHERE `id` = '" + Habbo.Id + "' LIMIT 1");
                    var Row = dbClient.getRow();

                    if (Row == null)
                    {
                        Session.SendWhisper("Não foi possível encontrar esse usuário! Talvez você tenha digitado o nome errado", 1);
                        return;
                    }

                    int TargetMarriedTo = Convert.ToInt32(Row["id"]);

                    if (Session.GetRoleplay().MarriedTo != TargetMarriedTo)
                    {
                        Session.SendWhisper("Você não é casado(a) com essa pessoa!", 1);
                        return;
                    }
                    else
                    {
                        dbClient.RunQuery("UPDATE `rp_stats` SET `married_to` = '0' WHERE `id` = '" + Habbo.Id + "'");
                        dbClient.RunQuery("DELETE FROM `user_badges` WHERE `badge_id` = 'WD0' AND `user_id` = '" + Habbo.Id + "'");
                        dbClient.RunQuery("UPDATE `rp_stats` SET `married_to` = '0' WHERE `id` = '" + Session.GetHabbo().Id + "'");

                        Session.GetRoleplay().MarriedTo = 0;
                        if (Session.GetHabbo().GetBadgeComponent().HasBadge("WD0"))
                            Session.GetHabbo().GetBadgeComponent().RemoveBadge("WD0");

                        if (PlusEnvironment.GetGame().GetCacheManager().ContainsUser(Session.GetHabbo().Id))
                            PlusEnvironment.GetGame().GetCacheManager().TryUpdateUser(Session);

                        UserCache Junk;

                        if (PlusEnvironment.GetGame().GetCacheManager().ContainsUser(Habbo.Id))
                            PlusEnvironment.GetGame().GetCacheManager().TryRemoveUser(Habbo.Id, out Junk);

                        Session.Shout("*Retira o anel de casamento e joga no chão, anunciando que está se divorciando de " + Habbo.Username + "*", 7);
                    }
                }
            }
            else
            {
                if (TargetClient.GetRoleplay().MarriedTo != Session.GetHabbo().Id)
                {
                    Session.SendWhisper("Você não é casado(a) com essa pessoa!", 1);
                    return;
                }

                TargetClient.GetRoleplay().MarriedTo = 0;
                Session.GetRoleplay().MarriedTo = 0;

                if (Session.GetHabbo().GetBadgeComponent().HasBadge("WD0"))
                    Session.GetHabbo().GetBadgeComponent().RemoveBadge("WD0");

                if (TargetClient.GetHabbo().GetBadgeComponent().HasBadge("WD0"))
                    TargetClient.GetHabbo().GetBadgeComponent().RemoveBadge("WD0");

                if (PlusEnvironment.GetGame().GetCacheManager().ContainsUser(Session.GetHabbo().Id))
                    PlusEnvironment.GetGame().GetCacheManager().TryUpdateUser(Session);

                if (PlusEnvironment.GetGame().GetCacheManager().ContainsUser(TargetClient.GetHabbo().Id))
                    PlusEnvironment.GetGame().GetCacheManager().TryUpdateUser(TargetClient);

                Session.Shout("*Retira o anel de casamento e joga no chão, anunciando que está se divorciando de " + TargetClient.GetHabbo().Username + "*", 7);
                TargetClient.SendNotification(Session.GetHabbo().Username + " acabou de se divorciar de você!");
            }
            #endregion
        }
    }
}