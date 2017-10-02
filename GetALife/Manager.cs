using UnityEngine;
using UnityEngine.UI;
using System;
using UnityEngine.SceneManagement;
using System.Collections;

public class Manager : MonoBehaviour {

    AudioSource audioS;
    public AudioClip examFailedSound;
    public AudioClip ambulanceSound;
    public AudioClip examPassedSound;
    public AudioClip moneyPickUp;

    public GameObject claimPopUp;
    public Text claimPopUpTitle;
    public Text claimPopUpInfo;
    public Text claimPopUpBtnText;

    GameObject[] _cultuurMarkers;
    GameObject[] _uitgaanMarkers;

    bool _cultuurBtnActivated = false;
    bool _uitgaanBtnActivated = false;

    bool isCultuurActive = false;
    bool isGezondheidActive = false;
    bool isPlezierActive = false;

    //For caching the markers state on info panel open
    bool wasCultuurBtnActivated = false;
    bool wasUitgaanBtnActivated = false;

    public Slider _cultuur, _plezier, _gezondheid;
    public Text _beginGeld, _beginStudiepunten, _titelInfo, _contentInfo, _adresInfo, _posStatsInfo, _negStatsInfo, _profileMoney, _profileSP, _profileExamCountDown, _profileExamenStudiepunten;
    public Image _campus, _studentenclub, _profielFoto, profielFotoUI;
    public Sprite alex, bart, brabo, frank, gaston, gert, herman, ivan, ivo, jacques, kevin, marie, nathalie, radja, romelu, rubens, samson, sandrine, tia, tom;
    public Button _studentenclubSelect;

    //For countdown timers
    public Image studyCountDown, sportCountDown, partyCountDown, cultureCountDown; //Count down image
    private TimeSpan maxCountDown; //Total countdown time
    private TimeSpan remainingCountDown; //Remaining countdown time

    private int _savedGeld, _savedStudiepunten;


    private int _moneyInfo;
    private bool removeMoney = false;
    private bool _isCultPos, _isPlezierPos, _isGezPos;
    private bool containsCult, containsPlezier, containsGezondheid, containsMoney = false;

    private int _moneyInfoCache;
    private bool removeMoneyCache = false;
    private bool _isCultPosCache, _isPlezierPosCache, _isGezPosCache;
    private bool containsCultCache, containsPlezierCache, containsGezondheidCache, containsMoneyCache = false;

    public Canvas _infoCanvas, _settingsCanvas, _playerCanvas;

    private string[] _textLines;
    private int _currentLine, _endAtLine;
    private float _cultuurSilderValue, _gezondheidSliderValue, _plezierSilderValue;

    private DateTime nextExamTime;
    private int nextExamStudyPoints;
    private int hoursStudied = 0;
    private int blackOutChance = 1;

    public GameObject actionResultPopUp;
    public Text actionTitle;
    public Text actionInfo, _cultuurSliderData, _gezondheidSliderData, _plezierSliderData;

    private DateTime activitiesLockedUntill;
    private string activityInProgress;

    private DateTime nextStatsUpdate; //Periodacally change the stats without having to do an action (else player could just stop playing without impact on stats)

    private int moneyWastedOnDrinking;
    private bool extraDrinking = false;
    private bool isUitgaan = false;
    private bool isCultuur = false;

    private bool isUitgaanCache = false;
    private bool isCultuurCache = false;

    //Tracked for alex agnew unlock
    private int totalTimeSportpaleisInfo;

    //Fun variables for the player
    private int totalBeersDrunk;
    private int totalHoursStudied;
    private int totalExamsFailed;
    private int totalMoneyCollected;
    private int totalTimeSported;
    private int totalMoneySpent;
    private int totalExamsPassed;
    private int totalTimeCulture;
    private int totalTimeParty;
    private int totalTimeComa;
    private int totalTimeBlackOut;

    //We only want sport pop up to show once per game session
    private bool showSportInfo = true;

    //Text objects in profile menu
    public Text totalBeers, totalStudy, totalExamsFail, totalMoneyCol, totalMoneySpe, totalExamsPass, totalCulture, totalParty, totalComa, totalBlackout, totalSport;



    // Use this for initialization
    void Start()
    {
        LoadAllPlayerPrefsValues();

        audioS = GetComponent<AudioSource>();

        _cultuurMarkers = GameObject.FindGameObjectsWithTag("CultuurMarker");
        _uitgaanMarkers = GameObject.FindGameObjectsWithTag("UitgaanMarker");

        HideMarkers(1);
        HideMarkers(2);
        
        UpdateCultuur();
        UpdateGezondheid();
        UpdateMoney();
        UpdatePlezier();
        UpdateStudypoints();

        InvokeRepeating("CheckForExam", 1f, 1f); //We'll check for the next exam every second
        UpdateCultuur();
        UpdatePlezier();
        UpdateGezondheid();
    }
	
	// Update is called once per frame
	void Update ()
    {

        CheckForTouchInput();

        //If player profile is active, live update the remaining time for exams
        if (_playerCanvas.gameObject.activeSelf)
        {
            TimeSpan examCountdown = (nextExamTime - DateTime.Now); //Calculate the remaining timespan
            _profileExamCountDown.text = String.Format("{0:00}:{1:00}:{2:00}", 
            (examCountdown.Hours + (examCountdown.Days*24)), examCountdown.Minutes, examCountdown.Seconds); //Display in format 00:00:00
        }

        CountDownTimerCheck();

        OverTimeStatsDecay();
    }

    public void CheckForTouchInput() //Handles touching a marker
    {
        for (int i = 0; i < Input.touchCount; ++i)
        {
            if (Input.GetTouch(i).phase == TouchPhase.Ended)
            {
                Vector2 ray = Camera.main.ScreenToWorldPoint(Input.GetTouch(i).position);
                RaycastHit2D hit = Physics2D.Raycast(ray, Vector2.zero);

                if (hit.collider != null)
                {
                    //Debug.Log(hit.collider.tag);

                    switch (hit.collider.tag)
                    {
                        case "CultuurMarker":
                            wasUitgaanBtnActivated = _uitgaanBtnActivated;
                            wasCultuurBtnActivated = _cultuurBtnActivated;
                            HideMarkers(1);
                            ShowInfoPanel(hit.collider.name);
                            break;

                        case "UitgaanMarker":
                            wasUitgaanBtnActivated = _uitgaanBtnActivated;
                            wasCultuurBtnActivated = _cultuurBtnActivated;
                            HideMarkers(2);
                            ShowInfoPanel(hit.collider.name);
                            break;


                    }
                }
            }
        }
    }

