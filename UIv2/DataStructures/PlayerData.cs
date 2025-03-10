﻿using System;
using System.Collections.Generic;

namespace CeeLoPlugin.UIv2.DataStructures
{
    public class PlayerData
    {
        // This is randomly generated id just for imgui to make life easier
        internal string ID = Guid.NewGuid().ToString();
        public string NameWithWorld = "";
        // Why list? Because we want to account for tiebreakers.
        public List<int> OrderDeterminingRolls = new List<int> { 0 };
        public bool BetCollected = false;
        public int[] GameRolls = new int[] { 0, 0, 0 };
        public bool IsWinner = false;

        // Changed FinalScore from a property to a nullable int field.
        // When no score is entered, this remains null.
        public int? FinalScore = null;

        // New property: FinalScoreInput holds the string entered in the UI.
        public string FinalScoreInput = "";
    }
}
