using UnityEngine;

public class AimLines
{
    LineRenderer lineRenderer;

    public AimLines(LineRenderer lr)
    {
        lineRenderer = lr;
        DisableAimLine();
    }

    public void EnableAimLine(Vector3 from, Vector3 to)
    {
        lineRenderer.SetPosition(0, from);
        lineRenderer.SetPosition(1, to);
        lineRenderer.enabled = true;
    }

    public void UpdateAimLinePositions(Vector3 from, Vector3 to)
    {
        lineRenderer.SetPosition(0, from);
        lineRenderer.SetPosition(1, to);
    }
    public void DisableAimLine()
    {
        lineRenderer.enabled = false;
    }
}