    public void ShowPlayerPanel() //Opens the player's profile menu and loads all data
    {

        if (PlayerPrefs.GetString("studentenclub_name") != "default")
        {
            _studentenclubSelect.gameObject.SetActive(false);
            _studentenclub.GetComponent<Image>().sprite = Resources.Load<Sprite>(PlayerPrefs.GetString("studentenclub_name"));
        }
        HideAllMarkers();
        _playerCanvas.GetComponentInChildren<TweenTransforms>().Begin();
        Camera.main.GetComponent<CameraHandler>().isMenuOpened = true;

        _campus.GetComponent<Image>().sprite = Resources.Load<Sprite>(PlayerPrefs.GetString("campus_name"));
        

        _profileMoney.text = PlayerPrefs.GetInt("geld").ToString();
        _profileSP.text = PlayerPrefs.GetInt("studiepunten").ToString();
        _profileExamenStudiepunten.text = nextExamStudyPoints.ToString();

        totalBeers.text = totalBeersDrunk.ToString();
        totalStudy.text = totalHoursStudied.ToString();
        totalExamsFail.text = totalExamsFailed.ToString();
        totalMoneyCol.text = totalMoneyCollected.ToString();
        totalMoneySpe.text = totalMoneySpent.ToString();
        totalExamsPass.text = totalExamsPassed.ToString();
        totalCulture.text = totalTimeCulture.ToString();
        totalParty.text = totalTimeParty.ToString();
        totalComa.text = totalTimeComa.ToString();
        totalBlackout.text = totalTimeBlackOut.ToString();
        totalSport.text = totalTimeSported.ToString();
    }

    public void GoToStudentClubScene() //Loads the student club selection scene
    {

        SceneManager.LoadScene("StudentenclubSelection");

    }

    public void HidePlayerPanel() //Hides the player's profile menu
    {
        _playerCanvas.GetComponentInChildren<TweenTransforms>().Begin();
        Camera.main.GetComponent<CameraHandler>().isMenuOpened = false;
    }

    public void ShowSettings() //Shows the settings menu
    {
        _settingsCanvas.GetComponentInChildren<TweenTransforms>().Begin();
        Camera.main.GetComponent<CameraHandler>().isMenuOpened = true;

        HideAllMarkers();

    }

    void ShowInfoPanel(string markerName) //Show the info panel (panel for a certain marker)
    {

        Camera.main.GetComponent<CameraHandler>().isMenuOpened = true;

        _infoCanvas.GetComponentInChildren<TweenTransforms>().Begin();

        HideAllMarkers();

        LoadTextFile(markerName);
    }

    void LoadTextFile(string markerName) //Loads the information for a certain marker and puts in into the info panel
    {

        if (markerName == "Sportpaleis")
        {
            totalTimeSportpaleisInfo++;
            if (totalTimeSportpaleisInfo == 15)
            {
                PlayerPrefs.SetString("alex", "true");
                actionTitle.text = "Alex Agnew";
                actionInfo.text = "Je opende 15 keer het Sportpaleis! Je speelde Alex Agnew vrij!";
                audioS.clip = examPassedSound;
                audioS.Play();
                OpenActionResultPanel();
            }
        }

        TextAsset txtAsset = (TextAsset)Resources.Load(markerName);

        if (txtAsset != null)
        {
            _textLines = (txtAsset.text.Split('\n'));

            _titelInfo.text = _textLines[0];
            _contentInfo.text = _textLines[1];
            _adresInfo.text = _textLines[2];

            for (int i = 3; i < _textLines.Length; i++)
            {
                if (_textLines[i].Contains("Uitgaan"))
                {
                    isUitgaan = true;
                }
                if (_textLines[i].Contains("Cultuur"))
                {
                    isCultuur = true;
                }
                if (_textLines[i].Contains("+"))
                {
                    if (_textLines[i].Contains("P"))
                    {
                        containsPlezier = true;
                        _isPlezierPos = true;
                        _posStatsInfo.text += "Plezier\n";
                    }
                    else if (_textLines[i].Contains("C"))
                    {
                        containsCult = true;
                        _isCultPos = true;
                        _posStatsInfo.text += "Cultuur\n";
                    }
                    else if (_textLines[i].Contains("M"))
                    {
                        removeMoney = false;
                        containsMoney = true;
                        Int32.TryParse(_textLines[i].Substring(2), out _moneyInfo);
                        _posStatsInfo.text += "+ " + _moneyInfo + " Geld\n";
                    }
                    else if (_textLines[i].Contains("G"))
                    {
                        _isGezPos = true;
                        containsGezondheid = true;
                        _posStatsInfo.text += "Gezondheid\n";
                    }
                }
                else if(_textLines[i].Contains("-"))
                {
                    if (_textLines[i].Contains("P"))
                    {
                        _isPlezierPos = false;
                        containsPlezier = true;
                        _negStatsInfo.text += "Plezier\n";
                    }
                    else if (_textLines[i].Contains("C"))
                    {
                        _isCultPos = false;
                        containsCult = true;
                        _negStatsInfo.text += "Cultuur\n";
                    }
                    else if (_textLines[i].Contains("M"))
                    {
                        removeMoney = true;
                        containsMoney = true;
                        Int32.TryParse(_textLines[i].Substring(2), out _moneyInfo);
                        _negStatsInfo.text += "- " + _moneyInfo + " Geld\n";
                    }
                    else if (_textLines[i].Contains("G"))
                    {
                        _isGezPos = false;
                        containsGezondheid = true;
                        _negStatsInfo.text += "Gezondheid\n";
                    }
                }             
            }
        }
    }

