using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using KModkit;
using System.Text.RegularExpressions;

public class TransmittedMorseScript : MonoBehaviour {

    public KMAudio audio;
    public KMBombInfo bomb;

    public KMSelectable[] buttons;

    public GameObject slider1butdisp;
    public GameObject slider2butdisp;
    public GameObject slider3butdisp;

    public GameObject slider1;
    public GameObject slider2;
    public GameObject slider3;

    public Renderer tled;
    public Renderer bled;
    public Renderer stage1led;
    public Renderer stage2led;

    public Material[] ledoptions;

    private string[] messages = {"BOMBS","SHORT","UNDERSTOOD","W1RES","SOS","MANUAL","STRIKED",
                                "WEREDEAD","GOTASOUV","EXPLOSION","EXPERT","RIP","LISTEN","DETONATE",
                                "ROGER","WELOSTBRO","AMIDEAF","KEYPAD","DEFUSER","NUCLEARWEAPONS",
                                "KAPPA","DELTA","PI3","SMOKE","SENDHELP","LOST","SWAN",
                                "NOMNOM","BLUE","BOOM","CANCEL","DEFUSED","BROKEN","MEMORY",
                                "R6S8T","TRANSMISSION","UMWHAT","GREEN","EQUATIONSX",
                                "RED","ENERGY","JESTER","CONTACT","LONG",
                                "CODERED","UNLUCKY"};
    private string message;
    private string messagetrans;

    private int stage;
    private int currentord;

    private string topLED;
    private string bottomLED;

    private int[] sliders;
    private int[] positions;

    private bool pressonce;
    private bool pressonce2;

    private Vector3 posslide1;
    private Vector3 posslide2;
    private Vector3 posslide3;

    private IEnumerator cour;
    private bool courrunning = false;

    static int moduleIdCounter = 1;
    int moduleId;
    private bool moduleSolved;
    private int announceonce = 0;

    void Awake()
    {
        moduleId = moduleIdCounter++;
        stage = 1;
        moduleSolved = false;
        foreach(KMSelectable obj in buttons){
            KMSelectable pressed = obj;
            pressed.OnInteract += delegate () { PressButton(pressed); return false; };
        }
    }

    void Start () {
        posslide1 = slider1.transform.localPosition;
        posslide2 = slider2.transform.localPosition;
        posslide3 = slider3.transform.localPosition;
        currentord = 0;
        if(stage == 1)
        {
            stage1led.material = ledoptions[6];
            stage2led.material = ledoptions[7];
        }
        else
        {
            stage1led.material = ledoptions[7];
            stage2led.material = ledoptions[6];
            audio.PlaySoundAtTransform("startup", transform);
        }
        pressonce = false;
        pressonce2 = true;
        randomizeMessage();
        randomizeLEDS();
        checkReverse();
        Debug.LogFormat("[Transmitted Morse #{0}] <Stage {1}> The Final Message is '{2}'", moduleId, stage, message);
        sliders = new int[message.Length];
        positions = new int[message.Length];
        calculateSlidersAndPositions();
        logSlidersAndPositions();
    }

