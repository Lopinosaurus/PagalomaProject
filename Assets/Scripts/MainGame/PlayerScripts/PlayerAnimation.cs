using UnityEngine;
using MainGame.PlayerScripts;
using Vector2 = UnityEngine.Vector2;

public class PlayerAnimation : MonoBehaviour
{
    // Scripts components
    private PlayerMovement _playerMovement;
    [SerializeField] private Avatar _villagerAvatar;
    [SerializeField] private Avatar _werewolfAvatar;
    private Animator _currentAnimator;

    // Boolean States hashes
    private readonly int _isCrouchingHash = Animator.StringToHash("isCrouching");
    
    // Trigger States hashes
    private readonly int _jumpHash = Animator.StringToHash("Jump");
    private readonly int _deathHash = Animator.StringToHash("Death");
    private readonly int _attackHash = Animator.StringToHash("Attack");

    // Float States hashes
    private readonly int _velocityXHash = Animator.StringToHash("VelocityX");
    private readonly int _velocityZHash = Animator.StringToHash("VelocityZ");
    
    // Layer hashes
    private int _WerewolfLayerIndex;

    // Movement settings
    private Vector2 velocity2D;
    private Vector2 velocity2Draw;

    private void Awake()
    {
        _playerMovement = GetComponent<PlayerMovement>();
        _currentAnimator = GetComponent<Animator>();
        _WerewolfLayerIndex = _currentAnimator.GetLayerIndex("WerewolfLayer");
    }

    public void UpdateAnimationsBasic()
    {
        var velocity3D = transform.InverseTransformDirection(_playerMovement._characterController.velocity);
        velocity2Draw = new Vector2
        {
            x = velocity3D.x,
            y = velocity3D.z
        };

        velocity2D = Vector2.Lerp(velocity2D, velocity2Draw, 10f * Time.deltaTime);

        CorrectDiagonalMovement(true);

        // Toggles "Crouch" animation
        _currentAnimator.SetBool(_isCrouchingHash, _playerMovement.currentMovementType == PlayerMovement.MovementTypes.Crouch);
        
        // Sets the velocity in X to the CharacterController X velocity
        _currentAnimator.SetFloat(_velocityXHash, velocity2D.x);
        
        // Sets the velocity in Z to the CharacterController Z velocity
        _currentAnimator.SetFloat(_velocityZHash, velocity2D.y);
    }

    private void CorrectDiagonalMovement(bool perform)
    {
        if (!perform) return;

        float angleX = -Vector2.SignedAngle(velocity2D, Vector2.right) * Mathf.Deg2Rad;
        float angleY = -Vector2.SignedAngle(velocity2D, Vector2.up) * Mathf.Deg2Rad;
        
        var magnitude = velocity2Draw.magnitude;
        
        float velocityX = Mathf.Clamp(Mathf.Abs(Mathf.Tan(angleX) * magnitude), 0, magnitude)
                          * (velocity2Draw.y > 0 ? 1 : -1);
        float velocityY = Mathf.Clamp(Mathf.Abs(Mathf.Tan(angleY) * magnitude), 0, magnitude)
                          * (velocity2Draw.x > 0 ? 1 : -1);

        velocity2D = new Vector2(velocityY, velocityX);
        // Debug.Log($"{velocity2D}");
    }

    public void EnableDeathAppearance()
    {
        // Toggles "Dying" animation
        _currentAnimator.SetTrigger(_deathHash);
    }
    
    public void StartJumpAnimation(bool active)
    {
        // Toggles "Dying" animation
        if (active)
        {
            _currentAnimator.SetTrigger(_jumpHash);
        }
        else
        {
            _currentAnimator.ResetTrigger(_jumpHash);
        }
    }

    public void EnableWerewolfAnimations(bool toWerewolf)
    {
        if (toWerewolf)
        {
            _currentAnimator.avatar = _werewolfAvatar;
            _currentAnimator.SetLayerWeight(_WerewolfLayerIndex, 1);
        }
        else
        {
            _currentAnimator.avatar = _villagerAvatar;
            _currentAnimator.SetLayerWeight(_WerewolfLayerIndex, 0);
        }        
    }
    
    public void StartAttackAnimation(bool active)
    {
        // Toggles "Attack" animation
        if (active)
        {
            _currentAnimator.SetTrigger(_attackHash);
        }
        else
        {
            _currentAnimator.ResetTrigger(_attackHash);
        }
    }
}


