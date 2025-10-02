using FC.Core.Interop.DependencyInjection;
using UnityEngine;

public abstract class Singleton<T> : MonoBehaviour where T : Component
{
    #region Properties

    /// <summary>
    ///     Gets the instance.
    /// </summary>
    /// <value>The instance.</value>
    public static T Instance
    {
        get
        {
            if (ApplicationLifecycle.IsQuitting)
                return null;

            if (_instance) return _instance;
            
            _instance = FindFirstObjectByType<T>();
            
            if (!_instance)
            {
                var obj = new GameObject
                {
                    name = typeof(T).Name
                };
                    
                _instance = obj.AddComponent<T>();
            }

            return _instance;
        }
    }

    #endregion

    #region Fields

    private static T _instance;

    #endregion

    #region Methods

    protected void Awake()
    {
        if (!_instance)
        {
            _instance = this as T;
            DontDestroyOnLoad(gameObject);
        }
        else if (_instance != this)
        {
            Destroy(gameObject);
        }
        
    }

    #endregion
}