using Sandbox;

namespace Discount
{
	public partial class TeleporterExit : TeamBuilding
	{
		[Net]
		protected TeleporterEntrance Entrance { get; set; }

		[Net, Predicted]
		protected TimeSince timeSinceCreatedParticles_ { get; set; }

		public override string Model => "models/rust_props/ac_units/ac_unit_b_600x600.vmdl";
		public override float ModelScale => 0.2f;

		public TeleporterExit()
		{
			
		}

		public override void Spawn()
		{
			base.Spawn();

			RenderColor = Teams.GetLightTeamColor( Team );

			if ( IsServer )
			{
				(Owner as TeamPlayer)?.AddOwnedBuilding( this );

				Entrance = (Owner as TeamPlayer)?.TryGetOwnedBuildingOfType<TeleporterEntrance>();

				if ( Entrance != null )
				{
					Entrance.ConnectExit( this );
				}
			}
		}

		public void ConnectEntrance( TeleporterEntrance entrance )
		{
			Entrance = entrance;
		}

		[Event( "server.tick" )]
		public void Tick()
		{
			if ( timeSinceCreatedParticles_ > 1f
				&& Entrance != null )
			{
				timeSinceCreatedParticles_ = 0;

				Particles.Create( "particles/water_bob.vpcf", Position );
				PlaySound( "teleporter.idle" );
			}
		}

		protected override void OnDestroy()
		{
			base.OnDestroy();

			if ( IsServer )
			{
				Entrance?.ConnectExit( null );
			}
		}
	}
}
