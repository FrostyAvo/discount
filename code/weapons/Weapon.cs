using Sandbox;

namespace Discount.Weapons
{
	public abstract partial class Weapon : BaseWeapon
	{
		[Net, Predicted, Change]
		public int Slot { get; set; } = -1;

		protected void OnSlotChanged()
		{
			// This syncs inventories on clients to the corresponding inventories on the server
			if ( Host.IsClient )
			{
				(Owner.Inventory as ClassInventory)?.FillSlot( Slot, this );
			}
		}
	}
}
