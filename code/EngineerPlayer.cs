using Discount.Weapons;

using Sandbox;

namespace Discount
{
	partial class ClassPlayer : Player
	{
		[Net, Predicted] public int TeamIndex { get; set; }
		public ClassPlayer() : base()
		{
			Inventory = new BaseInventory( this );
		}

		public override void Respawn()
		{
			SetModel( "models/citizen/citizen.vmdl" );

			Wear( "models/citizen_clothes/hat/hat_hardhat.vmdl" );
			Wear( "models/citizen_clothes/shoes/shoes.police.vmdl" );
			Wear( TeamIndex == 0 ? "models/citizen_clothes/trousers/trousers.smarttan.vmdl" : "models/citizen_clothes/trousers/trousers.police.vmdl" );
			Wear( TeamIndex == 0 ? "models/citizen_clothes/shirt/shirt_longsleeve.plain.vmdl" : "models/citizen_clothes/shirt/shirt_longsleeve.scientist.vmdl" );

			Controller = new WalkController();
			Animator = new StandardPlayerAnimator();
			Camera = new FirstPersonCamera();

			EnableAllCollisions = true;
			EnableDrawing = true;
			EnableHideInFirstPerson = true;
			EnableShadowInFirstPerson = true;

			Inventory.Add( new HitscanWeapon( "shotgun" ), true );
			Inventory.Add( new HitscanWeapon( "pistol" ) );
			Inventory.Add( new MeleeWeapon( "wrench" ) );

			base.Respawn();
		}

		/// <summary>
		/// Called every tick, clientside and serverside.
		/// </summary>
		public override void Simulate(Client cl)
		{
			base.Simulate(cl);

			if ( Input.ActiveChild != null )
			{
				ActiveChild = Input.ActiveChild;
			}

			if ( LifeState != LifeState.Alive )
			{
				return;
			}

			//
			// If you have active children (like a weapon etc) you should call this to 
			// simulate those too.
			//
			SimulateActiveChild(cl, ActiveChild);

			if ( Input.Pressed( InputButton.View ) )
			{
				if ( Camera is not FirstPersonCamera )
				{
					Camera = new FirstPersonCamera();
				}
				else
				{
					Camera = new ThirdPersonCamera();
				}
			}

			if( Input.Pressed( InputButton.Drop ) )
			{
				TakeDamage( DamageInfo.Generic( 1000f ) );
			}
		}

		public override void OnKilled()
		{
			base.OnKilled();

			BecomeRagdollOnClient( Velocity, DamageFlags.Blunt, Position, Vector3.Zero, 0);

			Camera = new SpectateRagdollCamera();
			Controller = null;

			EnableAllCollisions = false;
			EnableDrawing = false;

			Inventory.DeleteContents();
		}

		public void Wear(string model)
		{
			ModelEntity wornEntity = new ModelEntity();
			wornEntity.SetModel(model);
			wornEntity.SetParent(this, true);

			wornEntity.EnableHideInFirstPerson = true;
			wornEntity.EnableShadowInFirstPerson = true;
		}

		public override void TakeDamage( DamageInfo info )
		{
			TookDamage( info.Flags, info.Position, info.Force );

			base.TakeDamage( info );
		}

		[ClientRpc]
		public void TookDamage( DamageFlags damageFlags, Vector3 forcePos, Vector3 force )
		{

		}

		[ClientRpc]
		private void BecomeRagdollOnClient( Vector3 velocity, DamageFlags damageFlags, Vector3 forcePos, Vector3 force, int bone )
		{
			ModelEntity ent = new ModelEntity();
			ent.Position = Position;
			ent.Rotation = Rotation;
			ent.Scale = Scale;
			ent.MoveType = MoveType.Physics;
			ent.UsePhysicsCollision = true;
			ent.EnableAllCollisions = true;
			ent.CollisionGroup = CollisionGroup.Debris;
			ent.SetModel( GetModelName() );
			ent.CopyBonesFrom( this );
			ent.CopyBodyGroups( this );
			ent.CopyMaterialGroup( this );
			ent.TakeDecalsFrom( this );
			ent.EnableHitboxes = true;
			ent.EnableAllCollisions = true;
			ent.SurroundingBoundsMode = SurroundingBoundsType.Physics;
			ent.RenderColorAndAlpha = RenderColorAndAlpha;
			ent.PhysicsGroup.Velocity = velocity;

			ent.SetInteractsAs( CollisionLayer.Debris );
			ent.SetInteractsWith( CollisionLayer.WORLD_GEOMETRY );
			ent.SetInteractsExclude( CollisionLayer.Player | CollisionLayer.Debris );

			foreach ( Entity child in Children )
			{
				if ( child is ModelEntity e )
				{
					string model = e.GetModelName();

					if ( model != null && !model.Contains( "clothes" ) )
					{
						continue;
					}

					ModelEntity clothing = new ModelEntity();
					clothing.SetModel( model );
					clothing.SetParent( ent, true );
					clothing.RenderColorAndAlpha = e.RenderColorAndAlpha;
				}
			}

			if ( damageFlags.HasFlag( DamageFlags.Bullet ) ||
				 damageFlags.HasFlag( DamageFlags.PhysicsImpact ) )
			{
				PhysicsBody body = bone > 0 ? ent.GetBonePhysicsBody( bone ) : null;

				if ( body != null )
				{
					body.ApplyImpulseAt( forcePos, force * body.Mass );
				}
				else
				{
					ent.PhysicsGroup.ApplyImpulse( force );
				}
			}

			if ( damageFlags.HasFlag( DamageFlags.Blast ) )
			{
				if ( ent.PhysicsGroup != null )
				{
					ent.PhysicsGroup.AddVelocity( (Position - (forcePos + Vector3.Down * 100.0f)).Normal * (force.Length * 0.2f) );
					var angularDir = (Rotation.FromYaw( 90 ) * force.WithZ( 0 ).Normal).Normal;
					ent.PhysicsGroup.AddAngularVelocity( angularDir * (force.Length * 0.02f) );
				}
			}

			Corpse = ent;

			ent.DeleteAsync( 10.0f );
		}
	}
}
