using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Bullet {

    private float _mass, _speed;
    private int _damage;
    private string _name;

    public string Name
    {

        get { return _name; }
        set { _name = value; }

    }

    public float Speed
    {

        get { return _speed; }
        set { _speed = value; }

    }

    public float Mass
    {

        get { return _mass; }
        set { _mass = value; }

    }

    public int Damage
    {

        get { return _damage; }
        set { _damage = value; }

    }

    public Bullet(string initName, float initSpeed, float initMass, int initDamage)
    {
        Name = initName;
        Speed = initSpeed;
        Mass = initMass;
        Damage = initDamage;
    }

    public Bullet()
    {
        Name = "Standard_Bullet";
        Speed = 60f;
        Mass = 1f;
        Damage = 50;
    }

}
