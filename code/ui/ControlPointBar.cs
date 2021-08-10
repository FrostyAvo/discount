using Sandbox.UI;

using System.Collections.Generic;

namespace Discount.UI
{
	public class ControlPointBar : Panel
	{
		protected readonly List<ControlPointIcon> slots_;

		public ControlPointBar()
		{
			slots_ = new List<ControlPointIcon>();
		}

		public override void Tick()
		{
			base.Tick();

			int pointCount = ControlPoint.AllPoints.Count;

			if ( pointCount != slots_.Count )
			{
				ChangeSlotCount( pointCount );
			}
		}

		private void ChangeSlotCount( int newCount )
		{
			if ( newCount < slots_.Count )
			{
				foreach ( ControlPointIcon slot in slots_.GetRange( newCount, slots_.Count - newCount ) )
				{
					slot?.Delete();
				}

				slots_.RemoveRange( newCount, slots_.Count - newCount );
			}
			else if ( newCount > slots_.Count )
			{
				int slotsToAdd = newCount - slots_.Count;

				for ( int i = 0; i < slotsToAdd; i++ )
				{
					slots_.Add( new ControlPointIcon( ControlPoint.AllPoints[i], this ) );
				}
			}
		}
	}
}
