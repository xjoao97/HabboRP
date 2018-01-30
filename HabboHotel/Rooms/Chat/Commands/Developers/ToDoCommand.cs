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
using System.Collections.Concurrent;
using Plus.Communication.Packets.Outgoing.Notifications;
using Plus.HabboHotel.Rooms.Chat.Commands;

namespace Plus.HabboHotel.Rooms.Chat.Commands.Developers
{
    class ToDoCommand : IChatCommand
    {
        public string PermissionRequired
        {
            get { return "command_todo"; }
        }

        public string Parameters
        {
            get { return ""; }
        }

        public string Description
        {
            get { return "Abre a lista de tarefas."; }
        }

        public void Execute(GameClients.GameClient Session, Rooms.Room Room, string[] Params)
        {
            var ToDoList = ToDoManager.ToDoList;

            if (ToDoList == null)
            {
                Session.SendWhisper("Desculpe, mas a lista de tarefas não conseguiu gerar!", 1);
                return;
            }

            if (Params[0].ToLower() == "tarefa")
            {
                if (ToDoList.Count < 1)
                {
                    Session.SendWhisper("A lista de tarefas está vazia no momento! Use ':addtarefa [tarefa]' para adicionar uma nova!", 1);
                    return;
                }

                StringBuilder List = new StringBuilder();
                List.Append("<----- LISTA DE TAREFAS ----->\n");
                List.Append("[USUÁRIO] [ID] [¥]TAREFA\n\n");

                foreach (ToDo ToDo in ToDoList.Values)
                {
                    List.Append("[" + PlusEnvironment.GetHabboById(ToDo.AddedBy).Username + "] [" + ToDo.Id + "]\n----- [¥] " + ToDo.String + "\n\n");
                }

                Session.SendMessage(new MOTDNotificationComposer(List.ToString()));
                return;
            }
            else if (Params[0].ToLower() == "addtarefa" || Params[0].ToLower() == "atf" || Params[0].ToLower() == "addtaref")
            {
                if (Params.Length == 1)
                {
                    Session.SendWhisper("Digite o que deseja adicionar!", 1);
                    return;
                }

                ToDo NewToDo = new ToDo(0, CommandManager.MergeParams(Params, 1), Session.GetHabbo().Id, PlusEnvironment.GetUnixTimestamp());
                ToDoManager.AddNewTodo(NewToDo);

                Session.SendWhisper("Você adicionou um a nova tarefa com sucesso!", 1);
                return;
            }
            else if (Params[0].ToLower() == "deltarefa" || Params[0].ToLower() == "deletartarefa" || Params[0].ToLower() == "remtarefa" || Params[0].ToLower() == "removertarefa" || Params[0].ToLower() == "texc")
            {
                if (Params.Length == 1)
                {
                    Session.SendWhisper("Por favor insira o id de trabalho que deseja excluir!", 1);
                    return;
                }

                int Id;
                if (!int.TryParse(Params[1], out Id))
                {
                    Session.SendWhisper("Por favor insira um número válido para o id da tarefa!", 1);
                    return;
                }

                if (!ToDoList.ContainsKey(Id))
                {
                    Session.SendWhisper("Desculpe, mas esta tarefa não existe!", 1);
                    return;
                }

                ToDoManager.DeleteToDo(Id);
                Session.SendWhisper("Você removeu com sucesso! Tarefa ID: " + Id + "!", 1);
                return;
            }
        }
    }
}
