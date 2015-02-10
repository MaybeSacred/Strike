using UnityEngine;
using System.Collections.Generic;
public class Property : MonoBehaviour, AttackableObject
{
	public string prettyName;
	public int startingOwner;
	private bool hasUnitSelectedMutex;
	public static int productionDisplayWidth = 220;
	public UnitNames propertyType;
	public Health health;
	private Player currentOwner;
	public PropertyAttributes propertyClass;
	public UnitState currentState {get; private set;}
	private bool currentlyDead;
	private int captureCount = 20;
	private float infoBoxTimeoutCounter;
	private bool wasTargeted;
	public TextMesh targetedDamageDisplay;
	public TextMesh targetedDamageOutline;
	public UnitController occupyingUnit;
	private UnitController capturingUnit;
	private TerrainBlock currentBlock;
	private Transform graphicsObject;
	public string description;
	private static ParticleSystem staticSmokeParticles, staticFireParticles;
	private ParticleSystem smokeParticles, fireParticles;
	public int baseCost;
	public bool isUnderConstruction {get; private set;}
	public bool justBuilt {get; private set;}
	public ParticleSystem mouseOverEffect;

	public int comTowerRange;
	public float AICapturePriority;
	//Distance to nearest enemy hq
	public float cachedDistanceFromEnemyHQ;
	[System.Serializable]
	public class PropertyAttributes
	{
		public int baseFunds;
		public bool capturable;
		public int defenseBonus;
		public UnitNames[] producableUnits;
	}
	public bool IsAlive ()
	{
		if(health.GetRawHealth() > 0)
		{
			return true;
		}
		return false;
	}

	public static int CompareByDistanceFromEnemyHQ (Property x, Property y)
	{
		if(x.cachedDistanceFromEnemyHQ < y.cachedDistanceFromEnemyHQ)
		{
			return -1;
		}
		else if(x.cachedDistanceFromEnemyHQ > y.cachedDistanceFromEnemyHQ)
		{
			return 1;
		}
		return 0;
	}
	public TerrainBlock GetOccupyingBlock()
	{
		return currentBlock;
	}
	public float NormalizedCaptureCount()
	{
		return ((float)captureCount)/20;
	}
	public int UnitCost ()
	{
		return baseCost;
	}
	void Awake()
	{
		health = new Health();
		if(collider != null)
		{
			collider.enabled = false;
		}
		Transform[] trans = GetComponentsInChildren<Transform>();
		for(int i = 0; i < trans.Length; i++)
		{
			if(trans[i].name.Equals("Graphics"))
			{
				graphicsObject = trans[i];
			}
		}
		RaycastHit hit;
		if(Physics.Raycast(transform.position + Vector3.up, Vector3.down, out hit, 10f, 1 << LayerMask.NameToLayer("Default")))
		{
			currentBlock = hit.collider.GetComponent<TerrainBlock>();
			currentBlock.AttachProperty(this);
		}
		isUnderConstruction = false;
		if(propertyType == UnitNames.ComTower)
		{
			mouseOverEffect = (ParticleSystem)Instantiate(mouseOverEffect, transform.position, transform.rotation);
			mouseOverEffect.particleSystem.Stop();
		}
	}
	void Start()
	{
		SetOwner(InGameController.GetPlayer(startingOwner));
		if(staticFireParticles == null)
		{
			staticFireParticles = Resources.Load<ParticleSystem>("BuildingFire");
		}
		if(staticSmokeParticles == null)
		{
			staticSmokeParticles = Resources.Load<ParticleSystem>("Smoke");
		}
		targetedDamageDisplay.gameObject.SetActive(true);
		targetedDamageOutline.gameObject.SetActive(true);
		targetedDamageDisplay.text = "";
		targetedDamageOutline.text = "";
	}
	public void StartConstruction()
	{
		health.SetRawHealth(50);
		isUnderConstruction = true;
		justBuilt = true;
		currentBlock.DetachProperty(this, true);
	}

