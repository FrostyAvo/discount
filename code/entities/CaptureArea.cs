using Sandbox;
using System.Collections.Generic;

namespace Discount
{
	[Library( "trigger_capture_area" )]
	[Hammer.AutoApplyMaterial( "materials/tools/toolstrigger.vmat" )]
	[Hammer.Solid]
	class CaptureArea : ModelEntity
	{
		[Property( Title = "Control Point" )]
		public string ControlPoint { get; set; }

		protected ControlPoint controlPoint_;
		protected List<TeamPlayer> touchingPlayers_ = new List<TeamPlayer>();

		public override void Spawn()
		{
			base.Spawn();

			SetupPhysicsFromModel( PhysicsMotionType.Static );
			CollisionGroup = CollisionGroup.Trigger;
			EnableSolidCollisions = false;
			EnableTouch = true;

			Transmit = TransmitType.Never;
		}

		[Event( "server.tick" )]
		public void Tick()
		{
			// Getting the control point in Spawn() doesn't work because the ControlPoints haven't been
			// spawned properly yet either and their names are unknown
			if ( controlPoint_ == null )
			{
				controlPoint_ = Discount.ControlPoint.GetByName( ControlPoint );
			}
		}

		public override void StartTouch( Entity other )
		{
			base.StartTouch( other );

			AddToucher( other );
		}

		// This is to make sure we can add a toucher after they have entered the trigger but we were on a cooldown or something (trigger_multiple's wait param)
		public override void Touch( Entity other )
		{
			base.Touch( other );

			AddToucher( other );
		}

		public override void EndTouch( Entity other )
		{
			base.EndTouch( other );

			if ( other.IsWorld
				|| other is not TeamPlayer teamPlayer )
			{
				return;
			}

			if ( !touchingPlayers_.Contains( teamPlayer ) )
			{
				return;
			}

			touchingPlayers_.Remove( teamPlayer );
			controlPoint_?.PlayerStoppedTouching( teamPlayer );
		}

		protected void AddToucher( Entity toucher )
		{
			if ( !toucher.IsValid()
				|| toucher is not TeamPlayer teamPlayer
				|| toucher is SpectatorPlayer)
			{
				return;
			}

			if ( touchingPlayers_.Contains( teamPlayer ) )
			{
				return;
			}

			touchingPlayers_.Add( teamPlayer );
			controlPoint_?.PlayerStartedTouching( teamPlayer );
		}
	}
}