    void DelayUitgaanResults() //Results of going out are delayed untill the countdown timer stops
    {
        totalTimeParty++;
        if (totalTimeParty == 2)
        {
            PlayerPrefs.SetString("tomWaes", "true");
            actionTitle.text = "Dos Cervezas";
            actionInfo.text = "2 keer uitgegaan! Je speelde Tom Waes vrij!";
            audioS.clip = examPassedSound;
            audioS.Play();
            OpenActionResultPanel();
        }
        if (totalTimeParty == 10)
        {
            PlayerPrefs.SetString("ivoPauwels", "true");
            actionTitle.text = "Nonkel Jeff";
            actionInfo.text = "10 keer uitgegaan! Je speelde Ivo Pauwels vrij!";
            audioS.clip = examPassedSound;
            audioS.Play();
            OpenActionResultPanel();
        }

        if (containsPlezierCache)
        {
            PlezierImpact(_isPlezierPosCache);
        }
        if (containsMoneyCache && removeMoneyCache)
        {
            RemoveMoney(_moneyInfoCache);
        }
        if (containsCultCache)
        {
            CultuurImpact(_isCultPosCache);
        }
        if (containsGezondheidCache)
        {
            GezondheidImpact(_isGezPosCache);
        }
        if (extraDrinking)
        {
            RemoveMoney(moneyWastedOnDrinking);
            int pintjes = /*UnityEngine.Random.Range(1, */(Convert.ToInt32(moneyWastedOnDrinking / 2)/*)*/);
            if (containsMoneyCache)
            {
                pintjes += Convert.ToInt32(_moneyInfoCache / 2);
            }
            totalBeersDrunk += pintjes;
            if (pintjes > UnityEngine.Random.Range(15, 31))
            {
                int ziekenhuisKosten = UnityEngine.Random.Range(0, _savedGeld+1);
                int totalCost = ((_moneyInfoCache + moneyWastedOnDrinking) + ziekenhuisKosten);
                actionTitle.text = "Comazuiper";
                actionInfo.text = "Je ging ver over de schreef! Je dronk " + pintjes + " pintjes en belandde zo in een coma! Slecht voor je gezondheid, plezier en portemonnee! Door de ziekenhuisopname komen de totale kosten op " + totalCost + " euro" ;
                audioS.clip = ambulanceSound;
                RemoveMoney(ziekenhuisKosten);
                GezondheidImpact(false);
                GezondheidImpact(false);
                PlezierImpact(false);
                totalTimeComa++;
                OpenActionResultPanel();
                audioS.Play();
            }
            else
            {
                actionTitle.text = "Zuipschuit";
                actionInfo.text = "Je bent extra uitgeweest! Je dronk " + pintjes + "  pintjes en gaf in totaal " + (_moneyInfoCache + moneyWastedOnDrinking) + " euro uit";
                OpenActionResultPanel();
            }
        }
        else if (containsMoneyCache)
        {
            actionTitle.text = "Gelduitgave";
            actionInfo.text = "Je gaf het geplande bedrag van " + _moneyInfo + " euro uit. Je dronk " + Convert.ToInt32(_moneyInfo/2) + " pintjes";
            OpenActionResultPanel();
        }
        PlayerPrefs.SetInt("totalBeersDrunk", totalBeersDrunk);
        PlayerPrefs.SetInt("totalTimeComa", totalTimeComa);
        PlayerPrefs.SetInt("totalTimeParty", totalTimeParty);
        PlayerPrefs.Save();
        ClearInfoStatsCache();
    }

    void DelayCultuurResults() //Culture results are being delayed untill the countdown timer stopped
    {
        totalTimeCulture++;
        PlayerPrefs.SetInt("totalTimeCulture", totalTimeCulture);
        if (containsPlezierCache)
        {
            PlezierImpact(_isPlezierPosCache);
        }
        if (containsMoneyCache && removeMoneyCache)
        {
            RemoveMoney(_moneyInfoCache);
            actionTitle.text = "Gelduitgave";
            actionInfo.text = "Je gaf " + _moneyInfo + " euro uit aan cultuur";
            OpenActionResultPanel();
        }
        if (containsCultCache)
        {
            CultuurImpact(_isCultPosCache);
        }
        if (containsGezondheidCache)
        {
            GezondheidImpact(_isGezPosCache);
        }
        ClearInfoStatsCache();

        if (totalTimeCulture == 30)
        {
            PlayerPrefs.SetString("rubens", "true");
            actionTitle.text = "Rubens";
            actionInfo.text = "30 keer cultuur! Je speelde Rubens vrij!";
            audioS.clip = examPassedSound;
            audioS.Play();
            OpenActionResultPanel();
        }
        if (totalTimeCulture == 50)
        {
            PlayerPrefs.SetString("sandrineAndre", "true");
            actionTitle.text = "Sandrine André";
            actionInfo.text = "50 keer cultuur! Je speelde Sandrine André vrij!";
            audioS.clip = examPassedSound;
            audioS.Play();
            OpenActionResultPanel();

        }
        if (totalTimeCulture == 100)
        {
            PlayerPrefs.SetString("marieVinck", "true");
            actionTitle.text = "Marie Vinck";
            actionInfo.text = "100 keer cultuur! Je speelde Marie Vinck vrij!";
            audioS.clip = examPassedSound;
            audioS.Play();
            OpenActionResultPanel();
        }
        if (totalTimeCulture == 300)
        {
            PlayerPrefs.SetString("nathalieMeskens", "true");
            actionTitle.text = "Nathalie Meskens";
            actionInfo.text = "300 keer cultuur! Je speelde Nathalie Meskens vrij!";
            audioS.clip = examPassedSound;
            audioS.Play();
            OpenActionResultPanel();
        }
        if (totalTimeCulture == 500)
        {
            PlayerPrefs.SetString("kevinJanssens", "true");
            actionTitle.text = "Kevin Janssens";
            actionInfo.text = "500 keer cultuur! Je speelde Kevin Janssens vrij!";
            audioS.clip = examPassedSound;
            audioS.Play();
            OpenActionResultPanel();
        }
        PlayerPrefs.Save();
    }

