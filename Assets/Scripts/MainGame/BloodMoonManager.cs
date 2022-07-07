using UnityEngine;

public class BloodMoonManager : MonoBehaviour
{
    public static BloodMoonManager Instance;
    
    // Game variables
    public int maximumBMProgress { get; private set; } = 1000;
    public int currentBMProgress { get; private set; } = 0;
    public readonly int standardBMProgressPerNight = 150;
    
    private void Awake()
    {
        if (Instance)
        {
            Destroy(this);
            return;
        }
        Instance = this;
    }
}
