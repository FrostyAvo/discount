using Sandbox;

namespace Discount.Weapons
{
	public partial class MeleeWeapon : HitscanWeapon
	{
		public MeleeWeapon() : base()
		{

		}

		public MeleeWeapon( string weaponData ) : base( weaponData )
		{

		}

		[ClientRpc]
		protected override void ShootEffects()
		{
			Host.AssertClient();

			ViewModelEntity?.SetAnimBool( "attack", true );
			CrosshairPanel?.CreateEvent( "fire" );

			PlaySound( Data.PrimaryFireSound );
		}
	}
}