    public void InfoBtnPressed() //When player presses the visit button 
    {
        bool infoMenuClosed = false;

        containsGezondheidCache = containsGezondheid;
        containsCultCache = containsCult;
        containsPlezierCache = containsPlezier;
        containsMoneyCache = containsMoney;
        _moneyInfoCache = _moneyInfo;
        isUitgaanCache = isUitgaan;
        isCultuurCache = isCultuur;
        _isPlezierPosCache = _isPlezierPos;
        removeMoneyCache = removeMoney;
        _isCultPosCache = _isCultPos;
        _isGezPosCache = _isGezPos;
        

        bool moneyCheckPassed = true;

        if (containsMoneyCache) //Als activiteit money vereist, check of het geld voldoende is
        {
            if (_savedGeld >= _moneyInfoCache)
            {
                moneyCheckPassed = true; //Speler heeft voldoende geld
            }
            else
            {
                moneyCheckPassed = false; //Speler heeft te weinig geld
            }
        }
        if (activitiesLockedUntill < DateTime.Now && moneyCheckPassed)
        {
            if (isUitgaanCache && isCultuurCache)
            {
                if (_gezondheidSliderValue <= 0 && !(_plezierSilderValue <= 0) && !(_cultuurSilderValue <= 0))
                {
                    actionTitle.text = "Futloos";
                    actionInfo.text = "Je voelt je echt niet goed genoeg hiervoor!";
                    HideInfoPanel();
                    infoMenuClosed = true;
                    OpenActionResultPanel();
                }
                else
                {
                    //Lock activities
                    activitiesLockedUntill = DateTime.Now.AddSeconds(UnityEngine.Random.Range(1, 16));

                    //Start countdown
                    activityInProgress = "cultuur";
                    maxCountDown = activitiesLockedUntill - DateTime.Now;
                    Invoke("DelayCultuurResults", maxCountDown.Seconds);
                }
            }
            else if (isUitgaanCache)
            {
                if (_gezondheidSliderValue <= 0 && !(_plezierSilderValue <= 0))
                {
                    actionTitle.text = "Nee Feestbeest";
                    actionInfo.text = "Feesten zit er niet in op dit moment!";
                    HideInfoPanel();
                    infoMenuClosed = true;
                    OpenActionResultPanel();
                }
                else if (_cultuurSilderValue <= 0 && !(_plezierSilderValue <= 0))
                {
                    actionTitle.text = "Cultuurfeestje";
                    actionInfo.text = "Zijn er misschien ergens cultuurfeestjes?";
                    HideInfoPanel();
                    infoMenuClosed = true;
                    OpenActionResultPanel();
                }
                else
                {
                    if (UnityEngine.Random.Range(1, 101) <= 30 && _savedGeld > 0) //Check of we extra gaan uitgeven (30% kans)
                    {
                        activitiesLockedUntill = DateTime.Now.AddSeconds(UnityEngine.Random.Range(5, 31));
                        extraDrinking = true;
                        moneyWastedOnDrinking = UnityEngine.Random.Range(1, _savedGeld + 1);
                        if (moneyWastedOnDrinking > 30)
                        {
                            moneyWastedOnDrinking = 30;
                        }
                    }
                    else
                    {
                        activitiesLockedUntill = DateTime.Now.AddSeconds(UnityEngine.Random.Range(1, 16));
                        extraDrinking = false;
                        moneyWastedOnDrinking = 0;
                    }

                    //Start countdown
                    activityInProgress = "uitgaan";
                    maxCountDown = activitiesLockedUntill - DateTime.Now;
                    Invoke("DelayUitgaanResults", maxCountDown.Seconds);
                }
            }
            else if (isCultuurCache) //Indien bezig met cultuur
            {
                if (_gezondheidSliderValue <= 0 && !(_cultuurSilderValue <= 0))
                {
                    actionTitle.text = "Gezond is anders";
                    actionInfo.text = "Je hebt nu echt niet de gezondheid hiervoor!";
                    HideInfoPanel();
                    infoMenuClosed = true;
                    OpenActionResultPanel();
                }
                else if (_plezierSilderValue <= 0 && !(_cultuurSilderValue <= 0))
                {
                    actionTitle.text = "Feestje?";
                    actionInfo.text = "Je wilt echt eens liever gaan feesten ofzo";
                    HideInfoPanel();
                    infoMenuClosed = true;
                    OpenActionResultPanel();
                }
                else
                {
                    //Lock activities
                    activitiesLockedUntill = DateTime.Now.AddSeconds(UnityEngine.Random.Range(1, 16));

                    //Start countdown
                    activityInProgress = "cultuur";
                    maxCountDown = activitiesLockedUntill - DateTime.Now;
                    Invoke("DelayCultuurResults", maxCountDown.Seconds);
                }
            }
        }
        else if (!moneyCheckPassed) //Geen geld meer
        {
            actionTitle.text = "Geldproblemen";
            actionInfo.text = "Je hebt niet genoeg geld meer! Wacht op je zakgeld of ga op stap in Antwerpen!";
            HideInfoPanel();
            infoMenuClosed = true;
            OpenActionResultPanel();
            ClearInfoStatsCache();
        }
        else //Andere activiteit in progress
        {
            actionTitle.text = "Even geduld";
            actionInfo.text = "Je kan slechts 1 activiteit tegelijk doen! Je bent momenteel nog bezig met " + activityInProgress + "!";
            HideInfoPanel();
            infoMenuClosed = true;
            OpenActionResultPanel();
            ClearInfoStatsCache();
        }

        if (!infoMenuClosed)
        {
            HideInfoPanel();
        }

    }

    void ClearInfoStatsCache() //Clears the cached values of the info panel, cached values are used when a marker is being visited
    {
        _moneyInfoCache = 0;
        removeMoneyCache = false;
        _isCultPosCache = false;
        _isPlezierPosCache = false;
        _isGezPosCache = false;
        containsCultCache = false;
        containsPlezierCache = false;
        containsGezondheidCache = false;
        containsMoneyCache = false;
        isUitgaanCache = false;
        isCultuurCache = false;
}

    void ResetAllInfoStats() //resets the info panel text
    {
        _titelInfo.text = null;
        _contentInfo.text = null;
        _adresInfo.text = null;

        _posStatsInfo.text = null;
        _negStatsInfo.text = null;

        isCultuur = false;
        isUitgaan = false;

        containsCult = false;
        containsGezondheid = false;
        containsMoney = false;
        containsPlezier = false;

    }

    public void ExitApp() //returns to main menu and posts data to the server one last time
    {

        SceneManager.LoadScene("Main Menu");
        GetComponent<PostServerData>().InvokedPostData();
    }

    public void HideSettingsPanel() //Gets called by the exit button on settings panel
    {
        _settingsCanvas.GetComponentInChildren<TweenTransforms>().Begin();
        Camera.main.GetComponent<CameraHandler>().isMenuOpened = false;

    }

    public void HideInfoPanel() //Clears variables and reenables map markers
    {
        _infoCanvas.GetComponentInChildren<TweenTransforms>().Begin();
        Camera.main.GetComponent<CameraHandler>().isMenuOpened = false;

        if (wasCultuurBtnActivated)
        {
            ShowMarkers(1);
        }
        if (wasUitgaanBtnActivated)
        {
            ShowMarkers(2);
        }

        ResetAllInfoStats();

    }

    //Function probably no longer used, but commented out in case we still need it
    //void HideInfoPanelWithoutClearing()
    //{
    //    _infoCanvas.gameObject.SetActive(false);
    //    Camera.main.GetComponent<CameraHandler>().isMenuOpened = false;