    void PressButton(KMSelectable pressed)
    {
        if(moduleSolved != true)
        {
            if (pressed == buttons[0] && pressonce == false)
            {
                audio.PlayGameSoundAtTransform(KMSoundOverride.SoundEffect.ButtonPress, transform);
                buttons[0].transform.Translate(new Vector3(0.0F, 0.0F, -0.005F));
                pressonce = true;
                pressonce2 = false;
                cour = playSound();
                StartCoroutine(cour);
                courrunning = true;
            }
            else if (pressed == buttons[1] && pressonce2 == false)
            {
                audio.PlayGameSoundAtTransform(KMSoundOverride.SoundEffect.ButtonPress, transform);
                buttons[0].transform.Translate(new Vector3(0.0F, 0.0F, 0.005F));
                pressonce = false;
                pressonce2 = true;
                stopSound();
                courrunning = false;
            }
            else if (pressed == buttons[2])
            {
                audio.PlayGameSoundAtTransform(KMSoundOverride.SoundEffect.TypewriterKey, transform);
                currentord = 0;
                Debug.LogFormat("[Transmitted Morse #{0}] <Stage {1}> Deleted Stored Inputs (Reset Pressed), Please Start Inputs from the Beginning", moduleId, stage);
                slider1butdisp.GetComponentInChildren<TextMesh>().text = "1";
                slider2butdisp.GetComponentInChildren<TextMesh>().text = "1";
                slider3butdisp.GetComponentInChildren<TextMesh>().text = "1";
                slider1.transform.localPosition = posslide1;
                slider2.transform.localPosition = posslide2;
                slider3.transform.localPosition = posslide3;
            }
            else if (pressed == buttons[3] || pressed == buttons[4] || pressed == buttons[5])
            {
                pressed.AddInteractionPunch(0.5f);
                audio.PlayGameSoundAtTransform(KMSoundOverride.SoundEffect.ButtonPress, transform);
                bool pass1 = false;
                bool pass2 = false;
                if(pressed == buttons[3] && sliders[currentord] == 1)
                {
                    pass1 = true;
                }else if (pressed == buttons[4] && sliders[currentord] == 2)
                {
                    pass1 = true;
                }else if (pressed == buttons[5] && sliders[currentord] == 3)
                {
                    pass1 = true;
                }
                if (pressed == buttons[3])
                {
                    int temp;
                    int.TryParse(slider1butdisp.GetComponentInChildren<TextMesh>().text, out temp);
                    if (temp == positions[currentord])
                    {
                        pass2 = true;
                    }
                }else if (pressed == buttons[4])
                {
                    int temp;
                    int.TryParse(slider2butdisp.GetComponentInChildren<TextMesh>().text, out temp);
                    if (temp == positions[currentord])
                    {
                        pass2 = true;
                    }
                }
                else if (pressed == buttons[5])
                {
                    int temp;
                    int.TryParse(slider3butdisp.GetComponentInChildren<TextMesh>().text, out temp);
                    if (temp == positions[currentord])
                    {
                        pass2 = true;
                    }
                }
                if (pass1 == true && pass2 == true)
                {
                    currentord++;
                    Debug.LogFormat("[Transmitted Morse #{0}] <Stage {1}> That Slider and Position Input was Correct", moduleId, stage);
                }
                else
                {
                    Debug.LogFormat("[Transmitted Morse #{0}] <Stage {1}> Incorrect Slider or Position Press, try again!", moduleId, stage);
                    GetComponent<KMBombModule>().HandleStrike();
                    slider1butdisp.GetComponentInChildren<TextMesh>().text = "1";
                    slider2butdisp.GetComponentInChildren<TextMesh>().text = "1";
                    slider3butdisp.GetComponentInChildren<TextMesh>().text = "1";
                    slider1.transform.localPosition = posslide1;
                    slider2.transform.localPosition = posslide2;
                    slider3.transform.localPosition = posslide3;
                }
                if (pass1 == true && pass2 == true && currentord == message.Length)
                {
                    if(stage == 1)
                    {
                        audio.PlaySoundAtTransform("shutdown", transform);
                        if (courrunning == true)
                        {
                            StopCoroutine(cour);
                        }
                        if(pressonce == true)
                        {
                            buttons[0].transform.Translate(new Vector3(0.0F, 0.0F, 0.005F));
                        }
                        stage++;
                        Invoke("Start", 3);
                        Invoke("shutdownLEDS", 0.5F);
                    }
                    else
                    {
                        audio.PlaySoundAtTransform("shutdown", transform);
                        GetComponent<KMBombModule>().HandlePass();
                        if(courrunning == true)
                        {
                            StopCoroutine(cour);
                        }
                        if (pressonce == true)
                        {
                            buttons[0].transform.Translate(new Vector3(0.0F, 0.0F, 0.005F));
                        }
                        moduleSolved = true;
                        Invoke("shutdownLEDS", 0.5F);
                    }
                }
            }
            else if (pressed == buttons[6])
            {
                audio.PlayGameSoundAtTransform(KMSoundOverride.SoundEffect.ButtonPress, transform);
                pressed.AddInteractionPunch(0.25f);
                if (!slider1butdisp.GetComponentInChildren<TextMesh>().text.Equals("1"))
                {
                    int temp = 0;
                    int.TryParse(slider1butdisp.GetComponentInChildren<TextMesh>().text, out temp);
                    temp--;
                    slider1butdisp.GetComponentInChildren<TextMesh>().text = ""+temp;
                    slider1.transform.Translate(new Vector3(-0.0033F, 0.0F, 0.0F));
                }
            }else if (pressed == buttons[7])
            {
                audio.PlayGameSoundAtTransform(KMSoundOverride.SoundEffect.ButtonPress, transform);
                pressed.AddInteractionPunch(0.25f);
                if (!slider1butdisp.GetComponentInChildren<TextMesh>().text.Equals("20"))
                {
                    int temp = 0;
                    int.TryParse(slider1butdisp.GetComponentInChildren<TextMesh>().text, out temp);
                    temp++;
                    slider1butdisp.GetComponentInChildren<TextMesh>().text = "" + temp;
                    slider1.transform.Translate(new Vector3(0.0033F, 0.0F, 0.0F));
                }
            }else if (pressed == buttons[8])
            {
                audio.PlayGameSoundAtTransform(KMSoundOverride.SoundEffect.ButtonPress, transform);
                pressed.AddInteractionPunch(0.25f);
                if (!slider2butdisp.GetComponentInChildren<TextMesh>().text.Equals("1"))
                {
                    int temp = 0;
                    int.TryParse(slider2butdisp.GetComponentInChildren<TextMesh>().text, out temp);
                    temp--;
                    slider2butdisp.GetComponentInChildren<TextMesh>().text = "" + temp;
                    slider2.transform.Translate(new Vector3(-0.0033F, 0.0F, 0.0F));
                }
            }
            else if (pressed == buttons[9])
            {
                audio.PlayGameSoundAtTransform(KMSoundOverride.SoundEffect.ButtonPress, transform);
                pressed.AddInteractionPunch(0.25f);
                if (!slider2butdisp.GetComponentInChildren<TextMesh>().text.Equals("20"))
                {
                    int temp = 0;
                    int.TryParse(slider2butdisp.GetComponentInChildren<TextMesh>().text, out temp);
                    temp++;
                    slider2butdisp.GetComponentInChildren<TextMesh>().text = "" + temp;
                    slider2.transform.Translate(new Vector3(0.0033F, 0.0F, 0.0F));
                }
            }else if (pressed == buttons[10])
            {
                audio.PlayGameSoundAtTransform(KMSoundOverride.SoundEffect.ButtonPress, transform);
                pressed.AddInteractionPunch(0.25f);
                if (!slider3butdisp.GetComponentInChildren<TextMesh>().text.Equals("1"))
                {
                    int temp = 0;
                    int.TryParse(slider3butdisp.GetComponentInChildren<TextMesh>().text, out temp);
                    temp--;
                    slider3butdisp.GetComponentInChildren<TextMesh>().text = "" + temp;
                    slider3.transform.Translate(new Vector3(-0.0033F, 0.0F, 0.0F));
                }
            }
            else if (pressed == buttons[11])
            {
                audio.PlayGameSoundAtTransform(KMSoundOverride.SoundEffect.ButtonPress, transform);
                pressed.AddInteractionPunch(0.25f);
                if (!slider3butdisp.GetComponentInChildren<TextMesh>().text.Equals("20"))
                {
                    int temp = 0;
                    int.TryParse(slider3butdisp.GetComponentInChildren<TextMesh>().text, out temp);
                    temp++;
                    slider3butdisp.GetComponentInChildren<TextMesh>().text = "" + temp;
                    slider3.transform.Translate(new Vector3(0.0033F, 0.0F, 0.0F));
                }
            }
        }
    }

