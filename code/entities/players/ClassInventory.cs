using Discount.Weapons;

using Sandbox;
using System.Collections.Generic;
using System.Linq;

namespace Discount
{
	public class ClassInventory : IBaseInventory
	{
		public Entity Owner { get; init; }
		public List<Weapon> Contents { get; protected set; } = new List<Weapon>();

		public ClassInventory( Entity owner )
		{
			Owner = owner;
		}

		public virtual void Fill( IEnumerable<Weapon> contents )
		{
			Host.AssertServer();

			if ( contents == null )
			{
				return;
			}

			Contents = new List<Weapon>(contents);

			for ( int i = 0; i < Contents.Count; i++ )
			{
				Weapon thisWeapon = Contents[i];

				thisWeapon.Slot = i;

				if ( thisWeapon.Owner != null
					|| !thisWeapon.CanCarry( Owner ) )
				{
					continue;
				}

				thisWeapon.Parent = Owner;

				thisWeapon.OnCarryStart( Owner );
			}

			if ( Contents.Count > 0 )
			{
				SetActive( Contents[0] );
			}
		}

		public virtual bool FillSlot( int slot, Weapon weapon )
		{
			if ( slot < 0 )
			{
				return false;
			}

			if ( slot >= Contents.Count )
			{
				Contents.AddRange( Enumerable.Repeat<Weapon>( null, slot - ( Contents.Count - 1 ) ) );
			}

			Contents[slot] = weapon;

			return true;
		}

		public virtual void GiveAmmo( float percentage )
		{
			foreach ( Weapon weapon in Contents )
			{
				if ( weapon is AssetWeapon assetWeapon )
				{
					assetWeapon.GiveAmmo( percentage );
				}
			}
		}

		/// <summary>
		/// Return true if this item belongs in the inventory
		/// </summary>
		public virtual bool CanAdd( Entity ent )
		{
			return ent.CanCarry( Owner );
		}

		/// <summary>
		/// Delete every entity we're carrying. Useful to call on death.
		/// </summary>
		public virtual void DeleteContents()
		{
			Host.AssertServer();

			for ( int i = 0; i < Contents.Count; i++ )
			{
				Contents[i].Delete();
				Contents[i] = null;
			}
		}

		/// <summary>
		/// Get the item in this slot
		/// </summary>
		public virtual Entity GetSlot( int i )
		{
			if ( i < 0 || i >= Contents.Count )
			{
				return null;
			}

			return Contents[i];
		}

		/// <summary>
		/// Returns the number of items in the inventory
		/// </summary>
		public virtual int Count() => Contents.Count;

		/// <summary>
		/// Returns the index of the currently active child
		/// </summary>
		public virtual int GetActiveSlot()
		{
			Entity activeEntity = Owner.ActiveChild;
			int count = Count();

			for ( int i = 0; i < count; i++ )
			{
				if ( Contents[i] == activeEntity )
				{
					return i;
				}
			}

			return -1;
		}

		/// <summary>
		/// Try to pick this entity up
		/// </summary>
		public virtual void Pickup( Entity ent )
		{

		}

		/// <summary>
		/// A child has been added to the Owner (player). Do we want this
		/// entity in our inventory? Yeah? Add it then.
		/// </summary>
		public virtual void OnChildAdded( Entity child )
		{
			/*if ( !CanAdd( child )
				|| child is not Weapon weapon
				|| weapon.Slot < 0
				|| weapon.Slot >= Contents.Length )
			{
				return;
			}

			Contents[weapon.Slot] = weapon;*/
		}

		/// <summary>
		/// A child has been removed from our Owner. This might not even
		/// be in our inventory, if it is then we'll remove it from our list
		/// </summary>
		public virtual void OnChildRemoved( Entity child )
		{
			int count = Count();

			if ( Contains( child ) )
			{
				for ( int i = 0; i < count; i++ )
				{
					if ( Contents[i] == child )
					{
						Contents[i] = null;

						return;
					}
				}
			}
		}

		/// <summary>
		/// Set our active entity to the entity on this slot
		/// </summary>
		public virtual bool SetActiveSlot( int i, bool evenIfEmpty = false )
		{
			if ( i < 0 || i >= Contents.Count )
			{
				return false;
			}

			Entity ent = GetSlot( i );

			if ( Owner.ActiveChild == ent )
			{
				return false;
			}
				
			if ( !evenIfEmpty && ent == null )
			{
				return false;
			}
				
			Owner.ActiveChild = ent;

			return ent.IsValid();
		}

		/// <summary>
		/// Switch to the slot next to the slot we have active.
		/// </summary>
		public virtual bool SwitchActiveSlot( int idelta, bool loop )
		{
			int count = Count();

			if ( count == 0 )
			{
				return false;
			}

			int slot = GetActiveSlot();
			int nextSlot = slot + idelta;

			if ( loop )
			{
				nextSlot %= count;
			}
			else
			{
				if ( nextSlot < 0 || nextSlot >= count )
				{
					return false;
				}
			}

			return SetActiveSlot( nextSlot, false );
		}

		/// <summary>
		/// Drop the active entity. If we can't drop it, will return null
		/// </summary>
		public virtual Entity DropActive()
		{
			return null;
		}

		/// <summary>
		/// Drop this entity. Will return true if successfully dropped.
		/// </summary>
		public virtual bool Drop( Entity ent )
		{
			return false;
		}

		/// <summary>
		/// Returns true if this inventory contains this entity
		/// </summary>
		public virtual bool Contains( Entity ent )
		{
			int count = Count();

			for ( int i = 0; i < count; i++ )
			{
				if ( Contents[i] == ent )
				{
					return true;
				}
			}

			return false;
		}

		/// <summary>
		/// Returns the active entity
		/// </summary>
		public virtual Entity Active => Owner.ActiveChild;

		/// <summary>
		/// Make this entity the active one
		/// </summary>
		public virtual bool SetActive( Entity ent )
		{
			if ( Active == ent )
			{
				return false;
			}

			if ( !Contains( ent ) )
			{
				return false;
			}

			Owner.ActiveChild = ent;

			return true;
		}

		/// <summary>
		/// Try to add this entity to the inventory. Will return true
		/// if the entity was added successfully. 
		/// </summary>
		public virtual bool Add( Entity ent, bool makeActive = false )
		{
			Host.AssertServer();

			return false;
		}
	}
}
