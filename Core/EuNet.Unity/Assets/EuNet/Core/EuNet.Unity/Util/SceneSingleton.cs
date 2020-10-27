using UnityEngine;

namespace EuNet.Unity
{
    // 특정 씬에만 전역적으로 사용될 싱글톤
    public class SceneSingleton<T> : MonoBehaviour where T : Component
    {
        private static T s_instance;
        public static T Instance
        {
            get
            {
                if (s_instance == null)
                {
                    s_instance = FindObjectOfType<T>();
                }
                return s_instance;
            }
        }

        protected virtual void Awake()
        {
            s_instance = this as T;
        }

        protected virtual void OnDestroy()
        {
            s_instance = null;
        }
    }
}