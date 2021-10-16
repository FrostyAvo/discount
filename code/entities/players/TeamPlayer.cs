using Sandbox;

using System;
using System.Collections.Generic;

namespace Discount
{
	public abstract partial class TeamPlayer : Player, ITeamEntity
	{
		[Net, Predicted]
		public Team Team { get; set; }

		[Net]
		protected List<TeamBuilding> OwnedBuildings { get; set; }

		public virtual int MaxHealth => 100;

		public virtual void TakeHealthPickup( PickupSize size )
		{
			switch ( size )
			{
				case PickupSize.Small:
					Heal( MaxHealth * 0.20f );
					break;

				case PickupSize.Medium:
					Heal( MaxHealth * 0.50f );
					break;

				case PickupSize.Large:
					Heal( MaxHealth * 1.00f );
					break;
			}
		}

		public virtual void TakeAmmoPickup( PickupSize size )
		{
			switch ( size )
			{
				case PickupSize.Small:
					GiveAmmo( 0.2f );
					break;

				case PickupSize.Medium:
					GiveAmmo( 0.5f );
					break;

				case PickupSize.Large:
					GiveAmmo( 1.0f );
					break;
			}
		}

		public virtual void Heal( float amount )
		{
			if ( amount <= 0 )
			{
				return;
			}

			Health += Math.Min( amount, MaxHealth - Health );
		}

		public virtual void GiveAmmo( float percentage )
		{
			
		}

		public void AddOwnedBuilding( TeamBuilding building )
		{
			if ( !OwnedBuildings.Contains( building ) )
			{
				OwnedBuildings.Add( building );
			}
		}

		public void RemoveOwnedBuilding( TeamBuilding building )
		{
			if ( OwnedBuildings.Contains( building ) )
			{
				OwnedBuildings.Remove( building );
			}
		}

		public bool OwnsBuildingOfType<T>() where T : TeamBuilding
		{
			foreach ( TeamBuilding building in OwnedBuildings )
			{
				if ( building.GetType() == typeof(T) )
				{
					return true;
				}
			}

			return false;
		}

		public T TryGetOwnedBuildingOfType<T>() where T : TeamBuilding
		{
			foreach ( TeamBuilding building in OwnedBuildings )
			{
				if ( building.GetType() == typeof( T ) )
				{
					return (T)building;
				}
			}

			return null;
		}

		public void DeleteOwnedBuildings()
		{
			if ( IsServer )
			{
				Queue<TeamBuilding> deletionQueue = new Queue<TeamBuilding>( OwnedBuildings );

				while ( deletionQueue.TryDequeue( out TeamBuilding building ) )
				{
					building?.Delete();
				}
			}

			OwnedBuildings.Clear();
		}

		protected override void OnDestroy()
		{
			DeleteOwnedBuildings();
		}
	}
}
