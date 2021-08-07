using Sandbox;

namespace Discount
{
	public partial class Dispenser : TeamBuilding
	{
		[Net]
		protected ResupplyField ResupplyField { get; set; }

		public Dispenser() : this( Team.Unassigned )
		{

		}

		public Dispenser( Team team )
		{
			Team = team;
		}

		public override void Spawn()
		{
			base.Spawn();

			SetModel( "models/rust_props/electrical_boxes/electrical_box_c.vmdl" );

			MoveType = MoveType.Physics;
			CollisionGroup = CollisionGroup.Interactive;
			PhysicsEnabled = true;
			UsePhysicsCollision = true;
			Health = 150;

			RenderColor = Teams.GetLightTeamColor( Team );

			Transmit = TransmitType.Always;

			if ( IsServer )
			{
				ResupplyField = new ResupplyField( Team );
				ResupplyField.Parent = this;
				ResupplyField.Spawn();
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

			if ( IsServer )
			{
				ResupplyField?.Delete();
			}
		}
	}
}
