namespace Assets.Code.Tools
{
    public abstract class Singleton<T> where T : Singleton<T>, new()
    {
        private static T _instance;

        public static T Instance
        {
            get
            {
                if (_instance == null)
                    lock(typeof(T))
                        if (_instance == null)
                        {
                            _instance = new T();
                            ScriptHost.Instance.Destroyed += InstOnDestroyed;
                        }

                return _instance;
            }
        }

        protected virtual void OnDestroy()
        {
            
        }

        private static void InstOnDestroyed()
        {
            _instance.OnDestroy();
            _instance = default(T);
        }
    }
}