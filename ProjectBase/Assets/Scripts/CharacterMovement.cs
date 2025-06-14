using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.UIElements;

[RequireComponent(typeof(Rigidbody))]
[RequireComponent(typeof(NavMeshAgent))]
public class CharacterMovement : MonoBehaviour
{
    [SerializeField] private bool _isPlayer = false;
    [Header("Movement")]
    [SerializeField] private float _speed = 3f;
    [SerializeField] private float _runSpeed = 8f;
    [SerializeField] private float _acceleration = 15f;
    [SerializeField] private float _turnSpeed = 27f;
    [SerializeField] private bool _controlRotation = true;
    private Camera _camera;

    [Header("Airborne")]
    [SerializeField] private float _gravity = -20f;
    [SerializeField] private float _jumpHeight = 2.25f;
    [SerializeField] private float _airControl = 0.1f;
    [SerializeField] private bool _airTurning = true;
    [SerializeField] private float _swordControl = 0.3f;
    [SerializeField] private bool _swordMode = false;


    [Header("Dash")]
    [SerializeField] private float _dashForce = 1f;
    [SerializeField] private float _dashTime = 0.5f;
    [SerializeField] private float _dashSmoothShift = 0.6f;
    [SerializeField] private float _dashDodgingInputMod = 0.2f;
    private float _dashTimer = 0f;

    [Header("Grounding")]
    [SerializeField] private float _groundCheckOffset = 0.1f;
    [SerializeField] private float _groundCheckDistance = 0.4f;
    [SerializeField] private float _maxSlopeAngle = 40f;
    [SerializeField] private float _groundFudgeTime = 0.25f;
    [SerializeField] private LayerMask _groundMask = 1 << 0;
    [SerializeField] private Transform _groundCheck;

    private GameObject _target;
    private Collider[] _col = new Collider[5];
    [Header("Attack")]

    [SerializeField] private GameObject _bossHealthBar;

    public float MoveSpeedMultiplier = 1f;
    public float Speed => _speed;
    public bool IsFudgeGrounded => Time.timeSinceLevelLoad < _lastGroundedTime + _groundFudgeTime;
    public Vector3 Velocity { get => _rigidbody.velocity; protected set => _rigidbody.velocity = value; }
    public Vector3 MoveInput;
    public Vector3 LocalMoveInput;
    public Vector3 LookDirection;
    public float AimAngle;

    public bool IsRunning = false;
    
    public Transform LookTarget;
    public bool HasMoveInput;
    public bool IsGrounded;
    public Vector3 SurfaceVelocity;
    public bool CanMove = true;
    public Vector3 GroundNormal = Vector3.up;
    private float _lastGroundedTime;
    private bool _isDashing = false;
    private NavMeshAgent _navMeshAgent;
    public float TurnSpeedMultiplier { get; set; } = 1f;
    public Vector3 GroundPoint;
    private Vector3 _groundCheckStart => transform.position + transform.up * _groundCheckOffset;
    //private CharacterAnimations _characterAnimations;



    [SerializeField] private float _findTargetRadius = 5f;
    [SerializeField] private float _targetLostLimit = 10f;
    [SerializeField] private LayerMask _findTargetMask = 1 << 6;
    [Header("AttackCorrection")]
    [SerializeField] private float _attackCorrectionMove;
    [SerializeField] private float _attackCorrectionThresholdDist = 0.35f;
    [SerializeField] private float _attackCorrectionMaxDist = 1.2f;

    public float MaxStamina = 100f;
    public float Stamina = 100f;
    public float StaminaPercent => Stamina / MaxStamina;

    private bool _applyAttackCorrection;


    // Boss / Enemy only
    [SerializeField] bool isAI;
    float lastMoveTime;
    [SerializeField] float moveCooldown = 4.0f; // Prevent "Move" commands resulting in jiggling back and forth unnaturally.

    private Rigidbody _rigidbody;
   // private EnemyMethods _em;

