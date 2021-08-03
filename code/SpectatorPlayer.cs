using Sandbox;

namespace Discount
{
	public partial class SpectatorPlayer : TeamPlayer
	{
		public override void Respawn()
		{
			Controller = new NoclipController();
			Camera = new FirstPersonCamera();

			base.Respawn();

			// Spectators spawn at ground level, move them out of the ground
			Position += Vector3.Up * 150f;
		}

		public override void TakeDamage( DamageInfo info )
		{
			// Don't take damage from triggers etc
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
