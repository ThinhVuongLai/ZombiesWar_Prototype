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
        Rigidbody _rigidbody;

        public void Initialize(float lifespan, float actionRadius, float damage,
            float gravityScale, IThrowAction action, Vector3 velocity)
        {
            _lifespan = lifespan;
            _action = action;
            _actionRadius = actionRadius;
            _actionDamage = damage;
            _gravityScale = gravityScale;

            _rigidbody = GetComponent<Rigidbody>();
            _rigidbody.useGravity = false;
            _rigidbody.velocity = velocity;
        }

        void FixedUpdate()
        {
            _rigidbody.AddForce(Physics.gravity * _gravityScale, ForceMode.Acceleration);
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
