using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using Plus.Database.Interfaces;

namespace Plus.HabboHotel.Polls
{
    /// <summary>
    ///     Class PollManager.
    /// </summary>
    internal class PollManager
    {
        /// <summary>
        ///     The polls
        /// </summary>
        internal Dictionary<int, Poll> Polls;

        /// <summary>
        ///     Initializes a new instance of the <see cref="PollManager" /> class.
        /// </summary>
        internal PollManager()
        {
            Polls = new Dictionary<int, Poll>();
        }

        /// <summary>
        ///     Initializes the specified database client.
        /// </summary>
        /// <param name="dbClient">The database client.</param>
        /// <param name="pollLoaded">The poll loaded.</param>
        internal void Init(out int pollLoaded)
        {
            Initialize();
            pollLoaded = Polls.Count;
        }

        /// <summary>
        ///     Initializes the specified database client.
        /// </summary>
        /// <param name="dbClient">The database client.</param>
        internal void Initialize()
        {
            Polls.Clear();

            using (var dbClient = PlusEnvironment.GetDatabaseManager().GetQueryReactor())
            {
                dbClient.SetQuery("SELECT * FROM polls_data WHERE enabled = '1'");
                DataTable table = dbClient.getTable();

                if (table == null)
                    return;

                foreach (DataRow dataRow in table.Rows)
                {
                    int num = Convert.ToInt32(dataRow["id"]);

                    dbClient.SetQuery("SELECT * FROM polls_questions WHERE poll_id = '" + num + "'");
                    DataTable table2 = dbClient.getTable();

                    List<PollQuestion> list = new List<PollQuestion>();

                    foreach (DataRow dataRow2 in table2.Rows)
                    {
                        int id = Convert.ToInt32(dataRow2["id"]);
                        string question = dataRow2["question"].ToString();
                        int answertype = Convert.ToInt32(dataRow2["answertype"]);
                        string answers = dataRow2["answers"].ToString();
                        string correct_answers = dataRow2["correct_answer"].ToString();

                        PollQuestion newquestion = new PollQuestion(id, question, answertype, answers, correct_answers);

                        if (!list.Contains(newquestion))
                            list.Add(newquestion);
                    }

                    Poll value = new Poll(num, Convert.ToInt32(dataRow["room_id"]), dataRow["caption"].ToString(),
                        dataRow["invitation"].ToString(), dataRow["greetings"].ToString(), dataRow["prize"].ToString(),
                        Convert.ToInt32(dataRow["type"]), list);

                    Polls.Add(num, value);
                }
            }
        }

        /// <summary>
        ///     Tries the get pollquestion.
        /// </summary>
        internal PollQuestion getPollQuestion(Poll Poll, int questionid)
        {
            try
            {
                PollQuestion thequestion = null;

                foreach (PollQuestion question in Poll.Questions)
                {
                    if (question.Index == questionid)
                    {
                        return question;
                    }
                }

                return thequestion;
            }
            catch
            {
                return null;
            }
        }

        /// <summary>
        ///     Tries the get poll.
        /// </summary>
        internal Poll getPollByRoomId(int roomid)
        {
            try
            {
                Poll thepoll = null;

                foreach (Poll poll in Polls.Values)
                {
                    if (poll.RoomId == roomid)
                    {
                        return poll;
                    }
                }

                return thepoll;
            }
            catch
            {
                return null;
            }
        }

        /// <summary>
        ///     Tries the get poll by identifier.
        /// </summary>
        /// <param name="id">The identifier.</param>
        /// <returns>Poll.</returns>
        internal Poll TryGetPollById(int id)
        {
            return Polls.Values.FirstOrDefault(current => current.Id == id);
        }
    }
}