using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.Text;

namespace WcfService
{

    [ServiceContract(CallbackContract = typeof(IConnectFourServiceCallback))]

    public interface IConnectFourService
    {
        [FaultContract(typeof(UserExistsFault))]
        [OperationContract]
        void Connect(string userName);

        [OperationContract]
        void Disconnect(string userName);

        [FaultContract(typeof(UserExistsFault))]
        [OperationContract]
        void JoinStatus(string userName);

        [OperationContract]
        void LeftStatus(string userName);

        [OperationContract]
        bool SendMessage(string Message, string fromClient, string toClient);

        [OperationContract]
        bool AddPlayer(Player_Entity player_);

        [OperationContract]
        bool RemovePlayer(string userName, string password = null);
        [OperationContract]
        IEnumerable<Games> GetHistoryGamesOfPlayer(int id);

        [OperationContract]
        int GetPlayerIdByUserName(string userName);
        [OperationContract]
        string GetPlayerUserNameById(int Id);
        [OperationContract]
        IEnumerable<string> ShowPlayersByUserName();
        [OperationContract]
        bool SearchPlayer(string userName);
        [OperationContract]
        void AddCurrentGame(string firstPlayerName, string secondPlayerName);
        [OperationContract]
        int GetGameID(int fPlayerId, int sPlayerId);

        [OperationContract]
        bool RemoveCurrentGame(string firstPlayerName, string secondPlayerName, string winnerGame);

        [OperationContract]
        ResultGame SetBallToThePlayer(string player1_name, string player2_name, int column, string color, Side currSide);

        [OperationContract]

        void InitGameBoardOfAnotherPlayer(string fromClient, string toClient, string colorPlayer, string colorAnotherPlayer, Side playerSide);

        [OperationContract]
        string GetPlayerTurn(string fromClient, string toClient);

        [OperationContract]
        bool isConnected(string userName);
    }
}
