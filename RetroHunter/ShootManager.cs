using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ShootManager : MonoBehaviour {

    public Text _ammoText;

    public bool _pauseActive = false;

    public GameObject _projectile, _bulletSpawn;

    //variable ammo
    public int _maxAmmo = 3;
    public int _startAmmo = 3;

    //prefabs
    GameObject _theBullet, _birdInScene, _BulletInScene;

    //obj
    Bullet _huntingBullet;

    //sound
    public AudioClip _shootSound;
    public AudioClip _gameOver;

    //canvas gameover
    public Canvas deathScreenCanvas;

    void Start () {

        //bullet object
        _huntingBullet = new Bullet();

    }
	
	void Update () {

        _birdInScene = GameObject.FindGameObjectWithTag("Bird");
        _BulletInScene = GameObject.FindGameObjectWithTag("Bullet");

        //ammo check
        if (_startAmmo > 0)
        {
            if (Time.timeScale != 0)
            {
                //fire
                Fire();
            }   

            //change ammo text
            _ammoText.text = _startAmmo.ToString();
               
        }

        //if there no bullets
        //and a bird in scene

        else if (_BulletInScene == null && _birdInScene != null)
        {


            Invoke("CheckEndConditions", 2f);

        }
        

    }

    void Fire()
    {

        for (int i = 0; i < Input.touchCount; ++i)
        {
            if (Input.GetTouch(i).phase == TouchPhase.Ended && _pauseActive == false)
            {
                //declare camera
                Camera cam = Camera.main;

                //Instantiate bullet

                //cam.transform.position
                _theBullet = (GameObject)Instantiate(_projectile, _bulletSpawn.transform.position, _bulletSpawn.transform.rotation);

                //Play gun sound
                AudioSource.PlayClipAtPoint(_shootSound, GameObject.FindGameObjectWithTag("Gun").transform.position);

                //add physics
                _theBullet.GetComponent<Rigidbody>().velocity = _bulletSpawn.transform.forward * _huntingBullet.Speed;
                _theBullet.GetComponent<Rigidbody>().mass = _huntingBullet.Mass;

                //remove 1 bullet
                _startAmmo--;
            }
        }

    }

    public void CheckEndConditions()
    {

        GameObject.Find("Managers").GetComponent<ScoreManager>().CheckHighscore();

        //open game over screen
        GameObject.Find("Managers").GetComponent<MenuManager>().GameOver();


        //Debug.Log("test");

        AudioSource.PlayClipAtPoint(_gameOver, Camera.main.transform.position, 0.3f);

    }

    public int GetStartAmmo() {

        return _startAmmo;

    }

    public int GetMaxAmmo()
    {

        return _maxAmmo;

    }

    public void AddRemainingAmmo(int ammo) {

        _startAmmo += ammo;

    }

}
