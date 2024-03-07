

using FishNet.Object;
using FishNet.Object.Synchronizing;
using System;
using UnityEngine;

namespace FirstGearGames.LobbyAndWorld.Clients
{

    public class PlayerSettings : NetworkBehaviour
    {

        #region Private.
        /// <summary>
        /// Username for this client.
        /// </summary>
        private readonly SyncVar<string> _username = new SyncVar<string>();
        #endregion

        /// <summary>
        /// Sets Username.
        /// </summary>
        /// <param name="value"></param>
        public void SetUsername(string value)
        {
            _username.Value = value;
        }
        /// <summary>
        /// Returns Username.
        /// </summary>
        /// <returns></returns>
        public string GetUsername() 
        {
            return _username.Value; 
        }

    }

}
