using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class testChild : testParent
{
    private void OnTriggerEnter2D(Collider2D collision)
    {
        TryGetComponent<testParent>(out testParent s);
        print(s.s);
    }
}