    private void shutdownLEDS()
    {
        tled.material = ledoptions[7];
        bled.material = ledoptions[7];
        stage1led.material = ledoptions[7];
        stage2led.material = ledoptions[7];
        slider1butdisp.GetComponentInChildren<TextMesh>().text = "1";
        slider2butdisp.GetComponentInChildren<TextMesh>().text = "1";
        slider3butdisp.GetComponentInChildren<TextMesh>().text = "1";
        slider1.transform.localPosition = posslide1;
        slider2.transform.localPosition = posslide2;
        slider3.transform.localPosition = posslide3;
    }

    private void randomizeMessage()
    {
        int mess = Random.RandomRange(0,44);
        messagetrans = messages[mess];
        Debug.LogFormat("[Transmitted Morse #{0}] <Stage {1}> The Transmitted Message is '{2}'", moduleId, stage, messages[mess]);
        if (mess > 38 && mess <= 43 && mess != 40)
        {
            message = messages[45];
            Debug.LogFormat("[Transmitted Morse #{0}] <Stage {1}> Message is not in the table without vowel start, using new message 'UNLUCKY'", moduleId, stage);
        }
        else if(mess == 40)
        {
            message = messages[44];
            Debug.LogFormat("[Transmitted Morse #{0}] <Stage {1}> Message is not in the table with vowel start, using new message 'CODERED'", moduleId, stage);
        }
        else
        {
            message = messages[mess];
            Debug.LogFormat("[Transmitted Morse #{0}] <Stage {1}> Message is in the table, using transmitted message as new message", moduleId, stage);
        }
    }

    private void randomizeLEDS()
    {
        int led1 = Random.RandomRange(0, 7);
        int led2 = Random.RandomRange(0, 7);
        switch (led1)
        {
            case 0: topLED = "yellow";
                tled.material = ledoptions[0];
                break;
            case 1:
                topLED = "blue";
                tled.material = ledoptions[1];
                break;
            case 2:
                topLED = "red";
                tled.material = ledoptions[2];
                break;
            case 3:
                topLED = "green";
                tled.material = ledoptions[3];
                break;
            case 4:
                topLED = "pink";
                tled.material = ledoptions[4];
                break;
            case 5:
                topLED = "orange";
                tled.material = ledoptions[5];
                break;
            case 6:
                topLED = "white";
                tled.material = ledoptions[6];
                break;
            default: break;
        }
        Debug.LogFormat("[Transmitted Morse #{0}] <Stage {1}> The color of the Top LED is {2}", moduleId, stage, topLED);
        switch (led2)
        {
            case 0:
                bottomLED = "yellow";
                bled.material = ledoptions[0];
                break;
            case 1:
                bottomLED = "blue";
                bled.material = ledoptions[1];
                break;
            case 2:
                bottomLED = "red";
                bled.material = ledoptions[2];
                break;
            case 3:
                bottomLED = "green";
                bled.material = ledoptions[3];
                break;
            case 4:
                bottomLED = "pink";
                bled.material = ledoptions[4];
                break;
            case 5:
                bottomLED = "orange";
                bled.material = ledoptions[5];
                break;
            case 6:
                bottomLED = "white";
                bled.material = ledoptions[6];
                break;
            default: break;
        }
        Debug.LogFormat("[Transmitted Morse #{0}] <Stage {1}> The color of the Bottom LED is {2}", moduleId, stage, bottomLED);
    }

    private void checkReverse()
    {
        if((topLED.Equals("red") || topLED.Equals("pink")) && (bottomLED.Equals("yellow") || bottomLED.Equals("blue")))
        {
            Debug.LogFormat("[Transmitted Morse #{0}] <Stage {1}> Due to the color of the LEDs, the message must be reversed", moduleId, stage);
            reverseMessage();
        }
        else
        {
            Debug.LogFormat("[Transmitted Morse #{0}] <Stage {1}> Due to the color of the LEDs, the message must NOT be reversed", moduleId, stage);
        }
    }

    private void reverseMessage()
    {
        char[] messagechars = message.ToCharArray();
        string tempmessage = "";
        for(int i = messagechars.Length-1; i >= 0; i--)
        {
            tempmessage += messagechars[i];
        }
        message = tempmessage;
    }

