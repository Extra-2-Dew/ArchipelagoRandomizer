using System;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace ArchipelagoRandomizer
{
    public class GPS
    {
        private const float minRequiredDistanceForUpdate = 3.0f;
        public static GPS Instance = new();
        private Vector2 lastPosition = Vector2.zero;


        Entity player;

        GPS()
        {
            Events.OnPlayerSpawn += OnPlayerSpawn;
            Events.OnRoomChanged += OnRoomChanged;
        }

        private Vector2 GetPosition()
        {
            if (player == null)
            {
                return Vector2.zero;
            }

            return new Vector2(
                player.transform.position.x,
                player.transform.position.z
            );
        }

        private void OnPlayerSpawn(Entity player, GameObject _camera, PlayerController _controller)
        {
            this.player = player;
            UpdatePosition();
        }

        private void OnRoomChanged(Entity _entity, LevelRoom toRoom, LevelRoom _fromRoom, EntityEventsOwner.RoomEventData _data)
        {
            var levelName = toRoom.LevelRoot.LevelData.LevelName;
            Plugin.Log.LogMessage($"YOOO, level changed to `{levelName}`");
            APHandler.Instance.SetLevelName(levelName);
        }

        public void OnEntityUpdate(Entity entity)
        {
            if (entity != player)
            {
                return;
            }

            UpdatePosition();
        }

        public void UpdatePosition()
        {
            var position = GetPosition();
            var distance = Vector2.Distance(position, lastPosition);

            if (distance < minRequiredDistanceForUpdate)
            {
                return;
            }

            APHandler.Instance.SetPosition(position);

            lastPosition = position;
        }
    }
}