    //    if (wasCultuurBtnActivated)
    //    {
    //        ShowMarkers(1);
    //    }
    //    if (wasUitgaanBtnActivated)
    //    {
    //        ShowMarkers(2);
    //    }
    //}

    public void CultuurBtnPressed() //Toggle culture markers on map
    {

        if (!_cultuurBtnActivated)
        {
            ShowMarkers(1);
        }
        else
        {
            HideMarkers(1);
        }

        
    }

    public void UitgaanBtnPressed() //Toggle fun markers on map
    {

        if (!_uitgaanBtnActivated)
        {
            ShowMarkers(2);
        }
        else
        {
            HideMarkers(2);
        }

        
    }

    public void SportBtnPressed() //Handles everything that needs to be done when the player presses the sport button
    {
        if (activitiesLockedUntill < DateTime.Now)
        {
            if (_cultuurSilderValue <= 0 && !(_gezondheidSliderValue <= 0))
            {
                actionTitle.text = "Gat in uwe cultuur";
                actionInfo.text = "Misschien moet je toch meer eens wat cultuur gaan afzien";
                OpenActionResultPanel();
            }
            else if (_plezierSilderValue <= 0 && !(_gezondheidSliderValue <= 0))
            {
                actionTitle.text = "Depressief";
                actionInfo.text = "Waarom ga je zo weinig uit?";
                OpenActionResultPanel();
            }
            else
            {
                activitiesLockedUntill = DateTime.Now.AddSeconds(UnityEngine.Random.Range(5, 16));
                activityInProgress = "sporten";
                totalTimeSported++;
                maxCountDown = activitiesLockedUntill - DateTime.Now;
                GezondheidImpact(true);
                actionTitle.text = "Sporten";
                actionInfo.text = "Je gaat sporten met één van de talloze activiteiten van de SportSticker! Meer info op sportsticker.be!";
                if (showSportInfo)
                {
                    OpenActionResultPanel();
                    showSportInfo = false;
                }
                if (totalTimeSported >= 50)
                {
                    PlayerPrefs.SetString("dunneBart", "true");
                    actionTitle.text = "Bart de Wever";
                    actionInfo.text = "50 keer gesport! Je speelde Bart de Wever vrij!";
                    audioS.clip = examPassedSound;
                    audioS.Play();
                    OpenActionResultPanel();
                }
                if (totalTimeSported == 100)
                {
                    PlayerPrefs.SetString("tiaHellebaut", "true");
                    actionTitle.text = "Tia Hellebaut";
                    actionInfo.text = "100 keer gesport! Je speelde Tia Hellebaut vrij!";
                    audioS.clip = examPassedSound;
                    audioS.Play();
                    OpenActionResultPanel();
                }
                if (totalTimeSported == 250)
                {
                    PlayerPrefs.SetString("radjaNainggolan", "true");
                    actionTitle.text = "Radja Nainggolan";
                    actionInfo.text = "250 keer gesport! Je speelde Radja Nainggolan vrij!";
                    audioS.clip = examPassedSound;
                    audioS.Play();
                    OpenActionResultPanel();
                }
                if (totalTimeSported == 500)
                {
                    PlayerPrefs.SetString("romeluLukaku", "true");
                    actionTitle.text = "Romelu Lukaku";
                    actionInfo.text = "500 keer gesport! Je speelde Romelu Lukaku vrij!";
                    audioS.clip = examPassedSound;
                    audioS.Play();
                    OpenActionResultPanel();
                }
                PlayerPrefs.SetString("activityInProgress", activityInProgress);
                PlayerPrefs.SetInt("totalTimeSported", totalTimeSported);
                PlayerPrefs.SetString("activitiesLockedUntill", activitiesLockedUntill.ToString());
                PlayerPrefs.Save();
            }
        }
        else
        {
            ActivitiesAreLocked();
        }
        
    }

    public void StuderentBtnPressed() //Handles everything that needs to be done when the player presses the study button
    {
        
        if (activitiesLockedUntill < DateTime.Now)
        {
            if (_plezierSilderValue <= 0)
            {
                actionTitle.text = "Depressief";
                actionInfo.text = "Studeren? Echt waar? Ga toch eens uit man!";
                OpenActionResultPanel();
            }
            else if (_cultuurSilderValue <= 0)
            {
                actionTitle.text = "Cultuurbarbaar";
                actionInfo.text = "Je zou beter eens wat cultuur gaan bekijken in plaats van er oer te leren";
                OpenActionResultPanel();
            }
            else if (_gezondheidSliderValue <= 0)
            {
                actionTitle.text = "Hoofdpijn";
                actionInfo.text = "Met deze hoofdpijn kan je toch niet studeren? Doe eens gezond!";
                OpenActionResultPanel();
            }
            else
            {
                activitiesLockedUntill = DateTime.Now.AddSeconds(UnityEngine.Random.Range(5, 16));
                activityInProgress = "studeren";
                maxCountDown = activitiesLockedUntill - DateTime.Now; //Set the countdown timer
                hoursStudied++;
                totalHoursStudied++;
                PlezierImpact(false);
                PlayerPrefs.SetInt("plezier", Convert.ToInt32(_plezierSilderValue));
                PlayerPrefs.SetInt("hoursStudied", hoursStudied);
                PlayerPrefs.SetInt("totalHoursStudied", totalHoursStudied);
                PlayerPrefs.SetString("activityInProgress", activityInProgress);
                PlayerPrefs.SetString("activitiesLockedUntill", activitiesLockedUntill.ToString());
                if (totalHoursStudied == 50)
                {
                    PlayerPrefs.SetString("hermanVerbruggen", "true");
                    actionTitle.text = "Markske";
                    actionInfo.text = "50 keer gestudeerd! Je speelde Herman Verbruggen vrij!";
                    audioS.clip = examPassedSound;
                    audioS.Play();
                    OpenActionResultPanel();
                }
                PlayerPrefs.Save();
            }
        }
        else
        {
            ActivitiesAreLocked();
        }

    }