    private void calculateSlidersAndPositions()
    {
        char[] chars = message.ToCharArray();
        int pos = 0;
        for(int posi = 0; posi < chars.Length; posi++)
        {
            if(topLED.Equals("orange") || topLED.Equals("white"))
            {
                if (charIsDigit(chars[posi]))
                {
                    sliders[posi] = 1;
                    int.TryParse(("" + chars[posi]), out pos);
                }
                else
                {
                    pos = char.ToUpper(chars[posi]) - 64;
                    if (pos >= 1 && pos <= 7)
                    {
                        sliders[posi] = 2;
                    }
                    else if (pos >= 8 && pos <= 13)
                    {
                        sliders[posi] = 3;
                    }
                    else if (pos >= 14 && pos <= 26)
                    {
                        sliders[posi] = 1;
                    }
                    if (pos > 20)
                    {
                        pos = pos % 20;
                    }
                }
                positions[posi] = pos;
            }else if (bottomLED.Equals("orange") || bottomLED.Equals("yellow"))
            {
                if (charIsDigit(chars[posi]))
                {
                    sliders[posi] = 2;
                    int.TryParse(("" + chars[posi]), out pos);
                }
                else
                {
                    pos = char.ToUpper(chars[posi]) - 64;
                    if (pos >= 1 && pos <= 7)
                    {
                        sliders[posi] = 1;
                    }
                    else if (pos >= 8 && pos <= 13)
                    {
                        sliders[posi] = 2;
                    }
                    else if (pos >= 14 && pos <= 26)
                    {
                        sliders[posi] = 3;
                    }
                    if (pos > 20)
                    {
                        pos = pos % 20;
                    }
                }
                positions[posi] = pos;
            }
            else
            {
                if (charIsDigit(chars[posi]))
                {
                    sliders[posi] = 3;
                    int.TryParse(("" + chars[posi]), out pos);
                }
                else
                {
                    pos = char.ToUpper(chars[posi]) - 64;
                    if (pos >= 1 && pos <= 7)
                    {
                        sliders[posi] = 3;
                    }
                    else if (pos >= 8 && pos <= 13)
                    {
                        sliders[posi] = 2;
                    }
                    else if (pos >= 14 && pos <= 26)
                    {
                        sliders[posi] = 1;
                    }
                    if (pos > 20)
                    {
                        pos = pos % 20;
                    }
                }
                positions[posi] = pos;
            }
        }
    }

    private void logSlidersAndPositions()
    {
        string mess = "[Transmitted Morse #{0}] <Stage {1}> The Correct Order of Inputs is:";
        for(int i = 0; i < sliders.Length; i++)
        {
            if(i == sliders.Length - 1)
            {
                mess += " Slider " + sliders[i] + " to Position " + positions[i];
            }
            else
            {
                mess += " Slider " + sliders[i] + " to Position " + positions[i] + ",";
            }
        }
        Debug.LogFormat(mess, moduleId, stage);
    }