    // Start is called before the first frame update
    void Start()
    {
        lastMoveTime = Time.time - 999f;
        _camera = Camera.main;
        _rigidbody = GetComponent<Rigidbody>();
        //_characterAnimations = GetComponent<CharacterAnimations>();
        _rigidbody.constraints = RigidbodyConstraints.FreezeRotationX | RigidbodyConstraints.FreezeRotationZ;
        _rigidbody.interpolation = RigidbodyInterpolation.Interpolate;
        _rigidbody.useGravity = false;
        LookDirection = transform.forward;
        //if (!_isPlayer)
        //{ _em = GetComponent<EnemyMethods>(); }
        //else
        //    GlobalObjects.CharMove = this;

        _navMeshAgent = GetComponent<NavMeshAgent>();
        _navMeshAgent.updatePosition = false;
        _navMeshAgent.updateRotation = false;
    }

    public void SetAttackCorrection(bool b)
    {
        _applyAttackCorrection = b;
    }

    public void SetMoveInput(Vector3 input)
    {
        if (!CanMove)
        {
            MoveInput = Vector3.zero;
            return;
        }
        input = Vector3.ClampMagnitude(input, 1f);
        HasMoveInput = input.magnitude > 0.1f;
        input = HasMoveInput ? input : Vector3.zero;
        Vector3 flattened = new Vector3(input.x, 0f, input.z);
        flattened = flattened.normalized * input.magnitude;
        MoveInput = flattened;
        LocalMoveInput = transform.InverseTransformDirection(MoveInput);
    }

    public void SetBodyDirection(Vector3 direction, bool isCooldownConstrained = false)
    {


            if (_applyAttackCorrection)
            {
                if (_target != null)
                    direction = _target.transform.position - transform.position;
            }
        

        if (isCooldownConstrained && Time.time < lastMoveTime + moveCooldown) return;
        if (!CanMove) return;
        if (direction.magnitude < 0.1f) return;

        LookDirection = new Vector3(direction.x, 0f, direction.z).normalized;
        AimAngle = Mathf.Atan2(LookDirection.y, LookDirection.x) * Mathf.Rad2Deg - 90f;
        lastMoveTime = Time.time;
    }

    public void Dash()
    {
        if (Stamina < 30f) return;
        _dashTimer += _dashTime;
        Stamina -= 30f;
    }

