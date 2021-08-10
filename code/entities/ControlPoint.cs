using Sandbox;
using Sandbox.UI;
using System.Collections.Generic;

namespace Discount
{
	[Library( "team_control_point" )]
	[Hammer.EntityTool( "Control Point", "Logic", "Defines a control point players can capture for their team." )]
	public partial class ControlPoint : Entity
	{
		public static List<ControlPoint> AllPoints = new List<ControlPoint>();

		[Property( Title = "Default Owner" )]
		public Team DefaultOwner { get; protected set; }

		[Net]
		public Team OwningTeam { get; protected set; }
		[Net]
		public Team CapturingTeam { get; protected set; }
		[Net]
		public bool Blocked { get; protected set; }
		[Net]
		public float CaptureProgress { get; protected set; }

		protected int[] teamPlayersOnPoint_;
		protected int index_;

		[Property( Title = "Control Point Index" )]
		public int Index
		{
			get
			{
				return index_;
			}

			set
			{
				index_ = value;

				// This makes sure the point order doesn't get messed up on the server
				AllPoints.Sort( ( ControlPoint point1, ControlPoint point2 ) =>
				{
					return point1.index_ - point2.index_;
				} );
			}
		}

		public ControlPoint()
		{
			teamPlayersOnPoint_ = new int[4];

			AllPoints.Add( this );

			Transmit = TransmitType.Always;
		}

		public override void Spawn()
		{
			base.Spawn();

			OwningTeam = DefaultOwner;
			CapturingTeam = OwningTeam;

			// This makes sure the point order doesn't get messed up on the client
			if ( IsClient )
			{
				AllPoints.Sort( ( ControlPoint point1, ControlPoint point2 ) =>
				{
					return point1.index_ - point2.index_;
				} );
			}
		}


		[Event( "server.tick" )]
		public void Tick()
		{
			// If nobody is capturing, do nothing
			if ( CapturingTeam == OwningTeam )
			{
				return;
			}

			Blocked = false;

			if ( NonCapturersOnPoint() )
			{
				if ( CapturersOnPoint() )
				{
					Blocked = true;
				}
				else
				{
					CaptureProgress -= Time.Delta;

					if ( CaptureProgress <= 0f )
					{
						CaptureProgress = 0f;

						CapturingTeam = OwningTeam;
					}
				}
			}
			else if ( CapturersOnPoint() )
			{
				CaptureProgress += Time.Delta;

				if ( CaptureProgress >= 1f )
				{
					CaptureProgress = 0f;

					OwningTeam = CapturingTeam;

					ChatBox.AddInformation( To.Everyone, $"{ Teams.GetLongTeamName( OwningTeam ) } has captured a control point!" );
				}
			}
		}

		public void PlayerStartedTouching( TeamPlayer player )
		{
			if ( !EnemiesOnPoint()
				&& player.Team != OwningTeam )
			{
				CapturingTeam = player.Team;

				ChatBox.AddInformation( To.Everyone, $"{ Teams.GetLongTeamName( CapturingTeam ) } has started capturing a control point!" );
			}

			teamPlayersOnPoint_[(int)player.Team]++;
		}

		public void PlayerStoppedTouching( TeamPlayer player )
		{
			teamPlayersOnPoint_[(int)player.Team]--;

			if ( !EnemiesOnPoint() )
			{
				CapturingTeam = OwningTeam;
			}
		}

		protected bool EnemiesOnPoint()
		{
			for (int i = 0; i < 4; i++ )
			{
				if ( i == (int)OwningTeam )
				{
					continue;
				}

				if ( teamPlayersOnPoint_[i] > 0 )
				{
					return true;
				}
			}

			return false;
		}

		protected bool CapturersOnPoint()
		{
			return teamPlayersOnPoint_[(int)CapturingTeam] > 0;
		}

		protected bool NonCapturersOnPoint()
		{
			for ( int i = 0; i < 4; i++ )
			{
				if ( i == (int)CapturingTeam )
				{
					continue;
				}

				if ( teamPlayersOnPoint_[i] > 0 )
				{
					return true;
				}
			}

			return false;
		}

		protected bool PointEmpty()
		{
			for ( int i = 0; i < 4; i++ )
			{
				if ( teamPlayersOnPoint_[i] > 0 )
				{
					return true;
				}
			}

			return false;
		}

		public static ControlPoint GetByName( string name )
		{
			foreach ( ControlPoint point in AllPoints )
			{
				if ( point.EntityName == name )
				{
					return point;
				}
			}

			return null;
		}
	}
}