    public void CheckForExam() //Calculates the results of an exam when the exam time has passed
    {
        if (nextExamTime <= DateTime.Now)
        {
            int slaagKans = Mathf.RoundToInt((hoursStudied / (nextExamStudyPoints * 2)) * 100);
            if (slaagKans > 100)
            {
                blackOutChance = slaagKans - 100;
                PlayerPrefs.SetInt("blackOutChance", blackOutChance);
                PlayerPrefs.Save();
                slaagKans = 100;
            }
            if (UnityEngine.Random.Range(0, 101) <= slaagKans)
            {
                if (UnityEngine.Random.Range(0, 101) <= blackOutChance)
                {
                    actionInfo.text = "Je had een blackout op je examens. Je verloor " + nextExamStudyPoints + " studiepunten";
                    actionTitle.text = "Examenresultaten";
                    audioS.clip = examFailedSound;
                    RemoveStudypoints(nextExamStudyPoints);
                    audioS.Play();
                    OpenActionResultPanel();
                    nextExamTime = DateTime.Now.AddMinutes(UnityEngine.Random.Range(5, 61));
                    nextExamStudyPoints = UnityEngine.Random.Range(3, 21);
                    blackOutChance = 1;
                    hoursStudied = 0;
                    totalExamsFailed++;
                    totalTimeBlackOut++;
                    PlayerPrefs.SetString("nextExamTime", nextExamTime.ToString());
                    PlayerPrefs.SetInt("nextExamStudyPoints", nextExamStudyPoints);
                    PlayerPrefs.SetInt("blackOutChance", blackOutChance);
                    PlayerPrefs.SetInt("hoursStudied", hoursStudied);
                    PlayerPrefs.SetInt("totalExamsFailed", totalExamsFailed);
                    PlayerPrefs.SetInt("totalTimeBlackOut", totalTimeBlackOut);
                    PlayerPrefs.Save();
                    if (_playerCanvas.gameObject.activeSelf)
                    {
                        ShowPlayerPanel();
                    }
                }
                else
                {
                    actionInfo.text = "Je bent geslaagd op al je examens! Proficiat! Doe zo verder!";
                    actionTitle.text = "Examenresultaten";
                    audioS.clip = examPassedSound;
                    audioS.Play();
                    OpenActionResultPanel();
                    nextExamTime = DateTime.Now.AddMinutes(UnityEngine.Random.Range(5, 61));
                    nextExamStudyPoints = UnityEngine.Random.Range(3, 21);
                    blackOutChance = 1;
                    hoursStudied = 0;
                    totalExamsPassed++;
                    PlayerPrefs.SetString("nextExamTime", nextExamTime.ToString());
                    PlayerPrefs.SetInt("nextExamStudyPoints", nextExamStudyPoints);
                    PlayerPrefs.SetInt("blackOutChance", blackOutChance);
                    PlayerPrefs.SetInt("hoursStudied", hoursStudied);
                    PlayerPrefs.SetInt("totalExamsPassed", totalExamsPassed);
                    PlayerPrefs.Save();
                    if (_playerCanvas.gameObject.activeSelf)
                    {
                        ShowPlayerPanel();
                    }
                }
            }
            else
            {
                RemoveStudypoints(nextExamStudyPoints);
                actionInfo.text = "Je bent niet geslaagd voor je examens. Je verloor " + nextExamStudyPoints + " studiepunten. Je hebt nu nog " + _savedStudiepunten + " studiepunten over";
                actionTitle.text = "Examenresultaten";
                audioS.clip = examFailedSound;
                audioS.Play();
                OpenActionResultPanel();
                nextExamTime = DateTime.Now.AddMinutes(UnityEngine.Random.Range(5, 61));
                nextExamStudyPoints = UnityEngine.Random.Range(3, 21);
                blackOutChance = 1;
                hoursStudied = 0;
                totalExamsFailed++;
                PlayerPrefs.SetInt("hoursStudied", hoursStudied);
                PlayerPrefs.SetString("nextExamTime", nextExamTime.ToString());
                PlayerPrefs.SetInt("nextExamStudyPoints", nextExamStudyPoints);
                PlayerPrefs.SetInt("blackOutChance", blackOutChance);
                PlayerPrefs.SetInt("totalExamsFailed", totalExamsFailed);
                PlayerPrefs.Save();
            }
            
        }

    }

    void HideMarkers(int marker) //Hide culture (1) or fun (2) markers
    {

        switch (marker)
        {
            case 1:
                foreach (GameObject cult in _cultuurMarkers)
                {
                    cult.SetActive(false);
                }
                _cultuurBtnActivated = false;
                break;

            case 2:
                foreach (GameObject uit in _uitgaanMarkers)
                {
                    uit.SetActive(false);
                }
                _uitgaanBtnActivated = false;
                break;

            default:
                break;
        }

    }

    void ShowMarkers(int marker) //Shows culture markers (1) or fun markers (2)
    {

        switch (marker)
        {
            case 1:
                foreach (GameObject cult in _cultuurMarkers)
                {
                    cult.SetActive(true);
                }
                _cultuurBtnActivated = true;
                break;

            case 2:
                foreach (GameObject uit in _uitgaanMarkers)
                {
                    uit.SetActive(true);
                }
                _uitgaanBtnActivated = true;
                break;

            default:
                break;
        }

    }

    void HideAllMarkers() //Hides all markers
    {

        //hide markers
        HideMarkers(1);
        HideMarkers(2);

    }

    public void AddMoney(int newMoney) //Handles all necessary steps when money gets added to the player's balance
    {
        _savedGeld += newMoney;
        totalMoneyCollected += newMoney;
        PlayerPrefs.SetInt("totalMoneyCollected", totalMoneyCollected);
        PlayerPrefs.SetInt("geld", _savedGeld);
        PlayerPrefs.Save();
        UpdateMoney();
        if (_savedGeld >= 1000000)
        {
            PlayerPrefs.SetString("jacquesVermeire", "true");
            actionTitle.text = "Jacques Vermeire";
            actionInfo.text = "1.000.000 euro in totaal verdiend! Je speelde Jacques Vermeire vrij!";
            audioS.clip = examPassedSound;
            audioS.Play();
            OpenActionResultPanel();
        }
        if (totalMoneyCollected >= 1000)
        {
            PlayerPrefs.SetString("gert", "true");
            actionTitle.text = "Gert Verhulst";
            actionInfo.text = "1000 euro in totaal verdiend! Je speelde Gert Verhulst vrij!";
            audioS.clip = examPassedSound;
            audioS.Play();
            OpenActionResultPanel();
        }
    }

    public void RemoveMoney(int newMoney) //Handles all necessary steps when money has to be taken from the player
    {
        _savedGeld -= newMoney;
        totalMoneySpent += newMoney;
        PlayerPrefs.SetInt("totalMoneySpent", totalMoneySpent);
        PlayerPrefs.SetInt("geld", _savedGeld);
        PlayerPrefs.Save();
        UpdateMoney();
    }

