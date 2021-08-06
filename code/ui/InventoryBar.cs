using Sandbox;
using Sandbox.UI;
using System.Collections.Generic;

namespace Discount.UI
{
	public class InventoryBar : Panel
	{
		protected readonly List<InventoryIcon> slots_;

		public InventoryBar()
		{
			slots_ = new List<InventoryIcon>();

			for ( int i = 0; i < 3; i++ )
			{
				slots_.Add( new InventoryIcon( i + 1, this ) );
			}
		}

		public override void Tick()
		{
			base.Tick();

			Entity player = Local.Pawn;

			if ( player == null || player.Inventory == null )
			{
				if ( slots_.Count != 0 )
				{
					ChangeSlotCount( 0 );
				}

				return;
			}

			int playerItemCount = player.Inventory.Count();

			if ( playerItemCount != slots_.Count )
			{
				ChangeSlotCount( playerItemCount );
			}

			for ( int i = 0; i < playerItemCount; i++ )
			{
				UpdateIcon( player.Inventory.GetSlot( i ), slots_[i], i );
			}
		}

		private void ChangeSlotCount( int newCount )
		{
			/*if ( newCount < slots_.Count )
			{
				slots_.RemoveRange( newCount, slots_.Count - newCount );
			}
			else if ( newCount > slots_.Count )
			{
				int slotsToAdd = newCount - slots_.Count;

				for ( int i = 0; i < slotsToAdd; i++ )
				{
					slots_.Add( new InventoryIcon( i + 1, this ) );
				}
			}*/
		}

		private static void UpdateIcon( Entity ent, InventoryIcon inventoryIcon, int i )
		{
			if ( ent == null )
			{
				inventoryIcon.Clear();
				return;
			}

			inventoryIcon.TargetEnt = ent;
			inventoryIcon.Label.Text = ent.ToString();
			inventoryIcon.SetClass( "active", ent.IsActiveChild() );
		}

		[Event.BuildInput]
		public void ProcessClientInput( InputBuilder input )
		{
			Player player = Local.Pawn as Player;

			if ( player == null )
			{
				return;
			}

			IBaseInventory inventory = player.Inventory;

			if ( inventory == null )
			{
				return;
			}

			if ( input.Pressed( InputButton.Slot1 ) ) SetActiveSlot( input, inventory, 0 );
			if ( input.Pressed( InputButton.Slot2 ) ) SetActiveSlot( input, inventory, 1 );
			if ( input.Pressed( InputButton.Slot3 ) ) SetActiveSlot( input, inventory, 2 );
			if ( input.Pressed( InputButton.Slot4 ) ) SetActiveSlot( input, inventory, 3 );
			if ( input.Pressed( InputButton.Slot5 ) ) SetActiveSlot( input, inventory, 4 );
			if ( input.Pressed( InputButton.Slot6 ) ) SetActiveSlot( input, inventory, 5 );
			if ( input.Pressed( InputButton.Slot7 ) ) SetActiveSlot( input, inventory, 6 );
			if ( input.Pressed( InputButton.Slot8 ) ) SetActiveSlot( input, inventory, 7 );
			if ( input.Pressed( InputButton.Slot9 ) ) SetActiveSlot( input, inventory, 8 );

			if ( input.MouseWheel != 0 )
			{
				SwitchActiveSlot( input, inventory, -input.MouseWheel );
			}
		}

		private static void SetActiveSlot( InputBuilder input, IBaseInventory inventory, int i )
		{
			Entity player = Local.Pawn;

			if ( player == null )
			{
				return;
			}

			Entity ent = inventory.GetSlot( i );

			if ( player.ActiveChild == ent )
			{
				return;
			}

			if ( ent == null )
			{
				return;
			}

			input.ActiveChild = ent;
		}

		private static void SwitchActiveSlot( InputBuilder input, IBaseInventory inventory, int idelta )
		{
			int count = inventory.Count();

			if ( count == 0 )
			{
				return;
			}

			int slot = inventory.GetActiveSlot();
			int nextSlot = slot + idelta;

			while ( nextSlot < 0 )
			{
				nextSlot += count;
			}

			while ( nextSlot >= count )
			{
				nextSlot -= count;
			}

			SetActiveSlot( input, inventory, nextSlot );
		}
	}
}

