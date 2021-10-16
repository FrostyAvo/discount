using Sandbox;

namespace Discount
{
	public partial class TeleporterEntrance : TeamBuilding
	{
		[Net]
		protected TeleporterExit Exit { get; set; }
		[Net]
		protected TeleportField TeleportField { get; set; }

		protected TimeSince timeSinceCreatedParticles_;

		public override string Model => "models/rust_props/ac_units/ac_unit_b_600x600.vmdl";
		public override float ModelScale => 0.2f;

		public TeleporterEntrance()
		{
			
		}

		public override void Spawn()
		{
			base.Spawn();

			RenderColor = Teams.GetLightTeamColor( Team );

			if ( IsServer )
			{
				(Owner as TeamPlayer)?.AddOwnedBuilding( this );

				Exit = (Owner as TeamPlayer)?.TryGetOwnedBuildingOfType<TeleporterExit>();

				if ( Exit != null )
				{
					Exit.ConnectEntrance( this );
				}

				if ( TeleportField == null )
				{
					TeleportField = new TeleportField( Team )
					{
						Parent = this,
						Position = Position + Vector3.Up * 30f
					};

					TeleportField.Spawn();
					TeleportField.SetExit(Exit);
				}
				else
				{
					TeleportField.Team = Team;

					TeleportField.SetExit( Exit );
				}
			}
		}

		public void ConnectExit( TeleporterExit exit )
		{
			Exit = exit;

			TeleportField.SetExit( Exit );
		}

		[Event( "server.tick" )]
		public void Tick()
		{
			if ( timeSinceCreatedParticles_ > 1f
				&& Exit != null )
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
				TeleportField?.Delete();
			}
		}
	}
}
