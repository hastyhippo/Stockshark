namespace Chess
{
    using System.Collections;
    using System.Collections.Generic;
    using UnityEngine;
    public class UI : MonoBehaviour
    {
        //RAINBOW! 🌈🌈🌈
        public GameObject square1; // rainbow
        public float Speed = 1;
        private Renderer rend;

        //normal\\ 😒😒😒
        public GameObject square;
        public GameObject square2;
        public GameObject[] coloursquares = new GameObject[8];

        public bool rainbow;
        private void Awake()
        {
            CreateBoard();
        }

        public void Update(){

        }

        private void CreateBoard()
        {
            var GO = GameObject.FindGameObjectsWithTag("Square");
            for (int i = 0; i < GO.Length; i++)
            {
                Destroy(GO[i]);
            }
            for (int y = 0; y < 8; y++)
            {
                for (int x = 0; x < 8; x++)
                {
                    if ((x % 2 == 0 && y % 2 == 0) || (x % 2 == 1 && y % 2 == 1))
                    {
                        if (rainbow)
                        {
                            Instantiate(square1, new Vector2(x * 2, y * 2 + 1), Quaternion.identity);
                        }
                        else
                        {
                            Instantiate(square, new Vector2(x * 2, y * 2 + 1), Quaternion.identity);
                        }
                    }
                    else
                    {
                        if (rainbow)
                        {
                            Instantiate(coloursquares[y], new Vector2(x * 2, y * 2 + 1), Quaternion.identity);
                        }
                        else
                        {
                            Instantiate(square2, new Vector2(x * 2, y * 2 + 1), Quaternion.identity);
                        }
                    }
                }
            }
        }
    }

}