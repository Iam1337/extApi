using System;
using UnityEngine;

namespace extApi
{
    public class ApiHooks : MonoBehaviour
    {
        public static event Action OnUpdate
        {
            add
            {
                _onUpdate += value;

                if (_instance == null)
                    _instance = new GameObject("Api Hooks").AddComponent<ApiHooks>();
            }
            remove => _onUpdate -= value;
        }

        private static event Action _onUpdate;
        private static ApiHooks _instance;

        protected void Awake() => DontDestroyOnLoad(gameObject);
        protected void Update() => _onUpdate?.Invoke();
    }
}