using System.Collections.Generic;
using UnityEngine;

namespace Assets.Scripts
{
    public class Blender : MonoBehaviour
    {
        private const int AtlasResolution = 256;

        public void Start()
        {
            Combine();
        }

        private void Combine()
        {
            var combinedObject = BuildBaseEmptyObject();
            MeshFilter[] meshFilters = GetComponentsInChildren<MeshFilter>();


            int uniqueTexturesCount = CountMeshesWithUniqueTextures(meshFilters);
            int tileCount = Mathf.CeilToInt(Mathf.Sqrt(uniqueTexturesCount));
            var tilePixels = AtlasResolution / tileCount;
            var scale = 1.0f / tileCount;

            var backupUv = BackupUV(meshFilters);
            UpdateUVInMeshes(meshFilters, scale, tileCount);
            CombineMeshesIntoObject(meshFilters, combinedObject);
            RestoreFromBackup(meshFilters, backupUv);

            Material material = MakeMaterial(meshFilters, AtlasResolution, tileCount, tilePixels);
            combinedObject.GetComponent<Renderer>().material = material;

            combinedObject.transform.position += Vector3.right * 3;
            combinedObject.AddComponent<MeshCollider>();
            combinedObject.AddComponent<Clicker>().ClickMap = MapObjects(meshFilters, tileCount, tilePixels);
        }

   



        private static Texture2D MakeTexture(MeshFilter[] meshFilters, int atlasResolution, int tileCount,
            int tilePixels)
        {
            var atlas = new Texture2D(atlasResolution, atlasResolution);
            int tileX = 0, tileY = 0;

            // filling the atlas with pixels (bilinear filtered)
            foreach (var meshFilter in meshFilters)
            {
                var texture = meshFilter.GetComponent<Renderer>().material.mainTexture as Texture2D;
               
                for (int x = tileX * tilePixels; x < tileX * tilePixels + tilePixels; x++)
                {
                    for (int y = tileY * tilePixels; y < tileY * tilePixels + tilePixels; y++)
                    {
                        atlas.SetPixel(x, y, texture.GetPixelBilinear(x / (float)tilePixels, y / (float)tilePixels));
                    }
                }

                tileX += 1;
                if (tileX >= tileCount)
                {
                    tileX = 0;
                    tileY += 1;
                }
            }
            atlas.Apply();
            return atlas;
        }

        private static Dictionary<Rect, GameObject> MapObjects(MeshFilter[] meshFilters, int tileCount, int tilePixels)
        {
            int tileX = 0, tileY = 0;
            Dictionary<Rect, GameObject> map = new Dictionary<Rect, GameObject>();
            foreach (var meshFilter in meshFilters)
            {

                Rect rect = new Rect(tileX * tilePixels, tileY * tilePixels, tilePixels, tilePixels);
                map.Add(rect, meshFilter.gameObject);
                tileX += 1;
                if (tileX >= tileCount)
                {
                    tileX = 0;
                    tileY += 1;
                }
            }
            return map;
        }


        /// <returns>key - textureID, value - offset Vector2D on atlas</returns>
        private static Dictionary<int, Vector2> DefineTexturesOffset(MeshFilter[] meshFilters, int tileCount)
        {
            var offsets = new Dictionary<int, Vector2>();
            int tileX = 0, tileY = 0;

            foreach (var meshFilter in meshFilters)
            {
                var texture = meshFilter.GetComponent<Renderer>().material.mainTexture as Texture2D;
                var textureId = texture.GetInstanceID();

                var offset = new Vector2(tileX / (float)tileCount, tileY / (float)tileCount);
                offsets[textureId] = offset;

                tileX += 1;
                if (tileX >= tileCount)
                {
                    tileX = 0;
                    tileY += 1;
                }
            }
            return offsets;
        }

        private static Material MakeMaterial(MeshFilter[] meshFilters, int atlasResolution, int tileCount, int tilePixels)
        {
            Material resultMaterial = new Material(Shader.Find("Standard"));
            resultMaterial.name = "Generated atlas Material";
            resultMaterial.mainTexture = MakeTexture(meshFilters, atlasResolution, tileCount, tilePixels);
            return resultMaterial;
        }

        private static void UpdateUVInMeshes(MeshFilter[] meshFilters, float scale, int tileCount)
        {
            var offsets = DefineTexturesOffset(meshFilters, tileCount);
            foreach (var meshFilter in meshFilters)
            {
                var newMesh = meshFilter.mesh;
                var newUVs = newMesh.uv;

                if (meshFilter.GetComponent<Renderer>().material.mainTexture != null)
                {
                    var tid = meshFilter.GetComponent<Renderer>().material.mainTexture.GetInstanceID();
                    Debug.Log("Searching for " + tid);
                    var offset = offsets[tid];

                    for (int n = 0; n < newUVs.Length; n++)
                    {
                        newUVs[n] *= scale;
                        newUVs[n] += offset;
                    }

                    meshFilter.mesh.uv = newUVs;
                }
            }
        }


        private static GameObject BuildBaseEmptyObject()
        {
            var baseObject = new GameObject("Custom Combined");
            baseObject.AddComponent<MeshRenderer>();
            baseObject.AddComponent<MeshFilter>().mesh = new Mesh();
            return baseObject;
        }




        private void CombineMeshesIntoObject(MeshFilter[] meshFilters, GameObject target)
        {
            CombineInstance[] combine = new CombineInstance[meshFilters.Length];

            int i = 0;
            while (i < meshFilters.Length)
            {
                combine[i].mesh = meshFilters[i].sharedMesh;
                combine[i].transform = meshFilters[i].transform.localToWorldMatrix;
                i++;
            }

            target.transform.GetComponent<MeshFilter>().mesh.CombineMeshes(combine);
        }



        private static int CountMeshesWithUniqueTextures(MeshFilter[] meshFilters)
        {
            var meshFiltersUniqueCounter = 0;
            var textureIDs = new HashSet<int>();
            foreach (var meshFilter in meshFilters)
            {
                var tid = meshFilter.GetComponent<Renderer>().material.mainTexture.GetInstanceID();
                if (!textureIDs.Contains(tid))
                {
                    textureIDs.Add(tid);
                    meshFiltersUniqueCounter++;
                }
            }
            return meshFiltersUniqueCounter;
        }

        private Vector2[][] BackupUV(MeshFilter[] meshFilters)
        {
            Vector2[][] backup = new Vector2[meshFilters.Length][];
            for (int i = 0; i < meshFilters.Length; i++)
            {
                var meshFilter = meshFilters[i];
                backup[i] = new Vector2[meshFilter.mesh.uv.Length];
                System.Array.Copy(meshFilter.mesh.uv, backup[i], meshFilter.mesh.uv.Length);
            }
            return backup;
        }

        private void RestoreFromBackup(MeshFilter[] meshFilters, Vector2[][] backupUVs)
        {
            for (int i = 0; i < meshFilters.Length; i++)
            {
                meshFilters[i].mesh.uv = backupUVs[i];
            }
        }
    }
}