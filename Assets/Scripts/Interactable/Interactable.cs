using UnityEngine;


    public class Interactable : Entity
    {
        [Header("Animator")]
        [SerializeField] protected Animator animator = null;
        protected bool useState = false;
        protected bool interactState = false;
        #region GetterSetter
        public bool InteractState
        {
            get
            {
                return interactState;
            }
        }
        #endregion

        public virtual void StartInteract()
        {
            useState = true;
            animator.SetTrigger("Start");
        }
        
        public virtual void Interact(CharacterMove character)
        {
            if (useState)
            {
                animator.SetTrigger("Use");
                InteractServerRpc();
            }            
        }

        [ServerRpc(RequireOwnership = false)]
        public void InteractServerRpc()
        {
            Debug.Log("");
        }

        public virtual void StopInteract()
        {
            if (useState)
            {
                animator.SetTrigger("Stop");
                useState = false;
            }
        }

        public override void OnGameStateChanged(GameState newGameState)
        {
            base.OnGameStateChanged(newGameState);
            if(animator != null)
            {
                animator.speed = newGameState == GameState.GamePlay ? 1.0f : 0.0f;
            }
        }


        public virtual void ChangeInteract(){}

        protected virtual void Start(){}        
        protected virtual void Update(){}
    }

