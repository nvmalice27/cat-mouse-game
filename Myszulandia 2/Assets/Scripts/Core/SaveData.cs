using System;

[Serializable]
public class SaveData
{
    public int    coins               = 10;
    public int    dayNumber           = 1;
    public float  hunger              = 0f;
    public float  dirt                = 0f;
    public int    mouseStateValue     = 0;
    public bool[] unlockedMouseTypes  = new bool[18];
    public int    crumbsTotal         = 0;
    public int    crumbsInInventory   = 0;
    public int    socksCollected      = 0;
    public int    alarmTypeValue      = 0;
    public bool   bedMade             = false;
}
