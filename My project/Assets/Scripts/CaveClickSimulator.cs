
using UnityEngine;

public class CaveClickSimulator : MonoBehaviour
{
    public GridGenerator gridGenerator;

    void Update()
    {
        if (gridGenerator == null || Camera.main == null)
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
            gridGenerator.TrySelectAtWorldPosition(
                player,
                hit.point
            );
        }
    }
}
