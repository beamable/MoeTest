using System;
using UnityEngine;

namespace Multiplayer
{
    public class PlayerEventsData
    {
        public string name;
        public long playerId;
        public Vector3 position;
        
        public PlayerEventsData(string name, long playerId, Vector3 position)
        {
            this.name = name;
            this.playerId = playerId;
            this.position = position;
        }
    }
    
    public class PlayerEventsTest : MonoBehaviour
    {
        #region EXPOSED_VARIABLES

        #endregion

        #region PRIVATE_VARIABLES

        private PlayerEventsData _playerEventsData;

        #endregion

        #region PUBLIC_VARIABLES

        #endregion

        #region UNITY_CALLS

        private void Start()
        {
            _playerEventsData = new PlayerEventsData("Player One", 0, transform.position);
        }

        private void Update()
        {
            _playerEventsData.position = transform.position;
        }

        #endregion

        #region PRIVATE_METHODS

        #endregion

        #region

        public PlayerEventsData GetPlayerEvent()
        {
            return _playerEventsData;
        }

        public void UpdatePlayerId(long id)
        {
            _playerEventsData.playerId = id;
        }
        
        public void UpdateEvent(PlayerEventsData playerEventsData)
        {
            _playerEventsData = playerEventsData;
            transform.position = _playerEventsData.position;
        }

        #endregion

        
    }
}