using UnityEngine;

public class DisassembleManager : MonoBehaviour
{
    public static bool isDisassembled = false;

    private void Awake()
    {
        isDisassembled = false;
    }


    public void OnDisassembleClick()
    {
        isDisassembled = true;

        Debug.Log("Disassemble Mode ON");
    }

    public void OnAssembleClick()
    {
        isDisassembled = false;

        Debug.Log("Disassemble Mode OFF");
    }
}
