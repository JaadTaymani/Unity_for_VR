using System;
using System.Collections.Generic;

// A class used to store saved structure data in its simplest form to be converted to or from JSON.
[Serializable]
public class SaveFile
{
    public string Name;
    public string Info;
    public int Size;

    // The three basis vectors:
    public float[] A1;
    public float[] A2;
    public float[] A3;

    public List<SaveAtom> BasisAtoms;
}

// A class used to store saved StaticCrystal.Atom type object data in its simplest form to be converted to or from JSON.
[Serializable]
public class SaveAtom
{
    public float[] RelativePos; // RelativePos is now saved in (u, v, w) form, whereas the StaticCrystal.Atom class stores it as a Vector3 equal to u*a1 + v*a2 + w*a3.
    public float[] Colour;
}
