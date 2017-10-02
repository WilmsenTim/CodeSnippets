using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class SelectionScript : MonoBehaviour {

    public Image _image;
    public Text _name;

    public Image _rechts, _links;
    public Text _rechtsText, _linksText;

    private GameObject[] _campussen, _studentenClubs;
    private GameObject[] _icons;

    int _campusLength, _studentenclubLength, _swipePicIndex;
    int _currentIndex = 0;

    

    // Use this for initialization
    void Start()
    {
        _swipePicIndex = 1;

        _icons = GameObject.FindGameObjectsWithTag("CampusPic");

        if (SceneManager.GetActiveScene().name == "CampusSelection")
        {
            _campussen = GameObject.FindGameObjectsWithTag("Campus");
            _campusLength = _campussen.Length;
        }
        else
        {
            _studentenClubs = GameObject.FindGameObjectsWithTag("Studentenclub");
            _studentenclubLength = _studentenClubs.Length;
        }

        //Debug.Log(_campusLength);

        CheckIcons(_swipePicIndex);

        if (_campussen != null)
        {
            _image.sprite = Resources.Load<Sprite>(_campussen[_currentIndex].name);
            _name.text = _campussen[_currentIndex].name;
        }
        else
        {
            _image.sprite = Resources.Load<Sprite>(_studentenClubs[_currentIndex].name);
            _name.text = _studentenClubs[_currentIndex].name;
        }
        

    }

    void Update() {

        if (Input.touchCount > 0 && Input.GetTouch(0).phase == TouchPhase.Stationary)
        {
            EnableSwipeText();
        }

        if (Input.touchCount > 0 && Input.GetTouch(0).phase == TouchPhase.Ended)
        {

            DisableSwipeText();

            Vector2 touchDelta = Input.GetTouch(0).deltaPosition;

            if (touchDelta.x > 2)
            {

                AcceptBtn();

            } //This was a flick to the left with magnitude of 5 or more
            if (touchDelta.x < -2)
            {

                DenyBtn();

            } //This was a flick to the right with magnitude of 5 or more
        }

    }

    void CheckIcons(int index)
    {

        for (int i = 0; i < _icons.Length; i++)
        {
            if (_icons[i].transform.name == "Pic" + index)
            {
                _icons[i].GetComponent<SpriteRenderer>().color = Color.red;

            }
            else
            {
                _icons[i].GetComponent<SpriteRenderer>().color = Color.white;
            }
        }

    }

    public void AcceptBtn() {

        //Debug.Log(_campussen[_currentIndex].name);

        if (_campussen != null)
        {
            PlayerPrefs.SetString("campus_name", _campussen[_currentIndex].name);
        }
        else
        {
            PlayerPrefs.SetString("studentenclub_name", _studentenClubs[_currentIndex].name);
        }
        

        StartGame();

    }

    public void DenyBtn()
    {
        _currentIndex++;

        //Debug.Log(_currentIndex);


        if (_currentIndex < _campusLength && _campussen != null)
        {
            _swipePicIndex++;

            _image.sprite = Resources.Load<Sprite>(_campussen[_currentIndex].name);
            _name.text = _campussen[_currentIndex].name;
        }
        else if (_currentIndex < _studentenclubLength && _campussen == null) {

            _swipePicIndex++;

            _image.sprite = Resources.Load<Sprite>(_studentenClubs[_currentIndex].name);
            _name.text = _studentenClubs[_currentIndex].name;
        }
        else
        {
            SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
        }

        CheckIcons(_swipePicIndex);
    }


    void StartGame()
    {

        SceneManager.LoadScene("LoadingScene");

    }

    void DisableSwipeText() {
        _rechts.gameObject.SetActive(false);
        _rechtsText.gameObject.SetActive(false);
        _links.gameObject.SetActive(false);
        _linksText.gameObject.SetActive(false);
    }

    void EnableSwipeText()
    {
        _rechts.gameObject.SetActive(true);
        _rechtsText.gameObject.SetActive(true);
        _links.gameObject.SetActive(true);
        _linksText.gameObject.SetActive(true);
    }


}
