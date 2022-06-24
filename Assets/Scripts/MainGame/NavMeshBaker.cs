using System;
using Unity.AI.Navigation;
using UnityEngine;

public class NavMeshBaker : MonoBehaviour
{
    private NavMeshSurface _surface;

    private void Awake()
    {
        _surface = GetComponent<NavMeshSurface>();
    }

    public void BuildNavMeshAtRuntime()
    {
        _surface.BuildNavMesh();
    }
}
