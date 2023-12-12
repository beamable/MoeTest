using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Beamable.Common.Content;
using Beamable.Experimental.Api.Matchmaking;
using Helper;
using UnityEngine;
using UnityEngine.Events;

namespace MatchMaking
{
    public class MyMatchMakingEvent : UnityEvent<MyMatchMakingResult>{}

    [System.Serializable]
    public class MyMatchMakingResult
    {
        public string ErrorMessage = "";
        private long _localPlayer;
        private bool _isInProgress = false;
        private MatchmakingHandle _matchmakingHandle = null;
        private SimGameType _simGameType = null;

        public long LocalPlayer => _localPlayer;
        public SimGameType MySimGameType => _simGameType;
        public MatchmakingHandle MyMatchmakingHandle
        {
            get => _matchmakingHandle;
            set => _matchmakingHandle = value;
        }
        public bool IsInProgress
        {
            get => _isInProgress;
            set => _isInProgress = value;
        }

        public List<long> Players
        {
            get
            {
                if(_matchmakingHandle == null || _matchmakingHandle.Status == null)
                {
                    return new List<long>();
                }

                return _matchmakingHandle.Status.Players.Select(long.Parse).ToList();
            }
        }

        public int PlayerCountMin
        {
            get
            {
                int playerCountMin = 0;
                foreach (TeamContent team in _simGameType.teams)
                {
                    playerCountMin += team.minPlayers;
                }

                return playerCountMin;
            }
        }

        public int PlayerCountMax
        {
            get
            {
                int playerCountMax = 0;
                foreach (TeamContent team in _simGameType.teams)
                {
                    playerCountMax += team.maxPlayers;
                }

                return playerCountMax;
            }
        }

        public string MatchId => _matchmakingHandle?.Match.matchId;
        
        public MyMatchMakingResult(SimGameType simGameType, long localPlayer)
        {
            _simGameType = simGameType;
            _localPlayer = localPlayer;
        }
        
        public override string ToString()
        {
            return $"[MyMatchmakingResult (" +
                   $"MatchId = {MatchId}, " +
                   $"Teams = {_matchmakingHandle?.Match?.teams}, " +
                   $"players.Count = {Players?.Count})]";
        }
    }


    public class MyMatchMaking
    {
        private MyDebugger _myDebugger = null;
        private MyMatchMakingResult _myMatchmakingResult = null;
        private MatchmakingService _matchmakingService = null;
        public const string TimeoutErrorMessage = "Timeout";
        
        public MyMatchMakingEvent OnProgress = new MyMatchMakingEvent();
        public MyMatchMakingEvent OnComplete = new MyMatchMakingEvent();
        public MyMatchMakingEvent OnError = new MyMatchMakingEvent();
        
        public MyMatchMakingResult MyMatchResult => _myMatchmakingResult;
        
        public MyMatchMaking(MatchmakingService matchmakingService,
            SimGameType simGameType, long localPlayerDbid, MyDebugger myDebugger)
        {
            _matchmakingService = matchmakingService;
            _myMatchmakingResult = new MyMatchMakingResult(simGameType, localPlayerDbid);
            _myDebugger = myDebugger;
        }

        public async Task StartMatchMaking()
        {
            if (_myMatchmakingResult.IsInProgress)
            {
                _myDebugger.ErrorDebug(($"MyMatchmaking.StartMatchmaking() failed. " +
                                        $"IsInProgress must not be {_myMatchmakingResult.IsInProgress}.\n\n"));
                return;
            }
            
            _myMatchmakingResult.IsInProgress = true;
            _myMatchmakingResult.MyMatchmakingHandle = await _matchmakingService.StartMatchmaking(
                _myMatchmakingResult.MySimGameType.Id,
                maxWait: TimeSpan.FromSeconds(10),
                updateHandler: handle => { OnUpdateHandler(handle); },
                readyHandler: handle =>
                {
                    OnUpdateHandler(handle);
                    OnReadyHandler(handle);
                },
                timeoutHandler: handle =>
                {
                    OnUpdateHandler(handle);
                    OnTimeoutHandler(handle);
                });
        }

        public async Task CancelMatchMaking()
        {
            await _matchmakingService.CancelMatchmaking(_myMatchmakingResult.MyMatchmakingHandle.Tickets[0].ticketId);
            _myMatchmakingResult.IsInProgress = false;
        }

        private void OnUpdateHandler(MatchmakingHandle handle)
        {
            _myMatchmakingResult.ErrorMessage = $"{handle.Tickets[0].status} - {handle.Tickets[0].SecondsRemaining} seconds remaining" +
                                                $"\n {handle.State} + minPlayers reached: {handle.Status.MinPlayersReached}";
            OnProgress?.Invoke(_myMatchmakingResult);
        }
        
        private void OnReadyHandler(MatchmakingHandle handle)
        {
            _myMatchmakingResult.IsInProgress = false;
            OnComplete?.Invoke(_myMatchmakingResult);
        }
        
        private void OnTimeoutHandler(MatchmakingHandle handle)
        {
            _myMatchmakingResult.IsInProgress = false;
            _myMatchmakingResult.ErrorMessage = TimeoutErrorMessage;
            OnError?.Invoke(_myMatchmakingResult);
        }
        
    }
}