    public void RemoveStudypoints(int newSP) //Handles all necessary steps when study points need to be removed
    {
        _savedStudiepunten -= newSP;
        PlayerPrefs.SetInt("studiepunten", _savedStudiepunten);
        PlayerPrefs.Save();
        UpdateStudypoints();
    }

    public void GezondheidImpact(bool isPositiveChange) //Randomly affects the player's health bar, negatively (false) or positively (true)
    {
        int change = UnityEngine.Random.Range(1, 21);
        if (isPositiveChange)
        {
            _gezondheidSliderValue += change;
        }
        else
        {
            _gezondheidSliderValue -= change;
        }
        UpdateGezondheid();
    }

    public void CultuurImpact(bool isPositiveChange)//Randomly affects the player's culture bar, negatively (false) or positively (true)
    {
        int change = UnityEngine.Random.Range(1, 21);
        if (isPositiveChange)
        {
            _cultuurSilderValue += change;
        }
        else
        {
            _cultuurSilderValue -= change;
        }
        UpdateCultuur();
    }

    public void PlezierImpact(bool isPositiveChange) //Randomly affects the player's fun bar, negatively (false) or positively (true)
    {
        int change = UnityEngine.Random.Range(1, 21);
        if (isPositiveChange)
        {
            _plezierSilderValue += change;
        }
        else
        {
            _plezierSilderValue -= change;
        }
        UpdatePlezier();
    }

    void UpdateMoney() //Update money ui and post playerprefs
    {
        _beginGeld.text = _savedGeld.ToString();
        PlayerPrefs.SetInt("geld", _savedGeld);
    }

    void UpdateStudypoints() //Check min value and call game over if needed
    {
        _beginStudiepunten.text = _savedStudiepunten.ToString();
        PlayerPrefs.SetInt("studiepunten", _savedStudiepunten);
        if (_savedStudiepunten <= 0)
        {
            _savedStudiepunten = 0;
            SceneManager.LoadScene("GameOver");
        }
    }

    void UpdateGezondheid()
    {
        if (_gezondheidSliderValue < 0)
        {
            _gezondheidSliderValue = 0;
        }
        if (_gezondheidSliderValue > 100)
        {
            _gezondheidSliderValue = 100;
        }
        _gezondheid.value = _gezondheidSliderValue;
        if (_gezondheid.GetComponentInChildren<Text>() != null)
        {
            _gezondheid.GetComponentInChildren<Text>().text = _gezondheidSliderValue + "/100";
        }
        
        PlayerPrefs.SetInt("gezondheid", Convert.ToInt32(_gezondheidSliderValue));
    } //Check max and min values, then post to playerprefs

    void UpdateCultuur()
    {
        if (_cultuurSilderValue < 0)
        {
            _cultuurSilderValue = 0;
        }
        if (_cultuurSilderValue > 100)
        {
            _cultuurSilderValue = 100;
        }
        _cultuur.value = _cultuurSilderValue;
        if (_cultuur.GetComponentInChildren<Text>() != null)
        {
            _cultuur.GetComponentInChildren<Text>().text = _cultuurSilderValue + "/100";
        }
        PlayerPrefs.SetInt("cultuur", Convert.ToInt32(_cultuurSilderValue));
    } //Check max and min values, then post to playerprefs

    void UpdatePlezier()
    {
        if (_plezierSilderValue < 0)
        {
            _plezierSilderValue = 0;
        }
        if (_plezierSilderValue > 100)
        {
            _plezierSilderValue = 100;
        }
        _plezier.value = _plezierSilderValue;
        if (_plezier.GetComponentInChildren<Text>() != null)
        {
            _plezier.GetComponentInChildren<Text>().text = _plezierSilderValue + "/100";
        }
        PlayerPrefs.SetInt("plezier", Convert.ToInt32(_plezierSilderValue));
    } //Check max and min values, then post to playerprefs

    public void CloseActionResultPanel()
    {
        //close animation
        actionResultPopUp.GetComponentInChildren<TweenTransforms>().Begin();
        Camera.main.GetComponent<CameraHandler>().isMenuOpened = false;
    } //Called when exit is pressed on the action result canvas

    private void OverTimeStatsDecay() //Stats should decay over time
    {
        if (nextStatsUpdate < DateTime.Now)
        {
            nextStatsUpdate = DateTime.Now.AddMinutes(UnityEngine.Random.Range(3, 61));
            PlayerPrefs.SetString("nextStatsUpdate", nextStatsUpdate.ToString());
            CultuurImpact(false);
            PlezierImpact(false);
            GezondheidImpact(false);
            int zakgeld = UnityEngine.Random.Range(10, 201);
            claimPopUpTitle.text = "Zakgeld";
            claimPopUpInfo.text = "Je kreeg " + zakgeld + " euro zakgeld!";
            claimPopUpBtnText.text = "Claim";
            AddMoney(zakgeld);
            claimPopUp.GetComponentInChildren<TweenTransforms>().Begin();
        }
    }

    public void OpenActionResultPanel() 
    {
        actionResultPopUp.GetComponentInChildren<TweenTransforms>().Begin();
        Handheld.Vibrate();
        Camera.main.GetComponent<CameraHandler>().isMenuOpened = true;
    }

    public void ActivitiesAreLocked() //Opens pop up when another activity is already in progress
    {
        actionTitle.text = "Even geduld";
        actionInfo.text = "Je kan slechts 1 activiteit tegelijk doen! Je bent momenteel nog bezig met " + activityInProgress + "!";
        OpenActionResultPanel();
    }