    public void SetLookDirectionToCursor()
    {
        GameObject projectileSpawnObject = GameObject.Find("ProjectileSpawn");
        if (projectileSpawnObject != null)
        {
            Transform _projectileSpawn = projectileSpawnObject.transform;
            Vector3 mouseScreenPos = Input.mousePosition;
            Vector3 mouseWorldPos = Camera.main.ScreenToWorldPoint(new Vector3(mouseScreenPos.x, mouseScreenPos.y, _camera.nearClipPlane));
            Debug.DrawLine(_projectileSpawn.position, mouseWorldPos, Color.red);

            Plane playerPlane = new Plane(Vector3.up, transform.position);
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);

            if (playerPlane.Raycast(ray, out float hitDist))
            {
                Vector3 targetPoint = ray.GetPoint(hitDist);
                Quaternion targetRotation = Quaternion.LookRotation(targetPoint, -transform.position);
                targetRotation.x = 0;
                targetRotation.z = 0;
                transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, 7f * Time.deltaTime);
            }
        }
    }

    private Vector3 GetDirectionToCursor(Vector3 direction)
    {
        Vector3 mousePosition = Input.mousePosition;
        Ray ray = _camera.ScreenPointToRay(mousePosition);

        // Check for collision with the ground or desired plane
        if (Physics.Raycast(ray, out RaycastHit hitInfo, Mathf.Infinity, _groundMask))
        {
            // Calculate the direction from this object to the hit point
            return (hitInfo.point - transform.position).normalized;
        }

        // Default direction if no hit
        return new Vector3(direction.x, 0f, direction.z).normalized;
    }

    public void Jump()
    {
        if (!CanMove || !IsFudgeGrounded) return;
        float jumpVelocity = Mathf.Sqrt(2f * -_gravity * _jumpHeight);

        Velocity = Velocity != null ? new Vector3(Velocity.x, jumpVelocity, Velocity.z) : Vector3.zero;
    }


    IEnumerator DashTimer()
    {
        yield return new WaitForSeconds(_dashTime);
        _isDashing = false;
    }



    public void SetCanMove(bool canMove)
    {
        CanMove = canMove;
    }

    protected virtual Vector3 VelocityCalc()
    {
        Vector3 input = MoveInput;
        Vector3 right = Vector3.Cross(transform.up, input);
        Vector3 forward = Vector3.Cross(right, GroundNormal);

        Vector3 targetVelocity = forward * ((IsRunning? _runSpeed : _speed) * MoveSpeedMultiplier);
        if (!CanMove) targetVelocity = Vector3.zero;
        // adds velocity of surface under character, if character is stationary
        targetVelocity += SurfaceVelocity * (1f - Mathf.Abs(MoveInput.magnitude));

            if (_applyAttackCorrection)
            {
                float v = 0f;
                if (_target != null)
                {
                    float f = Vector3.Dot(_target.transform.position - transform.position, transform.forward);
                    //if too far then no correction
                    f = _attackCorrectionMaxDist - Mathf.Abs(f - _attackCorrectionMaxDist);
                    v = Mathf.Clamp(f, _attackCorrectionThresholdDist, _attackCorrectionMaxDist);
                    v = (v - _attackCorrectionThresholdDist) / (_attackCorrectionMaxDist - _attackCorrectionThresholdDist);
                }
                return transform.forward * _attackCorrectionMove * v;
            }
        
        return targetVelocity;
    }

    public void FindTarget()
    {
        GameObject target = null;
        if (Physics.OverlapSphereNonAlloc(transform.position, _findTargetRadius, _col, _findTargetMask) <= 0)
        {
            _target= null;
            return;
        }
        bool hasBoss = false;
        foreach (Collider c in _col)
        {
            if (c == null) break;
            //if (c.gameObject == GlobalObjects.Boss)
            //    hasBoss = true;
            if (_target != null)
            {
                if (c.gameObject == _target) continue;
            }

            if (target == null) target = c.gameObject;
            else
            {
                if (Vector3.Distance(transform.position, target.transform.position) > Vector3.Distance(transform.position, c.transform.position))
                { 
                    target = c.gameObject;
                }
            }
        }
        _target = target;
        _bossHealthBar.SetActive(hasBoss);

        return;
    }

    private void FixedUpdate()
    {
        if (Stamina < MaxStamina)
        {
            Stamina += Time.deltaTime * 10f;
            if (Stamina > MaxStamina) Stamina = MaxStamina;
        }

        Vector3 targetVelocity = VelocityCalc();
        // calculates acceleration required to reach desired velocity and applies air control if not grounded
        Vector3 velocityDiff = targetVelocity - Velocity;
        velocityDiff.y = 0f;
        float control = IsGrounded ? 1f : _airControl;
        control *= _swordMode ? _swordControl : 1f;
        Vector3 acceleration = velocityDiff * (_acceleration * control);

        if (_dashTimer > 0)
        {
            float forceModifier = Mathf.Sin(_dashTimer * Mathf.PI * _dashSmoothShift / _dashTime);
            _rigidbody.AddForce(transform.forward * _dashForce * (forceModifier + 0.5f) * (IsGrounded? 1:0.1f));
            if(Vector3.Dot(Velocity.normalized, Vector3.up) > 0.1)
            {
                _rigidbody.AddForce(Vector3.up * -1 * _dashForce);
            }
            _dashTimer--;
            //acceleration *= _dashDodgingInputMod;
            //mix lookdirection with forward transform
            LookDirection = LookDirection * 0.1f + transform.forward * 0.9f;
        }
        if (Vector3.Dot(MoveInput.normalized, transform.forward) < -0.8f && LocalMoveInput.magnitude > 0.2f)
        {
            //_characterAnimations.PlayAnimation("Turn");
            _rigidbody.angularVelocity = Vector3.zero;
        }
            // zeros acceleration if airborne and not trying to move (allows for nice jumping arcs)
            if (!IsGrounded && !HasMoveInput) acceleration = Vector3.zero;
        // add gravity
        acceleration += GroundNormal * _gravity;
        _rigidbody.AddForce(acceleration);

        // rotates character towards movement direction
        if (_controlRotation && (IsGrounded || _airTurning))
        {
            //look orthogonally if using orthogonal movement
            Vector3 forwardLocal = transform.forward;
            //if (_useOrthogonalMovement) transform.forward = Vector3.Cross(Vector3.up, transform.forward);
            float turnDirection = -Mathf.Sign(Vector3.Cross(LookDirection, forwardLocal).y);
            float turnMagnitude = 1f - Mathf.Clamp01(Vector3.Dot(forwardLocal, LookDirection));
            float angularVelocity = turnMagnitude * _turnSpeed * TurnSpeedMultiplier * turnDirection;
            _rigidbody.angularVelocity = new Vector3(0f, angularVelocity, 0f);
        }
    }

    void Update()
    {
        if (_isPlayer)
        {
            FindTarget();

        }
        else
        {
            //if (_em.Target != null)
            //{
            //    _target = _em.Target;
            //    Vector3 lookDir = _target.transform.position - transform.position;
            //    LookDirection = new Vector3(lookDir.x, 0f, lookDir.z).normalized;
            //}

        }
        //if(_target!= null)
        //Debug.Log($"{gameObject.name} has target on {_target}");

        IsGrounded = CheckGrounded();

        if (_navMeshAgent.hasPath)
        {
            Vector3 nextPathPoint = _navMeshAgent.path.corners[1];
            Vector3 pathDir = (nextPathPoint - transform.position).normalized;
            SetMoveInput(pathDir);
            SetBodyDirection(pathDir);

        }

        _navMeshAgent.nextPosition = transform.position;
    }


    public void MoveToward(Vector3 targetPosition, float distanceToMove=1, bool isCooldownConstrained=true)
    {
        if (isCooldownConstrained && Time.time < lastMoveTime + moveCooldown) return;

        Vector3 directionToTarget = (targetPosition - transform.position).normalized;
        Vector3 newDestination = transform.position + directionToTarget * distanceToMove;

        if (NavMesh.SamplePosition(newDestination, out NavMeshHit hit, 1.0f, NavMesh.AllAreas))
        {
            _navMeshAgent.SetDestination(hit.position);
            lastMoveTime = Time.time; // Update cooldown
        }
    }

    public void MoveAway(Vector3 targetPosition, float distanceToMove = 1, bool isCooldownConstrained = true)
    {

        if (isCooldownConstrained && Time.time < lastMoveTime + moveCooldown) return;
        Vector3 directionAway = (transform.position - targetPosition).normalized;
        Vector3 destination = transform.position + directionAway * distanceToMove;
        _navMeshAgent.SetDestination(destination);
        lastMoveTime = Time.time;
    }

    public void MoveSide(Vector3 target)
    {
        MoveSpeedMultiplier = 0.5f;
        Vector3 facing = (target - transform.position).normalized;
        Vector3 right = Vector3.Cross(facing, Vector3.up);
        //SetMoveInput(right * _em.SidewayDirection);
    }

    public void MoveTo(Vector3 destination)
    {
        MoveSpeedMultiplier = 1f;
        _navMeshAgent.SetDestination(destination);
    }

    public void MoveAwayFrom(Vector3 fromTarget)
    {
        MoveSpeedMultiplier = 0.7f;
        SetMoveInput(-transform.forward);
    }

    public void Stop()
    {
        _navMeshAgent.isStopped = true;
        _navMeshAgent.ResetPath();
        MoveSpeedMultiplier = 1;
        SetMoveInput(Vector3.zero);
    }

    private bool CheckGrounded()
    {
        // raycast to find ground
        //bool hit = Physics.Raycast(_groundCheckStart, -transform.up, out RaycastHit hitInfo, _groundCheckDistance, _groundMask)
        bool hit = Physics.CheckSphere(_groundCheck.position, _groundCheckDistance, _groundMask);
        return hit;
        // set default ground surface normal and SurfaceVelocity
        //GroundNormal = Vector3.up;
        //SurfaceVelocity = Vector3.zero;

        //// if ground wasn't hit, character is not grounded
        //if (!hit) return false;

        //// gets velocity of surface underneath character if applicable
        ////if (hitInfo.rigidbody != null) SurfaceVelocity = hitInfo.rigidbody.velocity;

        ////stores ground point
        ////GroundPoint = hitInfo.point;
        ////// test angle between character up and ground, angles above _maxSlopeAngle are invalid
        ////bool angleValid = Vector3.Angle(transform.up, hitInfo.normal) < _maxSlopeAngle;
        ////if (angleValid)
        ////{
        ////    // record last time character was grounded and set correct floor normal direction
        ////    _lastGroundedTime = Time.timeSinceLevelLoad;
        ////    GroundNormal = hitInfo.normal;
        ////    return true;
        ////}

        //return false;
    }
    public void InterpolateRigidBody() => _rigidbody.interpolation = RigidbodyInterpolation.Interpolate;


    public void AE_ResetAttackCorrection()
    {
        _applyAttackCorrection = false;
        SetCanMove(false);
    }
}