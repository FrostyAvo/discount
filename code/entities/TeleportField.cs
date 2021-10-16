using Sandbox;

using System.Collections.Generic;

namespace Discount
{
	public partial class TeleportField : ModelEntity
	{
		protected readonly List<TeamPlayer> touchingPlayers_;
		protected readonly float teleportDelay_ = 1f;
		protected readonly float fieldRadius_ = 10f;
		protected TimeSince timeSinceTeleported_;
		protected TeleporterExit exit_;

		public Team Team;

		public TeleportField()
		{
			touchingPlayers_ = new List<TeamPlayer>();
		}

		public TeleportField( Team team ) : this()
		{
			Team = team;
		}

		public override void Spawn()
		{
			base.Spawn();

			SetupPhysicsFromSphere( PhysicsMotionType.Static, Position, fieldRadius_ );

			timeSinceTeleported_ = 0;

			CollisionGroup = CollisionGroup.Trigger;
			EnableSolidCollisions = false;
			EnableTouch = true;
		}

		[Event( "server.tick" )]
		public void Tick()
		{
			if ( timeSinceTeleported_ > teleportDelay_
				&& touchingPlayers_.Count > 0
				&& exit_ != null )
			{
				timeSinceTeleported_ = 0;

				PlaySound( "teleporter.use" );
				exit_.PlaySound( "teleporter.use" );

				touchingPlayers_[0].Position = exit_.Position + 20f;
			}
		}

		public void SetExit( TeleporterExit exit )
		{
			exit_ = exit;
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
