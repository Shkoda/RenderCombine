using System.Collections.Generic;
using UnityEngine;

namespace Assets.Scripts
{
    public class Blender : MonoBehaviour
    {
        public int AtlasResolution = 256;

        private MaterialBuilder _materialBuilder;
        private ObjectFromMeshesBuilder _objectFromMeshesBuilder;


        private void Start()
        {
            _materialBuilder = new MaterialBuilder();
            _objectFromMeshesBuilder = new ObjectFromMeshesBuilder();
            GenerateSingleBlendedObject();
        }

        private void GenerateSingleBlendedObject()
        {     
            MeshFilter[] meshFilters = GetComponentsInChildren<MeshFilter>();

         

            //generate material, then mesh, because mesh builder uses 
            //textere offsets that are counted during material building
            Material material = _materialBuilder.MakeMaterial(meshFilters, AtlasResolution);

            var combinedObject = _objectFromMeshesBuilder.MakeObjectUnitedMeshesWithShiftedUv
                (meshFilters, 
                _materialBuilder.Scale, 
                _materialBuilder.TextureOffsets());

         
            combinedObject.GetComponent<Renderer>().material = material;

            combinedObject.AddComponent<Clicker>().ClickMap = _materialBuilder.ClickMap();

            //simple offset, nothing important
            combinedObject.transform.position += Vector3.right*5;
        } 
    }
}