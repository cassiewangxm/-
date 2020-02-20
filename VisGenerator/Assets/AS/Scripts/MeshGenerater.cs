using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Jobs;
using Unity.Collections;

public class MeshGenerater : MonoBehaviour
{
    public struct MeshJob : IJob
    {
        public float deltaTime;
        public NativeArray<Vector3> vertices;
        public NativeArray<Vector2> uvs;
        public NativeArray<int> tris;
        public NativeArray<Vector3> scales;       
        public NativeArray<Vector3> positions;
        public void Execute()
        {
            int[] contris = { 0, 3, 2, 2, 1, 0 };
            int n = scales.Length;
            int count = 0;
            int trisCount = 0;
            for(int i =0; i< n - 1;)
            {
                if(i+1 >= n)
                    break;


                float radius = scales[i].x/2;
                float radius2 = scales[i+1].x/2;
                float upY = positions[i].y;
                float downY = positions[i+1].y;

                //front
                {
                    uvs[count] = new Vector2(0,0);
                    vertices[count++] = new Vector3(-radius,upY,-radius);
                    uvs[count] = new Vector2(0,1);
                    vertices[count++] = new Vector3(radius,upY,-radius);
                }
                {
                    uvs[count] = new Vector2(1,1);
                    vertices[count++] = new Vector3(radius2,downY,-radius2);
                    uvs[count] = new Vector2(1,0);
                    vertices[count++] = new Vector3(-radius2,downY,-radius2);
                }

                for(int j =0; j<6; j++)
                {
                    tris[trisCount++] = (count/4 - 1)*4 + contris[j];
                }

                //right 
                {
                    uvs[count] = new Vector2(0,0);
                    vertices[count++] = new Vector3(radius,upY,-radius);
                    uvs[count] = new Vector2(0,1);
                    vertices[count++] = new Vector3(radius,upY,radius);
                }
                {
                    uvs[count] = new Vector2(1,1);
                    vertices[count++] = new Vector3(radius2,downY,radius2);
                    uvs[count] = new Vector2(1,0);
                    vertices[count++] = new Vector3(radius2,downY,-radius2);
                }

                for(int j =0; j<6; j++)
                {
                    tris[trisCount++] = (count/4 - 1)*4 + contris[j];
                }

                //back
                {
                    uvs[count] = new Vector2(0,0);
                    vertices[count++] = new Vector3(radius,upY,radius);
                    uvs[count] = new Vector2(0,1);
                    vertices[count++] = new Vector3(-radius,upY,radius);
                }
                {
                    uvs[count] = new Vector2(1,1);
                    vertices[count++] = new Vector3(-radius2,downY,radius2);
                    uvs[count] = new Vector2(1,0);
                    vertices[count++] = new Vector3(radius2,downY,radius2);
                }

                for(int j =0; j<6; j++)
                {
                    tris[trisCount++] = (count/4 - 1)*4 + contris[j];
                }

                //left
                {
                    uvs[count] = new Vector2(0,0);
                    vertices[count++] = new Vector3(-radius,upY,radius);
                    uvs[count] = new Vector2(0,1);
                    vertices[count++] = new Vector3(-radius,upY,-radius);
                }
                {
                    uvs[count] = new Vector2(1,1);
                    vertices[count++] = new Vector3(-radius2,downY,-radius2);
                    uvs[count] = new Vector2(1,0);
                    vertices[count++] = new Vector3(-radius2,downY,radius2);
                }

                for(int j =0; j<6; j++)
                {
                    tris[trisCount++] = (count/4 - 1)*4 + contris[j];
                }
                
                i++;
            }
        }
    }
    // public List<MeshFilter> quadlist;

    // public Mesh rootMesh;

    // //private float itemHeight = 0.5f; 

    // // Start is called before the first frame update
    // void Start()
    // {
    //     //MeshFilter mf = GetComponent<MeshFilter>();
    //     //mf.mesh = GenerateMesh(quadlist,quadlist.Count);
    //     int n = quadlist.Count;
    //     int quadN = 4*n-1;
    //     var vect = new NativeArray<Vector3>(quadN*4, Allocator.Persistent);
    //     var uvss = new NativeArray<Vector2>(quadN*4, Allocator.Persistent);
    //     var triangles = new NativeArray<int>(quadN*6, Allocator.Persistent);
    //     var scls = new NativeArray<Vector3>(n, Allocator.Persistent);
    //     var poss = new NativeArray<Vector3>(n, Allocator.Persistent);
    //     for(int i = 0; i < n; i++)
    //     {
    //         scls[i] = quadlist[i].transform.localScale;
    //         poss[i] = quadlist[i].transform.position;
    //     }

    //     var job = new MeshJob()
    //     {
    //         deltaTime = Time.deltaTime,
    //         vertices = vect,
    //         uvs = uvss,
    //         tris = triangles,
    //         scales = scls,
    //         positions = poss
    //     };

    //     JobHandle jobHandle = job.Schedule();

    //     // Ensure the job has completed
    //     // It is not recommended to Complete a job immediately,
    //     // since that gives you no actual parallelism.
    //     // You optimally want to schedule a job early in a frame and then wait for it later in the frame.
    //     jobHandle.Complete();

