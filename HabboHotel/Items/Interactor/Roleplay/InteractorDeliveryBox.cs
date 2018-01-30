using System;
using System.Linq;
using System.Collections.Generic;
using System.Threading;
using Plus.Utilities;
using Plus.HabboHotel.GameClients;
using Plus.HabboHotel.Pathfinding;
using Plus.HabboHotel.Groups;
using Plus.HabboHotel.Rooms;
using Plus.HabboRoleplay.Misc;
using Plus.HabboRoleplay.Weapons;
using Plus.HabboHotel.Items.Crafting;
using Plus.Communication.Packets.Outgoing.Inventory.Furni;

namespace Plus.HabboHotel.Items.Interactor
{
    public class InteractorDeliveryBox : IFurniInteractor
    {
        public void OnPlace(GameClient Session, Item Item)
        {
        }

        public void OnRemove(GameClient Session, Item Item)
        {
        }

        public void OnTrigger(GameClient Session, Item Item, int Request, bool HasRights)
        {
            if (Session == null)
                return;

            RoomUser User = Item.GetRoom().GetRoomUserManager().GetRoomUserByHabbo(Session.GetHabbo().Id);

            if (User == null)
                return;

            if (Session.GetHabbo() == null)
                return;

            string Type = Item.DeliveryType;

            if (!Gamemap.TilesTouching(Item.GetX, Item.GetY, User.Coordinate.X, User.Coordinate.Y))
                User.MoveTo(Item.SquareInFront);
            else
            {
                User.SetRot(Rotation.Calculate(User.Coordinate.X, User.Coordinate.Y, Item.GetX, Item.GetY), false);

                switch (Type.ToLower())
                {
                    case "weapon":
					case "arma":
					case "armas":
                        {
                            if (!GroupManager.HasJobCommand(Session, "weapon") && !Session.GetHabbo().GetPermissions().HasRight("offer_anything"))
                            {
                                Session.SendWhisper("Apenas um trabalhador da loja de armas está certificado para abrir esta caixa de entrega!", 1);
                                break;
                            }

                            var Weapon = RoleplayManager.DeliveryWeapon;

                            if (Weapon != null)
                                Session.Shout("*Abre a caixa de entrega e tira um novo estoque de " + Weapon.PublicName + "*", 4);
                            else
                                Session.SendWhisper("Esta caixa de entrega parece estar vazia!", 1);

                            Item.GetRoom().GetRoomItemHandler().RemoveFurniture(null, Item.Id);

                            int NewStock = 50;

                            if (Weapon != null)
                            {
                                WeaponManager.Weapons[Weapon.Name.ToLower()].Stock = NewStock;

                                using (var dbClient = PlusEnvironment.GetDatabaseManager().GetQueryReactor())
                                {
                                    dbClient.RunQuery("UPDATE `rp_weapons` SET `stock` = '" + NewStock + "' WHERE `name` = '" + Weapon.Name.ToLower() + "'");
                                }
                            }
                            RoleplayManager.UserWhoCalledDelivery = 0;
                            RoleplayManager.DeliveryWeapon = null;
                            RoleplayManager.CalledDelivery = false;
                            break;
                        }
                    default:
                        {
                            Session.SendWhisper("O conteúdo desta caixa de entrega não pôde ser encontrado!", 1);
                            break;
                        }
                }
            }
        }

        public void OnWiredTrigger(Item Item)
        {

        }
    }
}