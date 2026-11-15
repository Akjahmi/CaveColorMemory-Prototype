using UnityEngine;
using UnityEngine.Serialization;

// Mouse test: left click = Player 1 selects a tile, right click = Player 2.
public class CaveClickSimulator : MonoBehaviour
{
    [FormerlySerializedAs("gridGenerator")]
    public GameManager game;

    void Start()
    {
        if (game == null)
            game = FindObjectOfType<GameManager>();
    }

    void Update()
    {
        if (game == null || Camera.main == null)
            return;

        int player = -1;

        if (Input.GetMouseButtonDown(0))
            player = 0; // Player 1

        else if (Input.GetMouseButtonDown(1))
            player = 1; // Player 2

        if (player == -1)
            return;

        Ray ray = Camera.main.ScreenPointToRay(
            Input.mousePosition
        );

        if (Physics.Raycast(ray, out RaycastHit hit))
        {
            game.TrySelectAtWorldPosition(player, hit.point);
        }
    }
}
