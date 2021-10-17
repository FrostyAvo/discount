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

			if ( !Host.IsServer )
			{
				return;
			}

			for ( int i = 0; i < Data.BulletsPerShot; i++ )
			{
				Vector3 traceDirection =
					(Owner.EyeRot
						* Rotation.FromRoll( Rand.Float( 0f, 360f ) )
						* Rotation.FromPitch( Rand.Float( 0f, Data.SpreadAngle ) )
					).Forward;

				Vector3 targetPosition = Owner.EyePos + traceDirection * 10000;

				IEnumerator<TraceResult> traceResults = TraceBullet(
					Owner.EyePos,
					Owner.EyePos + traceDirection * 10000
					).GetEnumerator();

				// Only grab the first trace result and reject traces that didn't hit
				if ( traceResults.MoveNext() && !traceResults.Current.Hit )
				{
					targetPosition = traceResults.Current.EndPos;
				}

				Vector3 projectileSpawnPosition = Owner.EyePos + Owner.EyeRot * new Vector3(30f, -15f, -10f);
				Vector3 projectileDirection = targetPosition - projectileSpawnPosition;

				if ( projectileDirection != Vector3.Zero )
				{
					projectileDirection = projectileDirection.Normal;
				}

				Projectile projectile = new Projectile();

				projectile.Position = projectileSpawnPosition;
				projectile.Rotation = Owner.EyeRot * Rotation.FromPitch(90f);

				projectile.Damage = Data.Damage;
				projectile.ExplosionRadius = Data.ExplosionRadius;
				projectile.GravityAffected = Data.GravityAffected;
				projectile.Explode = Data.Explosive;
				projectile.DisarmAfterFirstHit = Data.DisarmAfterFirstHit;
				projectile.Sticky = Data.Sticky;
				projectile.EmitSmoke = Data.EmitSmoke;
				projectile.Lifetime = Data.ProjectileLifetime;
				projectile.Owner = Owner;

				if ( Owner is ITeamEntity teamOwner )
				{
					projectile.Team = teamOwner.Team;
				}

				projectile.Spawn();

				projectile.Velocity = projectileDirection * Data.ProjectileSpeed;
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
