using System;
using System.Linq;
using System.Text;
using System.Collections.Generic;

using Plus.HabboHotel.Rooms;

namespace Plus.HabboHotel.Rooms.Chat.Commands.Developers
{
    class SetWhisperTileCommand : IChatCommand
    {
        public string PermissionRequired
        {
            get { return "command_set_whisper_tile"; }
        }

        public string Parameters
        {
            get { return "%mensagem%"; }
        }

        public string Description
        {
            get { return "Define a mensagem do objeto que pisar."; }
        }

        public void Execute(GameClients.GameClient Session, Rooms.Room Room, string[] Params)
        {
            if (Params.Length == 1)
            {
                Session.SendWhisper("Você esqueceu de inserir uma mensagem!", 1);
                return;
            }

            RoomUser User = Session.GetRoomUser();
            if (User == null)
                return;

            string Message = CommandManager.MergeParams(Params, 1);

            var Items = Room.GetGameMap().GetAllRoomItemForSquare(User.Coordinate.X, User.Coordinate.Y);
            bool HasWhisperTile = Items.Where(x => x.GetBaseItem().InteractionType == HabboHotel.Items.InteractionType.WHISPER_TILE).ToList().Count > 0;

            if (HasWhisperTile)
            {
                var Item = Items.FirstOrDefault(x => x.GetBaseItem().InteractionType == HabboHotel.Items.InteractionType.WHISPER_TILE);

                if (Item == null)
                {
                    Session.SendWhisper("O mobi não pôde ser encontrado!", 1);
                    return;
                }

                if (Item.WhisperTileData == null)
                    Item.WhisperTileData = new Items.Data.WhisperTile.WhisperTileData(Item.Id);

                using (var dbClient = PlusEnvironment.GetDatabaseManager().GetQueryReactor())
                {
                    dbClient.SetQuery("UPDATE `room_items_whisper_tile` SET `message` = @message WHERE `item_id` = @itemid LIMIT 1");
                    dbClient.AddParameter("itemid", Item.Id);
                    dbClient.AddParameter("message", Message);
                    dbClient.RunQuery();
                }

                Item.WhisperTileData.Message = Message;
                Session.SendWhisper("Você atualizou com êxito esta mensagem de sussurro deste mobi!", 1);
                return;
            }
            else
            {
                Session.SendWhisper("Você não está de pé em um mobi!", 1);
                return;
            }
        }
    }
}