    public void LoadAllPlayerPrefsValues() //Loads all values we need from playerprefs
    {
        _savedGeld = PlayerPrefs.GetInt("geld");
        _savedStudiepunten = PlayerPrefs.GetInt("studiepunten");
        _plezierSilderValue = PlayerPrefs.GetInt("plezier");
        _cultuurSilderValue = PlayerPrefs.GetInt("cultuur");
        _gezondheidSliderValue = PlayerPrefs.GetInt("gezondheid");
        nextExamTime = Convert.ToDateTime(PlayerPrefs.GetString("nextExamTime"));
        nextExamStudyPoints = PlayerPrefs.GetInt("nextExamStudyPoints");
        hoursStudied = PlayerPrefs.GetInt("hoursStudied");
        blackOutChance = PlayerPrefs.GetInt("blackOutChance");
        activitiesLockedUntill = Convert.ToDateTime(PlayerPrefs.GetString("activitiesLockedUntill"));
        activityInProgress = PlayerPrefs.GetString("activityInProgress");

        _beginGeld.text = _savedGeld.ToString();
        _beginStudiepunten.text = _savedStudiepunten.ToString();

        nextStatsUpdate = Convert.ToDateTime(PlayerPrefs.GetString("nextStatsUpdate"));

        totalBeersDrunk = PlayerPrefs.GetInt("totalBeersDrunk");
        totalHoursStudied = PlayerPrefs.GetInt("totalHoursStudied");
        totalExamsFailed = PlayerPrefs.GetInt("totalExamsFailed");
        totalExamsPassed = PlayerPrefs.GetInt("totalExamsPassed");
        totalMoneyCollected = PlayerPrefs.GetInt("totalMoneyCollected");
        totalMoneySpent = PlayerPrefs.GetInt("totalMoneySpent");
        totalTimeSported = PlayerPrefs.GetInt("totalTimeSported");
        totalTimeCulture = PlayerPrefs.GetInt("totalTimeCulture");
        totalTimeParty = PlayerPrefs.GetInt("totalTimeParty");
        totalTimeComa = PlayerPrefs.GetInt("totalTimeComa");

        totalTimeSportpaleisInfo = PlayerPrefs.GetInt("totalTimeSportpaleisPanel");

        //Set profile picture
        switch (PlayerPrefs.GetString("selectedCharacter"))
        {
            case "gaston":
                _profielFoto.sprite = gaston;
                profielFotoUI.sprite = gaston;
                break;
            case "alex":
                _profielFoto.sprite = alex;
                profielFotoUI.sprite = alex;
                break;
            case "brabo":
                _profielFoto.sprite = brabo;
                profielFotoUI.sprite = brabo;
                break;
            case "bart":
                _profielFoto.sprite = bart;
                profielFotoUI.sprite = bart;
                break;
            case "frank":
                _profielFoto.sprite = frank;
                profielFotoUI.sprite = frank;
                break;
            case "gert":
                _profielFoto.sprite = gert;
                profielFotoUI.sprite = gert;
                break;
            case "herman":
                _profielFoto.sprite = herman;
                profielFotoUI.sprite = herman;
                break;
            case "ivan":
                _profielFoto.sprite = ivan;
                profielFotoUI.sprite = ivan;
                break;
            case "ivo":
                _profielFoto.sprite = ivo;
                profielFotoUI.sprite = ivo;
                break;
            case "jacques":
                _profielFoto.sprite = jacques;
                profielFotoUI.sprite = jacques;
                break;
            case "kevin":
                _profielFoto.sprite = kevin;
                profielFotoUI.sprite = kevin;
                break;
            case "marie":
                _profielFoto.sprite = marie;
                profielFotoUI.sprite = marie;
                break;
            case "nathalie":
                _profielFoto.sprite = nathalie;
                profielFotoUI.sprite = nathalie;
                break;
            case "radja":
                _profielFoto.sprite = radja;
                profielFotoUI.sprite = radja;
                break;
            case "romelu":
                _profielFoto.sprite = romelu;
                profielFotoUI.sprite = romelu;
                break;
            case "rubens":
                _profielFoto.sprite = rubens;
                profielFotoUI.sprite = rubens;
                break;
            case "samson":
                _profielFoto.sprite = samson;
                profielFotoUI.sprite = samson;
                break;
            case "sandrine":
                _profielFoto.sprite = sandrine;
                profielFotoUI.sprite = sandrine;
                break;
            case "tia":
                _profielFoto.sprite = tia;
                profielFotoUI.sprite = tia;
                break;
            case "tom":
                _profielFoto.sprite = tom;
                profielFotoUI.sprite = tom;
                break;
            default:
                _profielFoto.sprite = gaston;
                profielFotoUI.sprite = gaston;
                break;
        }
    }

    public void CountDownTimerCheck()//Activate countdown timer if an activity is in progress
    {
        if (DateTime.Now < activitiesLockedUntill)
        {
            remainingCountDown = (activitiesLockedUntill - DateTime.Now);
            float remaincntseconds = remainingCountDown.Seconds;
            float remaincntmiliseconds = remainingCountDown.Milliseconds;
            float maxcntdwnseconds = maxCountDown.Seconds;
            float maxcntdwnmiliseconds = maxCountDown.Milliseconds;
            float remain = ((remaincntseconds * 1000) + remaincntmiliseconds) / ((maxcntdwnseconds * 1000) + maxcntdwnmiliseconds);
            switch (activityInProgress)
            {
                case "studeren":
                    studyCountDown.fillAmount = remain;
                    break;
                case "sporten":
                    sportCountDown.fillAmount = remain;
                    break;
                case "uitgaan":
                    partyCountDown.fillAmount = remain;
                    break;
                case "cultuur":
                    cultureCountDown.fillAmount = remain;
                    break;
                default:
                    break;
            }
        }
        else
        {
            studyCountDown.fillAmount = 0;
            sportCountDown.fillAmount = 0;
            partyCountDown.fillAmount = 0;
            cultureCountDown.fillAmount = 0;
        }
    }

    public void ShowSliderInfo(string slider) {

        

        if (slider == "Cultuur")
        {

            if (!isCultuurActive)
            {
                _cultuurSliderData.gameObject.SetActive(true);
                _cultuur.GetComponentInChildren<Text>().text = _cultuurSilderValue + "/100";
                isCultuurActive = true;
            }
            else
            {
                _cultuurSliderData.gameObject.SetActive(false);

                isCultuurActive = false;
            }
            
            
        }
        else if (slider == "Gezondheid")
        {
            if (!isGezondheidActive)
            {
                _gezondheidSliderData.gameObject.SetActive(true);
                _gezondheid.GetComponentInChildren<Text>().text = _gezondheidSliderValue + "/100";
                isGezondheidActive = true;
            }
            else
            {
                _gezondheidSliderData.gameObject.SetActive(false);

                isGezondheidActive = false;
            }
        }
        else
        {
            if (!isPlezierActive)
            {
                _plezierSliderData.gameObject.SetActive(true);
                _plezier.GetComponentInChildren<Text>().text = _plezierSilderValue + "/100";
                isPlezierActive = true;
            }
            else
            {
                _plezierSliderData.gameObject.SetActive(false);

                isPlezierActive = false;
            }
        }
    }


}
