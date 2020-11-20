using UnityEngine;

namespace EuNet.Unity
{
    /// <summary>
    /// 특정 씬에만 전역적으로 사용될 싱글톤. 씬이 바뀌면 제거됨
    /// </summary>
    /// <typeparam name="T">싱글턴이 될 컴포넌트</typeparam>
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