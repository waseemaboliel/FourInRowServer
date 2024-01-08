using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.Text;
using System.Threading;

namespace WcfService
{
    [ServiceBehavior(InstanceContextMode = InstanceContextMode.Single,
           ConcurrencyMode = ConcurrencyMode.Multiple)]

    public class ConnectFourService : IConnectFourService
    {
        private SortedDictionary<string, IConnectFourServiceCallback> connectedClients;
        private SortedDictionary<string, IConnectFourServiceCallback> statusOfClients;
        private SortedDictionary<int, GameInfo> players;

        public ConnectFourService()
        {
            connectedClients = new SortedDictionary<string, IConnectFourServiceCallback>();
            statusOfClients = new SortedDictionary<string, IConnectFourServiceCallback>();
            players = new SortedDictionary<int, GameInfo>();
        }
        public void JoinStatus(string userName)
        {
            try
            {
                IConnectFourServiceCallback callback = OperationContext.Current.GetCallbackChannel<IConnectFourServiceCallback>();

                statusOfClients.Add("" + userName + " is Joined To The Game", callback);

                Thread updateStatusThread = new Thread(StatusClientsLists);
                updateStatusThread.Start();

            }
            catch (Exception)
            {
                UserExistsFault fault = new UserExistsFault()
                { message = "UserName " + userName + " already exists!" };
                throw new FaultException<UserExistsFault>(fault);
            }
        }

        public void LeftStatus(string userName)
        {
            IConnectFourServiceCallback callback = OperationContext.Current.GetCallbackChannel<IConnectFourServiceCallback>();

            statusOfClients.Add("" + userName + " is Lefted The Game", callback);

            Thread updateStatusThread = new Thread(StatusClientsLists);
            updateStatusThread.Start();

        }


        public void Connect(string userName)
        {
            try
            {
                IConnectFourServiceCallback callback = OperationContext.Current.GetCallbackChannel<IConnectFourServiceCallback>();
                connectedClients.Add(userName, callback);

                Thread updateThread = new Thread(UpdateClientsLists);
                updateThread.Start();

            }
            catch (Exception)
            {
                UserExistsFault fault = new UserExistsFault()
                { message = "UserName " + userName + " already exists!" };
                throw new FaultException<UserExistsFault>(fault);
            }

        }

        private void UpdateClientsLists()
        {
            foreach (var callback in connectedClients.Values)
            {
                callback.UpdateClientList(connectedClients.Keys);
            }
        }

        private void StatusClientsLists()
        {
            foreach (var callback in statusOfClients.Values)
            {
                callback.UpdateStatusOfClients(statusOfClients.Keys);
            }
        }

        public void Disconnect(string userName)
        {
            connectedClients.Remove(userName);
            Thread updateThread = new Thread(UpdateClientsLists);
            updateThread.Start();
        }

        public bool SendMessage(string Message, string fromClient, string toClient)
        {
            bool request = false;

            if (connectedClients.ContainsKey(toClient))
            {
                Thread tt = new Thread(() => { request = connectedClients[toClient].SendMessageToClient(Message, fromClient, toClient); });
                tt.Start();
                tt.Join();

            }
            return request;

        }

        public void InitGameBoardOfAnotherPlayer(string fromClient, string toClient, string colorPlayer, string colorAnotherPlayer, Side playerSide)
        {
            Thread tt = new Thread(() => { connectedClients[toClient].ShowGameBoardOfPlayer(toClient, fromClient, colorPlayer, colorAnotherPlayer, playerSide); });
            tt.Start();
            tt.Join();

        }
        public bool AddPlayer(Player_Entity player_)
        {
            /// try
            /// 
            try
            {
                using (var db = new four_in_row_DB_AbolielEntities())
                {
                    if (SearchPlayer(player_.UserName) == true)
                        return false;
                    db.Player_Entity.Add(player_);
                    db.SaveChanges();
                    return true;
                }
            }
            catch (Exception)
            {
                UserExistsFault fault = new UserExistsFault()
                { message = "UserName " + player_.UserName + " already exists!" };
                throw new FaultException<UserExistsFault>(fault);
            }
        }

        public bool SearchPlayer(string userName)
        {
            using (var db = new four_in_row_DB_AbolielEntities())
            {
               
                    var players = from p in db.Player_Entity
                                  where p.UserName == userName
                                  select p;
                    if (players.Count() == 0) return false;
                return true;
            }
        }

        public List<Player_Entity> SearchPlayerByUserName(string UserName)
        {
            using (var ctx = new four_in_row_DB_AbolielEntities())
            {

                var player = from p in ctx.Player_Entity
                             where p.UserName.StartsWith(UserName)
                             select p;
                return player.ToList();
            }
        }

