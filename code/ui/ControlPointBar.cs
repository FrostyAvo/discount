using Sandbox;
using Sandbox.UI;

using System.Collections.Generic;

namespace Discount.UI
{
	public class ControlPointBar : Panel
	{
		protected readonly List<ControlPointIcon> slots_;
		protected bool iconsCreated_;

		public ControlPointBar()
		{
			slots_ = new List<ControlPointIcon>();
		}

		public override void Tick()
		{
			base.Tick();

			if ( iconsCreated_ )
			{
				return;
			}

			foreach ( ControlPoint point in ControlPoint.AllPoints )
			{
				slots_.Add( new ControlPointIcon( point, this ) );
			}

			iconsCreated_ = true;
		}
	}
}
