using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;
using Random = UnityEngine.Random;

public class Map : MonoBehaviour
{
    public GameObject treesFolder;
    public GameObject villageFolder;
    public GameObject stonesFolder;
    public GameObject bushesFolder;
    public GameObject village;
    // public int seed ;
    public List<GameObject> deadTrees;
    public List<GameObject> hugeTrees;
    public List<GameObject> longTrees;
    public List<GameObject> simpleTrees;
    public List<GameObject> stones;
    public List<GameObject> bushes;

    // Start is called before the first frame update
    public void Generate(int seed)
    {
        Random.InitState(seed);
        // Random.seed = seed;
        AddVillage();
        AddStones();
        AddBushes();
        AddTrees();
    }

    // Update is called once per frame
    void Update()
    {
    }

    public void AddVillage()
    {
        Vector2 v = Random.insideUnitCircle * 100;
        float x = v.x;
        float z = v.y;
        float y = 0;
        RaycastHit hit;
        int layerMask = 1 << 3; // define the only layer that the raycast can touch
        layerMask = ~layerMask; // inverse the bits to ignore specifically the 3; 
        if (Physics.Raycast(new Vector3(x, 100, z), Vector3.down, out hit, 500, layerMask))
        {
            if (hit.transform.tag == "mapFloor")
            {
                y = hit.point.y + 4.5f;
                Vector3 direction = hit.normal;
                Instantiate(village, new Vector3(x, y, z), RandomRotation(), villageFolder.transform);
                
            }

        }
}

    public void AddBushes()
    {
        int add = 20; // a possible tree each 50 m2
        List<float[]> possibleBushes = RandomListXY(add); //List of position of all trees (maybe not possible)
        foreach (float[] possibleBush in possibleBushes)
        {
            float y = 0f;
            Vector3 direction = new Vector3(0, 0, 0);
            if (PositionValid(possibleBush, ref y, ref direction))
            { 
                GameObject bush = Instantiate(
                    RandomBush(), new Vector3(possibleBush[0], y-0.2f,possibleBush[1]), 
                    StoneRotation(direction), bushesFolder.transform);
                bush.tag = "bush";
            }
        }
    }
    
    public void AddStones()
    {
        int add = 50; // a possible tree each 50 m2
        List<float[]> possibleStones = RandomListXY(add); //List of position of all trees (maybe not possible)
        foreach (float[] possibleStone in possibleStones)
        {
            float y = 0f;
            Vector3 direction = new Vector3(0, 0, 0);
            if (PositionValid(possibleStone, ref y, ref direction))
            { 
                GameObject stone = Instantiate(
                    RandomStone(), new Vector3(possibleStone[0], y+0.1f,possibleStone[1]), 
                    StoneRotation(direction), stonesFolder.transform);
                stone.tag = "stone";
            }
        }
    }
    
    public void AddTrees()
    {
        int add = 4; // a possible tree each 4 m2
        List<float[]> possibleTrees = RandomListXY(add); //List of position of all trees (maybe not possible)
        foreach (float[] possibleTree in possibleTrees)
        {
            float y = 0f;
            Vector3 dir = new Vector3(0, 0, 0); // useless for the trees cause they only go straight up
            if (PositionValid(possibleTree, ref y, ref dir))
            { 
                GameObject tree = Instantiate(
                    RandomTree(), new Vector3(possibleTree[0], y,possibleTree[1]), 
                    RandomRotation(), treesFolder.transform);
                tree.tag = "tree";
            }
        }
    }

    public static List<float[]> RandomListXY(int add)
    {
        List<float[]> list = new List<float[]>();
        float randAdd = (float)add / 3f;
        for (int x = -490; x <= 490; x+=add)
        {
            for (int z = -490; z <= 490; z+=add)
            {
                float randomX = Random.Range(-randAdd, randAdd);
                float randomZ = Random.Range(-randAdd, randAdd);
                list.Add(new float[] {x+randomX, z+randomZ});
            }
        }
        return list;
    }
    
    public static bool PositionValid(float[] possibleObject,ref float y, ref Vector3 direction)
    {
        RaycastHit hit;
        float x = possibleObject[0];
        float z = possibleObject[1];
        if (Physics.Raycast(new Vector3(x, 100, z),Vector3.down,out hit,120))
        {
            if (hit.transform.tag == "mapFloor")
            {
                y = hit.point.y;
                direction = hit.normal;
                if (direction.y > 0.95f)
                {
                    return true;
                }
            }
        }

        return false;
    }
    
    public static Quaternion RandomRotation()
    {
        return Quaternion.Euler(new Vector3(0,Random.Range(-360, 360), 0));
    }

    public Quaternion StoneRotation(Vector3 dir)
    {
        float x = Random.Range(-10f, 10f);
        float z = Random.Range(-10f, 10f);
        return Quaternion.LookRotation(new Vector3(x,0,z),dir);
    }
    
    public GameObject RandomBush()
    {
        return bushes[Random.Range(0, bushes.Count)];
    }
    public GameObject RandomStone()
    {
        return stones[Random.Range(0, stones.Count)];
    }
    
    public GameObject RandomTree()
    {
        float x = Random.value;
        if (x < 0.5f)
        {
            return simpleTrees[Random.Range(0,simpleTrees.Count)];
        }
        if(x < 0.90)
        {
            return longTrees[Random.Range(0, longTrees.Count)];
        }

        if (x < 0.995)
        {
            return deadTrees[Random.Range(0, deadTrees.Count)];
        }
        else
        {
            return hugeTrees[Random.Range(0, hugeTrees.Count)];
        }
    }
}
