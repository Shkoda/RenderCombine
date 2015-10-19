using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace Assets.Scripts
{
   public class AtlasBuilder
   {
       public Dictionary<Rect, GameObject> ClickMap { get; private set; }
        public Dictionary<int, Vector2> TextureOffsets { get; private set; }

        public Texture2D MakeTexture(MeshFilter[] meshFilters, int atlasResolution, int tilesPerAtlasSide,
        int tileDimension)
        {
            ClickMap = new Dictionary<Rect, GameObject>();
            TextureOffsets = new Dictionary<int, Vector2>();

            var atlas = new Texture2D(atlasResolution, atlasResolution);
            for (var i = 0; i < meshFilters.Length; i++)
            {
                var texture = meshFilters[i].GetComponent<Renderer>().material.mainTexture as Texture2D;

                int tileX = i % tilesPerAtlasSide;
                int tileY = i / tilesPerAtlasSide;

                for (int x = tileX * tileDimension; x < (tileX + 1) * tileDimension; x++)
                {
                    for (int y = tileY * tileDimension; y < (tileY + 1) * tileDimension; y++)
                    {
                        atlas.SetPixel(x, y, texture.GetPixelBilinear(x / (float)tileDimension, y / (float)tileDimension));
                    }
                }

                Rect rect = new Rect(tileX * tileDimension, tileY * tileDimension, tileDimension, tileDimension);
                ClickMap.Add(rect, meshFilters[i].gameObject);

                var instanceId = meshFilters[i].gameObject.GetInstanceID();
                var offset = new Vector2(tileX / (float)tilesPerAtlasSide, tileY / (float)tilesPerAtlasSide);
                TextureOffsets.Add(instanceId, offset);
            }

            atlas.Apply();
            return atlas;
        }
    }
}
