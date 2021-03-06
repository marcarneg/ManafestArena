/*
	A record containing settings used for Arena gamemode.
*/
using System;
using System.Collections.Generic;

public class ArenaSettings {
	public bool useKits, usePowerups;
	public int duration; // Minutes
	public int bots;
	public List<int> botIds; 
	public List<ActorData> enemies;
	public ActorData player;


	public ArenaSettings(){
		useKits = true;
		usePowerups = true;
		duration = 5;
		bots = 0;
		botIds = new List<int>();
		enemies = new List<ActorData>();
	}

	public static ArenaSettings FromJson(string json){
		return null;//JsonConvert.DeserializeObject<ArenaSettings>(json);
	}

	public static string ToJson(ArenaSettings dat){
		return "";//JsonConvert.SerializeObject(dat, Formatting.Indented);
	}
}