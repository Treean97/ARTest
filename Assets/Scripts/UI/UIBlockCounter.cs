public static class UIBlockCounter
{
    private static int _Count;
    public static bool IsBlocking => _Count > 0;

    public static void Push()
    {
        _Count++;
    }

    public static void Pop()
    {
        _Count--;
        if (_Count < 0) _Count = 0; // 안전장치 (중복 Pop 방지)
    }
}