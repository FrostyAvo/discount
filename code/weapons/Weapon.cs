using Sandbox;

namespace Discount.Weapons
{
	public abstract partial class Weapon : BaseWeapon
	{
		[Net, Predicted]
		public WeaponData Data { get; protected set; }

		[Net, Predicted, OnChangedCallback]
		public int Slot { get; set; } = -1;

		protected Weapon()
		{
			Data = null;
		}

		protected Weapon( string weaponData )
		{
			Data = Resource.FromPath<WeaponData>( "data/weapons/" + weaponData + ".weapon" );

			SetModel( Data.WorldModelPath );
		}

		public override string ToString()
		{
			return Data != null ? Data.Name : "null";
		}

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
