using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Beamable;
using Beamable.Common.Api.Events;
using Helper;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Serialization;

namespace Events
{
    
    [Serializable]
    public class EventData
    {
        public string eventName;
        public string endTime;
        public string eventId;
        public long playerId;
        public double score;
    }
    
    public class EventsManagerTest : MonoBehaviour
    {
        #region EXPOSED_VARIABLES
        
        [SerializeField] private MyDebugger debugger;
        [SerializeField] private TMP_InputField scoreInputField = null;

        #endregion

        #region PRIVATE_VARIABLES
        
        private BeamContext _beamContext;
        private EventData _eventData = new EventData();

        #endregion

        #region PUBLIC_VARIABLES

        #endregion

        #region UNITY_CALLS

        private void Start()
        {
            SetupBeamable();
        }

        #endregion

        #region PRIVATE_METHODS

        private async void SetupBeamable()
        {
            _beamContext = await BeamContext.Default.Instance;
            debugger.SimpleDebug($"Beamable Setup for user {_beamContext.PlayerId}");

            _beamContext.Api.EventsService.Subscribe(eventsResponse =>
            {
                foreach (var eventView in eventsResponse.running)
                {
                    var eventName = eventView.name;
                    var endTime = $"{eventView.endTime.ToShortDateString()} at " +
                                     $"{eventView.endTime.ToShortTimeString()}";
                    var currentPhase = eventView.currentPhase.name;
                    _eventData.eventName = eventName;
                    _eventData.endTime = endTime;
                    _eventData.playerId = _beamContext.PlayerId;
                    _eventData.score = eventView.score;
                    _eventData.eventId = eventView.id;
                    debugger.SimpleDebug($"Event: {eventName} \n" +
                                          $"End Time: {endTime} \n" +
                                          $"Current Phase: {currentPhase} \n" +
                                          $"Score: {eventView.score}");

                }

            });

        }

        #endregion

        #region PUBLIC_METHODS
        
        public async void SetScore()
        {
            var score = Convert.ToDouble(scoreInputField.text);
            var stat = new Dictionary<string, object>();
            var response = await _beamContext.Api.EventsService.GetCurrent();
            foreach (var view in response.running)
            {
                if(view.name != _eventData.eventName) continue; 
                await _beamContext.Api.EventsService.SetScore(_eventData.eventId, score, true);
                debugger.SimpleDebug($"Set score to {score} for event {_eventData.eventName}");
            }
        }

        public async void ClaimRewards()
        {
            var eventGetResponse = await _beamContext.Api.EventsService.GetCurrent();
            foreach(var eventView in eventGetResponse.running)
            {
                if (eventView.name != _eventData.eventName) continue;
                var canCLaimScoreReaward = false;
                foreach (var scoreReward in eventView.scoreRewards)
                {
                    if (scoreReward.earned && !scoreReward.claimed)
                    {
                        debugger.SimpleDebug($"Can claim score reward for event {_eventData.eventName}");
                        canCLaimScoreReaward = true;
                    }
                }

                if (canCLaimScoreReaward)
                {
                    try
                    {
                        var eventClaimResponse = await _beamContext.Api.EventsService.Claim(eventView.id);
                        debugger.SimpleDebug($"Claimed score reward for event {_eventData.eventName}");
                        debugger.SimpleDebug($"End time is {eventClaimResponse.view.endTime}");
                    }
                    catch (Exception e)
                    {
                        debugger.ErrorDebug($"Error claiming score reward for event {_eventData.eventName}" +
                                             $"\n {e.Message}");
                    }
                }
            }
            
        }

        public async void CheckEndTimeForEndedEvents()
        {
            var response = await _beamContext.Api.EventsService.GetCurrent();
            foreach (var eventView in response.done)
            {
                debugger.SimpleDebug($"Event {eventView.name} has ended at {eventView.endTime}");
            }
        }

        #endregion

        
    }
}