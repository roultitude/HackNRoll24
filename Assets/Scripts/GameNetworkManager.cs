using FirstGearGames.LobbyAndWorld.Lobbies.JoinCreateRoomCanvases;
using FirstGearGames.LobbyAndWorld.Lobbies;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using FishNet.Object;

public class GameNetworkManager : MonoBehaviour
{
    #region Serialized
    [Header("Spawning")]
    /// <summary>
    /// Region players may spawn.
    /// </summary>
    [Tooltip("Region players may spawn.")]
    [SerializeField]
    private Vector3 _spawnRegion = Vector3.one;
    /// <summary>
    /// Prefab to spawn.
    /// </summary>
    [Tooltip("Prefab to spawn.")]
    [SerializeField]
    private NetworkObject _playerPrefab = null;
    /// <summary>
    /// DeathDummy to spawn.
    /// </summary>
    [Tooltip("DeathDummy to spawn.")]
    [SerializeField]
    private GameObject _deathDummy = null;
    #endregion

    /// <summary>
    /// RoomDetails for this game. Only available on the server.
    /// </summary>
    private RoomDetails _roomDetails = null;
    /// <summary>
    /// LobbyNetwork.
    /// </summary>
    private LobbyNetwork _lobbyNetwork = null;
    /// <summary>
    /// Becomes true once someone has won.
    /// </summary>
    private bool _winner = false;
    /// <summary>
    /// Currently spawned player objects. Only exist on the server.
    /// </summary>
    private List<NetworkObject> _spawnedPlayerObjects = new List<NetworkObject>();


    #region Initialization and Deinitialization.
    private void OnDestroy()
    {
        if (_lobbyNetwork != null)
        {
            _lobbyNetwork.OnClientJoinedRoom -= LobbyNetwork_OnClientStarted;
            _lobbyNetwork.OnClientLeftRoom -= LobbyNetwork_OnClientLeftRoom;
        }
    }

    /// <summary>
    /// Initializes this script for use.
    /// </summary>
    public void FirstInitialize(RoomDetails roomDetails, LobbyNetwork lobbyNetwork)
    {
        _roomDetails = roomDetails;
        _lobbyNetwork = lobbyNetwork;
        _lobbyNetwork.OnClientStarted += LobbyNetwork_OnClientStarted;
        _lobbyNetwork.OnClientLeftRoom += LobbyNetwork_OnClientLeftRoom;
    }

    /// <summary>
    /// Called when a client leaves the room.
    /// </summary>
    /// <param name="arg1"></param>
    /// <param name="arg2"></param>
    private void LobbyNetwork_OnClientLeftRoom(RoomDetails arg1, NetworkObject arg2)
    {
        //Destroy all of clients objects, except their client instance.
        for (int i = 0; i < _spawnedPlayerObjects.Count; i++)
        {
            NetworkObject entry = _spawnedPlayerObjects[i];
            //Entry is null. Remove and iterate next.
            if (entry == null)
            {
                _spawnedPlayerObjects.RemoveAt(i);
                i--;
                continue;
            }

            //If same connection to client (owner) as client instance of leaving player.
            if (_spawnedPlayerObjects[i].Owner == arg2.Owner)
            {
                //Destroy entry then remove from collection.
                entry.Despawn();
                _spawnedPlayerObjects.RemoveAt(i);
                i--;
            }

        }
    }

    /// <summary>
    /// Called when a client starts a game.
    /// </summary>
    /// <param name="roomDetails"></param>
    /// <param name="client"></param>
    private void LobbyNetwork_OnClientStarted(RoomDetails roomDetails, NetworkObject client)
    {
        //Not for this room.
        if (roomDetails != _roomDetails)
            return;
        //NetIdent is null or not a player.
        if (client == null || client.Owner == null)
            return;

        /* POSSIBLY USEFUL INFORMATION!!!!!
         * POSSIBLY USEFUL INFORMATION!!!!!
         * If you want to wait until all players are in the scene
         * before spaning then check if roomDetails.StartedMembers.Count
         * is the same as roomDetails.MemberIds.Count. A member is considered
         * started AFTER they have loaded all of the scenes. */
        
        //SpawnPlayer(client.Owner);
    }
    #endregion


}
