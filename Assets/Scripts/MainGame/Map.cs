using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;
using Random = UnityEngine.Random;
using Photon.Pun;
using Photon.Realtime;

public class Map : MonoBehaviour
{
    public GameObject tree1;
        public GameObject tree2;
        public GameObject village;
        public int seed ;
    
        // Start is called before the first frame update
        public void Generate(int _seed)
        {
            seed = _seed;
            Random.seed = seed;
            Vector3 villagePosition = Random.insideUnitSphere * 150;
            villagePosition.y = 0;
            Instantiate(village, villagePosition, Quaternion.identity);
            
            trees();
        }
    
   
        public void trees()
        {
            List<float[]> possibleTrees = RandomListXY(); //List of position of all trees (maybe not possible)
            foreach (float[] possibleTree in possibleTrees)
            {
                float y = 0f;
                if (PositionValid(possibleTree, ref y))
                { 
                    GameObject tree = Instantiate(RandomTree(), new Vector3(possibleTree[0], y,possibleTree[1]), RandomRotation());
                }
            }
        }
    
        public static List<float[]> RandomListXY()
        {
            List<float[]> list = new List<float[]>();
            for (int x = -490; x <= 490; x+=6)
            {
                for (int z = -490; z <= 490; z+=6)
                {
                    float randomX = Random.Range(-2.5f, 2.5f);
                    float randomZ = Random.Range(-2.5f, 2.5f);
                    list.Add(new float[] {x+randomX, z+randomZ});
                }
            }
            return list;
        }
        public static bool PositionValid(float[] possibleTree,ref float y)
        {
            RaycastHit hit;
            float x = possibleTree[0];
            float z = possibleTree[1];
            if (Physics.Raycast(new Vector3(x, 100, z), Vector3.down, out hit))
            {
                if (hit.transform.tag == "mapFloor")
                {
                    y = hit.point.y;
                    Vector3 direction = hit.normal;
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
    
        public GameObject RandomTree()
        {
            if (Random.value < 0.7f)
            {
                return tree1;
            }
            else
            {
                return tree2;
            }
        }
}