	public Health GetHealth ()
	{
		return health;
	}

	public int OffenseBonus ()
	{
		return 0;
	}

	public int DefenseBonus ()
	{
		return propertyClass.defenseBonus;
	}

	public UnitNames GetUnitClass ()
	{
		return propertyType;
	}

	public void FinishConstruction()
	{
		health.AddRawHealth(50);
		isUnderConstruction = false;
		currentBlock.AttachProperty(this);
	}

	public void AIProduceUnit (UnitNames unitNames)
	{
		UnitController newUnit = currentOwner.ProduceUnit(unitNames);
		newUnit.transform.position = transform.position;
		occupyingUnit = newUnit;
		EndTurn();
	}

	public void SetBlock(TerrainBlock inBlock)
	{
		if(currentBlock != null)
		{
			throw new UnityException("Property's TerrainBlock already set");
		}
		currentBlock = inBlock;
	}
	public void StartTurn()
	{
		if(!currentlyDead)
		{
			currentOwner.AddFunds((int)(propertyClass.baseFunds * Mathf.Round(health.PrettyHealth()/10)));
		}
		if(isUnderConstruction)
		{
			justBuilt = false;
		}
		else
		{
			HealBase(1);
		}
		InGameController.currentTerrain.ClearFog(currentBlock, 1, false);
		currentState = UnitState.UnMoved;
	}

