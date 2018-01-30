using System;
using System.Linq;
using System.Text;
using System.Drawing;
using System.Collections.Generic;

using Plus.HabboHotel.Rooms.AI;
using Plus.HabboHotel.Rooms;
using Plus.HabboHotel.GameClients;
using Plus.Communication.Packets.Outgoing.Inventory.Pets;
using Plus.Database.Interfaces;
using Plus.HabboHotel.Items;
using Plus.HabboRoleplay.Misc;

namespace Plus.HabboHotel.Rooms.Chat.Commands.Developers
{
    class BotCommand : IChatCommand
    {
        public string PermissionRequired
        {
            get { return "command_kick_pets"; }
        }

        public string Parameters
        {
            get { return "%comando%"; }
        }

        public string Description
        {
            get { return "Testando bots"; }
        }

        public void Execute(GameClients.GameClient Session, Rooms.Room Room, string[] Params)
        {

            if (Params.Length == 1)
            {
                Session.SendWhisper("Opa, você esqueceu de entrar em um comando!", 1);
                return;
            }

            string Command = Convert.ToString(Params[1]);

            switch(Command)
            {

                #region follow
                case "follow":
                case "followme":
				case "mesiga":
                    {
                        RoomUser Target = null;

                        foreach (RoomUser Bot in Room.GetRoomUserManager()._bots.Values)
                        {
                            if (!Bot.IsRoleplayBot)
                                continue;

                            Target = Bot;
                        }

                        if (Target == null)
                        {
                            Session.SendWhisper("Nenhum BOT para interagir foi encontrado, desculpe!");
                            return;
                        }

                        Target.GetBotRoleplay().UserFollowing = Session;
                        Target.GetBotRoleplay().Following = true;

                        Session.Shout("*Avisa " + Target.GetBotRoleplay().Name + " para me seguir");

                        return;
                    }
                    break;
                #endregion

                #region attack
                case "attack":
                case "attackme":
                case "fight":
                case "fightme":
				case "atacar":
                    {
                        RoomUser Target = null;

                        foreach (RoomUser Bot in Room.GetRoomUserManager()._bots.Values)
                        {
                            if (!Bot.IsRoleplayBot)
                                continue;

                            Target = Bot;
                        }

                        if (Target == null)
                        {
                            Session.SendWhisper("Nenhum BOT para interagir foi encontrado, desculpe!");
                            return;
                        }

                        Target.GetBotRoleplay().UserAttacking = Session;
                        Target.GetBotRoleplay().Roaming = false;
                        Target.GetBotRoleplay().Attacking = true;

                        Session.Shout("*Avisa " + Target.GetBotRoleplay().Name + " para me atacar");

                        return;
                    }
                    break;
                #endregion

                #region tele
                case "randomtele":
                case "tele":
                    {

                        RoomUser Target = null;
                        Item Randtele = null;
                        foreach(RoomUser Bot in Room.GetRoomUserManager()._bots.Values)
                        {
                            if (!Bot.IsRoleplayBot)
                                continue;

                            Target = Bot;
                            
                        }

                        if (Target == null)
                        {
                            Session.SendWhisper("Nenhum BOT para interagir foi encontrado, desculpe!");
                            return;
                        }

                        foreach (Item Item in Room.GetRoomItemHandler().GetFloor.ToList())
                        {
                            if (Item == null || Item.GetBaseItem() == null)
                                continue;

                            if (Item.GetBaseItem().InteractionType != InteractionType.ARROW)
                                continue;

                            Randtele = Item;
                        }

                        if (Randtele == null)
                        {
                            Session.SendWhisper("Não foi encontrado TELEPORTE para interagir, desculpe!");
                            return;
                        }


                        Target.GetBotRoleplay().TeleporterEntering = Randtele;
                        int LinkedTele = ItemTeleporterFinder.GetLinkedTele(Target.GetBotRoleplay().TeleporterEntering.Id, Room);
                        int TeleRoomId = ItemTeleporterFinder.GetTeleRoomId(LinkedTele, Room);

                        Room NewRoom = RoleplayManager.GenerateRoom(TeleRoomId);

                        Target.GetBotRoleplay().TeleporterExiting = NewRoom.GetRoomItemHandler().GetItem(LinkedTele);
                        Target.GetBotRoleplay().Teleporting = true;

                        Session.Shout("*Avisa " + Target.GetBotRoleplay().Name + " para pisar na seta*");

                        return;
                    }
                    break;
                    #endregion

            }

        }
    }
}
