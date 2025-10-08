using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpiderMovement : MonoBehaviour
{
    public bool IsGrounded;
    [SerializeField] private float _speed = 40f;
    [SerializeField] private float _acceleration = 15f;
    [SerializeField] private float _gravity = 20f;
    [SerializeField] private float _turnSpeed = 27f;
    //place these four coordinates by their name (fl = front left etc.)
    //put y a little above the object origin (like ~0.05f, but make sure smaller than ground check distance)
    [SerializeField] private Transform _rayObjfl;
    [SerializeField] private Transform _rayObjfr;
    [SerializeField] private Transform _rayObjbl;
    [SerializeField] private Transform _rayObjbr;
    [SerializeField] private LayerMask _groundLayer;
    [SerializeField] private float _groundCheckDistance = 0.1f;
    [SerializeField] private float _chaseDistance = 10f;
    [SerializeField] private Transform _player;
    [SerializeField][Range(0,1)] private float _mixUpwardMovement = 0.1f;
    private RaycastHit[] hitResultfl = new RaycastHit[3];
    private RaycastHit[] hitResultfr = new RaycastHit[3];
    private RaycastHit[] hitResultbl = new RaycastHit[3];
    private RaycastHit[] hitResultbr = new RaycastHit[3];
    private Rigidbody _rigidbody;
    public bool HasMoveInput;
    private Vector3 BodyForward;
    private Vector3 BodyRight;

    public Vector3 LookDirection;
    public Vector3 MoveInput;
    public Vector3 Velocity { get => _rigidbody.velocity; protected set => _rigidbody.velocity = value; }
    private void Start()
    {
        _rigidbody = GetComponent<Rigidbody>();
        _rigidbody.freezeRotation = true;
    }
    private void Update()
    {
        DetectSurface();
        Navigating();
        //give the spider boid behavior to avoid walls and obstacles while moving towards player
    }

    private void FixedUpdate()
    {
        
        //calculate velocity
        Vector3 input = MoveInput;
        Vector3 right = Vector3.Cross(transform.up, input);
        Vector3 forward = input - Vector3.Dot(input, transform.up) * transform.up;
        Vector3 targetVelocity = forward * _speed;
        Vector3 acceleration = (targetVelocity - Velocity) * (_acceleration * (IsGrounded? 1f: 0.15f));
        if (!IsGrounded && !HasMoveInput) acceleration = Vector3.zero;
        acceleration += Vector3.Cross(BodyRight, BodyForward) * _gravity;
        Debug.Log(acceleration);
        _rigidbody.AddForce(acceleration);

        //if (IsGrounded)
        {
            Quaternion targetRotation = Quaternion.LookRotation(LookDirection, Vector3.Cross(BodyForward, BodyRight));
            Quaternion delta = targetRotation * Quaternion.Inverse(_rigidbody.rotation);
            Quaternion newRotation = Quaternion.Slerp(_rigidbody.rotation, targetRotation, _turnSpeed * Time.fixedDeltaTime);
            _rigidbody.MoveRotation(newRotation);
            delta.ToAngleAxis(out float angle, out Vector3 axis);
            if (angle > 180f) angle -= 360f;
            if (Mathf.Abs(angle) > 0.01f)
            {
                _rigidbody.angularVelocity = axis.normalized * angle * Mathf.Deg2Rad / Time.deltaTime;
            }
            else
            {
                _rigidbody.angularVelocity = Vector3.zero;
            }
            /*
            if (MoveInput.magnitude < 0.1f) return;
            LookDirection = input - Vector3.Dot(input, transform.up) * transform.up;
            Vector3 forwardLocal = transform.forward;
            float turnDirection = -Mathf.Sign(Vector3.Cross(LookDirection, forwardLocal).y);
            float turnMagnitude = 1f - Mathf.Clamp01(Vector3.Dot(forwardLocal, LookDirection));
            float angularVelocity = turnMagnitude * _turnSpeed * turnDirection;
            _rigidbody.angularVelocity += new Vector3(0f, angularVelocity, 0f);
            //newRotation *= Quaternion.LookRotation(new Vector3(0f, angularVelocity, 0f));
            */
        }

    }

    private void Navigating()
    {
        Debug.Log(Vector3.Distance(_player.position, transform.position));
        if (Vector3.Distance(_player.position, transform.position) < _chaseDistance)
        {
            //MoveInput = (_player.position - transform.position).normalized;
            Vector3 direction = _player.position - transform.position;
            //subtraction non-horizontal component from the movement
            Vector3 updir = Vector3.Dot(direction, transform.up) * transform.up;
            direction -= updir;
            MoveInput = (direction + updir * _mixUpwardMovement).normalized;
            LookDirection = new Vector3(direction.x, 0f, direction.z).normalized;
            HasMoveInput = true;
        }
        else
        {
            HasMoveInput = false;
        }
    }

    private void DetectSurface()
    {

        //use average of front r-l and back r-l to get a right vector and same for front and store it for later time for smooth transition
        int hitsfl = Physics.RaycastNonAlloc(_rayObjfl.position, -transform.up, hitResultfl, _groundCheckDistance, _groundLayer);
        int hitsfr = Physics.RaycastNonAlloc(_rayObjfr.position, -transform.up, hitResultfr, _groundCheckDistance, _groundLayer);
        int hitsbr = Physics.RaycastNonAlloc(_rayObjbr.position, -transform.up, hitResultbr, _groundCheckDistance, _groundLayer);
        int hitsbl = Physics.RaycastNonAlloc(_rayObjbl.position, -transform.up, hitResultbl, _groundCheckDistance, _groundLayer);
        Vector3 rightVec = transform.right;
        Vector3 forwardVec = transform.forward;
        if (hitsbl >= 1 && hitsfl >= 1 && hitsbr >= 1 && hitsfr >= 1)
        {
            rightVec = ((hitResultfr[0].point - hitResultfl[0].point).normalized + (hitResultbr[0].point - hitResultbl[0].point).normalized) / 2;
            forwardVec = ((hitResultfr[0].point - hitResultbr[0].point).normalized + (hitResultfl[0].point - hitResultbl[0].point).normalized) / 2;
            BodyForward = forwardVec;
            BodyRight = rightVec;
            //these are the target forward/right vectors that would go into the spider SetLookRotation
        }
        //check grounding but using transform downward vector
        IsGrounded = (hitsbl >= 1 || hitsbr >= 1 || hitsfl >= 1 || hitsfr >= 1);
        Debug.Log(hitResultfl[0].collider.name);
        Debug.Log(hitsfl);
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = new Color(1f, 0f, 0f, 1f);
        Gizmos.DrawLine(transform.position, transform.position + BodyForward * 0.5f);
        Gizmos.DrawLine(transform.position, transform.position + BodyRight * 0.5f);
    }

}