    //     Mesh mesh = new Mesh();
    //     mesh.vertices = vect.ToArray();
    //     mesh.uv = uvss.ToArray();
    //     mesh.triangles = triangles.ToArray();
    //     mesh.RecalculateNormals();

    //     MeshFilter mf = GetComponent<MeshFilter>();
    //     mf.mesh = mesh;

    //     vect.Dispose();
    //     uvss.Dispose();
    //     triangles.Dispose();
    //     scls.Dispose();
    //     poss.Dispose();
    // }

    // void Update()
    // {
    //     // var velocity = new NativeArray<Mesh>(500, Allocator.Persistent);

    //     // var job = new MeshJob()
    //     // {
    //     //     deltaTime = Time.deltaTime,
    //     //     velocity = velocity
    //     // };

    //     // JobHandle jobHandle = job.Schedule();

    //     // // Ensure the job has completed
    //     // // It is not recommended to Complete a job immediately,
    //     // // since that gives you no actual parallelism.
    //     // // You optimally want to schedule a job early in a frame and then wait for it later in the frame.
    //     // jobHandle.Complete();

    //     // Debug.Log(velocity[0]);
    //     // // Native arrays must be disposed manually
    //     // velocity.Dispose();
    // }

    // Mesh GenerateMesh(List<MeshFilter> list, int n)
    // {
    //     int quadN = 4*n-1;
    //     Vector3[] vect = new Vector3[quadN*4];
    //     Vector2[] uvs = new Vector2[quadN*4];
    //     int[] tris = { 0, 1, 2, 0, 2, 3 };
    //     int count = 0;
    //     int trisCount = 0;
    //     int[] triangles = new int[quadN*6];
    //     for(int i =0; i< n - 1;)
    //     {
    //         if(i+1 == n)
    //             break;

    //         float radius = list[i].transform.localScale.x/2;
    //         float radius2 = list[i+1].transform.localScale.x/2;
    //         float upY = list[i].transform.position.y;
    //         float downY = list[i+1].transform.position.y;

    //         //front
    //         {
    //             uvs[count] = new Vector2(0,0);
    //             vect[count++] = new Vector3(-radius,upY,-radius);
    //             uvs[count] = new Vector2(0,1);
    //             vect[count++] = new Vector3(radius,upY,-radius);
    //         }
    //         {
    //             uvs[count] = new Vector2(1,1);
    //             vect[count++] = new Vector3(radius2,downY,-radius2);
    //             uvs[count] = new Vector2(1,0);
    //             vect[count++] = new Vector3(-radius2,downY,-radius2);
    //         }

    //         for(int j =0; j<6; j++)
    //         {
    //             triangles[trisCount++] = (count/4 - 1)*4 + tris[j];
    //         }

    //         //right 
    //         {
    //             uvs[count] = new Vector2(0,0);
    //             vect[count++] = new Vector3(radius,upY,-radius);
    //             uvs[count] = new Vector2(0,1);
    //             vect[count++] = new Vector3(radius,upY,radius);
    //         }
    //         {
    //             uvs[count] = new Vector2(1,1);
    //             vect[count++] = new Vector3(radius2,downY,radius2);
    //             uvs[count] = new Vector2(1,0);
    //             vect[count++] = new Vector3(radius2,downY,-radius2);
    //         }

    //         for(int j =0; j<6; j++)
    //         {
    //             triangles[trisCount++] = (count/4 - 1)*4 + tris[j];
    //         }

    //         //back
    //         {
    //             uvs[count] = new Vector2(0,0);
    //             vect[count++] = new Vector3(radius,upY,radius);
    //             uvs[count] = new Vector2(0,1);
    //             vect[count++] = new Vector3(-radius,upY,radius);
    //         }
    //         {
    //             uvs[count] = new Vector2(1,1);
    //             vect[count++] = new Vector3(-radius2,downY,radius2);
    //             uvs[count] = new Vector2(1,0);
    //             vect[count++] = new Vector3(radius2,downY,radius2);
    //         }

    //         for(int j =0; j<6; j++)
    //         {
    //             triangles[trisCount++] = (count/4 - 1)*4 + tris[j];
    //         }

    //         //left
    //         {
    //             uvs[count] = new Vector2(0,0);
    //             vect[count++] = new Vector3(-radius,upY,radius);
    //             uvs[count] = new Vector2(0,1);
    //             vect[count++] = new Vector3(-radius,upY,-radius);
    //         }
    //         {
    //             uvs[count] = new Vector2(1,1);
    //             vect[count++] = new Vector3(-radius2,downY,-radius2);
    //             uvs[count] = new Vector2(1,0);
    //             vect[count++] = new Vector3(-radius2,downY,radius2);
    //         }

    //         for(int j =0; j<6; j++)
    //         {
    //             triangles[trisCount++] = (count/4 - 1)*4 + tris[j];
    //         }
            
    //         i++;
    //     }

    //     Mesh mesh = new Mesh();
    //     mesh.vertices = vect;
    //     mesh.uv = uvs;
    //     mesh.triangles = triangles;
    //     mesh.RecalculateNormals();

    //     return mesh;
    // }

    
}
