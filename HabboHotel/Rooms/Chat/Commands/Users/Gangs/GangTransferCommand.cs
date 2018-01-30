using System;
using System.Linq;
using System.Text;
using System.Collections.Generic;
using System.Collections.Concurrent;

using Plus.HabboHotel.Groups;
using Plus.Database.Interfaces;
using Plus.Communication.Packets.Outgoing.Notifications;
using Plus.HabboHotel.GameClients;

namespace Plus.HabboHotel.Rooms.Chat.Commands.Users.Gangs
{
    class GangTransferCommand : IChatCommand
    {
        public string PermissionRequired
        {
            get { return "command_gang_transfer"; }
        }

        public string Parameters
        {
            get { return ""; }
        }

        public string Description
        {
            get { return "Transfere a propriedade de gangues para o co-fundador da gangue."; }
        }

        public void Execute(GameClients.GameClient Session, Rooms.Room Room, string[] Params)
        {
            #region Variables
            Group Gang = GroupManager.GetGang(Session.GetRoleplay().GangId);
            GroupRank GangRank = GroupManager.GetGangRank(Session.GetRoleplay().GangId, Session.GetRoleplay().GangRank);
            #endregion

            #region Conditions
            if (Gang == null)
            {
                Session.SendWhisper("Você não faz parte de nenhum grupo!", 1);
                return;
            }

            if (Gang.Id <= 1000)
            {
                Session.SendWhisper("Você não faz parte de nenhum grupo!", 1);
                return;
            }

            if (!GroupManager.HasGangCommand(Session, "gtransfer"))
            {
                Session.SendWhisper("Você não possui uma classificação suficientemente alta para usar esse comando!", 1);
                return;
            }

            if (Params.Length < 1)
            {
                Session.SendWhisper("Por favor digite ':gtransferir sim' se tiver certeza de que deseja desistir da sua gangue!", 1);
                return;
            }

            if (Params[1].ToString().ToLower() != "sim")
            {
                Session.SendWhisper("Por favor digite ':gtransferir sim' se tiver certeza de que deseja desistir da sua gangue!", 1);
                return;
            }

            if (Gang.Members.Values.Where(x => x.UserRank == 5).ToList().Count <= 0)
            {
                Session.SendWhisper("Não há co-fundador para transferir a gangue!", 1);
                return;
            }
            #endregion

            #region Execute
            int GangCoFounder = Gang.Members.Values.FirstOrDefault(x => x.UserRank == 5).UserId;
            GameClient TargetClient = PlusEnvironment.GetGame().GetClientManager().GetClientByUserID(GangCoFounder);

            if (TargetClient == null || TargetClient.GetHabbo() == null || TargetClient.GetRoleplay() == null)
            {
                Session.SendWhisper("Este usuário não pôde ser encontrado, talvez ele esteja offline.", 1);
                return;
            }

            Gang.TransferGangOwnership(Session, TargetClient);
            Session.SendWhisper("Você transferiu sua gangue com sucesso para " + TargetClient.GetHabbo().Username + "!", 1);

            foreach (int Member in Gang.Members.Keys)
            {
                GameClient Client = PlusEnvironment.GetGame().GetClientManager().GetClientByUserID(Member);

                if (Client == null)
                    continue;

                Client.SendWhisper("[GANGUE] A propriedade da gangue acaba de ser transferida para: " + TargetClient.GetHabbo().Username + "!", 34);
            }
            #endregion
        }
    }
}