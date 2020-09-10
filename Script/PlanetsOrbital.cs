using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class PlanetsOrbital : MonoBehaviour
{
    SetTime settime;    
    EarthOrbital earth;
    SunOrbital sun;

    public double Xq, Yq, Zq;

    string planetname;
    double au;
    double a, t0, e, omega, Omega, i, T; //軌道要素
    double n, M, jday, E, Stime, epsilon; //計算値
    double xq, yq, zq;    
    double alpha, delta, R; //赤経、赤緯、地心距離
    double Kaiten; //計算機用の変数
    double xl, ym, zn; //地平直行座標

    void Start () {
        // 軌道要素 //
        Dictionary<string, double> a_dictionary = new Dictionary<string, double>(){
            {"Mercury", 0.38710}, {"Venus", 0.72333}, {"Mars", 1.52368}, {"Jupiter", 5.20260}, {"Saturn", 9.55491}
        };//軌道長半径[au]
        Dictionary<string, double> t0_dictionary = new Dictionary<string, double>(){
            {"Mercury", 2455284.96736}, {"Venus", 2454884.30555556}, {"Mars", 2454942.90694}, {"Jupiter", 2455638.20833333}, {"Saturn", 2463566.125}
        };//近日点通過時刻[day](土星だけ調べられてない)
        Dictionary<string, double> e_dictionary = new Dictionary<string, double>(){
            {"Mercury", 0.20564}, {"Venus", 0.00676}, {"Mars", 0.09342}, {"Jupiter", 0.04853}, {"Saturn", 0.05549}
        };//離心率
        Dictionary<string, double> omega_dictionary = new Dictionary<string, double>(){
            {"Mercury", 77.7518}, {"Venus", 131.8301}, {"Mars", 336.4100}, {"Jupiter", 14.6376}, {"Saturn", 93.4304}
        };//近日点黄経[deg]
        Dictionary<string, double> Omega_dictionary = new Dictionary<string, double>(){
            {"Mercury", 48.5563}, {"Venus", 76.8511}, {"Mars", 49.7048}, {"Jupiter", 100.6584}, {"Saturn", 113.8321}
        };//昇交点黄経[deg]
        Dictionary<string, double> i_dictionary = new Dictionary<string, double>(){
            {"Mercury", 7.0053}, {"Venus", 3.3949}, {"Mars", 1.8496}, {"Jupiter", 1.3022}, {"Saturn", 2.4882}
        };//軌道傾斜角[deg]
        Dictionary<string, double> T_dictionary = new Dictionary<string, double>(){
            {"Mercury", 0.240852}, {"Venus", 0.615207}, {"Mars", 1.880866}, {"Jupiter", 11.86155}, {"Saturn", 29.53216}
        };//公転周期[year]

        planetname = transform.name;

        a = a_dictionary[planetname]; //惑星の軌道長半径[au]
        t0 = t0_dictionary[planetname]; //惑星の近日点通過時刻[day]
        e = e_dictionary[transform.name]; //惑星の離心率[-]
        omega = (Math.PI / 180.0) * (omega_dictionary[planetname] - Omega_dictionary[planetname]); //惑星の近日点引数[rad]
        Omega = (Math.PI / 180.0) * Omega_dictionary[planetname]; //惑星の昇交点経度[rad]
        i = (Math.PI / 180.0) * i_dictionary[planetname]; //惑星の軌道傾斜角[rad]
        T = 365.25 * T_dictionary[planetname]; //惑星の公転周期[day]
        n = 2 * Math.PI / T; //[/day]

        settime = GameObject.Find("TimeManager").GetComponent<SetTime>();
        earth = GameObject.Find("EarthOrbit").GetComponent<EarthOrbital>();
        sun = GameObject.Find("Sun").GetComponent<SunOrbital>();
    }

    void FixedUpdate () {
        jday = settime.jday;
        double C = settime.C;
        epsilon = (23.452294 - 0.0130125 * C - 0.00000164 * C * C + 0.000000503 * C * C * C) * Math.PI / 180;

        au = 0.005f * sun.au; //地球の半径をscaleとした時の天文単位
        /** 月と太陽の大きさ・位置関係を変えない（満ち欠け）
         * 他の天体が遠すぎて表示できないから、近づけた。
         * でも大きさは変えてない 20200813 **/

        GetGeocentric();
        Xq *= au;
        Yq *= au;
        Zq *= au;
        
        Transform orbitTransform = this.transform;
        Vector3 worldPos = orbitTransform.position;
        worldPos.x = (float)(au * Xq);
        worldPos.y = (float)(au * Zq);
        worldPos.z = (float)(au * Yq);

        orbitTransform.position = worldPos;
        //getEquator();
    }

    // 赤経赤緯
    public void GetEquator()
    {
        //getGeocentric(); //赤道座標のx,y,z
        ////赤経
        //alpha = Math.Atan2(Yq, Xq); //-π<a<π
        //if (alpha < 0) alpha += 2 * Math.PI;

        ////赤緯
        //R = Math.Sqrt(Xq * Xq + Yq * Yq + Zq * Zq); //地心距離
        //delta = Math.Asin(Zq / R);
        ////Debug.Log("2019年5月7日0時\n赤経:" + (alpha * 180 / Math.PI) / 15.0 + "[h],赤緯:" + delta * 180 / Math.PI + "[°],地心距離:" + R + "[au]");
    }

    //地心座標//
    public void GetGeocentric()
    {
        GetHeliocentric();
        double xq_earth = earth.xq;
        double yq_earth = earth.yq;
        double zq_earth = earth.zq;

        Xq = xq - xq_earth;
        Yq = yq - yq_earth;
        Zq = zq - zq_earth;
    }

    // 日心座標 //
    public void GetHeliocentric()
    {
        GetE();
        KaitenHosei(i);
        i = Kaiten;

        // ベクトル定数
        double Px = Math.Cos(omega) * Math.Cos(Omega) - Math.Sin(omega) * Math.Sin(Omega) * Math.Cos(i);
        double Py = Math.Cos(omega) * Math.Sin(Omega) + Math.Sin(omega) * Math.Cos(Omega) * Math.Cos(i);
        double Pz = Math.Sin(i) * Math.Sin(omega);
        double Qx = Math.Sin(omega) * Math.Cos(Omega) + Math.Cos(omega) * Math.Sin(Omega) * Math.Cos(i);
        double Qy = Math.Sin(omega) * Math.Sin(Omega) - Math.Cos(omega) * Math.Cos(Omega) * Math.Cos(i);
        double Qz = Math.Sin(i) * Math.Cos(omega);

        //軌道楕円上の座標
        double x = a * (Math.Cos(E) - e);
        double y = a * Math.Sin(E) * Math.Sqrt(1 - e * e);
        //z = 0;

        // 日心黄道座標                
        double xc = Px * x - Qx * y;
        double yc = Py * x - Qy * y;
        double zc = Pz * x + Qz * y;
        // 日心赤道座標
        xq = xc;
        yq = yc * Math.Cos(epsilon) - zc * Math.Sin(epsilon);
        zq = yc * Math.Sin(epsilon) + zc * Math.Cos(epsilon);
    }

    // ケプラーの方程式を解く(離心近点角) //
    public double GetE() {
        GetM() ;
        double E0 ;
        double delta_E;

        E0 = M ;
        do {
            delta_E = (M - E0 + e * Math.Sin(E0)) / (1 - e * Math.Cos(E0)) ;
            E = E0 + delta_E ;
            E0 = E ;
        }
        while(Math.Abs(delta_E) > 0.00001) ;
        return (E); //[rad]
    }

    // 平均近点角 //
    public double GetM()
    {
        M = n * (jday - t0) ; //角度[rad]
        KaitenHosei(M);
        M = Kaiten;
        return (M);
    } 

    public double KaitenHosei(double theta)
    {
        double kaiten = Math.Abs(Math.Floor(theta / (2 * Math.PI)));
        if (theta > 0)
        {
            theta -= 2 * Math.PI * kaiten;
        }
        if (theta < 0)
        {
            theta += 2 * Math.PI * kaiten;
        }
        return (Kaiten = theta);
    }
}