        public IEnumerable<string> ShowPlayersByUserName()
        {
            List<string> players = null;
            Thread tt = new Thread(() =>
            {
                using (var ctx = new four_in_row_DB_AbolielEntities())
                {
                    players = (from p in ctx.Player_Entity
                               select p.UserName).ToList();
                }
            });
            tt.Start();
            tt.Join();
            if (players == null || players.Count() == 0)
                return null;

            return players;

        }

        public bool RemovePlayer(string userName, string password)
        {
            using (var db = new four_in_row_DB_AbolielEntities())
            {
                if (SearchPlayer(userName) == false)
                    return false;

                db.Player_Entity.Remove(db.Player_Entity.FirstOrDefault());
                db.SaveChanges();
                return true;
            }
        }

        public IEnumerable<Games> GetHistoryGamesOfPlayer(int playerID)
        {
            List<Game_Entity> players = null;
            List<Game_Entity> games_players = null;

            List<int> GamesID = null;
            List<Games> games = null;
            games_players = new List<Game_Entity>();

            using (var ctx = new four_in_row_DB_AbolielEntities())
            {
                GamesID = (from p in ctx.HistoryGame_Entity
                           select p.IdGame).ToList();

                if (GamesID == null || GamesID.Count() == 0)
                    return null;

                players = (
               from d in ctx.Game_Entity
               select d).ToList();

                if (players == null || players.Count() == 0)
                    return null;

                foreach (var gameId in GamesID)
                {
                    foreach (var player in players)
                    {
                        if (gameId == player.Id && (playerID == player.Player_EntityId || playerID == player.Player_EntityId1))
                        {
                            Game_Entity g1 = new Game_Entity();
                            g1.Player_EntityId = player.Player_EntityId;
                            g1.Player_EntityId1 = player.Player_EntityId1;
                            g1.TimeStarting = player.TimeStarting;
                            g1.Winner = player.Winner;
                            g1.Id = player.Id;
                            games_players.Add(player);
                            break;
                        }
                    }

                }
            }

            games = new List<Games>();

            foreach (var item in games_players)
            {
                string player1_name = null;
                Thread tt = new Thread(() =>
                {
                    player1_name = GetPlayerUserNameById(item.Player_EntityId);
                });
                tt.Start();
                tt.Join();

                string player2_name = null;
                Thread tt1 = new Thread(() =>
                {
                    player2_name = GetPlayerUserNameById(item.Player_EntityId1);
                });
                tt1.Start();
                tt1.Join();

                Games g = new Games();
                g.FirstPlayer = player1_name;
                g.SecondPlayer = player2_name;
                g.TimeStarting = item.TimeStarting;
                g.Winner = item.Winner;
                games.Add(g);
            }
            return games;
        }
        public int GetPlayerIdByUserName(string userName)
        {
            Player_Entity players = null;
            using (var ctx = new four_in_row_DB_AbolielEntities())
            {
                players = (from p in ctx.Player_Entity
                           where p.UserName == userName
                           select p).FirstOrDefault();
            }

            if (players == null)
                return -1;

            return players.Id;
        }

        public string GetPlayerUserNameById(int Id)
        {
            Player_Entity players = null;
            using (var ctx = new four_in_row_DB_AbolielEntities())
            {
                players = (from p in ctx.Player_Entity
                           where p.Id == Id
                           select p).FirstOrDefault();
            }
            return players.UserName;
        }


        public int GetGameID(int fPlayerId, int sPlayerId)
        {
            using (var db = new four_in_row_DB_AbolielEntities())
            {

                var game = (from h in db.Game_Entity
                            where (h.Player_EntityId == fPlayerId && h.Player_EntityId1 == sPlayerId) || (h.Player_EntityId == sPlayerId && h.Player_EntityId1 == fPlayerId)
                            select h).FirstOrDefault();

                if (game == null) return -1;

                return game.Id;
            }
        }

        public void AddCurrentGame(string firstPlayerName, string secondPlayerName)
        {
            int fPlayerID = -1;
            Thread tt1 = new Thread(() => { fPlayerID = GetPlayerIdByUserName(firstPlayerName); });
            tt1.Start();
            tt1.Join();

            int sPlayerID = -1;
            Thread tt2 = new Thread(() => { sPlayerID = GetPlayerIdByUserName(secondPlayerName); });
            tt2.Start();
            tt2.Join();

            if (fPlayerID == -1 || sPlayerID == -1) return;
            //return true;

            int game_Id = -1;
            Thread tt5 = new Thread(() => { game_Id = GetGameID(fPlayerID, sPlayerID); });
            tt5.Start();
            tt5.Join();
            if (game_Id == -1)
            {
                using (var db = new four_in_row_DB_AbolielEntities())
                {
                    Game_Entity game = new Game_Entity();
                    game.Player_EntityId = fPlayerID;
                    game.Player_EntityId1 = sPlayerID;
                    // game.PlayerTurn = firstPlayerName;
                    game.PlayerTurn = firstPlayerName;
                    game.Winner = "";
                    game.TimeStarting = DateTime.Now;
                    db.Game_Entity.Add(game);//add specific game
                    db.SaveChanges();
                }

                Thread tt6 = new Thread(() => { game_Id = GetGameID(fPlayerID, sPlayerID); });
                tt6.Start();
                tt6.Join();

            }

            players.Add(game_Id, new GameInfo(firstPlayerName, secondPlayerName));

        }

