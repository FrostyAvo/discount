using Sandbox;

namespace Discount
{
	public partial class Dispenser : TeamBuilding
	{
		[Net]
		protected ResupplyField ResupplyField { get; set; }

		public Dispenser()
		{
			
		}

		public override void Spawn()
		{
			base.Spawn();

			SetModel( "models/rust_props/electrical_boxes/electrical_box_b.vmdl" );
			Scale = 0.8f;

			MoveType = MoveType.Physics;
			CollisionGroup = CollisionGroup.Interactive;
			PhysicsEnabled = true;
			UsePhysicsCollision = true;
			Health = 150;

			RenderColor = Teams.GetLightTeamColor( Team );

			if ( IsServer )
			{
				(Owner as TeamPlayer)?.AddOwnedBuilding( this );

				if ( ResupplyField == null )
				{
					ResupplyField = new ResupplyField( Team )
					{
						Parent = this,
						Position = Position + Vector3.Up * 30f
					};

					ResupplyField.Spawn();
				}
				else
				{
					ResupplyField.Team = Team;
				}
			}
		}

		public override void OnKilled()
		{
			base.OnKilled();

			Particles.Create( "particles/explosion/barrel_explosion/explosion_barrel.vpcf", Position );
			PlaySound( "building.break" );

			if ( IsServer )
			{
				Delete();
			}
		}

		protected override void OnDestroy()
		{
			base.OnDestroy();

			(Owner as TeamPlayer)?.RemoveOwnedBuilding( this );

			if ( IsServer )
			{
				ResupplyField?.Delete();
			}
		}
	}
}
