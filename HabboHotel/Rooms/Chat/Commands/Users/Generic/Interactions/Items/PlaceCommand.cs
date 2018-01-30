using System;
using System.Linq;
using System.Text;
using System.Threading;
using System.Collections.Generic;

using Plus.HabboRoleplay.Misc;
using Plus.HabboHotel.Items;
using Plus.HabboHotel.GameClients;
using Plus.Communication.Packets.Outgoing.Rooms.Engine;
using Plus.HabboHotel.Groups;
using Plus.HabboRoleplay.Farming;

namespace Plus.HabboHotel.Rooms.Chat.Commands.Users.Generic.Interactions.Items
{
    class PlaceCommand : IChatCommand
    {
        public string PermissionRequired
        {
            get { return "command_interactions_place"; }
        }

        public string Parameters
        {
            get { return "%item%"; }
        }

        public string Description
        {
            get { return "Permite colocar certos itens no chão."; }
        }

        public void Execute(GameClients.GameClient Session, Rooms.Room Room, string[] Params)
        {
            #region Conditions
            if (Params.Length == 1 && Params[0].ToLower() != "explodir" && Params[0].ToLower() != "reparar")
            {
                if (Params[0].ToLower() == "plantar")
                {
                    Session.SendWhisper("Digite o ID da planta que você deseja usar! Digite ':agricultura' para ver os IDs da planta.", 1);
                    return;
                }
                else
                {
                    Session.SendWhisper("Digite o item que deseja colocar para baixo!", 1);
                    return;
                }
            }

            string Type;
            if (Params[0].ToLower() == "explodir")
                Type = "dinamite";
            else if (Params[0].ToLower() == "reparar")
                Type = "grade";
            else if (Params[0].ToLower() == "plantar")
                Type = "planta";
            else
                Type = Params[1].ToLower();
            #endregion

            switch (Type)
            {
                #region Dynamite
                case "dynamite":
				case "dinamite":
                    {
                        if (Session.GetRoleplay().Dynamite < 1)
                        {
                            Session.SendWhisper("Você não tem nenhuma dinamite para colocar!", 1);
                            return;
                        }

                        if (JailbreakManager.JailbreakActivated)
                        {
                            Session.SendWhisper("Uma fuga da prisão já está em andamento!", 1);
                            JailbreakManager.JailbreakActivated = false;
                            return;
                        }

                        int RoomId = Convert.ToInt32(RoleplayData.GetData("jail", "outsideroomid"));
                        int X = Convert.ToInt32(RoleplayData.GetData("jailbreak", "xposition"));
                        int Y = Convert.ToInt32(RoleplayData.GetData("jailbreak", "yposition"));
                        double Z = Convert.ToDouble(RoleplayData.GetData("jailbreak", "zposition"));
                        int Rot = Convert.ToInt32(RoleplayData.GetData("jailbreak", "rotation"));

                        if (Session.GetRoomUser() == null)
                            return;

                        if (Room.Id != RoomId)
                        {
                            Session.SendWhisper("Você não está fora da prisão para iniciar uma fuga!", 1);
                            return;
                        }

                        Item BTile = Room.GetRoomItemHandler().GetFloor.FirstOrDefault(x => x.GetBaseItem().ItemName.ToLower() == "bb_rnd_tele" && x.Coordinate == Session.GetRoomUser().Coordinate);

                        if (BTile == null)
                        {
                            Session.SendWhisper("Você deve estar parado em um tele banzai da prisao para começar a explosão e causar a fuga!", 1);
                            return;
                        }

                        List <GameClient> CurrentJailedUsers = PlusEnvironment.GetGame().GetClientManager().GetClients.Where(x => x != null && x.GetHabbo() != null && x.GetRoleplay() != null && x.GetRoleplay().IsJailed).ToList();

                        if (CurrentJailedUsers == null || CurrentJailedUsers.Count <= 0)
                        {
                            Session.SendWhisper("Não há ninguém na prisão agora!", 1);
                            return;
                        }

                        Session.GetRoleplay().Dynamite--;
                        JailbreakManager.JailbreakActivated = true;
                        Session.Shout("*Coloca uma dinamite na parede, tentando explodir e libertar os prisioneiros*", 4);

                        if (!Session.GetRoleplay().WantedFor.Contains("fugindo da prisão"))
                            Session.GetRoleplay().WantedFor = Session.GetRoleplay().WantedFor + "fugindo da prisão, ";

                        Item Item = RoleplayManager.PlaceItemToRoom(null, 6088, 0, X, Y, Z, Rot, false, Room.Id, false, "0");
                        Item Item2 = RoleplayManager.PlaceItemToRoom(null, 3011, 0, X, Y, 0, Rot, false, Room.Id, false, "0");

                        object[] Items = { Session, Item, Item2 };
                        RoleplayManager.TimerManager.CreateTimer("dinamite", 500, false, Items);
                        break;
                    }
                #endregion

                #region Fence Repair
                case "repair":
				case "reparar":
                    {
                        if (!JailbreakManager.FenceBroken)
                        {
                            Session.SendWhisper("Não há grade que precise de reparo!", 1);
                            return;
                        }

                        if (!GroupManager.HasJobCommand(Session, "guide") && !Session.GetHabbo().GetPermissions().HasRight("corporation_rights"))
                        {
                            Session.SendWhisper("Apenas um policial tem o equipamento certo para reparar essa grade!", 1);
                            return;
                        }

                        if (!Session.GetRoleplay().IsWorking && !Session.GetHabbo().GetPermissions().HasRight("corporation_rights"))
                        {
                            Session.SendWhisper("Você deve estar trabalhando para reparar essa grade!", 1);
                            return;
                        }

                        if (Session.GetRoleplay().TimerManager.ActiveTimers.ContainsKey("reparar"))
                        {
                            Session.SendWhisper("Você já está reparando a grade!", 1);
                            return;
                        }

                        Item BTile = Room.GetRoomItemHandler().GetFloor.FirstOrDefault(x => x.GetBaseItem().ItemName.ToLower() == "bb_rnd_tele" && x.Coordinate == Session.GetRoomUser().Coordinate);

                        if (BTile == null)
                        {
                            Session.SendWhisper("Você deve estar parado em um tele banzai para começar a reparar a grade!", 1);
                            return;
                        }

                        Session.Shout("*Comece a reparar a grade*", 4);
                        Session.SendWhisper("Você tem 2 minutos até você reparar essa grade!", 1);
                        Session.GetRoleplay().TimerManager.CreateTimer("reparar", 1000, false, BTile.Id);

                        if (Session.GetRoomUser().CurrentEffect != 59)
                            Session.GetRoomUser().ApplyEffect(59);
                        break;
                    }
                #endregion

                #region Plant
                case "plant":
				case "plantar":
                    {
                        int Id;
                        if (!int.TryParse(Params[1], out Id))
                        {
                            Session.SendWhisper("Digite o ID da planta que você deseja usar! Digite ':agricultura' para ver os IDs das plantas.", 1);
                            break;
                        }

                        if (!Session.GetRoleplay().FarmingStats.HasSeedSatchel)
                        {
                            Session.SendWhisper("Você não tem uma bolsa de sementes para transportar sementes!", 1);
                            return;
                        }

                        if (Id == 0)
                        {
                            Session.SendWhisper("Você guardou todas as suas sementes de volta ao seu saco de sementes", 1);
                            Session.GetRoleplay().FarmingItem = null;
                            break;
                        }

                        FarmingItem Item = FarmingManager.GetFarmingItem(Id);

                        ItemData Furni;

                        if (Item.BaseItem == null)
                        {
                            Session.SendWhisper("Desculpe, mas este ID da planta não existe! Digite ':agricultura' para ver os IDs das planta.", 1);
                            return;
                        }

                        if (!PlusEnvironment.GetGame().GetItemManager().GetItem(Item.BaseItem, out Furni) || Item == null)
                        {
                            Session.SendWhisper("Desculpe, mas este ID da planta não existe! Digite ':agricultura' para ver os IDs das planta.", 1);
                            return;
                        }

                        if (Item.LevelRequired > Session.GetRoleplay().FarmingStats.Level)
                        {
                            Session.SendWhisper("Desculpe, mas você não tem um nível de agricultura alto suficiente para esta semente!", 1);
                            return;
                        }

                        Session.GetRoleplay().FarmingItem = Item;

                        int Amount;
                        if (FarmingManager.GetSatchelAmount(Session, false, out Amount))
                        {
                            if (Amount <= 0)
                            {
                                Session.SendWhisper("Você não tem nenhuma semente para plantar! Compre alguma no supermercado.", 1);
                                Session.GetRoleplay().FarmingItem = null;
                                break;
                            }
                            else
                            {
                                Session.SendWhisper("Você preparou sua semente " + Amount + " " + Furni.PublicName + " para a plantação!", 1);
                                break;
                            }
                        }
                        else
                        {
                            Session.SendWhisper("Você não tem sementes para plantar! Compre alguma no supermercado.", 1);
                            Session.GetRoleplay().FarmingItem = null;
                            break;
                        }
                    }
                #endregion

                #region Default
                default:
                    {
                        Session.SendWhisper("Desculpe, mas este item não pode ser encontrado!", 1);
                        break;
                    }
                #endregion
            }
        }
    }
}