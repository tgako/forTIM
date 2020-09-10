using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Guidecone : MonoBehaviour
{
    private GameObject target;
    private GameObject obj;
    public GameObject maincam;
    public GameObject up, down, left, right;
    //public GameObject scope;
    public Dropdown dropdown;
    
    float gaisekiLR;
    float deltaUD;
    bool right_flag = false;
    bool left_flag = false;
    bool up_flag = false;
    bool down_flag = false;

    float scopesize;
    void Start()
    {
        obj = this.gameObject;

    }
    void FixedUpdate()
    {
        obj.transform.position = maincam.transform.position;
    }

    void Update()
    {
        if (target != null)
        {
            transform.LookAt(target.transform);        
            // 左右の判定
            gaisekiLR = maincam.transform.forward.x * obj.transform.forward.z - maincam.transform.forward.z * obj.transform.forward.x;
            if (gaisekiLR > 0.001f)
            {
                right_flag = true;
                left_flag = false;
            }
            else if (gaisekiLR < -0.001f)
            {
                right_flag = false;
                left_flag = true;
            }
            else
            {
                right_flag = false;
                left_flag = false;
            }
            // 上下の判定
            deltaUD = obj.transform.forward.y - maincam.transform.forward.y;
            if(deltaUD > 0.001f)
            {
                up_flag = true;
                down_flag = false;
            }
            else if(deltaUD < -0.0001f)
            {
                up_flag = false;
                down_flag = true;
            }
            else
            {
                up_flag = false;
                down_flag = false;
            }

            if (right_flag)
            {
                right.SetActive(true);
                left.SetActive(false);
            }
            else if (left_flag)
            {
                right.SetActive(false);
                left.SetActive(true);
            }
            else
            {
                right.SetActive(false);
                left.SetActive(false);
            }
            if(up_flag)
            {
                down.SetActive(false);
                up.SetActive(true);
            }
            else if(down_flag)
            {
                down.SetActive(true);
                up.SetActive(false);
            }
            else
            {
                down.SetActive(false);
                up.SetActive(false);
            }
        }
        else
        {
            up.SetActive(false);
            down.SetActive(false);
            right.SetActive(false);
            left.SetActive(false);
        }
    }

    public void ChangeStar()
    {
        if (dropdown.value == 0)
        {
            target = null;
        }
        else if (dropdown.value == 1)
        {
            target = GameObject.Find("Moon");
        }
        else if (dropdown.value == 2)
        {
            target = GameObject.Find("Mercury");
        }
        else if (dropdown.value == 3)
        {
            target = GameObject.Find("Venus");
        }
        else if (dropdown.value == 4)
        {
            target = GameObject.Find("Mars");
        }
        else if (dropdown.value == 5)
        {
            target = GameObject.Find("Jupiter");
        }
        else if (dropdown.value == 6)
        {
            target = GameObject.Find("Saturn");
        }
    }
}
