using UnityEngine;
using System.Collections;

public class TV_Orbit : MonoBehaviour {

	public int TimeMultiplier = 200000;
	public string Name = "";
	public Transform Parent;

	public enum Orbit {Auto, Manual}
	public Orbit SetOrbit;

	public enum Rotation {Auto, Manual}
	public Rotation SetRotation;
	
	public enum Season {Auto, Manual}
	public Season SetSeason;
	
	public bool TidalLock;
	public bool LockOrbit;
	public float OrbitAngle;
	public bool KeepTime;
	private Transform ThisTransform;
	private float EarthDays = 365.242199f;
	
	// Orbit Stats
	public float OrbitalPeriod = 1.0f; // Earth Years
	public float OrbitalDistance = 2f;
	public Vector3 OrbitOffset;
	public float OrbitPosOffset;
	public float OrbitStartPos;
	public int OrbitYears;
	public int OrbitDays;
	public int OrbitHours;
	public int OrbitMinutes;
	public float OrbitSeconds;
	private float OrbitalTime;
	private float OrbitalDegSec;
	
	//Rotation Stats
	public float RotationOffset;
	public float RotationPeriod; // Earth Hours
	public int RotationYears;
	public int RotationDays;
	public int RotationHours;
	public int RotationMinutes;
	public float RotationSeconds;
	private float RotationDegSec;
	private float RotationTime;
	
	// Planetary Stats
	public float AxialTilt;
	public int HoursInDay;
	public int RotInOrbit;
	//Planet Counters
	public int CounterYear;
	public int CounterDay;
	public int CounterHour;
	public int CounterMinute;
	public float CounterSecond;
	public float CurrentOrbitPos;
	public bool OrbitOffSetYear;
	private float RotCounter;
	private float OrbCounter;
	
	//Draw Orbit
	public bool _DrawOrbit = false;
	public float DisplaySize = 0.05f;
	public Color DisplayColor = Color.blue;
	public int Segments = 100;
	public Texture2D DisplayTexture;
	public int DisplayTiling = 50;
	public bool UseTexture;
	private Transform ThisOrbit;
	private LineRenderer LR;
	
	private Transform OrbitCenter;
	
	public void Awake(){
		ThisTransform = transform;
		var ThisLEuler = ThisTransform.localEulerAngles;
		ThisLEuler.z = AxialTilt;
		ThisTransform.localEulerAngles = ThisLEuler;
		OrbitCenter = new GameObject("OrbitCenter").transform;
		OrbitCenter.position = Parent.position;
		ThisTransform.parent = OrbitCenter;
		ThisTransform.localPosition = new Vector3(0,0,OrbitalDistance);
		var OCEuler = OrbitCenter.eulerAngles;
		OCEuler.x = OrbitAngle;
		OrbitCenter.eulerAngles = OCEuler;
		OrbitCenter.Rotate(0,OrbitPosOffset,0);
		
		
		if(_DrawOrbit){
			if(GameObject.Find("Orbits") == null){
				GameObject Orbits = new GameObject("Orbits");
				Orbits.transform.position = Vector3.zero;
				Orbits.transform.eulerAngles = Vector3.zero;
			}
		}
	}
	
	public void Start(){
		SetupPlanet();
		
		if(OrbitOffSetYear){
			OrbCounter = OrbitPosOffset;
		}
		if(LockOrbit){
			KeepTime = false;
		}
		if(_DrawOrbit){
			SetupDrawOrbit();
		}
	}
	
	public void SetupDrawOrbit(){
		GameObject Orbit = new GameObject("Orbit_Path");
		GameObject Obits = GameObject.Find("Orbits");
		var OrbEA = Orbit.transform.eulerAngles;
		OrbEA.x = OrbitAngle;
		Orbit.transform.eulerAngles = OrbEA;
		Orbit.transform.parent = Obits.transform;
		Orbit.transform.position = Parent.position;
		Orbit.AddComponent<LineRenderer>();
		LR = Orbit.GetComponent<LineRenderer>();
		LR.SetWidth(DisplaySize,DisplaySize);
		LR.material.shader = Shader.Find("Particles/Additive");
		LR.material.SetColor ("_TintColor", DisplayColor);
		LR.useWorldSpace = false;
		LR.SetVertexCount (Segments + 1);
		
		if(DisplayTexture != null){
			var mTScale = LR.material.mainTextureScale;
			LR.material.mainTexture = DisplayTexture;
			mTScale.x = DisplayTiling;
			LR.material.mainTextureScale = mTScale;
		}
		
		ThisOrbit = Orbit.transform;
		
		float Angle = new float();
		for (int i = 0; i < (Segments + 1); i++){
			Vector2 NewRadius = new Vector2(Mathf.Sin (	Mathf.Deg2Rad * Angle) * OrbitalDistance, 
			                            	Mathf.Cos (Mathf.Deg2Rad * Angle) * OrbitalDistance);
			
			LR.SetPosition (i,new Vector3(NewRadius.y,0,NewRadius.x));
			Angle += (float)(360.0 / Segments);
		}
	}
	
