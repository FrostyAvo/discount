using Sandbox;
using System;

namespace Discount.Weapons
{
	public abstract partial class Weapon : BaseWeapon
	{
		[Net, Predicted]
		public WeaponData Data { get; protected set; }

		[Net, Predicted, OnChangedCallback]
		public int Slot { get; set; } = -1;

		[Net, Predicted]
		public int AmmoInClip { get; protected set; }
		[Net, Predicted]
		public int AmmoInReserve { get; protected set; }

		[Net, Predicted]
		public bool Reloading { get; protected set; }

		[Net, Predicted]
		public TimeSince TimeSinceDeployed { get; protected set; }
		[Net, Predicted]
		public TimeSince TimeSincePrimaryAttackHeld { get; protected set; }
		[Net, Predicted]
		public TimeSince TimeSinceStartedReloading { get; protected set; }

		protected Weapon()
		{
			Data = null;
		}

		protected Weapon( string weaponData )
		{
			Data = Resource.FromPath<WeaponData>( "data/weapons/" + weaponData + ".weapon" );

			AmmoInClip = Data.ClipSize;
			AmmoInReserve = Data.ReserveAmmo;

			SetModel( Data.WorldModelPath );
		}

		public override string ToString()
		{
			return Data != null ? Data.Name : "null";
		}

		public override void Simulate( Client player )
		{
			if ( CanReload() )
			{
				Reload();
			}

			//
			// Reload could have deleted us
			//
			if ( !this.IsValid() )
			{
				return;
			}

			// Windup logic
			if ( Input.Pressed( InputButton.Attack1 ) )
			{
				// Prevent player from "skipping" windup time during the deploy time
				if ( Data != null
					&& TimeSinceDeployed < Data.DeployTime )
				{
					TimeSincePrimaryAttackHeld = -( Data.DeployTime - TimeSinceDeployed );
				}
				else
				{
					TimeSincePrimaryAttackHeld = 0;
				}

				if ( Data != null
					&& Data.WindupTime > 0f )
				{
					PlaySound( Data.WindupSound );
				}
			}

			if ( CanPrimaryAttack() )
			{
				TimeSincePrimaryAttack = 0;
				Reloading = false;

				if ( Data != null )
				{
					AmmoInClip -= Data.AmmoPerShot;
				}

				AttackPrimary();
			}

			//
			// AttackPrimary could have deleted us
			//
			if ( !this.IsValid() )
			{
				return;
			}

			if ( CanSecondaryAttack() )
			{
				TimeSinceSecondaryAttack = 0;
				AttackSecondary();
			}
		}

		public override void SimulateAnimator( PawnAnimator anim )
		{
			if ( Data == null )
			{
				return;
			}

			anim.SetParam( "holdtype", Data.HoldType );
			anim.SetParam( "aimat_weight", 0.2f );
		}

		public override bool CanPrimaryAttack()
		{
			return base.CanPrimaryAttack()
				&& ( Data == null
					|| TimeSincePrimaryAttackHeld > Data.WindupTime
					&& TimeSinceDeployed > Data.DeployTime
					&& AmmoInClip >= Data.AmmoPerShot );
		}

		public override bool CanReload()
		{
			return
				Owner.IsValid()
				// Don't reload during shot cooldown
				&& ( PrimaryRate == 0
					|| TimeSincePrimaryAttack > ( 1 / PrimaryRate ) )
				// Don't reload if clip is full
				&& ( Data == null
					|| AmmoInClip < Data.ClipSize )
				// Don't reload if no ammo in reserve
				&& AmmoInReserve > 0
				// Reload if clip too empty to fire, earlier reloading hasn't been interrupted or player pressed reload
				&& ( Data == null
					|| AmmoInClip < Data.AmmoPerShot
					|| Reloading
					|| Input.Down( InputButton.Reload ) );
		}

		public override void Reload()
		{
			if ( Data == null )
			{
				return;
			}

			if ( !Reloading )
			{
				if ( Data.ReloadTime == 0 )
				{
					int ammoToReload = Math.Min( Data.ClipSize - AmmoInClip, AmmoInReserve );

					AmmoInClip += ammoToReload;
					AmmoInReserve -= ammoToReload;

					return;
				}

				TimeSinceStartedReloading = 0;

				(Owner as AnimEntity)?.SetAnimBool( "b_reload", true );
				ViewModelEntity?.SetAnimBool( "reload", true );

				Reloading = true;
			}

			if ( TimeSinceStartedReloading > Data.ReloadTime )
			{
				TimeSinceStartedReloading = 0;

				(Owner as AnimEntity)?.SetAnimBool( "b_reload", true );
				ViewModelEntity?.SetAnimBool( "reload", true );

				PlaySound( Data.ReloadSound );

				int ammoToReload = Math.Min( Math.Min( Data.AmmoPerReload, Data.ClipSize - AmmoInClip ), AmmoInReserve );

				AmmoInClip += ammoToReload;
				AmmoInReserve -= ammoToReload;
			}

			if ( AmmoInClip >= Data.ClipSize
				|| AmmoInReserve <= 0 )
			{
				(Owner as AnimEntity)?.SetAnimBool( "b_reload", false );
				ViewModelEntity?.SetAnimBool( "reload", false );
				ViewModelEntity?.SetAnimBool( "reload_finished", true );

				Reloading = false;
				return;
			}
		}

		public override void ActiveStart( Entity ent )
		{
			base.ActiveStart( ent );

			TimeSinceDeployed = 0;
		}

		protected void OnSlotChanged()
		{
			// This syncs inventories on clients to the corresponding inventories on the server
			if ( Host.IsClient )
			{
				(Owner.Inventory as ClassInventory)?.FillSlot( Slot, this );
			}
		}
	}
}
