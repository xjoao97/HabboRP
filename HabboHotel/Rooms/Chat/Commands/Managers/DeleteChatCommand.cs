using System;
using System.Linq;
using System.Text;
using System.Collections.Generic;

using Plus.HabboHotel.Rooms;
using Plus.HabboHotel.GameClients;
using Fleck;
using Plus.Communication.Packets.Outgoing.Notifications;
using Plus.HabboRoleplay.Web.Util.ChatRoom;
using Newtonsoft.Json;
using Plus.HabboHotel.Rooms.Games;
using Plus.HabboHotel.Groups;
using Plus.Database.Interfaces;

namespace Plus.HabboHotel.Rooms.Chat.Commands.Managers
{
    class DeleteChatCommand : IChatCommand
    {
        public string PermissionRequired
        {
            get { return "command_wonline"; }
        }

        public string Parameters
        {
            get { return ""; }
        }

        public string Description
        {
            get { return "Exclui um bate-papo por abuso."; }
        }

        public void Execute(GameClients.GameClient Session, Rooms.Room Room, string[] Params)
        {

            #region Variables
            if (Params.Length < 2)
            {
                Session.SendWhisper("Comando inválido, use :delchat <nomedochat>", 1);
                return;
            }

            bool BanUserFromMaking = false;
            bool BanUserFromChatting = false;

            if (Params.Length >= 3)
            {
                if (Convert.ToString(Params[2]).ToLower().StartsWith("sim"))
                BanUserFromMaking = true;
            }

            if (Params.Length == 4)
            {
                if (Convert.ToString(Params[3]).ToLower().StartsWith("sim"))
                    BanUserFromChatting = true;
            }

            string ChatName = Convert.ToString(Params[1]);

            #endregion

            #region Conditions

            if (!WebSocketChatManager.RunningChatRooms.ContainsKey(ChatName.ToLower()))
            {
                Session.SendWhisper("Este bate-papo '" + ChatName.ToLower() + "' não existe!", 1);
                return;
            }

            WebSocketChatRoom Chat = WebSocketChatManager.RunningChatRooms[ChatName.ToLower()];

            if (Chat == null)
            {
                Session.SendWhisper("Ocorreu um erro, este bate-papo não existe!", 1);
                return;
            }
            #endregion

            #region Execute          

            GameClient ChatOwner = null;

            if (Chat.ChatOwner > 0)
            {

               ChatOwner = PlusEnvironment.GetGame().GetClientManager().GetClientByUserID(Chat.ChatOwner);

                using (IQueryAdapter DB = PlusEnvironment.GetDatabaseManager().GetQueryReactor())
                {
                    if (BanUserFromChatting)
                    {
                        if (ChatOwner == null)
                            DB.RunQuery("UPDATE rp_stats SET wchat_banned = '1' WHERE id = '" + Chat.ChatOwner + "'");
                        else
                        {
                            ChatOwner.GetRoleplay().BannedFromChatting = true;
                            ChatOwner.SendWhisper("Você foi impedido de poder se juntar a uma sala de bate-papo ");
                        }
                    }

                    if (BanUserFromMaking)
                    {
                        if (ChatOwner == null)
                            DB.RunQuery("UPDATE rp_stats SET wchat_making_banned = '1' WHERE id = '" + Chat.ChatOwner + "'");
                        else
                        {
                            ChatOwner.GetRoleplay().BannedFromMakingChat = true;
                            ChatOwner.SendWhisper("Você foi impedido de poder se juntar a uma sala de bate-papo");
                        }
                    }
                }
            }
            else
            {
                if (BanUserFromChatting || BanUserFromMaking)
                Session.SendWhisper("Não havia nenhum proprietário para este bate-papo, então não tem ninguém para proibir", 1);
            }

            Session.SendWhisper("Excluiu o bate-papo com sucesso '" + Chat.ChatName + "'. O dono foi notificado!");

            if (ChatOwner != null)
            {
                ChatOwner.SendWhisper("Seu chat '" + Chat.ChatName + "' foi excluído por um membro da equipe!");
            }

            WebSocketChatManager.DeleteChat(Chat);

           #endregion

        }

    }
}
