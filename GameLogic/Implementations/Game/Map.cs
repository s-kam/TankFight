﻿using System;
using System.Collections.Generic;
using System.Linq;
using GameLogic.Enums;
using GameLogic.Implementations.GameObjects;
using GameLogic.Implementations.Public;
using GameLogic.Interfaces.Map;
using GameLogic.Interfaces.Public;
using GameLogic.Interfaces.Services;

namespace GameLogic.Implementations.Game
{
	internal sealed class Map : IMap
	{
		private readonly IBattlefield battlefield;

		public ICell GetCell(Coordinates coord)
		{
			return this.GetCellByIndex(this.GetIndex(coord));
		}

		public ICell GetCell(int x, int y)
		{
			return this.GetCellByIndex(this.GetIndex(x, y));
		}

		public ICell GetUserCell(string userId)
		{
			return this.battlefield.Cells.FirstOrDefault(
				x => !x.IsEmpty
				     && x.Content.Type == CellContentType.Tank
				     && (x.Content as Tank)?.UserId == userId);
		}

		public IReadOnlyCollection<ICellContentInfo> GetState()
		{
			return this.battlefield.Cells
				.Where(x => !x.IsEmpty)
				.Select(x => new CellContentInfo(x.Coordinates, x.Content.HealthPoint, x.Content.Type))
				.Cast<ICellContentInfo>()
				.ToList()
				.AsReadOnly();
		}

		public IReadOnlyCollection<Coordinates> ClearDeadCells()
		{
			return this.battlefield.Cells
				.Where(cell => !cell.IsEmpty && !cell.Content.IsAlive)
				.Select(cell =>
				{
					cell.Pop();
					return cell.Coordinates;
				})
				.ToList();
		}

		private ICell GetCellByIndex(int index)
		{
			if (index < 0 || index >= this.battlefield.Cells.Count)
			{
				throw new ArgumentOutOfRangeException(nameof(index));
			}

			return this.battlefield.Cells[index];
		}

		private int GetIndex(Coordinates coord) => this.GetIndex(coord.X, coord.Y);

		private int GetIndex(int x, int y) => y * this.battlefield.Width + x;

		public Map(IMapCreationData creationData,
			IBattlefieldBuilder battlefieldBuilder,
			ISpawnService spawnService)
		{
			this.battlefield = battlefieldBuilder.Build(creationData.MapInfo);
			
			spawnService.Spawn(this.battlefield.Cells, creationData.UserContents);
		}
	}
}