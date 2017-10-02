using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PostServerData : MonoBehaviour {

    private string url;
    private int userID;

    // Use this for initialization
    void Start () {
        userID = PlayerPrefs.GetInt("userID");
        url = "http://jesseopdebeeck.multimediatechnology.be/api/user/"+userID+"/score";
        InvokeRepeating("InvokedPostData", 5f, 120f);
    }
	
	// Update is called once per frame
	public void InvokedPostData () {

        if (Application.internetReachability != NetworkReachability.NotReachable && PlayerPrefs.GetString("userEmail", "") != "") //Als er connectie is
        {
            Debug.Log("posting data to server");
            StartCoroutine(postData());
        }
	}

    IEnumerator postData()
    {
        int totalBeersDrunk = PlayerPrefs.GetInt("totalBeersDrunk");
        int totalHoursStudied = PlayerPrefs.GetInt("totalHoursStudied");
        int totalExamsFailed = PlayerPrefs.GetInt("totalExamsFailed");
        int totalExamsPassed = PlayerPrefs.GetInt("totalExamsPassed");
        int totalMoneyCollected = PlayerPrefs.GetInt("totalMoneyCollected");
        int totalMoneySpent = PlayerPrefs.GetInt("totalMoneySpent");
        int totalTimeSported = PlayerPrefs.GetInt("totalTimeSported");
        int totalTimeCulture = PlayerPrefs.GetInt("totalTimeCulture");
        int totalTimeParty = PlayerPrefs.GetInt("totalTimeParty");
        int totalTimeComa = PlayerPrefs.GetInt("totalTimeComa");
        int totalTimeBlackOut = PlayerPrefs.GetInt("totalTimeBlackOut");

        Dictionary<string, string> headers = new Dictionary<string, string>();
        headers.Add("Accept", "application/json");
        headers.Add("Content-Type", "application/json");
        headers.Add("Authorization", "Bearer " + PlayerPrefs.GetString("accesToken"));
        string jsonString = "{\"totalBeersDrunk\": \"" + totalBeersDrunk +"\",\"totalHoursStudied\": \"" + totalHoursStudied + "\",\"totalExamsFailed\": \""+totalExamsFailed+"\",\"totalExamsPassed\": \""+totalExamsPassed+"\",\"totalMoneyCollected\": \""+totalMoneyCollected+"\",\"totalMoneySpent\": \""+totalMoneySpent+"\",\"totalTimeSported\": \""+totalTimeSported+"\",\"totalTimeCulture\": \""+totalTimeCulture+"\",\"totalTimeParty\": \""+totalTimeParty+"\",\"totalTimeComa\": \""+totalTimeComa+"\",\"totalTimeBlackout\": \""+totalTimeBlackOut+"\"}";
        byte[] data = System.Text.Encoding.ASCII.GetBytes(jsonString);
        WWW www = new WWW(url, data, headers);
        Debug.Log(www.url.ToString());
        yield return www;
        Debug.Log("data posting returned: \n" + www.text);
    }

    void OnApplicationQuit()
    {
        InvokedPostData();
    }
}
