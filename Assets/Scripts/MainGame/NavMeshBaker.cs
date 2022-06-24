using Unity.AI.Navigation;
using UnityEngine;
using UnityEngine.AI;

public class NavMeshBaker : MonoBehaviour
{
    private NavMeshSurface _surface;
    public NavMeshObstacle[] Obstacles;
    
    public bool buildOnAwake = true;

    private void Awake()
    {
        _surface = GetComponent<NavMeshSurface>();
        
        if (buildOnAwake) BuildNavMeshAtRuntime();
    }

    public void BuildNavMeshAtRuntime() => _surface.BuildNavMesh();
}
