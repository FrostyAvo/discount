namespace Sandbox
{
	public class PlayerController : WalkController
	{
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
	}
}
