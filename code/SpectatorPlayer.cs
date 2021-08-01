using Sandbox;

namespace Discount
{
	partial class SpectatorPlayer : Player
	{
		public override void Respawn()
		{
			Controller = new NoclipController();
			Camera = new FirstPersonCamera();

			base.Respawn();
		}

		public override void Simulate( Client cl )
		{
			base.Simulate( cl );

			if ( Input.Pressed( InputButton.Flashlight ) )
			{
				DiscountGame.ChangeTeam( cl );
			}
		}
	}
}
