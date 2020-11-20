using System.Collections.Generic;
using UnityEngine;

namespace EuNet.Unity
{
    /// <summary>
    /// 특정 에셋 디렉토리를 포함하여 스프라이트를 Resources처럼 사용할 수 있게함.
    /// UI에서 아이콘 등이 변경되어야 할때 사용할 아이콘을 모두 수동으로 등록하는 불편을 없애기 위해 사용됨
    /// </summary>
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

        /// <summary>
        /// 파일이름 해시로 스프라이트를 가져옴. string.GetHashCode()
        /// </summary>
        /// <param name="nameHash">파일이름 해시</param>
        /// <returns></returns>
        public static Sprite GetSprite(int nameHash)
        {
            Sprite sprite;
            Instance._sprites.TryGetValue(nameHash, out sprite);

            return sprite;
        }

        /// <summary>
        /// 파일이름으로 스프라이트를 가져옴
        /// </summary>
        /// <param name="name">파일이름</param>
        /// <returns></returns>
        public static Sprite GetSprite(string name)
        {
            return GetSprite(name.GetHashCode());
        }
    }
}