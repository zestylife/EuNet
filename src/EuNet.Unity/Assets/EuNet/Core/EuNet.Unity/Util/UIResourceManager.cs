using System.Collections.Generic;
using UnityEngine;

namespace EuNet.Unity
{
    [ExecutionOrder(-1000)]
    public class UIResourceManager : SceneSingleton<UIResourceManager>
    {
        public string[] SpriteAssetPath = new string[] { "Assets/Data/UI/Sprites" };

        [HideInInspector]
        public List<Sprite> Sprites;

        private Dictionary<int, Sprite> _sprites;

        protected override void Awake()
        {
            base.Awake();

            _sprites = new Dictionary<int, Sprite>(Sprites.Count);
            foreach (var item in Sprites)
            {
                _sprites.Add(item.name.GetHashCode(), item);
            }
        }

        public static Sprite GetSprite(int nameHash)
        {
            Sprite sprite;
            Instance._sprites.TryGetValue(nameHash, out sprite);

            return sprite;
        }

        public static Sprite GetSprite(string name)
        {
            return GetSprite(name.GetHashCode());
        }
    }
}