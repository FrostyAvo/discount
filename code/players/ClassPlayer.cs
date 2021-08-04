using Discount.Weapons;

using Sandbox;

namespace Discount
{
	public partial class ClassPlayer : TeamPlayer
	{
		[Net, Predicted]
		public ClassData Data { get; protected set; }

		public ClassPlayer()
		{
			Inventory = new ClassInventory( this );
			Data = null;
		}

		public ClassPlayer( string classData )
		{
			Inventory = new ClassInventory( this );
			Data = Resource.FromPath<ClassData>( "data/" + classData + ".class" );
		}

		public override void Respawn()
		{
			SetModel( "models/citizen/citizen.vmdl" );

			Controller = new WalkController();
			Animator = new StandardPlayerAnimator();
			Camera = new FirstPersonCamera();

			EnableAllCollisions = true;
			EnableDrawing = true;
			EnableHideInFirstPerson = true;
			EnableShadowInFirstPerson = true;

			if ( Data != null )
			{
				(Controller as WalkController).DefaultSpeed = Data.MoveSpeed;
				Health = Data.Health;

				Wear( Data.HatPath );
				Wear( Team == Team.Red ? Data.RedShirtPath : Data.BlueShirtPath );
				Wear( Team == Team.Red ? Data.RedPantsPath : Data.BluePantsPath );
				Wear( Data.ShoesPath );

				SetBodyGroup( "Legs", 1 );
				SetBodyGroup( "Chest", 1 );
				SetBodyGroup( "Feet", 1 );

				(Inventory as ClassInventory)?.Fill(
					new Weapon[]
					{
					new HitscanWeapon( Data.PrimaryWeapon ),
					new HitscanWeapon( Data.SecondaryWeapon ),
					new MeleeWeapon( Data.MeleeWeapon )
					} );
			}

			base.Respawn();
		}

		/// <summary>
		/// Called every tick, clientside and serverside.
		/// </summary>
		public override void Simulate( Client cl )
		{
			base.Simulate( cl );

			if ( Input.ActiveChild != null )
			{
				ActiveChild = Input.ActiveChild;
			}

			if ( LifeState != LifeState.Alive )
			{
				return;
			}

			SimulateActiveChild( cl, ActiveChild );

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

			if ( Input.Pressed( InputButton.Flashlight ) )
			{
				DiscountGame.ChangeTeam( cl );
			}
		}

		public override void OnKilled()
		{
			base.OnKilled();

			BecomeRagdollOnClient( Velocity, DamageFlags.Blunt, Position, Vector3.Zero, 0 );

			Camera = new SpectateRagdollCamera();
			Controller = null;

			EnableAllCollisions = false;
			EnableDrawing = false;

			Inventory.DeleteContents();
		}

		public void Wear( string model )
		{
			ModelEntity wornEntity = new ModelEntity();

			wornEntity.SetModel( model );
			wornEntity.SetParent( this, true );

			wornEntity.EnableShadowInFirstPerson = true;
			wornEntity.EnableHideInFirstPerson = true;
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