    private IEnumerator playSound()
    {
        while (true)
        {
            if (messagetrans.Equals("BOMBS"))
            {
                audio.PlaySoundAtTransform("bombs", transform);
                yield return new WaitForSeconds(12.0F);
            }else if (messagetrans.Equals("SHORT"))
            {
                audio.PlaySoundAtTransform("short", transform);
                yield return new WaitForSeconds(12.0F);
            }
            else if (messagetrans.Equals("UNDERSTOOD"))
            {
                audio.PlaySoundAtTransform("understood", transform);
                yield return new WaitForSeconds(22.0F);
            }
            else if (messagetrans.Equals("W1RES"))
            {
                audio.PlaySoundAtTransform("w1res", transform);
                yield return new WaitForSeconds(13.0F);
            }
            else if (messagetrans.Equals("SOS"))
            {
                audio.PlaySoundAtTransform("sos", transform);
                yield return new WaitForSeconds(8.0F);
            }
            else if (messagetrans.Equals("MANUAL"))
            {
                audio.PlaySoundAtTransform("manual", transform);
                yield return new WaitForSeconds(14.0F);
            }
            else if (messagetrans.Equals("STRIKED"))
            {
                audio.PlaySoundAtTransform("striked", transform);
                yield return new WaitForSeconds(15.0F);
            }
            else if (messagetrans.Equals("WEREDEAD"))
            {
                audio.PlaySoundAtTransform("weredead", transform);
                yield return new WaitForSeconds(17.0F);
            }
            else if (messagetrans.Equals("GOTASOUV"))
            {
                audio.PlaySoundAtTransform("gotasouv", transform);
                yield return new WaitForSeconds(19.0F);
            }
            else if (messagetrans.Equals("EXPLOSION"))
            {
                audio.PlaySoundAtTransform("explosion", transform);
                yield return new WaitForSeconds(21.0F);
            }
            else if (messagetrans.Equals("EXPERT"))
            {
                audio.PlaySoundAtTransform("expert", transform);
                yield return new WaitForSeconds(14.0F);
            }
            else if (messagetrans.Equals("RIP"))
            {
                audio.PlaySoundAtTransform("rip", transform);
                yield return new WaitForSeconds(8.0F);
            }
            else if (messagetrans.Equals("LISTEN"))
            {
                audio.PlaySoundAtTransform("listen", transform);
                yield return new WaitForSeconds(13.0F);
            }
            else if (messagetrans.Equals("DETONATE"))
            {
                audio.PlaySoundAtTransform("detonate", transform);
                yield return new WaitForSeconds(17.0F);
            }
            else if (messagetrans.Equals("ROGER"))
            {
                audio.PlaySoundAtTransform("roger", transform);
                yield return new WaitForSeconds(12.0F);
            }
            else if (messagetrans.Equals("WELOSTBRO"))
            {
                audio.PlaySoundAtTransform("welostbro", transform);
                yield return new WaitForSeconds(20.0F);
            }
            else if (messagetrans.Equals("AMIDEAF"))
            {
                audio.PlaySoundAtTransform("amideaf", transform);
                yield return new WaitForSeconds(15.0F);
            }
            else if (messagetrans.Equals("KEYPAD"))
            {
                audio.PlaySoundAtTransform("keypad", transform);
                yield return new WaitForSeconds(14.0F);
            }
            else if (messagetrans.Equals("DEFUSER"))
            {
                audio.PlaySoundAtTransform("defuser", transform);
                yield return new WaitForSeconds(15.0F);
            }
            else if (messagetrans.Equals("NUCLEARWEAPONS"))
            {
                audio.PlaySoundAtTransform("nuclearweapons", transform);
                yield return new WaitForSeconds(30.0F);
            }
            else if (messagetrans.Equals("KAPPA"))
            {
                audio.PlaySoundAtTransform("kappa", transform);
                yield return new WaitForSeconds(12.0F);
            }
            else if (messagetrans.Equals("DELTA"))
            {
                audio.PlaySoundAtTransform("delta", transform);
                yield return new WaitForSeconds(11.0F);
            }
            else if (messagetrans.Equals("PI3"))
            {
                audio.PlaySoundAtTransform("pi3", transform);
                yield return new WaitForSeconds(8.0F);
            }
            else if (messagetrans.Equals("SMOKE"))
            {
                audio.PlaySoundAtTransform("smoke", transform);
                yield return new WaitForSeconds(12.0F);
            }
            else if (messagetrans.Equals("SENDHELP"))
            {
                audio.PlaySoundAtTransform("sendhelp", transform);
                yield return new WaitForSeconds(17.0F);
            }
            else if (messagetrans.Equals("LOST"))
            {
                audio.PlaySoundAtTransform("lost", transform);
                yield return new WaitForSeconds(10.0F);
            }
            else if (messagetrans.Equals("SWAN"))
            {
                audio.PlaySoundAtTransform("swan", transform);
                yield return new WaitForSeconds(9.0F);
            }
            else if (messagetrans.Equals("NOMNOM"))
            {
                audio.PlaySoundAtTransform("nomnom", transform);
                yield return new WaitForSeconds(14.0F);
            }
            else if (messagetrans.Equals("BLUE"))
            {
                audio.PlaySoundAtTransform("blue", transform);
                yield return new WaitForSeconds(10.0F);
            }
            else if (messagetrans.Equals("BOOM"))
            {
                audio.PlaySoundAtTransform("boom", transform);
                yield return new WaitForSeconds(11.0F);
            }
            else if (messagetrans.Equals("CANCEL"))
            {
                audio.PlaySoundAtTransform("cancel", transform);
                yield return new WaitForSeconds(14.0F);
            }
            else if (messagetrans.Equals("DEFUSED"))
            {
                audio.PlaySoundAtTransform("defused", transform);
                yield return new WaitForSeconds(15.0F);
            }
            else if (messagetrans.Equals("BROKEN"))
            {
                audio.PlaySoundAtTransform("broken", transform);
                yield return new WaitForSeconds(14.0F);
            }
            else if (messagetrans.Equals("MEMORY"))
            {
                audio.PlaySoundAtTransform("memory", transform);
                yield return new WaitForSeconds(14.0F);
            }
            else if (messagetrans.Equals("R6S8T"))
            {
                audio.PlaySoundAtTransform("r6s8t", transform);
                yield return new WaitForSeconds(12.0F);
            }
            else if (messagetrans.Equals("TRANSMISSION"))
            {
                audio.PlaySoundAtTransform("transmission", transform);
                yield return new WaitForSeconds(24.0F);
            }
            else if (messagetrans.Equals("UMWHAT"))
            {
                audio.PlaySoundAtTransform("umwhat", transform);
                yield return new WaitForSeconds(13.0F);
            }
            else if (messagetrans.Equals("GREEN"))
            {
                audio.PlaySoundAtTransform("green", transform);
                yield return new WaitForSeconds(11.0F);
            }
            else if (messagetrans.Equals("EQUATIONSX"))
            {
                audio.PlaySoundAtTransform("equationsx", transform);
                yield return new WaitForSeconds(21.0F);
            }
            else if (messagetrans.Equals("RED"))
            {
                audio.PlaySoundAtTransform("red", transform);
                yield return new WaitForSeconds(7.0F);
            }
            else if (messagetrans.Equals("ENERGY"))
            {
                audio.PlaySoundAtTransform("energy", transform);
                yield return new WaitForSeconds(13.0F);
            }
            else if (messagetrans.Equals("JESTER"))
            {
                audio.PlaySoundAtTransform("jester", transform);
                yield return new WaitForSeconds(13.0F);
            }
            else if (messagetrans.Equals("CONTACT"))
            {
                audio.PlaySoundAtTransform("contact", transform);
                yield return new WaitForSeconds(16.0F);
            }
            else if (messagetrans.Equals("LONG"))
            {
                audio.PlaySoundAtTransform("long", transform);
                yield return new WaitForSeconds(10.0F);
            }
            else
            {
                break;
            }
        }
    }

