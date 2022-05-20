using UnityEngine;
using MainGame.PlayerScripts;
using Vector2 = UnityEngine.Vector2;

public class PlayerAnimation : MonoBehaviour
{
    // Scripts components
    [SerializeField] private PlayerMovement _playerMovement;
    private Animator _animator;
    [SerializeField] private Avatar _villager;
    [SerializeField] private Avatar _werewolf;

    // Boolean States hashes
    private readonly int _isCrouchingHash = Animator.StringToHash("isCrouching");
    private readonly int _deathHash = Animator.StringToHash("Death");
    private readonly int _jumpHash = Animator.StringToHash("Jump");

    // Float States hashes
    private readonly int _velocityXHash = Animator.StringToHash("VelocityX");
    private readonly int _velocityZHash = Animator.StringToHash("VelocityZ");

    // Movement settings
    const float halfPi = Mathf.PI * 0.5f;
    private Vector2 velocity2D;
    private Vector2 velocity2Draw;

    private void Awake()
    {
        _animator = GetComponent<Animator>();
    }

    public void UpdateAnimationsBasic()
    {
        var velocity3D = transform.InverseTransformDirection(_playerMovement._characterController.velocity);
        velocity2Draw = new Vector2
        {
            x = velocity3D.x,
            y = velocity3D.z
        };

        Vector2 unused = Vector2.zero;
        velocity2D = Vector2.SmoothDamp(velocity2D, velocity2Draw, ref unused, Time.deltaTime);

        CorrectDiagonalMovement(true);

        // Toggles "Crouch" animation
        _animator.SetBool(_isCrouchingHash, _playerMovement.currentMovementType == PlayerMovement.MovementTypes.Crouch);
        
        // Sets the velocity in X to the CharacterController X velocity
        _animator.SetFloat(_velocityXHash, velocity2D.x);
        
        // Sets the velocity in Z to the CharacterController Z velocity
        _animator.SetFloat(_velocityZHash, velocity2D.y);
    }

    private void CorrectDiagonalMovement(bool perform)
    {
        if (!perform) return;

        float angle = Vector2.SignedAngle(velocity2D, Vector2.right) * Mathf.Deg2Rad;
        var magnitude = velocity2D.magnitude;
        
        velocity2D.x = Mathf.Tan(angle) * magnitude;
        velocity2D.y = - Mathf.Tan(angle + halfPi) * magnitude;
    }

    public void EnableDeathAppearance()
    {
        // Toggles "Dying" animation
        _animator.SetTrigger(_deathHash);
    }
    
    public void StartJumpAnimation(bool active)
    {
        // Toggles "Dying" animation
        if (active)
        {
            _animator.SetTrigger(_jumpHash);
        }
        else
        {
            _animator.ResetTrigger(_jumpHash);
        }
    }

    public void EnableWerewolfAnimations(bool toWerewolf)
    {
        _animator.avatar = toWerewolf ? _werewolf : _villager;
    }
}