	public void EndTurn()
	{
		hasUnitSelectedMutex = InGameController.ReleaseUnitSelectedMutex();
		Utilities.gameCamera.otherMenuActive = false;
		currentState = UnitState.FinishedMove;
	}
	void Update()
	{
		if(currentState == UnitState.FinishedMove)
		{
			if(wasTargeted)
			{
				wasTargeted = false;
			}
			else
			{
				targetedDamageDisplay.text = "";
				targetedDamageOutline.text = "";
			}
		}
		if(currentState == UnitState.AwaitingOrder)
		{
			if(Input.GetMouseButtonDown(1))
			{
				currentState = UnitState.UnMoved;
				hasUnitSelectedMutex = InGameController.ReleaseUnitSelectedMutex();
				Utilities.gameCamera.otherMenuActive = false;
			}
		}
		infoBoxTimeoutCounter += Time.deltaTime;
	}
	void OnGUI()
	{
		if(currentState == UnitState.AwaitingOrder)
		{
			if(propertyClass.producableUnits.Length < 8)
			{
				GUI.BeginGroup(new Rect(Screen.width/2 - Property.productionDisplayWidth/2, -UnitController.actionDisplayYOffset, Property.productionDisplayWidth, UnitController.actionDisplayHeight*propertyClass.producableUnits.Length + 1));
				for(int i = 0; i < propertyClass.producableUnits.Length; i++)
				{
					if(currentOwner.CanProduceUnit(propertyClass.producableUnits[i]))
					{
						if(GUI.Button(new Rect(0, i*UnitController.actionDisplayHeight, Property.productionDisplayWidth, UnitController.actionDisplayHeight), ((UnitController)Utilities.GetPrefabFromUnitName(propertyClass.producableUnits[i])).prettyName + "    " + ((UnitController)Utilities.GetPrefabFromUnitName(propertyClass.producableUnits[i])).baseCost))
						{
							AIProduceUnit(propertyClass.producableUnits[i]);
						}
					}
					else
					{
						GUI.Box(new Rect(0, i*UnitController.actionDisplayHeight, Property.productionDisplayWidth, UnitController.actionDisplayHeight), ((UnitController)Utilities.GetPrefabFromUnitName(propertyClass.producableUnits[i])).prettyName + "    " + ((UnitController)Utilities.GetPrefabFromUnitName(propertyClass.producableUnits[i])).baseCost);
					}
				}
				if(GUI.Button(new Rect(0, propertyClass.producableUnits.Length * UnitController.actionDisplayHeight, Property.productionDisplayWidth, UnitController.actionDisplayHeight), "Back"))
				{
					currentState = UnitState.UnMoved;
					hasUnitSelectedMutex = InGameController.ReleaseUnitSelectedMutex();
					Utilities.gameCamera.otherMenuActive = false;
				}
				GUI.EndGroup();
			}
			else
			{
				GUI.BeginGroup(new Rect(Screen.width/2 - Property.productionDisplayWidth, -UnitController.actionDisplayYOffset, 2 * Property.productionDisplayWidth, UnitController.actionDisplayHeight*propertyClass.producableUnits.Length));
				int i = 0;
				for(; i < propertyClass.producableUnits.Length/2 + 1; i++)
				{
					if(currentOwner.CanProduceUnit(propertyClass.producableUnits[i]))
					{
						if(GUI.Button(new Rect(0, i*UnitController.actionDisplayHeight, Property.productionDisplayWidth, UnitController.actionDisplayHeight), ((UnitController)Utilities.GetPrefabFromUnitName(propertyClass.producableUnits[i])).prettyName + "    " + ((UnitController)Utilities.GetPrefabFromUnitName(propertyClass.producableUnits[i])).baseCost))
						{
							AIProduceUnit(propertyClass.producableUnits[i]);
						}
					}
					else
					{
						GUI.Box(new Rect(0, i*UnitController.actionDisplayHeight, Property.productionDisplayWidth, UnitController.actionDisplayHeight), ((UnitController)Utilities.GetPrefabFromUnitName(propertyClass.producableUnits[i])).prettyName + "    " + ((UnitController)Utilities.GetPrefabFromUnitName(propertyClass.producableUnits[i])).baseCost);
					}
				}
				for(; i < propertyClass.producableUnits.Length; i++)
				{
					if(currentOwner.CanProduceUnit(propertyClass.producableUnits[i]))
					{
						if(GUI.Button(new Rect(Property.productionDisplayWidth, (i-propertyClass.producableUnits.Length/2 - 1)*UnitController.actionDisplayHeight, Property.productionDisplayWidth, UnitController.actionDisplayHeight), ((UnitController)Utilities.GetPrefabFromUnitName(propertyClass.producableUnits[i])).prettyName + "    " + ((UnitController)Utilities.GetPrefabFromUnitName(propertyClass.producableUnits[i])).baseCost))
						{
							AIProduceUnit(propertyClass.producableUnits[i]);
						}
					}
					else
					{
						GUI.Box(new Rect(Property.productionDisplayWidth, (i-propertyClass.producableUnits.Length/2 - 1)*UnitController.actionDisplayHeight, Property.productionDisplayWidth, UnitController.actionDisplayHeight), ((UnitController)Utilities.GetPrefabFromUnitName(propertyClass.producableUnits[i])).prettyName + "    " + ((UnitController)Utilities.GetPrefabFromUnitName(propertyClass.producableUnits[i])).baseCost);
					}
				}
				if(GUI.Button(new Rect(Property.productionDisplayWidth/2, (propertyClass.producableUnits.Length/2 + 1) * UnitController.actionDisplayHeight, Property.productionDisplayWidth, UnitController.actionDisplayHeight), "Back"))
				{
					currentState = UnitState.UnMoved;
					hasUnitSelectedMutex = InGameController.ReleaseUnitSelectedMutex();
					Utilities.gameCamera.otherMenuActive = false;
				}
				GUI.EndGroup();
			}
		}
	}
	public Vector3 GetPosition()
	{
		return transform.position;
	}
	public void KillUnit ()
	{
		if(propertyType == UnitNames.Headquarters)
		{
			propertyType = UnitNames.City;
			InGameController.RemovePlayer(currentOwner);
		}
		else
		{
			currentOwner.RemoveProperty(this);
		}
		health.SetRawHealth(-59);
		if(InGameController.GetPlayer(0) != null)
		{	
			SetOwner(InGameController.GetPlayer(0));
		}
		if(propertyType == UnitNames.Bridge)
		{
			currentBlock.DetachProperty(this, false);
			Destroy(gameObject);
		}
	}
	public void DestroyHeadquarters()
	{
		propertyType = UnitNames.City;
		health.SetRawHealth(-59);
		if(InGameController.GetPlayer(0) != null)
		{
			SetOwner(InGameController.GetPlayer(0));
		}
	}
	public void OnMouseOverExtra()
	{
		infoBoxTimeoutCounter = 0;
		if(propertyType == UnitNames.ComTower)
		{
			if(!mouseOverEffect.particleSystem.isPlaying) mouseOverEffect.particleSystem.Play();
		}
	}
	public void OnMouseUpExtra()
	{
		if(InGameController.GetCurrentPlayer() == currentOwner && propertyClass.producableUnits.Length > 0 && !InGameController.isPaused)
		{
			if(!hasUnitSelectedMutex)
			{
				hasUnitSelectedMutex = InGameController.AcquireUnitSelectedMutex(this);
			}
			if(hasUnitSelectedMutex)
			{
				Utilities.gameCamera.otherMenuActive = true;
				Utilities.gameCamera.CenterCameraOnPoint(transform.position);
				switch(currentState)
				{
					case UnitState.UnMoved:
					{
						currentState = UnitState.AwaitingOrder;
						break;
					}
					case UnitState.Selected:
					{
						currentState = UnitState.AwaitingOrder;
						break;
					}
					case UnitState.AwaitingOrder:
					{
						break;
					}
				}
			}
		}
	}