    private void stopSound()
    {
        StopCoroutine(cour);
    }

    private int getIndicatorCount()
    {
        int tempcount = 0;
        if (bomb.IsIndicatorOn("NSA"))
        {
            tempcount++;
        }
        if (bomb.IsIndicatorOff("NSA"))
        {
            tempcount++;
        }
        if (bomb.IsIndicatorOn("MSA"))
        {
            tempcount++;
        }
        if (bomb.IsIndicatorOff("MSA"))
        {
            tempcount++;
        }
        if (bomb.IsIndicatorOn("CAR"))
        {
            tempcount++;
        }
        if (bomb.IsIndicatorOff("CAR"))
        {
            tempcount++;
        }
        if (bomb.IsIndicatorOn("SND"))
        {
            tempcount++;
        }
        if (bomb.IsIndicatorOff("SND"))
        {
            tempcount++;
        }
        if (bomb.IsIndicatorOn("IND"))
        {
            tempcount++;
        }
        if (bomb.IsIndicatorOff("IND"))
        {
            tempcount++;
        }
        if (bomb.IsIndicatorOn("CLR"))
        {
            tempcount++;
        }
        if (bomb.IsIndicatorOff("CLR"))
        {
            tempcount++;
        }
        if (bomb.IsIndicatorOn("SIG"))
        {
            tempcount++;
        }
        if (bomb.IsIndicatorOff("SIG"))
        {
            tempcount++;
        }
        if (bomb.IsIndicatorOn("TRN"))
        {
            tempcount++;
        }
        if (bomb.IsIndicatorOff("TRN"))
        {
            tempcount++;
        }
        if (bomb.IsIndicatorOn("FRQ"))
        {
            tempcount++;
        }
        if (bomb.IsIndicatorOff("FRQ"))
        {
            tempcount++;
        }
        if (bomb.IsIndicatorOn("FRK"))
        {
            tempcount++;
        }
        if (bomb.IsIndicatorOff("FRK"))
        {
            tempcount++;
        }
        if (bomb.IsIndicatorOn("BOB"))
        {
            tempcount++;
        }
        if (bomb.IsIndicatorOff("BOB"))
        {
            tempcount++;
        }
        return tempcount;
    }

    private int getWidgetCount()
    {
        int tempcount = 0;
        tempcount += bomb.GetBatteryHolderCount();
        tempcount += bomb.GetPortPlateCount();
        tempcount += getIndicatorCount();
        return tempcount;
    }

    private bool bombHasModule(string name)
    {
        List<string> modules = bomb.GetModuleNames();
        foreach(string mod in modules)
        {
            if (mod.EqualsIgnoreCase(name))
            {
                return true;
            }
        }
        return false;
    }

    private bool charIsDigit(char s)
    {
        if (s.Equals('1') || s.Equals('3') || s.Equals('6') || s.Equals('8'))
        {
            return true;
        }
        else
        {
            return false;
        }
    }

    private bool stringIsDigit(string s)
    {
        int temp = 0;
        int.TryParse(s, out temp);
        if(temp != 0)
        {
            return true;
        }
        else
        {
            return false;
        }
    }

    private bool serialHasOdd()
    {
        string serial = bomb.GetSerialNumber();
        if (serial.Contains("1") || serial.Contains("3") || serial.Contains("5") || serial.Contains("7") || serial.Contains("9"))
        {
            return true;
        }
        return false;
    }

    private bool serialHasVowels()
    {
        string serial = bomb.GetSerialNumber();
        if (serial.Contains("A") || serial.Contains("E") || serial.Contains("I") || serial.Contains("O") || serial.Contains("U"))
        {
            return true;
        }
        return false;
    }

    private bool isAnyPlateEmpty()
    {
        foreach(string[] plateports in bomb.GetPortPlates())
        {
            if(plateports.Length == 0)
            {
                return true;
            }
        }
        return false;
    }

    //twitch plays
    private bool inputIsValid(string cmd)
    {
        char[] validchars = {' ',',',';','0','1','2','3','4','5','6','7','8','9'};
        char[] cmdchars = cmd.ToCharArray();
        foreach(char c in cmdchars)
        {
            if (!validchars.Contains(c))
            {
                return false;
            }
        }
        string[] parameters = cmd.Split(' ',';',',');
        int counter = 0;
        foreach(string str in parameters)
        {
            if (!stringIsDigit(str))
            {
                return false;
            }
            else
            {
                if (counter == 0)
                {
                    int temp = 0;
                    int.TryParse(str, out temp);
                    if(!((temp >= 1) && (temp <= 3)))
                    {
                        return false;
                    }
                    else
                    {
                        counter = 1;
                    }
                }else if (counter == 1)
                {
                    int temp = 0;
                    int.TryParse(str, out temp);
                    if (!((temp >= 1) && (temp <= 20)))
                    {
                        return false;
                    }
                    else
                    {
                        counter = 0;
                    }
                }
            }
        }
        return true;
    }