	public void SetupPlanet(){
		//Setup Orbit Time
		if(SetOrbit == 0){
			OrbitalTime = ((((EarthDays * OrbitalPeriod) * 24) * 60) * 60);
			OrbitalDegSec = (360 / OrbitalTime) * TimeMultiplier;
		}else{
			OrbitalPeriod = 0;
			OrbitalTime = ((((((((OrbitYears * EarthDays) + OrbitDays) * 24) + OrbitHours) * 60) + OrbitMinutes) * 60) + OrbitSeconds);
			OrbitalDegSec = (360 / OrbitalTime) * TimeMultiplier;
		}
		
		//Setup Rotation Time
		if(!TidalLock){
			if(SetRotation == 0){
				RotationTime = (((24 * RotationPeriod) * 60) * 60);
				RotationDegSec = (360 / OrbitalTime) * TimeMultiplier;
			}else{
				RotationPeriod = 0;
				RotationTime = ((((((((RotationYears * EarthDays) + RotationDays) * 24) + RotationHours) * 60) + RotationMinutes) * 60) + RotationSeconds);
			}
			RotationDegSec = (360 / RotationTime) * TimeMultiplier;
			RotInOrbit = Mathf.RoundToInt(OrbitalTime / RotationTime);
			HoursInDay = Mathf.RoundToInt((RotationTime / 60) / 60);
		}
	}
	
	public void Update(){
		float ODS = OrbitalDegSec * Time.deltaTime;
		if(!LockOrbit){
			OrbitStartPos += ODS;
			OrbitCenter.Rotate(0,ODS,0);
		}
		
		// Update Rotation
		if(TidalLock){
			ThisTransform.LookAt(Parent);
			if(KeepTime){
				UpdateCounters(0f, ODS);
			}
		}else{
			float RotDegSec = RotationDegSec * Time.deltaTime;
			if(KeepTime){
				UpdateCounters(RotDegSec, ODS);
			}
			ThisTransform.Rotate(0,RotDegSec, 0, Space.Self);
		}
	}
	
	public void UpdateCounters(float RotDegSec ,float ODS ){
		
		//Count Orbits / Years
		if((OrbCounter + ODS) >= 360){
			CounterYear += 1;
			CounterDay = 0;
			OrbCounter = (OrbCounter + ODS) - 360;
		}else{
			OrbCounter += ODS;
		}
		
		CurrentOrbitPos = OrbCounter;
		
		//Count Days	
		if((RotCounter + RotDegSec)>= 360){
			CounterDay += 1;
			RotCounter = (RotCounter + RotDegSec) - 360;
		}else{
			RotCounter += RotDegSec;
		}
		
		var CurrentTime = (RotCounter / 360) * RotationTime;
		
		//Count Hours
		CounterHour = (int)(CurrentTime / 60) / 60;
		
		//Count Minutes	
		if(CounterHour > 0){
			CounterMinute = (int)(CurrentTime / 60) - (CounterHour * 60);
		}else{
			CounterMinute = (int)(CurrentTime / 60);
		}
		
		//Count Seconds
		if(CounterHour > 0 && CounterMinute > 0){
			CounterSecond = CurrentTime - ((CounterMinute + (CounterHour * 60)) * 60);
		}else if(CounterHour > 0 && CounterMinute == 0){
			CounterSecond = CurrentTime - ((CounterHour * 60) * 60);
		}else if(CounterHour == 0 && CounterMinute > 0){
			CounterSecond = CurrentTime - (CounterMinute * 60);
		}else if(CounterHour == 0 && CounterMinute == 0){
			CounterSecond = CurrentTime;
		}
	}
	
	void LateUpdate(){
		Vector3 CurPos = Parent.position + new Vector3(OrbitOffset.x, OrbitOffset.y,OrbitOffset.z);
		OrbitCenter.position = CurPos;
		if(_DrawOrbit){
			ThisOrbit.position = CurPos;
		}
	}
}
