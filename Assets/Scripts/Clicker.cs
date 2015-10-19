using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace Assets.Scripts
{
  public  class Clicker : MonoBehaviour
  {

      private Dictionary<Rect, GameObject> _clickMap;
      public Dictionary<Rect, GameObject> ClickMap
      {
          get { return _clickMap; }
          set
          {
              _clickMap = value;
              foreach (var key in _clickMap.Keys)
              {
                  Debug.LogWarning(string.Format("({0}, {1}) w={2} h={3} --- {4}", key.x, key.y, key.width, key.height, _clickMap[key].name));
              }
          }
      }

      private GameObject FindOwner(Vector2 textureCoordinate)
      {
          var textureRectangle = ClickMap.Keys.FirstOrDefault(rect => rect.Contains(textureCoordinate));
          return ClickMap[textureRectangle];
      }


      public void OnMouseDown()
        {
            RaycastHit hit;
            if (!Physics.Raycast(Camera.main.ScreenPointToRay(Input.mousePosition), out hit))
                return;

            Renderer renderer = hit.collider.GetComponent<Renderer>();
       
   
//            Texture2D tex = (Texture2D)renderer.material.mainTexture;
            Vector2 pixelUV = hit.textureCoord;

          var owner = FindOwner(new Vector2((int) (pixelUV.x*renderer.material.mainTexture.width),
              (pixelUV.y*renderer.material.mainTexture.height)));

            Debug.Log(owner.name+" clicked");

//            print((int)(pixelUV.x * renderer.material.mainTexture.width) + "--" + (int)(pixelUV.y * renderer.material.mainTexture.height));

        }
    }
}
