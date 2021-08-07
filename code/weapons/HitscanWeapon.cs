using Sandbox;
using System.Collections.Generic;

namespace Discount.Weapons
{
	public partial class HitscanWeapon : Weapon
	{
		public override float PrimaryRate => Data != null ? Data.PrimaryFireRate : 1f;
		public override string ViewModelPath => Data != null ? Data.ViewModelPath : "";

		public HitscanWeapon() : base()
		{

		}

		public HitscanWeapon( string weaponData ) : base( weaponData )
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

				TraceResult traceResult = traceResults.Current;

				// Don't hurt teammates
				if ( traceResult.Entity is ITeamEntity hitTeamEntity
					&& Owner is ITeamEntity ownerTeamEntity
					&& hitTeamEntity.Team == ownerTeamEntity.Team )
				{
					continue;
				}

				if ( IsServer )
				{
					using ( Prediction.Off() )
					{
						traceResult.Surface.DoBulletImpact( traceResult );
					}
				}

				// Don't damage if there's nothing to damage or if we're not the server
				if ( !IsServer || !traceResult.Entity.IsValid())
				{
					continue;
				}

				float damageToDeal = Data.Damage;

				// Apply headshot damage if relevant
				if ( Data.CanHeadshot
					&& traceResult.Entity is Player hitPlayer
					&& hitPlayer.GetHitboxGroup( traceResults.Current.HitboxIndex ) == 1 )
				{
					damageToDeal *= 3f;
				}

				using ( Prediction.Off() )
				{
					traceResult.Entity.TakeDamage(
						DamageInfo.FromBullet(
							traceResult.EndPos,
							pelletDirection * Data.Knockback,
							damageToDeal )
						.WithAttacker( Owner, this )
						.UsingTraceResult( traceResult ) );
				}
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
