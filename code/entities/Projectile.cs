using Sandbox;
using System;
using System.Collections.Generic;

namespace Discount
{
	public partial class Projectile : BasePhysics
	{
		[Net, Predicted]
		protected TimeSince timeSinceCreatedParticles_ { get; set; }

		[Net, Predicted]
		public Team Team { get; set; }
		[Net, Predicted]
		public float Damage { get; set; }
		[Net, Predicted]
		public float ExplosionRadius { get; set; } = 100f;
		[Net, Predicted]
		public bool GravityAffected { get; set; } = true;

		public override void Spawn()
		{
			base.Spawn();

			SetModel( "models/sbox_props/cola_can/cola_can.vmdl" );

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
			if ( timeSinceCreatedParticles_ > 0.3f )
			{
				timeSinceCreatedParticles_ = 0;

				Particles.Create( "particles/explosion_smoke.vpcf", Position );
			}
		}

		protected override void OnPhysicsCollision( CollisionEventData eventData )
		{
			if ( !IsValid
				 || LifeState == LifeState.Dead )
			{
				return;
			}

			if ( Host.IsServer )
			{
				Delete();
			}

			LifeState = LifeState.Dead;

			Sound.FromWorld( "explosion", eventData.Pos );
			Particles.Create( "particles/explosion/barrel_explosion/explosion_barrel.vpcf", eventData.Pos );

			if ( eventData.Entity is ModelEntity hitEnt
					&& hitEnt.IsValid()
					&& hitEnt.LifeState == LifeState.Alive
					&& hitEnt.PhysicsBody.IsValid()
					&& !hitEnt.IsWorld
					&& ( hitEnt is not ITeamEntity hitTeamEntity || hitTeamEntity.Team != Team ) )
			{
				using ( Prediction.Off() )
				{
					hitEnt.TakeDamage( DamageInfo.Explosion( eventData.Pos, Rotation.Forward * 1000f, Damage * 0.5f )
						.WithAttacker( Owner )
						.WithWeapon( this ) );
				}
			}

			Vector3 sourcePos = eventData.Pos;
			IEnumerable<Entity> overlaps = Physics.GetEntitiesInSphere( sourcePos, ExplosionRadius );

			foreach ( Entity overlap in overlaps )
			{
				if ( overlap is not ModelEntity ent 
					|| !ent.IsValid()
					|| ent.LifeState != LifeState.Alive 
					|| !ent.PhysicsBody.IsValid()
					|| ent.IsWorld
					|| ( ent is ITeamEntity teamEntity && teamEntity.Team == Team && overlap != Owner ) )
				{
					continue;
				}

				Vector3 targetPos = eventData.Pos;

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
	}
}
