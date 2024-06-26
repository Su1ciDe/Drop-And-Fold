using DG.Tweening;
using Fiber.AudioSystem;
using GamePlay.GridSystem;
using GamePlay.Shapes;
using Interfaces;
using TriInspector;
using UnityEngine;
using Grid = GamePlay.GridSystem.Grid;

namespace Models
{
	public class Tile : MonoBehaviour, ITile
	{
		[field: Title("Properties")]
		[field: SerializeField, ReadOnly] public Vector2Int Coordinates { get; set; }
		[SerializeField] protected Vector3 offset;
		public bool IsBusy { get; set; }

		public static readonly float SIZE = 1;
		public static float FOLD_DURATION = .15f;
		protected const float DROP_SPEED = 25f;
		protected const float DESTROY_DURATION = .1f;
		protected const string SEPARATOR_TAG = "Separator";

		protected const float SQUASH_AMOUNT = .15f;
		protected const float SQUASH_MOVE_AMOUNT = .15f;
		protected const float SQUASH_DURATION = .15f;

		public virtual void Drop(GridCell cellToPlace)
		{
			Coordinates = cellToPlace.Coordinates;

			cellToPlace.CurrentTile = this;

			IsBusy = true;
			transform.DOMove(cellToPlace.transform.position + offset, DROP_SPEED).SetSpeedBased(true)
				.SetEase(Ease.InCubic).OnComplete(() =>
				{
					AudioManager.Instance.PlayAudio(AudioName.Place);

					// feedbacks
					var height = Grid.Instance.GridCells.GetLength(1);
					for (int i = Grid.Instance.GridCells.GetLength(1) - 1; i > Coordinates.y; i--)
					{
						// squash
						var tileUnder = Grid.Instance.TryToGetCell(Coordinates.x, i).CurrentTile;

						if (tileUnder is not null)
						{
							if (tileUnder is ShapeCell shapeCell)
								shapeCell.FaceController.Blink(1 / (SQUASH_DURATION * 2f), SQUASH_DURATION * 2);

							if (tileUnder.IsBusy) continue;
							var tileUnderT = tileUnder.GetTransform();
							tileUnderT.DOComplete();
							tileUnderT.DOMoveY(
								-SQUASH_MOVE_AMOUNT - SQUASH_AMOUNT * (height - i) + tileUnderT.position.y,
								SQUASH_DURATION / 2f).SetLoops(2, LoopType.Yoyo);
							tileUnderT.DOScaleY(1f - SQUASH_AMOUNT, SQUASH_DURATION / 2f).SetLoops(2, LoopType.Yoyo);
						}
					}

					transform.DOMoveY(
						-SQUASH_MOVE_AMOUNT - SQUASH_AMOUNT * (height - Coordinates.y) + transform.position.y,
						SQUASH_DURATION / 2f).SetLoops(2, LoopType.Yoyo);
					transform.DOPunchScale(SQUASH_AMOUNT * Vector3.one, SQUASH_DURATION, 1).OnComplete(() =>
					{
						IsBusy = false;
						AfterSquashing();
					});

					AfterDropping();
				});
		}

		protected virtual void AfterDropping()
		{
		}

		protected virtual void AfterSquashing()
		{
		}

		public Transform GetTransform() => transform;
	}
}