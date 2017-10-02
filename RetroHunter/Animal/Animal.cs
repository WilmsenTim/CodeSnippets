using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Animal : MonoBehaviour {

    private float _speed, _mass;
    private int _health, _points;
    private string _name;

    public string Name {

        get { return _name; }
        set { _name = value; }

    }

    public int Health
    {

        get { return _health; }
        set { _health = value; }

    }

    public int Points
    {

        get { return _points; }
        set { _points = value; }

    }

    public float Speed {

        get { return _speed; }
        set { _speed = value; }

    }

    public float Mass
    {

        get { return _mass; }
        set { _mass = value; }

    }

    public Animal()
    {
        Name = "Retro_Object";
        Speed = 2f;
        Mass = 2f;
        Health = 50;
    }

    public Animal(string initName, float initSpeed, float initMass, int initHealth, int initPoints)
    {
        Name = initName;
        Speed = initSpeed;
        Mass = initMass;
        Health = initHealth;
        Points = initPoints;
    }

}
