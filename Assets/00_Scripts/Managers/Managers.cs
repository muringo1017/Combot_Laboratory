using UnityEngine;

public class Managers : MonoBehaviour
{
    private static Managers _instance;
    
    public static Managers Instance  {get {   return Initialize(); }  }
    
    //컴포넌트
    private InputManager _inputManager;

    //속성
    public static InputManager InputManager => Instance._inputManager;
    
    //싱글톤
    private void Awake()
    {
        if (_instance != null && _instance != this)
        {
            Destroy(gameObject);
            return;
        }

        _instance = this;
        DontDestroyOnLoad(gameObject);
        CreateManagers();

    }
    
    private void OnDestroy()
    {
        if (_instance == this)
        {
            _instance = null;
        }
    }

    private static Managers Initialize()
    {
         if (_instance == null)
            {
                _instance = FindObjectOfType<Managers>();
                if (_instance == null)
                {
                    var managersObject = new GameObject(nameof(Managers));
                    _instance = managersObject.AddComponent<Managers>();
                }
                _instance.CreateManagers();
            }
            //초기화 안되어있으면 초기화
            else if ( _instance._inputManager == null)
            {
                _instance.CreateManagers();
            }
            return _instance;
    }
    //초기화
    private void CreateManagers()
    {
        _inputManager = GetOrAddComponent<InputManager>(gameObject);
    }
    
    
    private static T GetOrAddComponent<T>(GameObject target) where T : Component
    {
        var component = target.GetComponent<T>();
        if (component == null)
        {
            component = target.AddComponent<T>();
        }
        return component;
    }
    
}
