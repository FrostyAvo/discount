using Discount.Weapons;

using Sandbox;
using Sandbox.UI;
using Sandbox.UI.Construct;

namespace Discount.UI
{
	public class AmmoPanel : Panel
	{
		public Label ClipLabel;
		public Label ReserveLabel;

		public AmmoPanel()
		{
			ClipLabel = Add.Label( "0", "clip-value" );
			ReserveLabel = Add.Label( "0", "reserve-value" );
		}

		public override void Tick()
		{
			var player = Local.Pawn;

			if ( player == null
				|| player.ActiveChild is not Weapon activeWeapon
				|| activeWeapon is MeleeWeapon )
			{
				if ( Style.Display != DisplayMode.None )
				{
					Style.Display = DisplayMode.None;
					Style.Dirty();
				}

				return;
			}

			if ( Style.Display != DisplayMode.Flex )
			{
				Style.Display = DisplayMode.Flex;
				Style.Dirty();
			}

			ClipLabel.Text = $"{ activeWeapon.AmmoInClip }";
			ReserveLabel.Text = $"{ activeWeapon.AmmoInReserve }";
		}
	}
}
