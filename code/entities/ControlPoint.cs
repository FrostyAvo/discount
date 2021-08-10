﻿using Sandbox;
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
		public bool Locked { get; protected set; }
		[Net]
		public float CaptureProgress { get; protected set; }
		[Net]
		protected List<int> TeamPlayersOnPoint { get; set; }

		protected int indexInAllPoints_;
		protected bool spawned_;

		[Property( Title = "Control Point Index" )]
		public int Index { get; set; }

		public ControlPoint()
		{
			TeamPlayersOnPoint = new List<int>(new int[4]);

			AllPoints.Add( this );

			Transmit = TransmitType.Always;
		}

		public override void Spawn()
		{
			base.Spawn();

			OwningTeam = DefaultOwner;
			CapturingTeam = OwningTeam;

			spawned_ = true;

			if ( AllPointsSpawned() )
			{
				AllPoints.Sort( ( ControlPoint point1, ControlPoint point2 ) =>
				{
					return point1.Index - point2.Index;
				} );

				for ( int i = 0; i < AllPoints.Count; i++ )
				{
					AllPoints[i].indexInAllPoints_ = i;
					UpdateLockStatus( i );
				}
			}
		}


		[Event( "server.tick" )]
		public void Tick()
		{
			if ( EnemiesOnPoint()
				&& !BeingCaptured()
				&& !Locked )
			{
				CapturingTeam = GetCapturingTeam();

				ChatBox.AddInformation( To.Everyone, $"{ Teams.GetLongTeamName( CapturingTeam ) } has started capturing a control point!" );
			}

			if ( !BeingCaptured() )
			{
				return;
			}

			Blocked = false;

			bool nonCapturersOnPoint = NonCapturersOnPoint();
			bool capturersOnPoint = CapturersOnPoint();

			if ( Locked || (!nonCapturersOnPoint && !capturersOnPoint) )
			{
				// Nobody on point or point is locked from capture
				CaptureProgress -= Time.Delta * 0.05f;

				if ( CaptureProgress <= 0f )
				{
					CaptureProgress = 0f;

					CapturingTeam = OwningTeam;
				}
			}
			else if ( !nonCapturersOnPoint && capturersOnPoint )
			{
				// Only capturers on point
				CaptureProgress += Time.Delta * 0.2f;

				if ( CaptureProgress >= 1f )
				{
					CaptureProgress = 0f;

					OwningTeam = CapturingTeam;

					// Update adjacent point locks
					UpdateLockStatus( indexInAllPoints_ - 1 );
					UpdateLockStatus( indexInAllPoints_ + 1 );

					ChatBox.AddInformation( To.Everyone, $"{ Teams.GetLongTeamName( OwningTeam ) } has captured a control point!" );

					if ( AllPointsOwnedByOneTeam() )
					{
						ChatBox.AddInformation( To.Everyone, $"{ Teams.GetLongTeamName( OwningTeam ) } wins the round!" );

						DiscountGame.Reset();
					}
				}
			}
			else if ( nonCapturersOnPoint && !capturersOnPoint )
			{
				// Only non-capturers on point
				CaptureProgress -= Time.Delta * 0.1f;

				if ( CaptureProgress <= 0f )
				{
					CaptureProgress = 0f;

					CapturingTeam = OwningTeam;
				}
			}
			else // ( nonCapturersOnPoint && capturersOnPoint )
			{
				// Both capturers and non-capturers on point
				Blocked = true;
			}
		}

		public void PlayerStartedTouching( TeamPlayer player )
		{
			TeamPlayersOnPoint[(int)player.Team]++;
		}

		public void PlayerStoppedTouching( TeamPlayer player )
		{
			TeamPlayersOnPoint[(int)player.Team]--;
		}

		public bool BeingCaptured()
		{
			return CapturingTeam != OwningTeam;
		}

		public int CapturerCount()
		{
			return BeingCaptured() ? TeamPlayersOnPoint[(int)CapturingTeam] : 0;
		}

		protected bool EnemiesOnPoint()
		{
			for (int i = 0; i < 4; i++ )
			{
				if ( i == (int)OwningTeam )
				{
					continue;
				}

				if ( TeamPlayersOnPoint[i] > 0 )
				{
					return true;
				}
			}

			return false;
		}

		protected bool CapturersOnPoint()
		{
			return BeingCaptured() && TeamPlayersOnPoint[(int)CapturingTeam] > 0;
		}

		protected bool NonCapturersOnPoint()
		{
			for ( int i = 0; i < 4; i++ )
			{
				if ( i == (int)CapturingTeam )
				{
					continue;
				}

				if ( TeamPlayersOnPoint[i] > 0 )
				{
					return true;
				}
			}

			return false;
		}

		protected Team GetCapturingTeam()
		{
			for ( int i = 0; i < 4; i++ )
			{
				if ( i == (int)OwningTeam )
				{
					continue;
				}

				if ( TeamPlayersOnPoint[i] > 0 )
				{
					return (Team)i;
				}
			}

			return OwningTeam;
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

		public static void ResetPoints()
		{
			foreach ( ControlPoint point in AllPoints )
			{
				point.OwningTeam = point.DefaultOwner;
			}

			for ( int i = 0; i < AllPoints.Count; i++ )
			{
				UpdateLockStatus( i );
			}
		}

		protected static bool AllPointsSpawned()
		{
			foreach ( ControlPoint point in AllPoints )
			{
				if ( !point.spawned_ )
				{
					return false;
				}
			}

			return true;
		}

		protected static bool AllPointsOwnedByOneTeam()
		{
			if ( AllPoints.Count == 0 )
			{
				return false;
			}

			Team firstPointOwner = AllPoints[0].OwningTeam;

			for ( int i = 1; i < AllPoints.Count; i++ )
			{
				if ( AllPoints[i].OwningTeam != firstPointOwner )
				{
					return false;
				}
			}

			return true;
		}

		protected static void UpdateLockStatus( int pointIndex )
		{
			if ( pointIndex < 0
				|| pointIndex >= AllPoints.Count )
			{
				return;
			}

			// Lock point if it's either
			// Option 1: Owned by nobody (unassigned team) and only has adjacent
			// points also owned by nobody
			// Option 2: Owned by a team and doesn't have an adjacent point
			// belonging to the other team

			if (// Option 1
				AllPoints[pointIndex].OwningTeam == Team.Unassigned
					// Check previous point
					&& ( pointIndex - 1 < 0
						|| AllPoints[pointIndex - 1].OwningTeam == Team.Unassigned )
					// Check next point
					&& ( pointIndex + 1 >= AllPoints.Count
						|| AllPoints[pointIndex + 1].OwningTeam == Team.Unassigned )
				// Option 2
				|| AllPoints[pointIndex].OwningTeam != Team.Unassigned
					// Check previous point
					&& ( pointIndex - 1 < 0
						|| AllPoints[pointIndex - 1].OwningTeam == AllPoints[pointIndex].OwningTeam
						|| AllPoints[pointIndex - 1].OwningTeam == Team.Unassigned )
					// Check next point
					&& ( pointIndex + 1 >= AllPoints.Count
						|| AllPoints[pointIndex + 1].OwningTeam == AllPoints[pointIndex].OwningTeam
						|| AllPoints[pointIndex + 1].OwningTeam == Team.Unassigned) )
			{
				AllPoints[pointIndex].Locked = true;
			}
			else
			{
				AllPoints[pointIndex].Locked = false;
			}
		}
	}
}