	public void OnMouseExitExtra ()
	{
		if(propertyType == UnitNames.ComTower)
		{
			mouseOverEffect.particleSystem.Stop();
		}
	}

	public Player GetOwner()
	{
		return currentOwner;
	}
	public void SetOwner(Player newOwner)
	{
		if(newOwner != currentOwner)
		{
			if(currentOwner != null)
			{
				currentOwner.RemoveProperty(this);
			}
			currentOwner = newOwner;
			currentOwner.AddProperty(this);
			graphicsObject.renderer.material.color = newOwner.mainPlayerColor;
		}
	}
	public void DisplayCapturableGraphics()
	{
		
	}
	public void Capture(int captureStrength, UnitController unit)
	{
		if(capturingUnit != unit)
		{
			captureCount = 20;
			capturingUnit = unit;
		}
		captureCount -= captureStrength;
		if(captureCount <= 0)
		{
			if(propertyType == UnitNames.Headquarters)
			{
				propertyType = UnitNames.City;
				InGameController.RemovePlayer(currentOwner);
			}
			captureCount = 20;
			if(unit.AITarget == this)
			{
				unit.AITarget = null;
			}
			SetOwner(capturingUnit.GetOwner());
		}
	}

	public void UnOccupy ()
	{
		occupyingUnit = null;
		captureCount = 20;
	}

