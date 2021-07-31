using Sandbox;
using System.Collections.Generic;

namespace Discount.Weapons
{
	partial class HitscanWeapon : BaseWeapon
	{
		[Net, Predicted]
		public WeaponData Data { get; private set; }

		public override float PrimaryRate => Data != null ? Data.PrimaryFireRate : 1f;
		public override string ViewModelPath => Data != null ? Data.ViewModelPath : "";

		public HitscanWeapon()
		{
			Data = null;
		}

		public HitscanWeapon(string weaponData)
		{
			Data = Resource.FromPath<WeaponData>( "data/" + weaponData + ".weapon" );

			SetModel( Data.WorldModelPath );
		}

		public override string ToString()
		{
			return Data != null ? Data.Name : "null";
		}

		public override void Spawn()
		{
			base.Spawn();

			if ( Data == null )
			{
				return;
			}
		}

		public override void AttackPrimary()
		{
			if (Data == null)
			{
				return;
			}

			(Owner as AnimEntity)?.SetAnimBool( "b_attack", true );

			ShootEffects();

			for ( int i = 0; i < Data.BulletsPerShot; i++ )
			{
				Vector3 pelletDirection =
					(Owner.EyeRot
						* Rotation.FromRoll( Rand.Float( 0f, 360f ) )
						* Rotation.FromPitch( Rand.Float( 0f, Data.SpreadAngle ) )
							).Forward;

				IEnumerator<TraceResult> traceResults = TraceBullet(
					Owner.EyePos,
					Owner.EyePos + pelletDirection * Data.Range
					).GetEnumerator();

				// Only grab the first trace result and reject traces that didn't hit
				if ( !traceResults.MoveNext() || !traceResults.Current.Hit )
				{
					continue;
				}

				// Don't hurt teammates
				if ( traceResults.Current.Entity is TeamPlayer hitTeamPlayer
					&& Owner is TeamPlayer ownerTeamPlayer
					&& hitTeamPlayer.TeamIndex == ownerTeamPlayer.TeamIndex )
				{
					continue;
				}

				traceResults.Current.Surface.DoBulletImpact( traceResults.Current );

				// Don't damage if there's nothing to damage or if we're not the server
				if ( !IsServer || !traceResults.Current.Entity.IsValid())
				{
					continue;
				}

				using ( Prediction.Off() )
				{
					traceResults.Current.Entity.TakeDamage(
						DamageInfo.FromBullet(
							traceResults.Current.EndPos,
							pelletDirection * Data.Knockback,
							Data.Damage ).WithAttacker( Owner, this ) );
				}
			}
		}

		/*public override void Reload()
		{
			(Owner as AnimEntity)?.SetAnimBool( "b_reload", true );
			ViewModelEntity?.SetAnimBool( "reload", true );
		}*/

		public override void SimulateAnimator( PawnAnimator anim )
		{
			if ( Data == null )
			{
				return;
			}

			anim.SetParam( "holdtype", Data.HoldType );
			anim.SetParam( "aimat_weight", 0.2f );
		}

		[ClientRpc]
		protected virtual void ShootEffects()
		{
			Host.AssertClient();

			Particles.Create( "particles/pistol_muzzleflash.vpcf", EffectEntity, "muzzle" );
			Particles.Create( "particles/pistol_ejectbrass.vpcf", EffectEntity, "ejection_point" );

			ViewModelEntity?.SetAnimBool( "fire", true );
			CrosshairPanel?.CreateEvent( "fire" );

			PlaySound( Data.PrimaryFireSound );
		}
	}
}
