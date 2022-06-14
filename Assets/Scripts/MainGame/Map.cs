using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

public class Map : MonoBehaviour
{
    public bool shouldGenerateViaSeed = true;
    [Space]
    public GameObject treesFolder;
    public GameObject villageFolder;
    public GameObject stonesFolder;
    public GameObject bushesFolder;
    public GameObject village;
    
    public List<GameObject> deadTrees;
    public List<GameObject> hugeTrees;
    public List<GameObject> longTrees;
    public List<GameObject> simpleTrees;
    public List<GameObject> stones;
    public List<GameObject> bushes;

    // Start is called before the first frame update
    public void Generate(int seed)
    {
        if (shouldGenerateViaSeed)
        {
            Random.InitState(seed);
            // Random.seed = seed;
            AddVillage();
            AddStones();
            AddBushes();
            AddTrees();
        }
    }

    private void AddVillage()
    {
        Vector2 randomVector = Random.insideUnitCircle * 60;

        float x = randomVector.x;
        float y = 16.5f;
        float z = randomVector.y;

        int layerMask = 1 << 3; // define the only layer that the raycast can touch
        layerMask = ~layerMask; // inverse the bits to ignore specifically the 3; 
        if (Physics.Raycast(new Vector3(x, 100, z), Vector3.down, out RaycastHit hit, 500, layerMask))
        {
            if (hit.transform.CompareTag("mapFloor"))
                Instantiate(village, new Vector3(x, y, z), RandomRotation(), villageFolder.transform);
        }   
    }

    private void AddBushes()
    {
        int add = 20; // a possible tree each 50 m2
        List<Vector3> possibleBushes = RandomListXY(add); //List of position of all trees (maybe not possible)
        foreach (Vector3 possibleBush in possibleBushes)
        {
            float y = 0f;
            Vector3 direction = new Vector3(0, 0, 0);
            if (PositionValid(possibleBush, ref y, ref direction))
            {
                GameObject bush = Instantiate(
                    RandomBush(), new Vector3(possibleBush.x, y - 0.2f, possibleBush.z),
                    StoneRotation(direction), bushesFolder.transform);
                bush.tag = "bush";
            }
        }
    }

    private void AddStones()
    {
        int add = 50; // a possible tree each 50 m2
        List<Vector3> possibleStones = RandomListXY(add); //List of position of all trees (maybe not possible)
        foreach (Vector3 possibleStone in possibleStones)
        {
            float y = 0f;
            Vector3 direction = new Vector3(0, 0, 0);
            if (PositionValid(possibleStone, ref y, ref direction))
            {
                GameObject stone = Instantiate(
                    RandomStone(), new Vector3(possibleStone.x, y + 0.1f, possibleStone.z),
                    StoneRotation(direction), stonesFolder.transform);
                stone.tag = "stone";
            }
        }
    }

    private void AddTrees()
    {
        int add = 4; // a possible tree each 4 m2
        List<Vector3> possibleTrees = RandomListXY(add); //List of position of all trees (maybe not possible)
        foreach (Vector3 possibleTree in possibleTrees)
        {
            float y = 0f;
            Vector3 dir = new Vector3(0, 0, 0); // useless for the trees cause they only go straight up
            if (PositionValid(possibleTree, ref y, ref dir))
            {
                GameObject tree = Instantiate(
                    RandomTree(), new Vector3(possibleTree.x, y, possibleTree.z),
                    RandomRotation(), treesFolder.transform);
                tree.tag = "tree";
            }
        }
    }

    private static List<Vector3> RandomListXY(int add)
    {
        List<Vector3> pos = new List<Vector3>();
        float randAdd = add / 3f;
        for (int x = -490; x <= 490; x += add)
        {
            for (int z = -490; z <= 490; z += add)
            {
                float randomX = Random.Range(-randAdd, randAdd);
                float randomZ = Random.Range(-randAdd, randAdd);
                pos.Add(new Vector3(x + randomX, 0, z + randomZ));
            }
        }

        return pos;
    }

    private static bool PositionValid(Vector3 posObject, ref float y, ref Vector3 direction)
    {
        float x = posObject.x;
        float z = posObject.z;
        if (Physics.Raycast(new Vector3(x, 100, z), Vector3.down, out RaycastHit hit, 120))
        {
            if (hit.transform.CompareTag("mapFloor"))
            {
                y = hit.point.y;
                direction = hit.normal;
                if (direction.y > 0.95f) return true;
            }
        }

        return false;
    }

    private static Quaternion RandomRotation()
    {
        return Quaternion.Euler(new Vector3(0, Random.Range(-360, 360), 0));
    }

    private Quaternion StoneRotation(Vector3 dir)
    {
        float x = Random.Range(-10f, 10f);
        float z = Random.Range(-10f, 10f);
        return Quaternion.LookRotation(new Vector3(x, 0, z), dir);
    }

    private GameObject RandomBush()
    {
        return bushes[Random.Range(0, bushes.Count)];
    }

    private GameObject RandomStone()
    {
        return stones[Random.Range(0, stones.Count)];
    }

    private GameObject RandomTree()
    {
        float x = Random.value;
        if (x < 0.5f) return simpleTrees[Random.Range(0, simpleTrees.Count)];

        if (x < 0.90) return longTrees[Random.Range(0, longTrees.Count)];

        if (x < 0.995) return deadTrees[Random.Range(0, deadTrees.Count)];
        
        return hugeTrees[Random.Range(0, hugeTrees.Count)];
    }

    public static GameObject FindMap() => GameObject.FindWithTag("village");
}