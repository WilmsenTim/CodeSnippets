using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MoneyPlace
{
    public string name; //Name of the special place
    public int moneyToGive; //amount of money to receive for visiting this place
    public string infoText; //Text to display in pop up
    public float latitudePos; //Latitude of the special place
    public float longitudePos; //Longitude of the special place

    public DateTime timeOfUnlock; //Time when the claim should be available again

    public MoneyPlace(string n, int money, float lat, float lon) //Constructor
    {
        name = n;
        moneyToGive = money;
        infoText = "Je bent in de buurt van " + name + "! Hier heb je " + moneyToGive + " euro.";
        latitudePos = lat;
        longitudePos = lon;
        if (PlayerPrefs.HasKey(n.ToString() + "MoneyUnlockTime")) //check if the variable is already saved
        {
            timeOfUnlock = Convert.ToDateTime(PlayerPrefs.GetString(n.ToString() + "MoneyUnlockTime")); //If yes, get value
        }
        else
        {
            PlayerPrefs.SetString(n.ToString() + "MoneyUnlockTime", DateTime.Now.ToString()); //Else make the variable in playerprefs
            timeOfUnlock = DateTime.Now;
        }
    }
}

public class LocationCheck : MonoBehaviour
{
    AudioSource audioS;

    public AudioClip moneyPickUp;

    public GameObject claimPopUp;
    public Text claimPopUpTitle;
    public Text claimPopUpInfo;
    public Text claimPopUpBtnText;

    public GameObject managers;

    private List<MoneyPlace> places = new List<MoneyPlace>();

    private int queuedMoneyClaim;

    public TextAsset jsonFileText;


    // Use this for initialization
    void Start()
    {
        audioS = GetComponent<AudioSource>();
        Input.location.Start(1f, 1f);

        string str = jsonFileText.text;
        Container dataFromJson = JsonUtility.FromJson<Container>(str); //Put json data in class container
        foreach (LocationInfo item in dataFromJson.locations)
        {
            places.Add(new MoneyPlace(item.name, item.money, item.latitude, item.longitude)); //Make objects with the data
        }

        InvokeRepeating("CheckLocation", 2.0f, 10.0f); //Call CheckLocation after 2 seconds, then repeat every x seconds
    }

