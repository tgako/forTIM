using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System;
using UnityEngine.Rendering.PostProcessing;

public class SerialCube : MonoBehaviour
{
    public SerialHandler serialHandler;
    public GameObject maincam;
    public GameObject orient;
    public double Latitude;
    public double Longitude;
    public int getyear, getmonth, getday, gethour, getminute, getsecond;

    bool first;
    bool setnorth;
    public float ax, ay, az;
    public float gx, gy, gz;
    public float mx, my, mz;
    public float roll, yaw;
    float yaw_row;
    float preroll, preyaw;
    Vector3 angle;
    public float north_angle;

    [SerializeField] private PostProcessVolume volume;
    public Camera cam;

    void Start()
    {
        serialHandler.OnDataReceived += OnDataReceived;
        // 北のオフセットを取得
        north_angle = PlayerPrefs.GetFloat("north");
        first = true;
        setnorth = false;
    }

    void FixedUpdate()
    {
        maincam.transform.position = orient.transform.position;
    }

    void OnDataReceived(string message)
    {
        try
        {            
            string[] angles = message.Split(',');
            if (float.Parse(angles[0]) == 1)
            {
                gx = float.Parse(angles[1]);
                gy = float.Parse(angles[2]);
                gz = float.Parse(angles[3]);
                ax = float.Parse(angles[4]);
                ay = float.Parse(angles[5]);
                az = float.Parse(angles[6]);
                mx = float.Parse(angles[7]);
                my = float.Parse(angles[8]);
                mz = float.Parse(angles[9]);
                roll = float.Parse(angles[10]);
                yaw_row = float.Parse(angles[11]);
                yaw = yaw_row - north_angle;

                // 地球の表面にオフセット
                roll -= orient.transform.eulerAngles.x;
                yaw -= orient.transform.eulerAngles.y;
                if (first)
                {
                    preroll = roll;
                    preyaw = yaw;                    
                    first = false;
                    maincam.transform.rotation = Quaternion.Euler(-roll, -yaw, 0);
                }          
                if (!setnorth)
                {
                    if (Math.Abs(gz) < 2f)
                        yaw = preyaw;
                    if (Math.Abs(gx) < 2f)
                        roll = preroll;
                }
                angle = new Vector3(-roll, -yaw, 0);

                maincam.transform.rotation = Quaternion.Slerp(maincam.transform.rotation,
                      Quaternion.Euler(angle), 0.1f);
                preroll = roll;
                preyaw = yaw;
                setnorth = false;

                //FocusAdjust(int.Parse(angles[12]), int.Parse(angles[13]));
            }
            else if (float.Parse(angles[0]) == 0)
            {
                Latitude = float.Parse(angles[7]);
                Longitude = float.Parse(angles[8]);
                getyear = int.Parse(angles[1]);
                getmonth = int.Parse(angles[2]);
                getday = int.Parse(angles[3]);
                gethour = int.Parse(angles[4]);
                getminute = int.Parse(angles[5]);
                getsecond = int.Parse(angles[6]);
            }
            else
            {
                Debug.Log(message);
            }
            
		} catch (System.Exception e) {
            Debug.LogWarning(e.Message);
		}
    }

    public void SetNorth()
    {
        setnorth = true;
        north_angle = yaw_row;
        PlayerPrefs.SetFloat("north", north_angle);        
    }
    
    void FocusAdjust(int resist,int ver_resist)
    {
        // 可変抵抗でピント調整
        volume.sharedProfile.GetSetting<DepthOfField>().focusDistance.value
            = ver_resist;
        // 抵抗で倍率変更
        if (resist < 10)
        {
            //text.text = "未接続";
            cam.fieldOfView = 150;
        }
        else if (resist > 500 && resist < 520)
        {
            //text.text = "抵抗１kΩ";
            cam.fieldOfView = 0.2f;
        }
        else if (resist > 330 && resist < 350)
        {
            //text.text = "抵抗２kΩ";
            cam.fieldOfView = 1;
        }
        //else text.text = "エラー";
    }
}
