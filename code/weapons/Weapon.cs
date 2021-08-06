using Sandbox;

namespace Discount.Weapons
{
	public abstract partial class Weapon : BaseWeapon
	{
		[Net, Predicted]
		public WeaponData Data { get; protected set; }

		[Net, Predicted, OnChangedCallback]
		public int Slot { get; set; } = -1;

		[Net, Predicted]
		public TimeSince TimeSinceDeployed { get; set; }

		[Net, Predicted]
		public TimeSince TimeSincePrimaryAttackHeld { get; set; }

		protected Weapon()
		{
			Data = null;
		}

		protected Weapon( string weaponData )
		{
			Data = Resource.FromPath<WeaponData>( "data/weapons/" + weaponData + ".weapon" );

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
					&& TimeSinceDeployed > Data.DeployTime );
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
