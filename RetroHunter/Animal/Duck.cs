using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Duck : Animal {

    public AudioClip _quack;
    public AudioClip _hit;
    public AudioClip _point;

    private void Start()
    {
        StartCoroutine(PlaySound());
    }

    private IEnumerator PlaySound()
    {
        yield return new WaitForSeconds(UnityEngine.Random.Range(1, 4));

        AudioSource.PlayClipAtPoint(_quack, gameObject.transform.position, 2f);
        StartCoroutine(PlaySound());
    }

    public Duck()
    {
        Name = "Duck";
        Speed = 2f;
        Mass = 2f;
        Health = 50;
        Points = 500;

    }

    void OnTriggerEnter(Collider other)
    {      

        if (other.gameObject.name == "Bullet(Clone)")
        {

            //play hit sound + point sound
            AudioSource.PlayClipAtPoint(_hit, gameObject.transform.position, 2f);
            AudioSource.PlayClipAtPoint(_point, GameObject.FindGameObjectWithTag("ScoreBoard").transform.position, 10f);

            //destroy gameobjects
            Destroy(gameObject);
            Destroy(other.gameObject);

            //call checkscorebirds
            GameObject.Find("Managers").GetComponent<ScoreManager>().CheckScoreBirds();

            //add points
            GameObject.Find("Managers").GetComponent<ScoreManager>().AddPoints();

        }

        

    }


}