    #pragma warning disable 414
    private readonly string TwitchHelpMessage = @"!{0} start [Starts the looping morse message] | !{0} stop [Stops the looping morse message] | !{0} 1 13 [Moves the first slider to the position of 13 and inputs it] | !{0} 1 13;3 2;2 8 [Example of Slider Input Chain Sequence] | !{0} reset [Resets the inputted sequence back to the beginning]";
    #pragma warning restore 414

    IEnumerator ProcessTwitchCommand(string command)
    {
        if (Regex.IsMatch(command, @"^\s*start\s*$", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant))
        {
            yield return null;
            yield return new[] { buttons[0] };
            yield break;
        }
        if (Regex.IsMatch(command, @"^\s*stop\s*$", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant))
        {
            yield return null;
            yield return new[] { buttons[1] };
            yield break;
        }
        if (Regex.IsMatch(command, @"^\s*reset\s*$", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant))
        {
            yield return null;
            yield return new[] { buttons[2] };
            yield break;
        }
        if (inputIsValid(command))
        {
            yield return null;
            string[] parameters = command.Split(' ',';',',');
            for (int j = 0; j < parameters.Length; j += 2)
            {
                int tester1 = 0;
                int tester2 = 0;
                int.TryParse(parameters[j], out tester1);
                int.TryParse(parameters[j + 1], out tester2);
                if (tester1 == 1)
                {
                    int temp = tester2;
                    int temp2;
                    int.TryParse(slider1butdisp.GetComponentInChildren<TextMesh>().text, out temp2);
                    int moveamt = temp - temp2;
                    if (moveamt < 0)
                    {
                        moveamt = Mathf.Abs(moveamt);
                        for (int i = 0; i < moveamt; i++)
                        {
                            yield return new[] { buttons[6] };
                        }
                        yield return new[] { buttons[3] };
                    }
                    else if (moveamt == 0)
                    {
                        yield return new[] { buttons[3] };
                    }
                    else if (moveamt > 0)
                    {
                        for (int i = 0; i < moveamt; i++)
                        {
                            yield return new[] { buttons[7] };
                        }
                        yield return new[] { buttons[3] };
                    }
                }
                else if (tester1 == 2)
                {
                    int temp = tester2;
                    int temp2;
                    int.TryParse(slider2butdisp.GetComponentInChildren<TextMesh>().text, out temp2);
                    int moveamt = temp - temp2;
                    if (moveamt < 0)
                    {
                        moveamt = Mathf.Abs(moveamt);
                        for (int i = 0; i < moveamt; i++)
                        {
                            yield return new[] { buttons[8] };
                        }
                        yield return new[] { buttons[4] };
                    }
                    else if (moveamt == 0)
                    {
                        yield return new[] { buttons[4] };
                    }
                    else if (moveamt > 0)
                    {
                        for (int i = 0; i < moveamt; i++)
                        {
                            yield return new[] { buttons[9] };
                        }
                        yield return new[] { buttons[4] };
                    }
                }
                else if (tester1 == 3)
                {
                    int temp = tester2;
                    int temp2;
                    int.TryParse(slider3butdisp.GetComponentInChildren<TextMesh>().text, out temp2);
                    int moveamt = temp - temp2;
                    if (moveamt < 0)
                    {
                        moveamt = Mathf.Abs(moveamt);
                        for (int i = 0; i < moveamt; i++)
                        {
                            yield return new[] { buttons[10] };
                        }
                        yield return new[] { buttons[5] };
                    }
                    else if (moveamt == 0)
                    {
                        yield return new[] { buttons[5] };
                    }
                    else if (moveamt > 0)
                    {
                        for (int i = 0; i < moveamt; i++)
                        {
                            yield return new[] { buttons[11] };
                        }
                        yield return new[] { buttons[5] };
                    }
                }
            }
        }
        else
        {
            yield break;
        }
    }

