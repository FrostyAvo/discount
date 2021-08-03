using Sandbox;

namespace Discount
{
	[Library( "func_respawnroomvisualizer" )]
	[Hammer.Solid]
	public class TeamBarrier : ModelEntity
	{
		[Property( Title = "Team" )]
		public int TeamIndex { get; protected set; } = 0;

		public override void Spawn()
		{
			base.Spawn();

			SetupPhysicsFromModel( PhysicsMotionType.Static );
			CollisionGroup = CollisionGroup.Trigger;
			EnableSolidCollisions = false;
			EnableTouch = true;
			EnableDrawing = true;
			RenderColor = Teams.GetLightTeamColor( (Team)TeamIndex );
		}

		public override void StartTouch( Entity other )
		{
			base.StartTouch( other );

			if ( other.IsWorld || other is not TeamPlayer teamPlayer )
			{
				return;
			}

			AddToucher( teamPlayer );
		}

		public override void Touch( Entity other )
		{
			base.Touch( other );

			if ( other.IsWorld || other is not TeamPlayer teamPlayer )
			{
				return;
			}

			AddToucher( teamPlayer );
		}

		protected void AddToucher( TeamPlayer toucher )
		{
			if ( !toucher.IsValid()
				|| (int)toucher.Team == TeamIndex )
			{
				return;
			}

			// There's no way to disable collisions based on a predicate yet, so just kill enemy players for now
			toucher.TakeDamage( DamageInfo.Generic( 10000f ).WithAttacker( this ) );
		}
	}
}