    public void CheckLocation() //Check if user's current location matches with a location for money claiming
    {
        if (Input.location.status == LocationServiceStatus.Running)
        {
            foreach (MoneyPlace item in places)
            {
                if (Measure(Input.location.lastData.latitude, Input.location.lastData.longitude, item.latitudePos, item.longitudePos) < 20) //Check if we are closer than x meters to a certain place
                {
                    if (item.timeOfUnlock < DateTime.Now) //Check if the place is unlocked (not been claimed within 24 hours)
                    {
                        if (item.name == "Joske Vermeulen Standbeeld" && !Convert.ToBoolean(PlayerPrefs.GetString("gaston")))
                        {
                            PlayerPrefs.SetString("gaston", "true");
                            GetComponent<Manager>().actionTitle.text = "Gaston Berghmans";
                            GetComponent<Manager>().actionInfo.text = "Je bent bij het standbeeld Joske Vermeulen! Je speelde Gaston Berghmans vrij!";
                            audioS.clip = GetComponent<Manager>().examPassedSound;
                            audioS.Play();
                            GetComponent<Manager>().OpenActionResultPanel();
                        }
                        if (item.name == "Brabo" && !Convert.ToBoolean(PlayerPrefs.GetString("brabo")))
                        {
                            PlayerPrefs.SetString("brabo", "true");
                            GetComponent<Manager>().actionTitle.text = "Brabo";
                            GetComponent<Manager>().actionInfo.text = "Je bent aan de fontein van Brabo! Je speelde Brabo vrij!";
                            audioS.clip = GetComponent<Manager>().examPassedSound;
                            audioS.Play();
                            GetComponent<Manager>().OpenActionResultPanel();
                        }
                        if (item.name == "Den Bengel" && !Convert.ToBoolean(PlayerPrefs.GetString("ivanPecknik")))
                        {
                            PlayerPrefs.SetString("ivanPecknik", "true");
                            GetComponent<Manager>().actionTitle.text = "Ivan Pecknik";
                            GetComponent<Manager>().actionInfo.text = "Je bent aan café Den Bengel! Je speelde Ivan Pecknik vrij!";
                            audioS.clip = GetComponent<Manager>().examPassedSound;
                            audioS.Play();
                            GetComponent<Manager>().OpenActionResultPanel();
                        }
                        if (item.name == "Het Raamtheater" && !Convert.ToBoolean(PlayerPrefs.GetString("frankAendenboom")))
                        {
                            PlayerPrefs.SetString("frankAendenboom", "true");
                            GetComponent<Manager>().actionTitle.text = "Frank Aendenboom";
                            GetComponent<Manager>().actionInfo.text = "Je bent aan het Raamtheater! Je speelde Frank Aendenboom vrij!";
                            audioS.clip = GetComponent<Manager>().examPassedSound;
                            audioS.Play();
                            GetComponent<Manager>().OpenActionResultPanel();
                        }
                        claimPopUpTitle.text = item.name; //Set the title
                        claimPopUpInfo.text = item.infoText; //Set the info text
                        claimPopUpBtnText.text = "Claim " + item.moneyToGive + " euro"; //Set the button's text
                        claimPopUp.GetComponentInChildren<TweenTransforms>().Begin();
                        Handheld.Vibrate();
                        queuedMoneyClaim = item.moneyToGive; //Queue the money (we want the user to press claim)
                        item.timeOfUnlock = DateTime.Now.AddDays(1); //Next unlock is set to the current time, added one day
                        PlayerPrefs.SetString(item.name.ToString() + "MoneyUnlockTime", DateTime.Now.AddDays(1).ToString()); //Save the date into playerprefs
                        PlayerPrefs.Save();
                    }
                }
            }
        }
    }

    double Measure(float lat1, float lon1, float lat2, float lon2)
    {  // generally used geo measurement function
        var R = 6378.137; // Radius of earth in KM
        var dLat = lat2 * Mathf.PI / 180 - lat1 * Mathf.PI / 180;
        var dLon = lon2 * Mathf.PI / 180 - lon1 * Mathf.PI / 180;
        var a = Mathf.Sin(dLat / 2) * Mathf.Sin(dLat / 2) +
        Mathf.Cos(lat1 * Mathf.PI / 180) * Mathf.Cos(lat2 * Mathf.PI / 180) *
        Mathf.Sin(dLon / 2) * Mathf.Sin(dLon / 2);
        var c = 2 * Mathf.Atan2(Mathf.Sqrt(a), Mathf.Sqrt(1 - a));
        var d = R * c;
        return d * 1000; // meters
    } //Calculate distance between 2 world coördinates

    public void Claim() //When the claim button is pressed
    {
        int playerPrefsTotalMoneyCollected = PlayerPrefs.GetInt("totalMoneyCollected");
        managers.GetComponent<Manager>().AddMoney(queuedMoneyClaim); //Add the money to the player's balance
        playerPrefsTotalMoneyCollected += queuedMoneyClaim;
        PlayerPrefs.SetInt("totalMoneyCollected", playerPrefsTotalMoneyCollected);
        queuedMoneyClaim = 0; //Reset the money in the queue (for safety)
        claimPopUp.GetComponentInChildren<TweenTransforms>().Begin();
        audioS.clip = moneyPickUp;
        audioS.Play();
    }

    [System.Serializable] //For json
    public class Container
    {
        public LocationInfo[] locations;
    }

    [System.Serializable] //For json
    public class LocationInfo
    {
        public string name;
        public int money;
        public float longitude;
        public float latitude;
    }

}