    /*IEnumerator ProcessTwitchCommand(string command)
    {
        if (command.Contains(","))
        {
            string[] parameters = command.Split(' ', ',');
            for (int j = 0; j < parameters.Length; j++)
            {
                yield return null;
                if (parameters[j].Equals("slider"))
                {
                    if (!stringIsDigit(parameters[j + 2]) || !stringIsDigit(parameters[j + 1]))
                    {
                        break;
                    }
                    if (parameters[j + 1].Equals("1"))
                    {
                        int temp;
                        int temp2;
                        int.TryParse(parameters[j + 2], out temp);
                        int.TryParse(slider1butdisp.GetComponentInChildren<TextMesh>().text, out temp2);
                        int moveamt = temp - temp2;
                        if (moveamt < 0)
                        {
                            moveamt = Mathf.Abs(moveamt);
                            for (int i = 0; i < moveamt; i++)
                            {
                                buttons[6].OnInteract();
                                yield return new WaitForSeconds(.1f);
                            }
                            buttons[3].OnInteract();
                        }
                        else if (moveamt == 0)
                        {
                            buttons[3].OnInteract();
                        }
                        else
                        {
                            for (int i = 0; i < moveamt; i++)
                            {
                                buttons[7].OnInteract();
                                yield return new WaitForSeconds(.1f);
                            }
                            buttons[3].OnInteract();
                        }
                    }
                    else if (parameters[j + 1].Equals("2"))
                    {
                        int temp;
                        int temp2;
                        int.TryParse(parameters[j + 2], out temp);
                        int.TryParse(slider2butdisp.GetComponentInChildren<TextMesh>().text, out temp2);
                        int moveamt = temp - temp2;
                        if (moveamt < 0)
                        {
                            moveamt = Mathf.Abs(moveamt);
                            for (int i = 0; i < moveamt; i++)
                            {
                                buttons[8].OnInteract();
                                yield return new WaitForSeconds(.1f);
                            }
                            buttons[4].OnInteract();
                        }
                        else if (moveamt == 0)
                        {
                            buttons[4].OnInteract();
                        }
                        else
                        {
                            for (int i = 0; i < moveamt; i++)
                            {
                                buttons[9].OnInteract();
                                yield return new WaitForSeconds(.1f);
                            }
                            buttons[4].OnInteract();
                        }
                    }
                    else if (parameters[j + 1].Equals("3"))
                    {
                        int temp;
                        int temp2;
                        int.TryParse(parameters[j + 2], out temp);
                        int.TryParse(slider3butdisp.GetComponentInChildren<TextMesh>().text, out temp2);
                        int moveamt = temp - temp2;
                        if (moveamt < 0)
                        {
                            moveamt = Mathf.Abs(moveamt);
                            for (int i = 0; i < moveamt; i++)
                            {
                                buttons[10].OnInteract();
                                yield return new WaitForSeconds(.1f);
                            }
                            buttons[5].OnInteract();
                        }
                        else if (moveamt == 0)
                        {
                            buttons[5].OnInteract();
                        }
                        else
                        {
                            for (int i = 0; i < moveamt; i++)
                            {
                                buttons[11].OnInteract();
                                yield return new WaitForSeconds(.1f);
                            }
                            buttons[5].OnInteract();
                        }
                    }
                    yield return new WaitForSeconds(.1f);
                }
            }
        }
        else
        {
            string[] parameters = command.Split(' ');
            foreach (string param in parameters)
            {
                yield return null;
                if (param.Equals("start"))
                {
                    buttons[0].OnInteract();
                    yield return new WaitForSeconds(.1f);
                    break;
                }
                else if (param.Equals("stop"))
                {
                    buttons[1].OnInteract();
                    yield return new WaitForSeconds(.1f);
                    break;
                }
                else if (param.Equals("reset"))
                {
                    buttons[2].OnInteract();
                    yield return new WaitForSeconds(.1f);
                    break;
                }
                else if (param.EqualsIgnoreCase("slider"))
                {
                    if (!stringIsDigit(parameters[2]) || !stringIsDigit(parameters[1]))
                    {
                        break;
                    }
                    if (parameters[1].Equals("1"))
                    {
                        int temp;
                        int temp2;
                        int.TryParse(parameters[2], out temp);
                        int.TryParse(slider1butdisp.GetComponentInChildren<TextMesh>().text, out temp2);
                        int moveamt = temp - temp2;
                        if (moveamt < 0)
                        {
                            moveamt = Mathf.Abs(moveamt);
                            for (int i = 0; i < moveamt; i++)
                            {
                                buttons[6].OnInteract();
                                yield return new WaitForSeconds(.1f);
                            }
                            buttons[3].OnInteract();
                        }
                        else if (moveamt == 0)
                        {
                            buttons[3].OnInteract();
                        }
                        else
                        {
                            for (int i = 0; i < moveamt; i++)
                            {
                                buttons[7].OnInteract();
                                yield return new WaitForSeconds(.1f);
                            }
                            buttons[3].OnInteract();
                        }
                    }
                    else if (parameters[1].Equals("2"))
                    {
                        int temp;
                        int temp2;
                        int.TryParse(parameters[2], out temp);
                        int.TryParse(slider2butdisp.GetComponentInChildren<TextMesh>().text, out temp2);
                        int moveamt = temp - temp2;
                        if (moveamt < 0)
                        {
                            moveamt = Mathf.Abs(moveamt);
                            for (int i = 0; i < moveamt; i++)
                            {
                                buttons[8].OnInteract();
                                yield return new WaitForSeconds(.1f);
                            }
                            buttons[4].OnInteract();
                        }
                        else if (moveamt == 0)
                        {
                            buttons[4].OnInteract();
                        }
                        else
                        {
                            for (int i = 0; i < moveamt; i++)
                            {
                                buttons[9].OnInteract();
                                yield return new WaitForSeconds(.1f);
                            }
                            buttons[4].OnInteract();
                        }
                    }
                    else if (parameters[1].Equals("3"))
                    {
                        int temp;
                        int temp2;
                        int.TryParse(parameters[2], out temp);
                        int.TryParse(slider3butdisp.GetComponentInChildren<TextMesh>().text, out temp2);
                        int moveamt = temp - temp2;
                        if (moveamt < 0)
                        {
                            moveamt = Mathf.Abs(moveamt);
                            for (int i = 0; i < moveamt; i++)
                            {
                                buttons[10].OnInteract();
                                yield return new WaitForSeconds(.1f);
                            }
                            buttons[5].OnInteract();
                        }
                        else if (moveamt == 0)
                        {
                            buttons[5].OnInteract();
                        }
                        else
                        {
                            for (int i = 0; i < moveamt; i++)
                            {
                                buttons[11].OnInteract();
                                yield return new WaitForSeconds(.1f);
                            }
                            buttons[5].OnInteract();
                        }
                    }
                    yield return new WaitForSeconds(.1f);
                    break;
                }
                else
                {
                    break;
                }
            }
        }
    }*/
}
