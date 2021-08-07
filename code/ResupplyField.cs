using Sandbox;

using System.Collections.Generic;

namespace Discount
{
	public partial class ResupplyField : ModelEntity
	{
		protected readonly List<TeamPlayer> touchingPlayers_ = new List<TeamPlayer>();
		protected readonly float resupplyDelay_ = 1f;
		protected readonly float fieldRadius_ = 50f;
		protected TimeSince timeSinceGaveSupplies_;

		[Net]
		public Team Team { get; protected set; }

		public ResupplyField()
		{
			touchingPlayers_ = new List<TeamPlayer>();
		}

		public ResupplyField( Team team ) : this()
		{
			Team = team;
		}

		public override void Spawn()
		{
			base.Spawn();

			SetupPhysicsFromSphere( PhysicsMotionType.Dynamic, Position, fieldRadius_ );

			timeSinceGaveSupplies_ = 0;

			CollisionGroup = CollisionGroup.Trigger;
			EnableSolidCollisions = false;
			EnableTouch = true;
		}

		[Event( "server.tick" )]
		public void Tick()
		{
			if ( timeSinceGaveSupplies_ > resupplyDelay_
				&& touchingPlayers_.Count > 0 )
			{
				timeSinceGaveSupplies_ = 0;

				Particles.Create( "particles/water_bob.vpcf", Position + Vector3.Up * 30f );
				PlaySound( "resupply" );

				foreach ( TeamPlayer touchingPlayer in touchingPlayers_ )
				{
					touchingPlayer.Heal( 20f );
					touchingPlayer.GiveAmmo( 0.3f );
				}
			}
		}

		public override void StartTouch( Entity other )
		{
			base.StartTouch( other );

			if ( other.IsWorld
				|| other is not TeamPlayer teamPlayer )
			{
				return;
			}

			AddToucher( teamPlayer );
		}

		public override void Touch( Entity other )
		{
			base.Touch( other );

			if ( other.IsWorld
				|| other is not TeamPlayer teamPlayer )
			{
				return;
			}

			AddToucher( teamPlayer );
		}

		public override void EndTouch( Entity other )
		{
			base.EndTouch( other );

			if ( other.IsWorld
				|| other is not TeamPlayer teamPlayer )
			{
				return;
			}

			if ( touchingPlayers_.Contains( teamPlayer ) )
			{
				touchingPlayers_.Remove( teamPlayer );
			}
		}

		protected void AddToucher( Entity toucher )
		{
			if ( !toucher.IsValid()
				|| toucher is not TeamPlayer teamPlayer
				|| teamPlayer.Team != Team )
			{
				return;
			}

			if ( touchingPlayers_.Contains( teamPlayer ) )
			{
				return;
			}

			touchingPlayers_.Add( teamPlayer );
		}
	}
}
