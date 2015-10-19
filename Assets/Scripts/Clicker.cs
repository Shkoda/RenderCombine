using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace Assets.Scripts
{
    public class Clicker : MonoBehaviour
    {
        private int _textureHeight;
        private int _textureWidth;
        public Dictionary<Rect, GameObject> ClickMap { get; set; }

        private void Start()
        {
            var mainTexture = gameObject.GetComponent<Renderer>().material.mainTexture;
            _textureWidth = mainTexture.width;
            _textureHeight = mainTexture.height;
        }

        private GameObject FindOwner(Vector2 textureCoordinate)
        {
            var textureRectangle = ClickMap.Keys.FirstOrDefault(rect => rect.Contains(textureCoordinate));
            return ClickMap[textureRectangle];
        }


        public void OnMouseDown()
        {
            RaycastHit hit;
            Physics.Raycast(Camera.main.ScreenPointToRay(Input.mousePosition), out hit);             

            Vector2 pixelUV = hit.textureCoord;

            var point = new Vector2((int) (pixelUV.x*_textureWidth),
                (pixelUV.y*_textureHeight));

            var owner = FindOwner(point);

            Debug.Log(string.Format("{0} clicked on ({1}  {2})", owner.name, point.x, point.y));
        }
    }
}