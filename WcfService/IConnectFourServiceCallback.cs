using System.Collections.Generic;
using System.ServiceModel;

namespace WcfService
{
    public interface IConnectFourServiceCallback
    {
        [OperationContract]
        bool SendMessageToClient(string message, string fromClient, string toClient);

        [OperationContract(IsOneWay = true)]
        void SendPostionToClient(int postionOfBall, string colorBall, string secondryPlayerName);

        [OperationContract(IsOneWay = true)]
        void WaitToClient(string currPlayer);

        [OperationContract(IsOneWay = true)]

        void UpdateClientList(IEnumerable<string> users);

        [OperationContract(IsOneWay = true)]

        void UpdateStatusOfClients(IEnumerable<string> users);

        [OperationContract]

        void ShowGameBoardOfPlayer(string fromClient, string toClient, string colorPlayer, string colorAnotherPlayer, WcfService.Side playerSide);

    }
}