        public bool RemoveCurrentGame(string firstPlayerName, string secondPlayerName, string winnerGame)
        {
            int fPlayerID = -1;
            Thread tt1 = new Thread(() => { fPlayerID = GetPlayerIdByUserName(firstPlayerName); });
            tt1.Start();
            tt1.Join();

            int sPlayerID = -1;
            Thread tt2 = new Thread(() => { sPlayerID = GetPlayerIdByUserName(secondPlayerName); });
            tt2.Start();
            tt2.Join();

            int game_Id = -1;
            Thread tt = new Thread(() => { game_Id = GetGameID(fPlayerID, sPlayerID); });
            tt.Start();
            tt.Join();

            if (fPlayerID == -1 || sPlayerID == -1 || game_Id == -1) return false;


            using (var db = new four_in_row_DB_AbolielEntities())
            {
                var update_winner = db.Game_Entity.Find(game_Id);
                update_winner.Winner = winnerGame;
                HistoryGame_Entity history = new HistoryGame_Entity();
                history.IdGame = game_Id;
                history.Game_Entity_Id = game_Id;
                db.HistoryGame_Entity.Add(history); //add specific history game 
                db.SaveChanges();

                //var currentgame = (from h in db.CurrentGame_Entity
                //                   where (h.IdGame == game_Id)
                //                   select h).FirstOrDefault();
                //if (currentgame != null)
                //    db.CurrentGame_Entity.Remove(currentgame);

            }
            players.Remove(game_Id);

            return true;


        }




        public ResultGame SetBallToThePlayer(string player1_name, string player2_name, int column, string color, Side currSide)
        {
            int fPlayerID = -1;
            Thread tt1 = new Thread(() => { fPlayerID = GetPlayerIdByUserName(player1_name); });
            tt1.Start();
            tt1.Join();


            int sPlayerID = -1;
            Thread tt2 = new Thread(() => { sPlayerID = GetPlayerIdByUserName(player2_name); });
            tt2.Start();
            tt2.Join();

            if (fPlayerID == -1 || sPlayerID == -1) return null;

            int game_Id = -1;
            Thread tt5 = new Thread(() => { game_Id = GetGameID(fPlayerID, sPlayerID); });
            tt5.Start();
            tt5.Join();


            players[game_Id].boardGame.Insert(currSide, column);
            ResultGame resultGame = new ResultGame();
            resultGame.Tie = players[game_Id].boardGame.Tied();
            resultGame.Winner = players[game_Id].boardGame.Winner();
            connectedClients[player2_name].SendPostionToClient(column, ((color == "RED") ? "Black" : "RED"), player2_name);
            players[game_Id].player_turn = player2_name;
            return resultGame;
        }

        public bool isConnected(string userName)
        {
            if (connectedClients.ContainsKey(userName))
                return true;
            return false;

        }

        public string GetPlayerTurn(string player1_name, string player2_name)
        {
            int fPlayerID = -1;
            Thread tt1 = new Thread(() => { fPlayerID = GetPlayerIdByUserName(player1_name); });
            tt1.Start();
            tt1.Join();


            int sPlayerID = -1;
            Thread tt2 = new Thread(() => { sPlayerID = GetPlayerIdByUserName(player2_name); });
            tt2.Start();
            tt2.Join();

            if (fPlayerID == -1 || sPlayerID == -1) return null;
            //return true;

            int game_Id = -1;
            Thread tt5 = new Thread(() => { game_Id = GetGameID(fPlayerID, sPlayerID); });
            tt5.Start();
            tt5.Join();



            return players[game_Id].player_turn;
        }

    }

    public class GameInfo
    {
        public string fname, sname;
        public four_in_a_row boardGame;
        public string player_turn;
        public GameInfo(string player1, string player2)
        {
            boardGame = new four_in_a_row(6, 7);
            fname = player1;
            sname = player2;
            player_turn = player1;
        }


    }

    public class Games
    {
        public DateTime TimeStarting;
        public string FirstPlayer;
        public string SecondPlayer;
        public string Winner;
    }
    /// <summary>
    /// //todo
    /// </summary>
    public class ResultGame
    {
        public Side Winner;
        public bool Tie;
    }
}
