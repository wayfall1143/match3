using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class BoardManager : MonoBehaviour {
	public static BoardManager instance;
	public List<Sprite> characters = new List<Sprite>();
	public GameObject tile;
	public int xSize, ySize;

	private GameObject[,] tiles;

	public bool IsShifting { get; set; }

	void Start () {
		instance = GetComponent<BoardManager>();

		Vector2 offset = tile.GetComponent<SpriteRenderer>().bounds.size;
        CreateBoard(offset.x, offset.y);
		ShuffleBoard();
    }



	private void CreateBoard (float xOffset, float yOffset) {
		tiles = new GameObject[xSize, ySize];

        float startX = transform.position.x;
		float startY = transform.position.y;

		Sprite[] previousLeft = new Sprite[ySize]; 	
		Sprite previousBelow = null; 	

		for (int x = 0; x < xSize; x++) {
			for (int y = 0; y < ySize; y++) {
				GameObject newTile = Instantiate(tile, new Vector3(startX + (xOffset * x), startY + (yOffset * y), 0), tile.transform.rotation);
				tiles[x, y] = newTile;
				newTile.transform.parent = transform; 

				List<Sprite> possibleCharacters = new List<Sprite>();
				possibleCharacters.AddRange(characters);

				possibleCharacters.Remove(previousLeft[y]);
				possibleCharacters.Remove(previousBelow);

				Sprite newSprite = possibleCharacters[Random.Range(0, possibleCharacters.Count)];
				newTile.GetComponent<SpriteRenderer>().sprite = newSprite;
				previousLeft[y] = newSprite;
				previousBelow = newSprite;
			}
        }
    }

	public IEnumerator FindNullTiles() {
		for (int x = 0; x < xSize; x++) {
			for (int y = 0; y < ySize; y++) {
				if (tiles[x, y].GetComponent<SpriteRenderer>().sprite == null) {
					yield return StartCoroutine(ShiftTilesDown(x, y));
					break;
				}
			}
		}

		for (int x = 0; x < xSize; x++) {
			for (int y = 0; y < ySize; y++) {
				tiles[x, y].GetComponent<Tile>().ClearAllMatches();
			}
		}
		CheckEndGame(); 
	}

	private IEnumerator ShiftTilesDown(int x, int yStart, float shiftDelay = .03f) {
		IsShifting = true;
		List<SpriteRenderer> renders = new List<SpriteRenderer>();
		int nullCount = 0;

		for (int y = yStart; y < ySize; y++) {
			SpriteRenderer render = tiles[x, y].GetComponent<SpriteRenderer>();
			if (render.sprite == null) {
				nullCount++;
			}
			renders.Add(render);
		}

		for (int i = 0; i < nullCount; i++) {
			GUIManager.instance.Score += 50; 
			yield return new WaitForSeconds(shiftDelay);
			for (int k = 0; k < renders.Count - 1; k++) {
				renders[k].sprite = renders[k + 1].sprite;
				renders[k + 1].sprite = GetNewSprite(x, ySize - 1);
			}
		}
		IsShifting = false;
		
	}

	private Sprite GetNewSprite(int x, int y) {
		List<Sprite> possibleCharacters = new List<Sprite>();
		possibleCharacters.AddRange(characters);

		if (x > 0) {
			possibleCharacters.Remove(tiles[x - 1, y].GetComponent<SpriteRenderer>().sprite);
		}
		if (x < xSize - 1) {
			possibleCharacters.Remove(tiles[x + 1, y].GetComponent<SpriteRenderer>().sprite);
		}
		if (y > 0) {
			possibleCharacters.Remove(tiles[x, y - 1].GetComponent<SpriteRenderer>().sprite);
		}

		return possibleCharacters[Random.Range(0, possibleCharacters.Count)];
	}

private void CheckEndGame() {
    for (int x = 0; x < xSize; x++) {
        for (int y = 0; y < ySize; y++) {
            Sprite sprite = tiles[x, y].GetComponent<SpriteRenderer>().sprite;
            if (sprite != null) {
                if (x < xSize - 2 && sprite == tiles[x + 1, y].GetComponent<SpriteRenderer>().sprite && sprite == tiles[x + 2, y].GetComponent<SpriteRenderer>().sprite) {
                    return;
                }
                if (y < ySize - 2 && sprite == tiles[x, y + 1].GetComponent<SpriteRenderer>().sprite && sprite == tiles[x, y + 2].GetComponent<SpriteRenderer>().sprite) {
                    return;
                }
            }
        }
    }
    Debug.Log("No more valid moves, game over!");
    GameOver();
    ShuffleBoard(); // добавляем вызов метода ShuffleBoard()
}

private void GameOver() {
    
}

private void ShuffleBoard() {
		List<GameObject> allTiles = new List<GameObject>();
		foreach (Transform row in transform) {
			foreach (Transform tile in row.transform) {
				allTiles.Add(tile.gameObject);
			}
		}

		foreach (GameObject tile in allTiles)
			;} 
			List<GameObject> GetNeighbors(int x, int y) {
				List<GameObject> neighbors = new List<GameObject>();

				// Check top neighbor
				if (y < ySize - 1) {
					neighbors.Add(tiles[x, y + 1]);
				}

				// Check bottom neighbor
				if (y > 0) {
					neighbors.Add(tiles[x, y - 1]);
				}

				// Check left neighbor
				if (x > 0) {
					neighbors.Add(tiles[x - 1, y]);
				}

				// Check right neighbor
				if (x < xSize - 1) {
					neighbors.Add(tiles[x + 1, y]);
				}

				return neighbors;
			}

private bool HasPossibleMatches() {
    List<GameObject> matches = new List<GameObject>();
    for (int x = 0; x < xSize; x++) {
        for (int y = 0; y < ySize; y++) {
            Sprite sprite = tiles[x, y].GetComponent<SpriteRenderer>().sprite;
            if (sprite != null) {
                // Check right neighbor
                if (x < xSize - 2 && sprite == tiles[x + 1, y].GetComponent<SpriteRenderer>().sprite && sprite == tiles[x + 2, y].GetComponent<SpriteRenderer>().sprite) {
                    matches.Add(tiles[x, y]);
                    matches.Add(tiles[x + 1, y]);
                    matches.Add(tiles[x + 2, y]);
                }
                // Check top neighbor
                if (y < ySize - 2 && sprite == tiles[x, y + 1].GetComponent<SpriteRenderer>().sprite && sprite == tiles[x, y + 2].GetComponent<SpriteRenderer>().sprite) {
                    matches.Add(tiles[x, y]);
                    matches.Add(tiles[x, y + 1]);
                    matches.Add(tiles[x, y + 2]);
                }
            }
        }
    }
    return matches.Count > 0;
}

void Update() {
    CheckEndGame();
	if (!IsShifting && !HasPossibleMatches()) {
    ShuffleBoard();
}
}

}


