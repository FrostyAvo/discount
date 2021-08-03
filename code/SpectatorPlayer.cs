using Sandbox;

namespace Discount
{
	public partial class SpectatorPlayer : Player
	{
		public override void Respawn()
		{
			Controller = new NoclipController();
			Camera = new FirstPersonCamera();

			base.Respawn();

			// Spectators spawn at ground level, move them out of the ground
			Position += Vector3.Up * 150f;
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
