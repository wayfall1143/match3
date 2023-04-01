using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class Tile : MonoBehaviour {
	private static Color selectedColor = new Color(.5f, .5f, .5f, 1.0f);
	private static Tile previousSelected = null;

	private SpriteRenderer render;
	private bool isSelected = false;

	private Vector2[] adjacentDirections = new Vector2[] { Vector2.up, Vector2.down, Vector2.left, Vector2.right };

	void Awake() {
		render = GetComponent<SpriteRenderer>();
    }

	private void Select() {
		isSelected = true;
		render.color = selectedColor;
		previousSelected = gameObject.GetComponent<Tile>();
		SFXManager.instance.PlaySFX(Clip.Select);
	}

	private void Deselect() {
		isSelected = false;
		render.color = Color.white;
		previousSelected = null;
	}

	void OnMouseDown()
{
    
    if (render.sprite == null || BoardManager.instance.IsShifting)
    {
        return;
    }
 
    
    if (isSelected)
    {
        Deselect();
        return;
    }
 
 
    
    if (previousSelected == null)
    {
        Select();
        return;
    }
 
 
    
    if (GetAllAdjacentTiles().Contains(previousSelected.gameObject))
    {
        Sprite prev = previousSelected.render.sprite;
        Sprite current = render.sprite;
 
        previousSelected.render.sprite = current;
        render.sprite = prev;
 
        if (CombinationExists() || previousSelected.CombinationExists())
        {
            
            previousSelected.render.sprite = prev;
            render.sprite = current;
 
            SwapSprite(previousSelected.render);
            previousSelected.ClearAllMatches();
            previousSelected.Deselect();
            ClearAllMatches();
        }
        else
        {
            
            previousSelected.render.sprite = prev;
            render.sprite = current;
 
            previousSelected.GetComponent<Tile>().Deselect();
            Select();
        }
    }
    else
    {
        previousSelected.GetComponent<Tile>().Deselect();
        Select();
    }
 
}

	public void SwapSprite(SpriteRenderer render2) {
		if (render.sprite == render2.sprite) {
			return;
		}

		Sprite tempSprite = render2.sprite;
		render2.sprite = render.sprite;
		render.sprite = tempSprite;
		SFXManager.instance.PlaySFX(Clip.Swap);
		GUIManager.instance.MoveCounter--; 
	}

	private GameObject GetAdjacent(Vector2 castDir) {
		RaycastHit2D hit = Physics2D.Raycast(transform.position, castDir);
		if (hit.collider != null) {
			return hit.collider.gameObject;
		}
		return null;
	}

	private List<GameObject> GetAllAdjacentTiles() {
		List<GameObject> adjacentTiles = new List<GameObject>();
		for (int i = 0; i < adjacentDirections.Length; i++) {
			adjacentTiles.Add(GetAdjacent(adjacentDirections[i]));
		}
		return adjacentTiles;
	}

	private List<GameObject> FindMatch(Vector2 castDir) {
		List<GameObject> matchingTiles = new List<GameObject>();
		RaycastHit2D hit = Physics2D.Raycast(transform.position, castDir);
		while (hit.collider != null && hit.collider.GetComponent<SpriteRenderer>().sprite == render.sprite) {
			matchingTiles.Add(hit.collider.gameObject);
			hit = Physics2D.Raycast(hit.collider.transform.position, castDir);
		}
		return matchingTiles;
	}

	private void ClearMatch(Vector2[] paths) {
		List<GameObject> matchingTiles = new List<GameObject>();
		for (int i = 0; i < paths.Length; i++) { matchingTiles.AddRange(FindMatch(paths[i])); }
		if (matchingTiles.Count >= 2) {
			for (int i = 0; i < matchingTiles.Count; i++) {
				matchingTiles[i].GetComponent<SpriteRenderer>().sprite = null;
			}
			matchFound = true;
		}
	}

	private bool matchFound = false;
	public void ClearAllMatches() {
		if (render.sprite == null)
			return;

		ClearMatch(new Vector2[2] { Vector2.left, Vector2.right });
		ClearMatch(new Vector2[2] { Vector2.up, Vector2.down });
		if (matchFound) {
			render.sprite = null;
			matchFound = false;
			StopCoroutine(BoardManager.instance.FindNullTiles()); 
			StartCoroutine(BoardManager.instance.FindNullTiles()); 
			SFXManager.instance.PlaySFX(Clip.Clear);
		}
	}

	public bool CombinationExists()
{
    var horizontal = new[] { Vector2.left, Vector2.right };
    var vertical = new[] { Vector2.up, Vector2.down };
 
    List<GameObject> matchingTiles = new List<GameObject>();
 
    for (int i = 0; i < horizontal.Length; i++)
        matchingTiles.AddRange(FindMatch(horizontal[i]));
 
    if (matchingTiles.Count >= 2)
        return true;
 
    matchingTiles.Clear();
    for (int i = 0; i < vertical.Length; i++)
        matchingTiles.AddRange(FindMatch(vertical[i]));
 
    if (matchingTiles.Count >= 2)
        return true;
 
    return false;
 
}

}