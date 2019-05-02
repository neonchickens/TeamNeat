
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Data;
using System.Data.Common;
using System.Data.SqlClient;

public class Stats
{
    public int season;
    public int playerid;

    public int points;
    public int throws;
    public int assists;

    public int steals;
    public int turnovers;

    public int saves;
    
    public Stats(int season, int playerid)
    {
        this.season = season;
        this.playerid = playerid;
    }
}
