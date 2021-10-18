namespace Sandbox
{
	public class PlayerController : WalkController
	{
		public int MaxAirJumps { get; set; } = 0;

		protected int airJumps_;

		public PlayerController() : base()
		{

		}

		// Prevent sprinting and walking
		public override float GetWishSpeed()
		{
			float ws = Duck.GetWishSpeed();

			if ( ws >= 0 )
			{
				return ws;
			}

			return DefaultSpeed;
		}

		public override void CheckJumpButton()
		{
			// If we are in the water most of the way...
			if ( Swimming )
			{
				// swimming, not jumping
				ClearGroundEntity();

				Velocity = Velocity.WithZ( 100 );

				return;
			}

			bool airJumping = false;

			if ( GroundEntity == null )
			{
				if ( airJumps_ >= MaxAirJumps )
				{
					return;
				}

				airJumps_++;
				airJumping = true;
			}

			ClearGroundEntity();

			float flGroundFactor = 1.0f;
			float flMul = 268.3281572999747f * 1.2f;
			float startz = airJumping ? 0 : Velocity.z;

			if ( Duck.IsActive )
			{
				flMul *= 0.8f;
			}

			// If jumping in the air, snap horizontal velocity to input direction for extra air control
			if ( airJumping )
			{
				WishVelocity = new Vector3( Input.Forward, Input.Left, 0 );
				var inSpeed = WishVelocity.Length.Clamp( 0, 1 );
				WishVelocity *= Input.Rotation;

				WishVelocity = WishVelocity.Normal * inSpeed;
				WishVelocity *= GetWishSpeed();

				Velocity = WishVelocity;
			}

			Velocity = Velocity.WithZ( startz + flMul * flGroundFactor );

			if ( !airJumping )
			{
				Velocity -= new Vector3( 0, 0, Gravity * 0.5f ) * Time.Delta;
			}

			AddEvent( "jump" );
		}

		public override void UpdateGroundEntity( TraceResult tr )
		{
			base.UpdateGroundEntity( tr );

			airJumps_ = 0;
		}
	}
}
