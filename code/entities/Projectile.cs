using Sandbox;
using System;
using System.Collections.Generic;

namespace Discount
{
	public partial class Projectile : BasePhysics
	{
		protected TimeSince timeSinceCreatedParticles_;
		[Net, Predicted]
		protected TimeSince timeSinceSpawned_ { get; set; }

		[Net, Predicted]
		public Team Team { get; set; }
		[Net, Predicted]
		public float Damage { get; set; }
		[Net, Predicted]
		public float ExplosionRadius { get; set; } = 100f;
		[Net, Predicted]
		public bool GravityAffected { get; set; } = true;
		[Net, Predicted]
		public bool Explode { get; set; } = false;
		[Net, Predicted]
		public bool DisarmAfterFirstHit { get; set; } = false;
		[Net, Predicted]
		public bool Sticky { get; set; } = false;
		[Net, Predicted]
		public bool EmitSmoke { get; set; } = false;
		[Net, Predicted]
		public float Lifetime { get; set; } = 5f;

		public override void Spawn()
		{
			base.Spawn();

			SetModel( "models/sbox_props/cola_can/cola_can.vmdl" );

			timeSinceCreatedParticles_ = 1f;
			timeSinceSpawned_ = 0f;

			MoveType = MoveType.Physics;
			CollisionGroup = CollisionGroup.Interactive;
			PhysicsEnabled = true;
			UsePhysicsCollision = true;
			EnableHideInFirstPerson = true;
			EnableShadowInFirstPerson = true;

			SetupPhysicsFromModel( PhysicsMotionType.Dynamic, false );

			if ( !GravityAffected )
			{
				PhysicsBody.GravityEnabled = false;
			}
		}

		[Event( "server.tick" )]
		public void Tick()
		{
			if ( timeSinceCreatedParticles_ > 0.3f
				&& EmitSmoke )
			{
				timeSinceCreatedParticles_ = 0;

				Particles.Create( "particles/explosion_smoke.vpcf", Position );
			}

			if ( timeSinceSpawned_ > Lifetime
				&& Host.IsServer )
			{
				Delete();
			}
		}

		protected override void OnPhysicsCollision( CollisionEventData eventData )
		{
			if ( !IsValid
				 || LifeState == LifeState.Dead )
			{
				return;
			}

			if ( DisarmAfterFirstHit )
			{
				LifeState = LifeState.Dead;
			}

			if ( eventData.Entity is ModelEntity hitEnt
					&& hitEnt.IsValid()
					&& hitEnt.LifeState == LifeState.Alive
					&& hitEnt.PhysicsBody.IsValid()
					&& !hitEnt.IsWorld
					&& ( hitEnt is not ITeamEntity hitTeamEntity || hitTeamEntity.Team != Team ) )
			{
				using ( Prediction.Off() )
				{
					hitEnt.TakeDamage( DamageInfo.Explosion( eventData.Pos, Rotation.Forward * 1000f, Explode ? Damage * 0.5f : Damage )
						.WithAttacker( Owner )
						.WithWeapon( this ) );
				}
			}

			if ( Sticky
				&& eventData.Entity.IsWorld )
			{
				Position = eventData.Pos;

				if ( PhysicsBody != null )
				{
					PhysicsBody.GravityEnabled = false;
				}

				// For some reason you can't disable physics movement without disabling this callback, so this at least resets the projectile's movement
				PhysicsEnabled = false;
				PhysicsEnabled = true;
			}

			if ( !Explode
				|| ( DisarmAfterFirstHit && eventData.Entity.IsWorld ) )
			{
				return;
			}

			if ( Host.IsServer )
			{
				Delete();
			}
		}

		protected override void OnDestroy()
		{
			base.OnDestroy();

			if ( !Explode )
			{
				return;
			}

			if ( IsServer )
			{
				using ( Prediction.Off() )
				{
					Sound.FromWorld( "explosion", Position );
					Particles.Create( "particles/explosion/barrel_explosion/explosion_barrel.vpcf", Position );
				}
			}

			Vector3 sourcePos = Position;
			IEnumerable<Entity> overlaps = Physics.GetEntitiesInSphere( sourcePos, ExplosionRadius );

			foreach ( Entity overlap in overlaps )
			{
				if ( overlap is not ModelEntity ent
					|| !ent.IsValid()
					|| ent.LifeState != LifeState.Alive
					|| !ent.PhysicsBody.IsValid()
					|| ent.IsWorld
					|| (ent is ITeamEntity teamEntity && teamEntity.Team == Team && overlap != Owner) )
				{
					continue;
				}

				Vector3 targetPos = Position;

				float dist = Vector3.DistanceBetween( sourcePos, targetPos );

				if ( dist > ExplosionRadius )
				{
					continue;
				}

				TraceResult tr = Trace.Ray( sourcePos, targetPos )
					.Ignore( this )
					.WorldOnly()
					.Run();

				if ( tr.Fraction < 0.95f )
				{
					continue;
				}

				float distanceMul = 1.0f - Math.Clamp( dist / ExplosionRadius, 0.0f, 1.0f );
				float damage = Damage * 0.5f * distanceMul;
				float force = (1000f * distanceMul);
				Vector3 forceDir = (targetPos - sourcePos);

				if ( forceDir != Vector3.Zero )
				{
					forceDir = forceDir.Normal;
				}

				using ( Prediction.Off() )
				{
					ent.TakeDamage( DamageInfo.Explosion( sourcePos, forceDir * force, damage )
						.WithAttacker( Owner )
						.WithWeapon( this ) );
				}
			}
		}

		[ClientRpc]
		protected void ImpactEffects()
		{
			Host.AssertClient();

						Sound.FromWorld( "explosion", Position );
			Particles.Create( "particles/explosion/barrel_explosion/explosion_barrel.vpcf", Position );
		}
	}
}