	public void DisplayTargetDamage(int damage)
	{
		wasTargeted = true;
		targetedDamageDisplay.text = damage + "%";
		targetedDamageDisplay.transform.rotation = Camera.main.transform.rotation;
		targetedDamageOutline.text = damage + "%";
		targetedDamageOutline.transform.rotation = Camera.main.transform.rotation;
	}
	public void TakeDamage(int damage)
	{
		health.AddRawHealth(-damage);
		if(health.GetRawHealth() <= 0)
		{
			KillUnit();
		}
		UpdateSmokeParticles();
	}
	void UpdateSmokeParticles()
	{
		if(health.GetRawHealth() < 20)
		{
			if(fireParticles == null)
			{
				fireParticles = Instantiate(staticFireParticles, transform.position, staticFireParticles.transform.rotation) as ParticleSystem;
			}
		}
		else if(health.GetRawHealth() < 50)
		{
			if(fireParticles != null)
			{
				Destroy(fireParticles.gameObject);
			}
			if(smokeParticles == null)
			{
				smokeParticles = Instantiate(staticSmokeParticles, transform.position, staticSmokeParticles.transform.rotation) as ParticleSystem;
			}
		}
		else
		{
			if(fireParticles != null)
			{
				Destroy(fireParticles.gameObject);
			}
			if(smokeParticles != null)
			{
				Destroy(smokeParticles.gameObject);
			}
		}
	}
	public void HealBase(int attemptedHealAmount)
	{
		if(health.GetRawHealth() < 100)
		{
			for(int i = 0; i < attemptedHealAmount; i++)
			{
				if(currentOwner.RemoveFunds(100))
				{
					health.AddRawHealth(10);
					if(health.GetRawHealth() > 0)
					{
						currentlyDead = false;
					}
				}
			}
		}
		UpdateSmokeParticles();
	}
	public bool CanHealUnit(UnitController inUnit)
	{
		switch(propertyType)
		{
			case UnitNames.Headquarters:
			{
				return true;
			}
			case UnitNames.City:
			case UnitNames.Factory:
			{
				if(inUnit.moveClass == MovementType.Amphibious || inUnit.moveClass == MovementType.Infantry 
				|| inUnit.moveClass == MovementType.HeavyVehicle || inUnit.moveClass == MovementType.LightVehicle
				||inUnit.moveClass == MovementType.Sniper ||inUnit.moveClass == MovementType.Tank)
				{
					return true;
				}
				break;
			}
			case UnitNames.Shipyard:
			{
				if(inUnit.moveClass == MovementType.Amphibious || inUnit.moveClass == MovementType.Littoral
				   || inUnit.moveClass == MovementType.Sea)
				{
					return true;
				}
				break;
			}
			case UnitNames.Airport:
			{
				if(inUnit.moveClass == MovementType.Air)
				{
					return true;
				}
				break;
			}
		}
		return false;
	}
	public bool CanProduceUnit(UnitNames inUnit)
	{
		for(int i = 0; i < propertyClass.producableUnits.Length; i++)
		{
			if(propertyClass.producableUnits[i] == inUnit)
			{
				return true;
			}
		}
		return false;
	}
	public void ResetUnit()
	{
		hasUnitSelectedMutex = InGameController.ReleaseUnitSelectedMutex();
		if(currentState != UnitState.FinishedMove)
		{
			currentState = UnitState.UnMoved;
		}
	}
	public int TerrainBonus()
	{
		return propertyClass.defenseBonus;
	}
	public TerrainBlock GetCurrentBlock()
	{
		return currentBlock;
	}
	public bool IsInComTowerRange(TerrainBlock block)
	{
		if(TerrainBuilder.ManhattanDistance(block, currentBlock) <= comTowerRange)
		{
			return true;
		}
		return false;
	}
	/// <summary>
	/// Returns a list of units within range, if property is a com tower
	/// </summary>
	/// <returns>The in range.</returns>
	public List<UnitController> UnitsInRange()
	{
		List<UnitController> outObjects = new List<UnitController>();
		foreach(UnitController u in currentOwner.units)
		{
			if(TerrainBuilder.ManhattanDistance(u.awaitingOrdersBlock, currentBlock) <= comTowerRange)
			{
				outObjects.Add(u);
			}
		}
		return outObjects;
	}
	/// <summary>
	/// Sets the property GUI view variables.
	/// </summary>
	/// <param name="propertyView">Property view.</param>
	public void SetPropertyGUIView (PropertyGameView propertyView)
	{
		propertyView.SetValues(prettyName, UnitGameViewer.FormatSlashedString(health.ToString(), "10"), 
							   DefenseBonus().ToString(), UnitGameViewer.FormatSlashedString(captureCount.ToString(), "20"));
	}

}

