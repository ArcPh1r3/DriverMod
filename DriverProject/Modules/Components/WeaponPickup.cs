﻿using UnityEngine;
using RoR2;
using UnityEngine.Networking;
using R2API.Networking;
using R2API.Networking.Interfaces;

namespace RobDriver.Modules.Components
{
    public class WeaponPickup : MonoBehaviour
    {
		[Tooltip("The base object to destroy when this pickup is consumed.")]
		public GameObject baseObject;
		[Tooltip("The team filter object which determines who can pick up this pack.")]
		public TeamFilter teamFilter;
		public DriverWeaponDef weaponDef;

		public GameObject pickupEffect;

		private bool alive = true;

		private void Awake()
        {
			// disable visuals for non driver
			if (!Modules.Config.sharedPickupVisuals.Value)
            {
				BeginRapidlyActivatingAndDeactivating blinker = this.transform.parent.GetComponentInChildren<BeginRapidlyActivatingAndDeactivating>();
				if (blinker)
				{
					bool isDriver = false;

					var localPlayers = LocalUserManager.readOnlyLocalUsersList;
					foreach (LocalUser i in localPlayers)
					{
						if (i.cachedBody.baseNameToken == Modules.Survivors.Driver.bodyNameToken) isDriver = true;
					}

					if (!isDriver)
					{
						blinker.blinkingRootObject.SetActive(false);
						Destroy(blinker);
					}
				}
			}

			// uh will this work?
			/*if (Run.instance)
			{
				float rng = Run.instance.stageRng.nextNormalizedFloat;

				if (rng > 0.5f) this.SetWeapon(DriverWeapon.MachineGun);
				else this.SetWeapon(DriverWeapon.Shotgun);
			}*/
			// no it doesn't, clients don't have the rng

			// i'm a dirty hack
			// lock me up and throw away the key
			this.Invoke("Fuck", 59.5f);
		}

		private void Start()
        {
			this.SetWeapon(this.weaponDef);
		}

		public void ServerSetWeapon(DriverWeaponDef newWeaponDef)
        {
			// this didn't work lole
			this.weaponDef = newWeaponDef;

			if (NetworkServer.active)
			{
				NetworkIdentity identity = this.transform.root.GetComponentInChildren<NetworkIdentity>();
				if (!identity) return;

				new SyncWeaponPickup(identity.netId, (ushort)this.weaponDef.index).Send(NetworkDestination.Clients);
			}
		}

		public void SetWeapon(DriverWeaponDef newWeapon)
        {
			this.weaponDef = newWeapon;

			// wow this is awful!
			RoR2.UI.LanguageTextMeshController textComponent = this.transform.parent.GetComponentInChildren<RoR2.UI.LanguageTextMeshController>();
			if (textComponent)
			{
				if (!this.weaponDef)
				{
					// band-aid i don't have the time to keep fighting with this code rn
					textComponent.token = "FUCK YOU FUCK YOU FUCK/nYOU FUCK YOU FUCK YOU";
					return;
				}

				textComponent.token = this.weaponDef.nameToken;

				switch (this.weaponDef.tier)
                {
					case DriverWeaponTier.Common:
						textComponent.textMeshPro.color = Modules.Helpers.whiteItemColor;
						break;
					case DriverWeaponTier.Uncommon:
						textComponent.textMeshPro.color = Modules.Helpers.greenItemColor;
						break;
					case DriverWeaponTier.Legendary:
						textComponent.textMeshPro.color = Modules.Helpers.redItemColor;
						break;
					case DriverWeaponTier.Unique:
						textComponent.textMeshPro.color = Modules.Helpers.yellowItemColor;
						break;
					case DriverWeaponTier.Lunar:
						textComponent.textMeshPro.color = Modules.Helpers.lunarItemColor;
						break;
					case DriverWeaponTier.Void:
						textComponent.textMeshPro.color = Modules.Helpers.voidItemColor;
						break;
				}
			}
		}

		private void OnTriggerStay(Collider collider)
		{
			// can this run on every client? i don't know but let's find out
			if (NetworkServer.active && this.alive && TeamComponent.GetObjectTeam(collider.gameObject) == this.teamFilter.teamIndex)
			{
				// well it can but it's not a fix.
				DriverController iDrive = collider.GetComponent<DriverController>();
				if (iDrive)
				{
					this.alive = false;
					iDrive.ServerPickUpWeapon(this.weaponDef, iDrive);
					EffectManager.SimpleEffect(this.pickupEffect, this.transform.position, Quaternion.identity, true);
					UnityEngine.Object.Destroy(this.baseObject);
				}
			}
		}

		private void Fuck()
        {
			if (this.alive)
			{
				Modules.Achievements.SupplyDropAchievement.weaponHasDespawned = true;
			}
		}
	}
}