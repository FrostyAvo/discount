using Sandbox;
using System.Collections.Generic;

namespace Discount.Weapons
{
	public partial class ProjectileWeapon : AssetWeapon
	{
		public override float PrimaryRate => Data != null ? Data.PrimaryFireRate : 1f;
		public override string ViewModelPath => Data != null ? Data.ViewModelPath : "";

		public ProjectileWeapon() : base()
		{

		}

		public ProjectileWeapon( WeaponData weaponData ) : base( weaponData )
		{

		}

		public override void AttackPrimary()
		{
			if (Data == null)
			{
				return;
			}

			(Owner as AnimEntity)?.SetAnimBool( "b_attack", true );

			ShootEffects();
			PlaySound( Data.PrimaryFireSound );

			for ( int i = 0; i < Data.BulletsPerShot; i++ )
			{
				Rotation projectileRotation =
					(Owner.EyeRot
						* Rotation.FromRoll( Rand.Float( 0f, 360f ) )
						* Rotation.FromPitch( Rand.Float( 0f, Data.SpreadAngle ) )
							);
			}
		}

		[ClientRpc]
		protected virtual void ShootEffects()
		{
			Host.AssertClient();

			Particles.Create( "particles/pistol_muzzleflash.vpcf", EffectEntity, "muzzle" );
			Particles.Create( "particles/pistol_ejectbrass.vpcf", EffectEntity, "ejection_point" );

			ViewModelEntity?.SetAnimBool( "fire", true );
			CrosshairPanel?.CreateEvent( "fire" );
		}
	}
}
