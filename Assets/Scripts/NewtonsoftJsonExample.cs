using System.Collections.Generic;
using UnityEngine;
using Newtonsoft.Json;

public class NewtonsoftJsonExample : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        JsonTestClass jTest1 = new JsonTestClass();
        string jsonData = JsonConvert.SerializeObject(jTest1);
        Debug.Log(jsonData);

        JsonTestClass jTest2 = JsonConvert.DeserializeObject<JsonTestClass>(jsonData);
        jTest2.Print();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public class JsonTestClass
    {
        public int i;
        public float f;
        public bool b;
        public string str;
        public int[] iArray;
        public List<int> iList = new List<int>();
        public Dictionary<string, float> fDictionary = new Dictionary<string, float>();
        public IntVector2 iVector;

        public JsonTestClass()
        {
            i = 10;
            f = 99.9f;
            b = true;
            str = "JSON Test String";
            iArray = new int[] { 1, 1, 2, 3, 5, 8, 12, 21, 34, 55 };

            for(int idx = 0; idx < 5; idx++)
            {
                iList.Add(2 * idx);
            }

            fDictionary.Add("PIE", Mathf.PI);
            fDictionary.Add("Epsilon", Mathf.Epsilon);
            fDictionary.Add("Sqrt(2)", Mathf.Sqrt(2));

            iVector = new IntVector2(3, 2);
        }

        public void Print()
        {
            Debug.Log("i = " + i);
            Debug.Log("f = " + f);
            Debug.Log("b = " + b);
            Debug.Log("str = " + str);

            for (int idx = 0; idx < iArray.Length; idx++)
            {
                Debug.Log(string.Format("iArray[{0}] = {1}", idx, iArray[idx]));
            }

            for (int idx = 0; idx < iList.Count; idx++)
            {
                Debug.Log(string.Format("iList[{0}] = {1}", idx, iList[idx]));
            }

            foreach (var data in fDictionary)
            {
                Debug.Log(string.Format("iDictionary[{0}] = {1}", data.Key, data.Value));
            }

            Debug.Log("iVector = " + iVector.x + ", " + iVector.y);
        }

        public class IntVector2
        {
            public int x;
            public int y;

            public IntVector2(int x, int y)
            {
                this.x = x;
                this.y = y;
            }
        }
    }
}
