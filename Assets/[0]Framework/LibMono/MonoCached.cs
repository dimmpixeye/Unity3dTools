/*===============================================================
Product:    Battlecruiser
Developer:  Dimitry Pixeye - pixeye@hbrew.store
Company:    Homebrew - http://hbrew.store
Date:       24/06/2017 20:56
================================================================*/
 
#if ODIN_INSPECTOR
using Sirenix.OdinInspector;
#endif
using UnityEngine;


namespace Homebrew
{
    public class MonoCached : MonoBehaviour
    {
        #region FIELDS

        [InfoBox("Monocached is a base monobehavior class of the ACTORS framework. It is used to initialize an object, handle object pooling/destroying.", InfoMessageType.Info)] 
 
        [FoldoutGroup("Mono")] public Pool pool;
        [FoldoutGroup("Mono")] public float timeDestroyDelay;
        [FoldoutGroup("Mono")] public float timeScale = 1;
        [FoldoutGroup("Mono")] public Actor actorParent;
        [FoldoutGroup("Mono")] public EntityState state;
 
       // [HideInInspector] public ProcessingSignals signals;
        [HideInInspector] public Transform selfTransform;

        #endregion

        #region MONO

        protected virtual void Awake()
        {
            selfTransform = transform;

            state.enabled = false;
            state.initialized = false;
 

            if (Starter.initialized == false)
            {
                state.requireStarter = true;

                return;
            }


            if (state.requireActorParent)
            {
                actorParent = GetComponentInParent<Actor>();
            }


            Setup();

            Timer.Add(Time.DeltaTimeFixed, () =>
            {
                state.initialized = true;

                PostSetup();
            });
        }

        public virtual void OnEnable()
        {
            if (state.enabled || state.requireStarter ||
                state.requireActorParent) return;

            state.released = false;
            state.enabled = true;


            ProcessingSignals.Default.Add(this);

            ProcessingUpdate.Default.Add(this);

            HandleEnable();
        }


        public virtual void OnDisable()
        {
           
            ProcessingSignals.Default.Remove(this);
            if (Toolbox.isQuittingOrChangingScene() || !state.enabled) return;

            state.enabled = false;

            
            ProcessingUpdate.Default.Remove(this); 

            HandleDisable();
        }

        #endregion

        #region SETUPS

        public virtual void SetupAfterStarter()
        {
            state.requireStarter = false;
            Setup();
            OnEnable();

            Timer.Add(Time.DeltaTimeFixed, () =>
            {
              
                PostSetup();
                state.initialized = true;
            });
        }

        public void SetupAfterActor()
        {
            if (!state.requireActorParent) return;
            state.requireActorParent = false;
            Setup();
            OnEnable();

            Timer.Add(Time.DeltaTimeFixed, () =>
            {
             
                PostSetup();
                state.initialized = true;
            });
        }

        #endregion

        #region METHODS

      
        protected virtual void Setup()
        {
        }

        protected virtual void PostSetup()
        {
        }

        protected virtual void HandleEnable()
        {
        }

        protected virtual void HandleDisable()
        {
        }

        #endregion

        #region DESTROY

        protected virtual void OnHandleDestroy()
        {
        }

        public void HandleDestroy()
        {
            if (state.released) return;
            state.released = true;


            if (pool == Pool.None)
            {
                OnHandleDestroy();
                Destroy(gameObject, timeDestroyDelay);
                return;
            }

            HandleReturnToPool();
        }

        protected virtual void HandleReturnToPool()
        {
            var processingPool = ProcessingGoPool.Default;
            if (timeDestroyDelay > 0)
                Timer.Add(timeDestroyDelay, () => processingPool.Despawn(pool, gameObject));
            else processingPool.Despawn(pool, gameObject);
        }

        #endregion
    }
}