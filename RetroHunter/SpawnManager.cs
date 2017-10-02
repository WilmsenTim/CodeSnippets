using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpawnManager : MonoBehaviour {

    public GameObject _bird;
    public float _spawnTime = 3f;
    //public byte birdLimit = 3;
    private GameObject _birdinscene;

    private int _ammoToGive;
    private int _ammo, _MaxAmmo;

    private GameObject[] _spawnPos;
    private GameObject _bullet;
    private int _rnd;

    // Use this for initialization
    void Start () {

        //reset ammo to refill
        _ammoToGive = 0;

        //find all spawns
        _spawnPos = GameObject.FindGameObjectsWithTag("Spawn");

        //call spawning
        InvokeRepeating("SpawnBird", _spawnTime, _spawnTime);

    }
	
	// Update is called once per frame
	void Update () {

        _birdinscene = GameObject.FindGameObjectWithTag("Bird");

        //get remaining ammo
        _ammo = GameObject.Find("Managers").GetComponent<ShootManager>().GetStartAmmo();

        //get max ammo
        _MaxAmmo = GameObject.Find("Managers").GetComponent<ShootManager>().GetMaxAmmo();

        //generate random number
        _rnd = Random.Range(0, 7);

    }

    void SpawnBird ()
    {
        //check if there is a bird in the scene
        if (_birdinscene == null)
        {
            //spawn copy bird
            Instantiate(_bird, _spawnPos[_rnd].transform.position + new Vector3(0,2f,0), Quaternion.identity);

            //check ammo to give to get max ammo
            _ammoToGive = _MaxAmmo - _ammo;

            //add ammo when bird spawns
            GameObject.Find("Managers").GetComponent<ShootManager>().AddRemainingAmmo(_ammoToGive);

        }


    }
}
