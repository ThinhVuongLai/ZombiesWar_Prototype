using UnityEngine;

namespace ZombiesWar.ThrowingWeapon
{
    [RequireComponent(typeof(Rigidbody))]
    public class ThrownObject : MonoBehaviour
    {
        float _lifespan;
        float _elapsed;
        IThrowAction _action;
        float _actionRadius;
        float _actionDamage;
        float _gravityScale;
        bool _executed;
        Rigidbody _rb;

        public void Initialize(ThrowWeaponConfig config, IThrowAction action, Vector3 velocity)
        {
            _lifespan = config.ObjectLifespan;
            _action = action;
            _actionRadius = config.ActionRadius;
            _actionDamage = config.ActionDamage;
            _gravityScale = config.GravityScale;

            _rb = GetComponent<Rigidbody>();
            _rb.useGravity = false;
            _rb.velocity = velocity;
        }

        void FixedUpdate()
        {
            _rb.AddForce(Physics.gravity * _gravityScale, ForceMode.Acceleration);
        }

        void Update()
        {
            _elapsed += Time.deltaTime;
            if (_elapsed >= _lifespan)
            {
                ExecuteAction();
            }
        }

        void OnCollisionEnter(Collision collision)
        {
            ExecuteAction();
        }

        void ExecuteAction()
        {
            if (_executed) return;
            _executed = true;

            _action?.Execute(transform.position, _actionRadius, _actionDamage);

            Destroy(gameObject);
        }
    }
}
