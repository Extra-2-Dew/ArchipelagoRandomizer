using UnityEngine;

namespace ArchipelagoRandomizer
{
    public class PreviewItemData
    {
        // name of the item
        public string key;
        // If the model needs to exist on multiple entries, put the extra entry here
        public string copyTo = "";
        // path from LevelRoot
        public string path;
        // if it's part of a selector, which index in the selector should be used?
        public int index = 0;
        // Index of child to apply visual edits
        public int child = 0;
        // use the Bad Dream card animation
        public bool spin = false;
        // offset from default position
        public Vector3 position = Vector3.zero;
        // euler angles
        public Vector3 rotation = Vector3.zero;
        // scale in all dimensions
        public float scale = 1;
    }
}
