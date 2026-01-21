using UnityEngine;

public class NoteCursor : MonoBehaviour
{
    public int NextIndex { get; private set; } = 0;

    public void Advance() => NextIndex++;

    public void ResetCursor() => NextIndex = 0;

    public void Set(int index) => NextIndex = Mathf.Max(0, index);
}
