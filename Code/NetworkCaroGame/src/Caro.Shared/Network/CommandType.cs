
namespace Caro.Shared.Network
{
    public enum CommandType
    {
        Login,
        LoginSuccess,
        LoginFailed,
        GetPlayers,
        UpdatePlayerList,
        ChallengeRequest,
        ChallengeResponse,
        StartGame,
        Move,
        TimerTick,
        TimeOut,
        GameOver,
        Chat,
        PlayerDisconnected,
        GetHistory,
        HistoryResponse,
        Win,
        Disconnect,
        MatchFinished,
        PlayerList,
        Challenge,
        Accept,
        Reject,
        HistoryData,
        Surrender,           
        DuplicateUsername,   
        InvalidInput,        
        OpponentDisconnected 
    }
}
